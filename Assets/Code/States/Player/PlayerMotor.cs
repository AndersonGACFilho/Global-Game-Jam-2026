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

    private void FixedUpdate()
    {
        if (movement == null || input == null) return;

        bool canMove = (flags == null) || flags.CanMove;

        var intent = canMove ? input.Move : Vector2.zero;
        movement.SetMoveIntent(intent);
    }
}