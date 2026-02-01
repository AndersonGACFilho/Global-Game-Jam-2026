using UnityEngine;
using UnityEngine.InputSystem;

public abstract class ThrowableLureItemDefinition : InventoryItemDefinition
{
    [Header("Throw")]
    public float throwDistance = 6f;
    public LayerMask obstacleMask = ~0;

    [Header("Lure")]
    public float lureRadius = 8f;
    public float investigateDuration = 4f;
    public LayerMask vampireMask = ~0;
    public bool forceOverrideChase = false;

    [Header("Visual (optional)")]
    public GameObject lurePrefab;
    public float lureLifetime = 2.5f;

    public override void Use(PlayerInventory inv, GameObject user, PlayerInventory.Slot slot)
    {
        Vector2 origin = user.transform.position;
        Vector2 dir = GetAimDirection(user);

        if (dir.sqrMagnitude < 0.001f) dir = Vector2.up;
        dir.Normalize();

        // raycast to stop at walls
        Vector2 target = origin + dir * throwDistance;
        var hit = Physics2D.Raycast(origin, dir, throwDistance, obstacleMask);
        if (hit.collider != null)
            target = hit.point - dir * 0.05f;

        SpawnLure(target);

        // thrown away
        slot.count -= 1;
    }

    private void SpawnLure(Vector2 pos)
    {
        GameObject go;

        if (lurePrefab != null)
        {
            go = Object.Instantiate(lurePrefab, pos, Quaternion.identity);
        }
        else
        {
            go = new GameObject("LurePoint");
            go.transform.position = pos;
        }

        var lure = go.GetComponent<LurePoint>();
        if (lure == null) lure = go.AddComponent<LurePoint>();

        lure.Init(lureRadius, investigateDuration, vampireMask, forceOverrideChase, lureLifetime);
    }

    private Vector2 GetAimDirection(GameObject user)
    {
        // 1) Aim with mouse if available
        if (Mouse.current != null && Camera.main != null)
        {
            Vector3 mw = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 dir = (Vector2)mw - (Vector2)user.transform.position;
            if (dir.sqrMagnitude > 0.01f) return dir.normalized;
        }

        // 2) fallback: last move input
        var input = user.GetComponent<PlayerInputHandler>();
        if (input != null && input.Move.sqrMagnitude > 0.01f)
            return input.Move.normalized;

        // 3) fallback: velocity direction
        var rb = user.GetComponent<Rigidbody2D>();
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
            return rb.linearVelocity.normalized;

        return Vector2.up;
    }
}
