using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI interactionText;
    
    private PlayerStateFlags _flags;

    private readonly HashSet<IInteractable> _inRange = new();
    private IInteractable _closest;
    private IInteractable _forced;

    private void Update()
    {
        UpdateClosest();
    }
    
    private void Awake()
    {
        _flags = GetComponentInParent<PlayerStateFlags>();
    }
    
    public void TryInteract()
    {
        if (_flags != null && !_flags.CanInteract) return;

        UpdateClosest();
        if (_closest == null) return;

        _closest.Interact(gameObject);
        UpdateClosest();
    }
    
    public void ForceInteractable(IInteractable target) 
    {
        _forced = target;
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

        if (best == _closest)
        {
            RefreshUI();
            return;
        }

        if (_closest != null)
            _closest.SetHighlighted(false);

        _closest = best;

        if (_closest != null)
            _closest.SetHighlighted(true);

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (interactionText == null) return;

        if (_closest != null)
        {
            var p = _closest.Prompt ?? "";
            var n = _closest.DisplayName ?? "";

            interactionText.text = string.IsNullOrEmpty(p) ? "" : $"{p} \"{n}\"";
        }
        else
        {
            interactionText.text = "";
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var it = other.GetComponentInParent<IInteractable>();
        if (it == null) return;

        _inRange.Add(it);
        it.SetInRange(true);
        UpdateClosest();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var it = other.GetComponentInParent<IInteractable>();
        if (it == null) return;

        it.SetInRange(false);
        _inRange.Remove(it);

        if (it == _closest)
            UpdateClosest();
    }
}
