using UnityEngine;

public class SideScrollerFacingSprite : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Tuning")]
    [SerializeField] private float deadZone = 0.01f;

    public float FacingDirection { get; private set; } = 1f;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void ApplyFacing(Vector2 move)
    {
        if (move.sqrMagnitude < deadZone * deadZone) return;

        if (Mathf.Abs(move.x) < deadZone) return;

        FacingDirection = Mathf.Sign(move.x);
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = FacingDirection < 0f;
        }
    }
}
