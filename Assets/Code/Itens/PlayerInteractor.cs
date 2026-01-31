using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI interactionText;

    private readonly HashSet<IInteractable> _inRange = new();
    private IInteractable _closest;

    private void Update()
    {
        UpdateClosest();
    }

    public void TryInteract()
    {
        UpdateClosest();
        if (_closest == null) return;

        _closest.Interact(gameObject);
        UpdateClosest();
    }

    private void UpdateClosest()
    {
        _inRange.RemoveWhere(i => i == null || i.Transform == null);

        IInteractable best = null;
        float bestDist = float.PositiveInfinity;
        Vector3 p = transform.position;

        foreach (var it in _inRange)
        {
            float d = (it.Transform.position - p).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = it; }
        }

        if (best == _closest) return;

        if (_closest != null)
        {
            _closest.SetHighlighted(false);
        }

        _closest = best;

        if (_closest != null)
        {
            _closest.SetHighlighted(true);

            if (interactionText != null)
                interactionText.text = $"{_closest.Prompt} \"{_closest.DisplayName}\"";
        }
        else
        {
            if (interactionText != null)
                interactionText.text = "";
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Item")) return;

        var it = other.GetComponentInParent<IInteractable>();
        if (it == null) return;

        _inRange.Add(it);
        it.SetInRange(true);        
        UpdateClosest();            
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Item")) return;

        var it = other.GetComponentInParent<IInteractable>();
        if (it == null) return;

        it.SetInRange(false);       
        _inRange.Remove(it);

        if (it == _closest)
            UpdateClosest();
    }
}
