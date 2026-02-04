using UnityEngine;

[DisallowMultipleComponent]
public class BreathingBehavior : MonoBehaviour
{
    [Header("Breath feel")]
    [Tooltip("Respirações por minuto (BPM). Ex: 10~18 idle.")]
    [SerializeField] private float breathsPerMinute = 14f;

    [Tooltip("Quanto expande no eixo Y (escala). Ex: 0.01~0.04")]
    [SerializeField] private float scaleYAmount = 0.02f;

    [Tooltip("Quanto expande no eixo X (escala). Ex: 0.0~0.02")]
    [SerializeField] private float scaleXAmount = 0.01f;

    [Tooltip("Opcional: sobe um pouco ao inspirar (use só se NÃO conflitar com outro script de posição).")]
    [SerializeField] private float positionYAmount = 0.0f;

    [Header("Smoothing")]
    [Tooltip("Quão rápido entra/sai do efeito (0 = instantâneo).")]
    [SerializeField] private float blendSpeed = 8f;

    [Tooltip("Se true, usa Time.unscaledTime (útil em pause UI).")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Control")]
    [Tooltip("Se marcado, você pode desligar respiração quando estiver andando via SetMoving(true).")]
    [SerializeField] private bool disableWhenMoving = true;

    private Vector3 _baseScale;
    private Vector3 _basePos;
    private float _blend = 1f;
    private bool _moving;

    void Awake()
    {
        _baseScale = transform.localScale;
        _basePos   = transform.localPosition;
    }

    void OnEnable()
    {
        _baseScale = transform.localScale;
        _basePos   = transform.localPosition;
        _blend = 1f;
    }

    void OnDisable()
    {
        transform.localScale = _baseScale;
        transform.localPosition = _basePos;
    }

    /// <summary>Chame com true quando estiver andando, false quando parado.</summary>
    public void SetMoving(bool moving)
    {
        _moving = moving;
    }

    void LateUpdate()
    {
        float targetBlend = (!disableWhenMoving || !_moving) ? 1f : 0f;
        _blend = Mathf.MoveTowards(_blend, targetBlend, blendSpeed * Time.deltaTime);

        // Se o objeto for reposicionado/escalado por outro sistema, recacheie base:
        // (descomente se necessário)
        // _baseScale = transform.localScale;
        // _basePos = transform.localPosition;

        float t = useUnscaledTime ? Time.unscaledTime : Time.time;

        // freq em Hz
        float hz = Mathf.Max(0.01f, breathsPerMinute / 60f);

        // 0..1 com curva suave (inspira->expira)
        float wave = (Mathf.Sin(t * hz * Mathf.PI * 2f) + 1f) * 0.5f;
        // deixa mais “orgânico”: inspira mais rápido, expira mais lento (opcional)
        float eased = wave * wave * (3f - 2f * wave); // smoothstep

        float sx = 1f + (eased * scaleXAmount * _blend);
        float sy = 1f + (eased * scaleYAmount * _blend);

        transform.localScale = new Vector3(_baseScale.x * sx, _baseScale.y * sy, _baseScale.z);

        if (positionYAmount != 0f)
            transform.localPosition = _basePos + new Vector3(0f, eased * positionYAmount * _blend, 0f);
        else
            transform.localPosition = _basePos;
    }
}
