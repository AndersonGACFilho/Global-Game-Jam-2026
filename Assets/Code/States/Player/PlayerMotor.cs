using UnityEngine;

[RequireComponent(typeof(MovementBehavior))]
public class PlayerMotor : MonoBehaviour
{
    public MovementBehavior movement;
    public PlayerInputHandler input;
    public PlayerStateFlags flags;

    private void Awake()
    {
        movement = GetComponent<MovementBehavior>();
        input = GetComponent<PlayerInputHandler>();
        flags = GetComponent<PlayerStateFlags>();
    }

    private void Update()
    {
        if (movement == null || input == null) return;

        bool canMove = (flags == null) || flags.CanMove;
        if (!canMove) return;

        if (input.ConsumeJumpPressed())
        {
            movement.RequestJump();
        }

        if (input.ConsumeDashPressed())
        {
            var dashDir = Mathf.Abs(input.Move.x) > 0.01f ? Mathf.Sign(input.Move.x) : movement.FacingDirection;
            movement.RequestDash(dashDir);
        }
    }

    private void FixedUpdate()
    {
        if (movement == null || input == null) return;

        bool canMove = (flags == null) || flags.CanMove;

        var intent = canMove ? input.Move : Vector2.zero;
        movement.SetMoveIntent(intent);
    }
}
