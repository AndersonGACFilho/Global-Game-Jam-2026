using UnityEngine;

[DisallowMultipleComponent]
public class CameraClamp2D : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;
    public float smoothTime = 0.12f;

    [Header("Look Ahead (locked)")]
    public Rigidbody2D targetRb;
    public float lookAheadX = 2.5f;
    public float lookAheadY = 2.0f;
    public float lookAheadSmoothTime = 0.10f;

    public float stopDeadZone = 0.05f;
    public float directionChangeThresholdDeg = 25f;

    [Header("Bounds (Polygon / Composite supported)")]
    public Collider2D boundsCollider;
    public int clampIterations = 8;
    public float insideEpsilon = 0.0005f; // slightly larger reduces micro-jitter

    Camera _cam;
    Vector3 _followVel;
    Vector3 _lookVel;
    Vector3 _lookOffset;

    Vector2 _lockedDir = Vector2.down;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        if (target != null && targetRb == null)
            targetRb = target.GetComponent<Rigidbody2D>();
    }

    void LateUpdate()
    {
        if (target == null || boundsCollider == null || _cam == null) return;
        if (targetRb == null) targetRb = target.GetComponent<Rigidbody2D>();

        // --- Update locked dir only when moving AND direction changed enough
        Vector2 v = targetRb.linearVelocity;
        float speed = v.magnitude;

        if (speed > stopDeadZone)
        {
            Vector2 dir = v / speed;

            float cosThreshold = Mathf.Cos(directionChangeThresholdDeg * Mathf.Deg2Rad);
            bool changedDir = Vector2.Dot(dir, _lockedDir) < cosThreshold;

            if (changedDir)
                _lockedDir = dir;
        }

        // --- Smooth look offset toward locked direction (offset remains when stopped)
        Vector3 desiredLook = new Vector3(_lockedDir.x * lookAheadX, _lockedDir.y * lookAheadY, 0f);
        _lookOffset = Vector3.SmoothDamp(_lookOffset, desiredLook, ref _lookVel, lookAheadSmoothTime);

        // --- Build desired camera pos (before clamp)
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);
        Vector3 desired = targetPos + _lookOffset;

        // 1) Clamp desired first
        Vector3 clampedDesired = ClampToCollider(desired, out _);

        // 2) SmoothDamp toward clamped target
        Vector3 smoothed = Vector3.SmoothDamp(transform.position, clampedDesired, ref _followVel, smoothTime);

        // 3) Final safety clamp + kill bounce velocity when we hit bounds
        Vector3 final = ClampToCollider(smoothed, out Vector3 correction);

        if (correction != Vector3.zero)
        {
            // prevent spring bounce on clamped axes
            if (Mathf.Abs(correction.x) > 0f) _followVel.x = 0f;
            if (Mathf.Abs(correction.y) > 0f) _followVel.y = 0f;

            // IMPORTANT: shrink lookOffset so it stops pushing into the wall
            // (keeps the "locked" feeling but respects the bounds)
            Vector3 allowedLook = final - targetPos;
            allowedLook.z = 0f;
            _lookOffset = allowedLook;
            _lookVel = Vector3.zero;
        }

        transform.position = final;
    }

    Vector3 ClampToCollider(Vector3 camPos, out Vector3 totalCorrection)
    {
        totalCorrection = Vector3.zero;
        if (!_cam.orthographic) return camPos;

        float halfH = _cam.orthographicSize;
        float halfW = halfH * _cam.aspect;

        for (int iter = 0; iter < clampIterations; iter++)
        {
            bool allInside = true;

            Vector2[] corners =
            {
                new Vector2(camPos.x - halfW, camPos.y - halfH),
                new Vector2(camPos.x - halfW, camPos.y + halfH),
                new Vector2(camPos.x + halfW, camPos.y - halfH),
                new Vector2(camPos.x + halfW, camPos.y + halfH),
            };

            // Apply only the largest needed correction each iteration (more stable)
            Vector2 bestDelta = Vector2.zero;
            float bestMag = 0f;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 p = corners[i];
                Vector2 closest = boundsCollider.ClosestPoint(p);
                Vector2 delta = closest - p;

                float mag = delta.sqrMagnitude;
                if (mag > insideEpsilon && mag > bestMag)
                {
                    bestMag = mag;
                    bestDelta = delta;
                }
            }

            if (bestMag > 0f)
            {
                camPos.x += bestDelta.x;
                camPos.y += bestDelta.y;
                totalCorrection.x += bestDelta.x;
                totalCorrection.y += bestDelta.y;
                allInside = false;
            }

            if (allInside) break;
        }

        return camPos;
    }

    public void SetBounds(Collider2D newBounds) => boundsCollider = newBounds;
}
