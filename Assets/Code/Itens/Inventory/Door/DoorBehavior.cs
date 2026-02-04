using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DoorBehavior : MonoBehaviour, IInteractable
{
    [Header("Door")]
    [SerializeField] private string doorName = "Door";
    [SerializeField] private KeyItemDefinition requiredKey;
    [SerializeField] private string requiredKeyId = "";

    [Header("Open")]
    [SerializeField] private bool startOpen = false;
    [SerializeField] private Collider2D solidCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private bool hideWhenOpen = false;

    [Header("Prompt")]
    [SerializeField] private string promptOpen = "Press [E] to open";
    [SerializeField] private string promptLocked = "Locked - you don't have the key";

    [Header("Highlight")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.cyan;
    
    [Header("Win Condition")]
    [SerializeField] private bool winOnOpen = true;
    [SerializeField] private float winDelayRealtime = 0.1f;

    private bool _isOpen;
    private bool _inRange;
    private bool _highlighted;

    public Transform Transform => transform;
    public string DisplayName => doorName;

    private string KeyId => requiredKey != null ? requiredKey.keyId : requiredKeyId;

    public string Prompt
    {
        get
        {
            if (_isOpen) return "";
            var inv = PlayerInventory.Instance;
            bool hasKey = inv != null && inv.HasKey(KeyId);
            return hasKey ? promptOpen : promptLocked;
        }
    }

    private void Awake()
    {
        if (solidCollider == null) solidCollider = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        SetOpen(startOpen);
        ApplyHighlight();
    }

    public void SetInRange(bool inRange)
    {
        _inRange = inRange;
        ApplyHighlight();
    }

    public void SetHighlighted(bool highlighted)
    {
        _highlighted = highlighted;
        ApplyHighlight();
    }

    private void ApplyHighlight()
    {
        if (spriteRenderer == null) return;
        if (!_inRange) { spriteRenderer.color = normalColor; return; }
        spriteRenderer.color = _highlighted ? highlightColor : normalColor;
    }

    public void Interact(GameObject interactor)
    {
        if (_isOpen) return;

        var inv = interactor.GetComponentInParent<PlayerInventory>();
        if (inv == null) inv = PlayerInventory.Instance;


        bool hasKey = inv != null && inv.HasKey(KeyId);
        if (!hasKey)
        {
            Debug.Log($"Door locked: missing keyId='{KeyId}'");
            return;
        }
        SetOpen(true);

        if (winOnOpen)
        {
            StartCoroutine(WinAfterDelay());
        }
    }
    
    private IEnumerator WinAfterDelay()
    {
        float t = 0f;
        while (t < winDelayRealtime)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (GameWinController.Instance != null)
            GameWinController.Instance.Win();
        else
            Debug.LogWarning("GameWinController não encontrado na cena.");
    }
    
    private void SetOpen(bool open)
    {
        _isOpen = open;

        if (solidCollider != null)
            solidCollider.enabled = !open;

        if (spriteRenderer != null)
        {
            if (open && openSprite != null) spriteRenderer.sprite = openSprite;
            if (open && hideWhenOpen) spriteRenderer.enabled = false;
        }
    }
}
