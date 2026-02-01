using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class FadeOccluderAbovePlayer : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform player;

    [Tooltip("Optional: if set, we use bounds from this collider to decide Y reference + optional X gating.")]
    [SerializeField] private Collider2D areaCollider;

    [Header("Fade")]
    [Range(0f, 1f)] public float opaqueAlpha = 1f;
    [Range(0f, 1f)] public float transparentAlpha = 0.25f;
    public float fadeSpeed = 10f;

    [Header("Rule")]
    [Tooltip("If true: fade when occluderY > playerY (what you asked). If false: fade when playerY > occluderY.")]
    public bool fadeWhenOccluderAbovePlayer = false;

    [Tooltip("Which Y of the bounds to use as reference.")]
    public BoundsYReference yReference = BoundsYReference.MinY;

    public enum BoundsYReference { MinY, CenterY, MaxY }

    [Header("Optional Gating (recommended)")]
    [Tooltip("If true, only fades when player.x is inside collider bounds.")]
    public bool requireInsideXBounds = false;

    public float xBoundsPadding = 0.25f;

    [Tooltip("If > 0, only consider occluders near player (prevents whole map fading).")]
    public float maxDistance = 0f;

    private SpriteRenderer[] _sprites;
    private Tilemap[] _tilemaps;
    private Color[] _baseSpriteColors;
    private Color[] _baseTilemapColors;

    private float _alpha;

    private void Awake()
    {
        if (areaCollider == null) areaCollider = GetComponent<Collider2D>();

        _sprites = GetComponentsInChildren<SpriteRenderer>(true);
        _tilemaps = GetComponentsInChildren<Tilemap>(true);

        _baseSpriteColors = new Color[_sprites.Length];
        for (int i = 0; i < _sprites.Length; i++) _baseSpriteColors[i] = _sprites[i].color;

        _baseTilemapColors = new Color[_tilemaps.Length];
        for (int i = 0; i < _tilemaps.Length; i++) _baseTilemapColors[i] = _tilemaps[i].color;

        _alpha = opaqueAlpha;
        ApplyAlpha(_alpha);
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        if (maxDistance > 0f)
        {
            float d2 = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;
            if (d2 > maxDistance * maxDistance)
            {
                // too far => keep opaque
                _alpha = Mathf.MoveTowards(_alpha, opaqueAlpha, fadeSpeed * Time.deltaTime);
                ApplyAlpha(_alpha);
                return;
            }
        }

        float occluderY = GetOccluderY();
        float playerY = player.position.y;

        bool shouldFade = fadeWhenOccluderAbovePlayer
            ? (occluderY > playerY)
            : (playerY > occluderY);

        if (shouldFade && requireInsideXBounds && areaCollider != null)
        {
            var b = areaCollider.bounds;
            float minX = b.min.x - xBoundsPadding;
            float maxX = b.max.x + xBoundsPadding;
            float px = player.position.x;

            shouldFade = (px >= minX && px <= maxX);
        }

        float target = shouldFade ? transparentAlpha : opaqueAlpha;
        _alpha = Mathf.MoveTowards(_alpha, target, fadeSpeed * Time.deltaTime);

        ApplyAlpha(_alpha);
    }

    private float GetOccluderY()
    {
        if (areaCollider != null)
        {
            var b = areaCollider.bounds;
            return yReference switch
            {
                BoundsYReference.MinY => b.min.y,
                BoundsYReference.CenterY => b.center.y,
                BoundsYReference.MaxY => b.max.y,
                _ => b.min.y
            };
        }

        return transform.position.y;
    }

    private void ApplyAlpha(float a)
    {
        for (int i = 0; i < _sprites.Length; i++)
        {
            if (_sprites[i] == null) continue;
            var c = _baseSpriteColors[i];
            c.a = a * _baseSpriteColors[i].a;
            _sprites[i].color = c;
        }

        for (int i = 0; i < _tilemaps.Length; i++)
        {
            if (_tilemaps[i] == null) continue;
            var c = _baseTilemapColors[i];
            c.a = a * _baseTilemapColors[i].a;
            _tilemaps[i].color = c;
        }
    }
}
