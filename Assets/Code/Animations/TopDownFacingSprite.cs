using UnityEngine;

public enum Facing { Down, Up, Side }

public class TopDownFacingSprite : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SpriteRenderer sr;

    [Header("Sprites")]
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite sideSprite; // usado pra left e right via flipX

    [Header("Tuning")]
    [SerializeField] private float deadZone = 0.01f;

    public Facing CurrentFacing { get; private set; } = Facing.Down;
    public Vector2 LastMoveDir { get; private set; } = Vector2.down;

    void Awake()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        ApplyFacing(Vector2.down);
    }

    // Chame isso com seu input/velocidade (WASD ou rb.velocity)
    public void ApplyFacing(Vector2 move)
    {
        if (move.sqrMagnitude < deadZone * deadZone) return;

        LastMoveDir = move.normalized;

        // Decide se é mais vertical ou horizontal
        if (Mathf.Abs(LastMoveDir.y) > Mathf.Abs(LastMoveDir.x))
        {
            if (LastMoveDir.y > 0f)
            {
                CurrentFacing = Facing.Up;
                sr.sprite = upSprite;
                sr.flipX = false;
            }
            else
            {
                CurrentFacing = Facing.Down;
                sr.sprite = downSprite;
                sr.flipX = false;
            }
        }
        else
        {
            CurrentFacing = Facing.Side;
            sr.sprite = sideSprite;
            sr.flipX = (LastMoveDir.x < 0f); // true = left, false = right
        }
    }
}