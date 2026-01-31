using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ItemBehavior : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private string itemName = "Item";
    [SerializeField] private string prompt = "Press [E] to pick up";

    [Header("Outline")]
    [SerializeField] private Color normalColor = Color.yellow; 
    [SerializeField] private Color highlightColor = Color.cyan; 
    [SerializeField, Range(1.01f, 1.35f)] private float outlineScale = 1.12f;

    private SpriteRenderer _main;
    private SpriteRenderer _outline;

    private bool _inRange;
    private bool _highlighted;

    public Transform Transform => transform;
    public string DisplayName => itemName;
    public string Prompt => prompt;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        _main = GetComponent<SpriteRenderer>();
        CreateOutlineIfNeeded();

        SetInRange(false);
        SetHighlighted(false);
    }

    private void CreateOutlineIfNeeded()
    {
        var t = transform.Find("OutlineSprite");
        if (t != null) _outline = t.GetComponent<SpriteRenderer>();

        if (_outline == null)
        {
            var go = new GameObject("OutlineSprite");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            _outline = go.AddComponent<SpriteRenderer>();
        }

        _outline.sprite = _main.sprite;
        _outline.sortingLayerID = _main.sortingLayerID;
        _outline.sortingOrder = _main.sortingOrder - 1;
        _outline.transform.localScale = new Vector3(outlineScale, outlineScale, 1f);
        _outline.transform.localPosition = Vector3.zero;
    }

    public void SetInRange(bool inRange)
    {
        _inRange = inRange;

        if (_outline == null) return;

        _outline.enabled = _inRange;
        if (_inRange)
            _outline.color = _highlighted ? highlightColor : normalColor;
    }

    public void SetHighlighted(bool highlighted)
    {
        _highlighted = highlighted;

        if (_outline == null) return;

        if (_inRange)
            _outline.color = _highlighted ? highlightColor : normalColor;
    }

    public void Interact(GameObject interactor)
    {
        Debug.Log($"Picked up: {itemName}");
        Destroy(gameObject);
    }

    private void LateUpdate()
    {
        if (_outline != null && _main != null && _outline.sprite != _main.sprite)
            _outline.sprite = _main.sprite;
    }
}
