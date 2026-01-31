using UnityEngine;

/// <summary>
/// Obstacle avoidance for top-down 2D agents (Unity Physics2D).
///
/// IMPORTANT (matches your current prefab architecture):
/// - Your root "Enemy" object likely has NO Collider2D.
/// - Your colliders live on child objects (e.g., "WorldCollider" / "InteractionCollider") as in EntityBehavior.
/// This script will automatically pick:
///  1) castCollider (if assigned)
///  2) EntityBehavior.entityWorldCollider
///  3) child named "WorldCollider"
///  4) Collider2D on this GO
///  5) any Collider2D in children
///
/// Core behavior:
/// - Uses Collider2D.Cast (shape cast) so it respects the agent's size.
/// - Slides along walls using hit normal + tangent.
/// - Adds smoothing to prevent jitter.
/// - Has optional stuck recovery.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(MovementBehavior))]
public class ObstacleAvoidance : MonoBehaviour
{
    [Header("Collider Source (Optional)")]
    [Tooltip("If set, this collider will be used for casting. If null, the script auto-resolves (see summary).")]
    public Collider2D castCollider;

    [Tooltip("Optional: override cast origin. If null, uses castCollider.bounds.center.")]
    public Transform originOverride;

    [Header("Cast Settings")]
    public float castDistance = 2.0f;
    public float skin = 0.05f;
    public LayerMask obstacleMask;

    [Header("Steering")]
    public float steeringResponsiveness = 12f;
    [Range(0f, 1f)] public float pushAwayStrength = 0.6f;

    [Header("Stuck Recovery")]
    public bool enableStuckRecovery = true;
    public float stuckDistance = 0.01f;
    public float stuckWindowSeconds = 0.5f;
    public float recoveryDurationSeconds = 0.35f;

    [Header("Debug")]
    public bool drawDebug = true;
    public bool logDetections = false;

    private Collider2D _col;

    private ContactFilter2D _filter;
    private readonly RaycastHit2D[] _hits = new RaycastHit2D[8];

    private Vector2 _smoothedDir = Vector2.zero;

    private Vector2 _lastPos;
    private float _stuckTimer = 0f;

    private float _recoveryTimer = 0f;
    private Vector2 _recoveryDir = Vector2.zero;

    private void Awake()
    {
        ResolveColliderOrDisable();

        _filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = obstacleMask,
            useTriggers = false
        };

