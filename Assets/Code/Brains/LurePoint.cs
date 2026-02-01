using UnityEngine;

public class LurePoint : MonoBehaviour
{
    [Header("Lure Settings")]
    [SerializeField] private float lureRadius = 8f;
    [SerializeField] private float investigateDuration = 4f;
    [SerializeField] private bool forceOverrideChase = false;

    [Header("Layers")]
    [SerializeField] private LayerMask vampireMask = ~0;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 2.5f;

    public void Init(float radius, float duration, LayerMask vampMask, bool forceChase, float life)
    {
        lureRadius = radius;
        investigateDuration = duration;
        vampireMask = vampMask;
        forceOverrideChase = forceChase;
        lifetime = life;
    }

    private void Start()
    {
        InstigateNearestVampire();
        Destroy(gameObject, Mathf.Max(0.05f, lifetime));
    }

    private void InstigateNearestVampire()
    {
        Vector2 p = transform.position;

        var hits = Physics2D.OverlapCircleAll(p, lureRadius, vampireMask);
        VampireFSM best = null;
        float bestDist = float.PositiveInfinity;

        foreach (var h in hits)
        {
            var fsm = h.GetComponentInParent<VampireFSM>();
            if (fsm == null) continue;

            float d = ((Vector2)fsm.transform.position - p).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = fsm;
            }
        }

        if (best != null)
            best.TryInvestigatePoint(p, investigateDuration, forceOverrideChase);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lureRadius);
    }
#endif
}