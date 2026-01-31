using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInteractor))]
public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 Move { get; private set; }

    private PlayerInteractor _interactor;

    private void Awake()
    {
        _interactor = GetComponent<PlayerInteractor>();
    }

    public void OnMove(InputValue value)
    {
        Move = value.Get<Vector2>();
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
            _interactor.TryInteract();
    }
}