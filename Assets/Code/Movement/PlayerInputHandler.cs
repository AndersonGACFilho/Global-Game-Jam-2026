using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public bool InteractPressed { get; private set; }

    private void LateUpdate()
    {
        InteractPressed = false;
    }

    public void OnMove(InputValue value)
    {
        Move = value.Get<Vector2>();
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed) InteractPressed = true;
    }
}