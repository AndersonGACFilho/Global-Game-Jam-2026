using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MovementBehavior))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerBehavior : EntityBehavior
{
    [Header("Combat")]
    [SerializeField] private Collider2D attackHitbox;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float attackCooldown = 0.35f;
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTrigger = "Attack";

    [Header("Dash Feedback (optional)")]
    [SerializeField] private AudioSource dashAudio;
    [SerializeField] private AudioClip dashClip;

    private PlayerInputHandler _input;
    private MovementBehavior _movement;
    private float _lastAttackTime = -999f;


    private void Awake()
    {
        _input = GetComponent<PlayerInputHandler>();
        _movement = GetComponent<MovementBehavior>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (attackHitbox != null)
        {
            attackHitbox.enabled = false;
        }
    }

    private void Update()
    {
        HandleAttack();
    }

    private void HandleAttack()
    {
        if (_input == null) return;
        if (!_input.ConsumeAttackPressed()) return;

        if (Time.time - _lastAttackTime < attackCooldown) return;
        _lastAttackTime = Time.time;

        if (attackHitbox != null)
        {
            attackHitbox.enabled = true;
            Invoke(nameof(DisableAttackHitbox), attackDuration);
        }

        if (animator != null && !string.IsNullOrWhiteSpace(attackTrigger))
        {
            animator.SetTrigger(attackTrigger);
        }
    }

    private void DisableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.enabled = false;
        }
    }

    public void PlayDashSfx()
    {
        if (dashAudio == null || dashClip == null) return;
        dashAudio.PlayOneShot(dashClip);
    }
}
