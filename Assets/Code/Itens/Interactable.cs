using UnityEngine;

public interface IInteractable
{
    Transform Transform { get; }
    string DisplayName { get; }
    string Prompt { get; }

    void SetInRange(bool inRange);
    void SetHighlighted(bool highlighted);
    void Interact(GameObject interactor);
}