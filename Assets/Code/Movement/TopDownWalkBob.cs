using UnityEngine;

[DisallowMultipleComponent]
public class TopDownWalkBob : MonoBehaviour
{
    [Header("Target (visual only)")]
    [Tooltip("Se vazio, usa este Transform. Ideal: um filho visual, não o Rigidbody do player.")]
    [SerializeField] private Transform target;

    [Header("Source (optional)")]
    [Tooltip("Se vazio, tenta pegar Rigidbody2D no pai automaticamente.")]
    [SerializeField] private Rigidbody2D rb2D;

    [Header("Bob settings")]
    [SerializeField] private float minSpeedToBob = 0.05f;
    [SerializeField] private float frequency = 8f;
    [SerializeField] private float blendSpeed = 12f;

    [Header("Shape")]
    [SerializeField] private float bobUp = 0.03f;
    [SerializeField] private float bobSide = 0.012f;

    [Range(0f, 1f)]
    [SerializeField] private float directionalAmount = 0.85f;

    [Header("Optional")]
    [SerializeField] private Vector2 localOffset = Vector2.zero;

    private Vector3 _baseLocalPos;
    private Vector3 _currentOffset;

    private Vector2 _moveIntent;
    private float _phase;
    private float _blend;

    void Awake()
    {
        if (target == null) target = transform;
        if (rb2D == null) rb2D = GetComponentInParent<Rigidbody2D>();

        _baseLocalPos = target.localPosition;
        _currentOffset = Vector3.zero;
    }

    void OnEnable()
    {
        _baseLocalPos = target.localPosition;
        _currentOffset = Vector3.zero;
        _phase = 0f;
        _blend = 0f;
    }

    void OnDisable()
    {
        RemoveOffset();
    }

    /// <summary>Se você já tem um vetor de movimento (WASD), pode usar isso.</summary>
    public void SetMoveIntent(Vector2 move)
    {
        _moveIntent = move;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Remove o offset anterior antes de recalcular (evita drift / acumular offsets)
        RemoveOffset();

        Vector2 vel = GetVelocity();
        float speed = vel.magnitude;

        bool walking = speed > minSpeedToBob;
        float targetBlend = walking ? 1f : 0f;
        _blend = Mathf.MoveTowards(_blend, targetBlend, blendSpeed * Time.deltaTime);

        if (_blend <= 0f)
            return;

        Vector2 dir = speed > 0.0001f ? (vel / speed) : Vector2.up;

        float f = frequency * Mathf.Clamp(speed, 0.6f, 2.2f);
        _phase += Time.deltaTime * f * Mathf.PI * 2f;

        float up = Mathf.Abs(Mathf.Sin(_phase));
        float side = Mathf.Cos(_phase);

        Vector2 baseOffset = new Vector2(side * bobSide, up * bobUp);

        float angle = Mathf.Atan2(dir.y, dir.x);
        float ca = Mathf.Cos(angle);
        float sa = Mathf.Sin(angle);

        Vector2 rotated =
            new Vector2(baseOffset.x * ca - baseOffset.y * sa,
                        baseOffset.x * sa + baseOffset.y * ca);

        Vector2 final2D = Vector2.Lerp(baseOffset, rotated, directionalAmount) * _blend;

        _currentOffset = new Vector3(final2D.x + localOffset.x, final2D.y + localOffset.y, 0f);
        target.localPosition = _baseLocalPos + _currentOffset;
    }

    private void RemoveOffset()
    {
        // Garante que a base não “anda” com o offset
        if (_currentOffset != Vector3.zero && target != null)
            target.localPosition = target.localPosition - _currentOffset;

        _baseLocalPos = target != null ? target.localPosition : _baseLocalPos;
        _currentOffset = Vector3.zero;
    }

    Vector2 GetVelocity()
    {
        if (rb2D != null) return rb2D.linearVelocity;
        return _moveIntent;
    }
}
