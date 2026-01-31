using UnityEngine;
using Code.States;

[DisallowMultipleComponent]
public class VampireVisionSensor : MonoBehaviour
{
    [Header("Refs")]
    public Transform fovOrigin;
    public Transform fovForward;
    public PlayerStateManager playerStateManager;
    private EntityBehavior _playerEntity;

    [Header("FOV Settings")]
    public float viewDistance = 6f;
    [Range(10f, 360f)] public float viewAngle = 90f;
    public float checkInterval = 0.1f;

    [Header("Layers")]
    public LayerMask obstacleMask;

    [Header("Debug")]
    public bool drawGizmos = true;

    public bool IsSeeingPlayer { get; private set; }
    public Vector2 LastSeenPlayerPosition { get; private set; }

    private float _nextCheckTime;

    private void Awake()
    {
        if (fovOrigin == null) fovOrigin = transform;
        if (fovForward == null) fovForward = transform;
        
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError($"VampireVisionSensor: No GameObject with tag 'Player' found!", this);
            return;
        }
        
        if (playerStateManager == null) 
        {
            playerStateManager = player.GetComponent<PlayerStateManager>();
        }
        
        // ADICIONE ESTA VALIDAÇÃO
        if (playerStateManager == null)
        {
            Debug.LogError($"VampireVisionSensor: PlayerStateManager not found on Player!", this);
            return;
        }
        
        // CACHE A REFERÊNCIA DO ENTITYBEHAVIOR
        _playerEntity = player.GetComponent<EntityBehavior>();
        if (_playerEntity == null)
        {
            Debug.LogError($"VampireVisionSensor: EntityBehavior not found on Player!", this);
            return;
        }
        
        if (_playerEntity.entityInteractionCollider == null)
        {
            Debug.LogError($"VampireVisionSensor: InteractionCollider not found on Player EntityBehavior!", this);
        }
    }

    public void ManualTick()
    {
        if (Time.time < _nextCheckTime) return;
        _nextCheckTime = Time.time + Mathf.Max(0.01f, checkInterval);
        RecomputeVision();
    }

    private void RecomputeVision()
    {
        // 1. Basic validation
        if (playerStateManager == null || playerStateManager.flags == null)
        {
            IsSeeingPlayer = false;
            return;
        }
        
        // Check if hidden
        if (playerStateManager.flags.IsHidden)
        {
            IsSeeingPlayer = false;
            return;
        }

        // 2. Get the interaction collider
        if (_playerEntity == null || _playerEntity.entityInteractionCollider == null)
        {
            // Fallback to player transform position if collider is missing
            Vector2 targetPos = playerStateManager.transform.position;
            CheckVisionToTarget(targetPos);
            return;
        }

        Vector2 colliderCenter = _playerEntity.entityInteractionCollider.bounds.center;
        CheckVisionToTarget(colliderCenter);
    }
    
    private void CheckVisionToTarget(Vector2 targetPos)
    {
        Vector2 origin = fovOrigin.position;
        Vector2 toTarget = targetPos - origin;
        float dist = toTarget.magnitude;

        // 3. Distance Check
        if (dist > viewDistance) 
        { 
            IsSeeingPlayer = false; 
            return; 
        }

        // 4. Angle Check
        Vector2 forward = fovForward.up;
        if (viewAngle < 360f)
        {
            float angle = Vector2.Angle(forward, toTarget);
            if (angle > viewAngle * 0.5f) 
            { 
                IsSeeingPlayer = false; 
                return; 
            }
        }

        // 5. Line of Sight Raycast
        Vector2 dir = (dist > 0.001f) ? (toTarget / dist) : Vector2.zero;
        float rayDistance = Mathf.Max(0.1f, dist - 0.1f);
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, rayDistance, obstacleMask);

        IsSeeingPlayer = hit.collider == null;

        if (IsSeeingPlayer)
        {
            LastSeenPlayerPosition = targetPos;
            Debug.DrawLine(origin, targetPos, Color.green, 0.1f);
        }
        else
        {
            Debug.DrawLine(origin, targetPos, Color.red, 0.1f);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Transform originT = fovOrigin != null ? fovOrigin : transform;
        Transform forwardT = fovForward != null ? fovForward : transform;

        Vector3 origin = originT.position;
        Vector3 forward = forwardT.up;

        Gizmos.color = IsSeeingPlayer ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(origin, viewDistance);

        if (viewAngle < 360f)
        {
            float half = viewAngle * 0.5f;
            Vector3 left = Quaternion.Euler(0f, 0f, +half) * forward;
            Vector3 right = Quaternion.Euler(0f, 0f, -half) * forward;

            Gizmos.DrawLine(origin, origin + left.normalized * viewDistance);
            Gizmos.DrawLine(origin, origin + right.normalized * viewDistance);
        }
    }
}