        if (obstacleMask.value == 0)
            Debug.LogError($"[{gameObject.name}] ObstacleAvoidance: ObstacleMask NOT SET!", this);
    }

    private void ResolveColliderOrDisable()
    {
        // 1) Explicit inspector override
        _col = castCollider;

        // 2) Use EntityBehavior's world collider if present (your prefab pattern)
        if (_col == null)
        {
            var entity = GetComponent<EntityBehavior>();
            if (entity != null && entity.entityWorldCollider != null)
                _col = entity.entityWorldCollider;
        }

        // 3) Child named "WorldCollider" (your prefab pattern)
        if (_col == null)
        {
            var t = transform.Find("WorldCollider");
            if (t != null) _col = t.GetComponent<Collider2D>();
        }

        // 4) Same GO
        if (_col == null) _col = GetComponent<Collider2D>();

        // 5) Any child collider (last resort; might pick InteractionCollider)
        if (_col == null) _col = GetComponentInChildren<Collider2D>();

        if (_col == null)
        {
            Debug.LogError(
                $"[{gameObject.name}] ObstacleAvoidance: NO Collider2D found.\n" +
                $"Fix: make sure your Enemy has a child named 'WorldCollider' with a Collider2D, " +
                $"or assign the correct collider to 'castCollider' in the inspector.",
                this
            );
            enabled = false;
            return;
        }

        _lastPos = GetOrigin();
    }

    private Vector2 GetOrigin()
    {
        if (originOverride != null) return originOverride.position;
        if (_col != null) return _col.bounds.center;
        return transform.position;
    }

    public Vector2 GetAvoidanceAdjustedDirection(Vector2 desiredDirection)
    {
        if (!enabled) return desiredDirection;

        if (_col == null)
        {
            // In case references got reset during hot-reload
            ResolveColliderOrDisable();
            if (!enabled) return desiredDirection;
        }

        if (desiredDirection.sqrMagnitude < 0.0001f)
        {
            _smoothedDir = Vector2.zero;
            return desiredDirection;
        }

        _filter.layerMask = obstacleMask;

        Vector2 desiredDir = desiredDirection.normalized;
        Vector2 origin = GetOrigin();

        UpdateStuckRecovery(origin, desiredDir);

        if (_recoveryTimer > 0f)
        {
            _recoveryTimer -= Time.deltaTime;

            if (drawDebug)
                Debug.DrawRay(origin, _recoveryDir * (castDistance * 0.9f), Color.magenta, 0.05f);

            return _recoveryDir * desiredDirection.magnitude;
        }

        int hitCount = _col.Cast(desiredDir, _filter, _hits, castDistance + skin);

        if (drawDebug)
            Debug.DrawRay(origin, desiredDir * castDistance, hitCount > 0 ? Color.red : Color.green, 0.05f);

        if (hitCount == 0)
        {
            _smoothedDir = SmoothDir(_smoothedDir, desiredDir);
            return _smoothedDir * desiredDirection.magnitude;
        }

        RaycastHit2D best = _hits[0];
        for (int i = 1; i < hitCount; i++)
        {
            if (_hits[i].distance < best.distance)
                best = _hits[i];
        }

        if (logDetections && best.collider != null)
            Debug.Log($"[{gameObject.name}] OBSTACLE: {best.collider.gameObject.name} d={best.distance:F2} n={best.normal}", this);

        Vector2 n = best.normal.normalized;

        Vector2 t1 = new Vector2(-n.y, n.x);
        Vector2 t2 = -t1;

        Vector2 slide = Vector2.Dot(t1, desiredDir) >= Vector2.Dot(t2, desiredDir) ? t1 : t2;

        float danger = Mathf.InverseLerp(castDistance + skin, skin, best.distance);
        Vector2 pushAway = n * (danger * pushAwayStrength);

        Vector2 steer = (slide + pushAway).normalized;

        if (drawDebug)
        {
            Debug.DrawRay(origin, slide * castDistance, Color.cyan, 0.05f);
            Debug.DrawRay(origin, steer * castDistance, Color.blue, 0.05f);
        }

        _smoothedDir = SmoothDir(_smoothedDir, steer);
        return _smoothedDir * desiredDirection.magnitude;
    }

    private Vector2 SmoothDir(Vector2 current, Vector2 target)
    {
        if (current.sqrMagnitude < 0.0001f)
            return target;

        float t = 1f - Mathf.Exp(-steeringResponsiveness * Time.deltaTime);
        Vector2 blended = Vector2.Lerp(current, target, t);
        return blended.sqrMagnitude > 0.0001f ? blended.normalized : target;
    }

    private void UpdateStuckRecovery(Vector2 currentPos, Vector2 desiredDir)
    {
        if (!enableStuckRecovery)
        {
            _lastPos = currentPos;
            _stuckTimer = 0f;
            return;
        }

        float moved = Vector2.Distance(currentPos, _lastPos);

        if (moved < stuckDistance)
        {
            _stuckTimer += Time.deltaTime;

            if (_stuckTimer >= stuckWindowSeconds)
            {
                Vector2 left = new Vector2(-desiredDir.y, desiredDir.x);
                Vector2 right = -left;
                Vector2 back = -desiredDir;

                TryStartRecovery(left);
                if (_recoveryTimer <= 0f) TryStartRecovery(right);
                if (_recoveryTimer <= 0f) TryStartRecovery(back);

                _stuckTimer = 0f;
            }
        }
        else
        {
            _stuckTimer = 0f;
        }

        _lastPos = currentPos;
    }

    private void TryStartRecovery(Vector2 candidateDir)
    {
        Vector2 dir = candidateDir.normalized;
        int count = _col.Cast(dir, _filter, _hits, (castDistance * 0.6f) + skin);

        if (count == 0)
        {
            _recoveryDir = dir;
            _recoveryTimer = recoveryDurationSeconds;

            if (logDetections)
                Debug.Log($"[{gameObject.name}] UNSTUCK: forcing {dir}", this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D c = castCollider;

        if (c == null)
        {
            var entity = GetComponent<EntityBehavior>();
            if (entity != null && entity.entityWorldCollider != null) c = entity.entityWorldCollider;
        }
        if (c == null)
        {
            var t = transform.Find("WorldCollider");
            if (t != null) c = t.GetComponent<Collider2D>();
        }
        if (c == null) c = GetComponent<Collider2D>();
        if (c == null) c = GetComponentInChildren<Collider2D>();
        if (c == null) return;

        Gizmos.color = Color.yellow;
        Vector3 center = originOverride != null ? originOverride.position : c.bounds.center;
        Gizmos.DrawWireSphere(center, castDistance);
    }
}
