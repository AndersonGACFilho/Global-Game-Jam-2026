using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EntityBehavior))]
public class MovementBehavior : MonoBehaviour
{
    public enum MovementMode
    {
        TopDown,
        SideScroller
    }

    [Header("Components")]
    public Rigidbody2D entityRigidbody;
    public EntityBehavior entityBehavior;
    
    [Header("Visual (optional)")]
    public TopDownWalkBob walkBob;
    public TopDownFacingSprite facingSprite;
    public AnimatorFacingDriver facingAnimator;
    public SideScrollerFacingSprite sideScrollerFacing;

    [Header("Movement Mode")]
    public MovementMode movementMode = MovementMode.SideScroller;

    [Header("Platformer")]
    public float jumpVelocity = 12f;
    public float coyoteTime = 0.1f;
    public float jumpBuffer = 0.1f;
    public float maxFallSpeed = -20f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayers;

    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.6f;

    private Vector2 _desiredDirection = Vector2.zero;
    private float _lastFacingDirection = 1f;
    private float _lastGroundedTime = -999f;
    private float _lastJumpPressedTime = -999f;
    private float _dashTimer = 0f;
    private float _dashCooldownTimer = 0f;
    private float _dashDirection = 1f;

    public bool IsGrounded { get; private set; }
    public float FacingDirection => _lastFacingDirection;
    
    private void Awake()
    {
        entityRigidbody = GetComponent<Rigidbody2D>();
        entityBehavior = GetComponent<EntityBehavior>();
        
        walkBob        = GetComponentInChildren<TopDownWalkBob>(true);
        facingSprite   = GetComponentInChildren<TopDownFacingSprite>(true);
        facingAnimator = GetComponentInChildren<AnimatorFacingDriver>(true);
        sideScrollerFacing = GetComponentInChildren<SideScrollerFacingSprite>(true);

        
        if (entityRigidbody == null)
        {
            Debug.LogError($"MovementBehavior: Rigidbody2D not found on entity {entityBehavior.entityName}" +
                           $" (ID: {entityBehavior.EntityId}).", this);
            throw new System.Exception("Rigidbody2D component is required for MovementBehavior.");
        }
        
        if (entityBehavior == null)
        {
            Debug.LogError($"MovementBehavior: EntityBehavior not found on entity {gameObject.name}.", this);
            throw new System.Exception("EntityBehavior component is required for MovementBehavior.");
        }

        entityRigidbody.bodyType = RigidbodyType2D.Dynamic;
        entityRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        entityRigidbody.gravityScale = movementMode == MovementMode.TopDown ? 0f : 3f;
        entityRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
    }
    
    public void SetMoveIntent(Vector2 direction, float? speed = null)
    {
        _desiredDirection = direction;
        if (Mathf.Abs(direction.x) > 0.01f)
        {
            _lastFacingDirection = Mathf.Sign(direction.x);
        }

        if (movementMode == MovementMode.SideScroller && entityBehavior != null)
        {
            entityBehavior.SetFacingDirection(_lastFacingDirection);
        }
        if (walkBob != null) walkBob.SetMoveIntent(direction);
        if (facingSprite != null) facingSprite.ApplyFacing(direction);
        if (facingAnimator != null) facingAnimator.ApplyFacing(direction);
        if (sideScrollerFacing != null) sideScrollerFacing.ApplyFacing(direction);
    }

    private void FixedUpdate()
    {
        UpdateGrounded();
        ApplyMovement(Time.fixedDeltaTime);
    }

    private void ApplyMovement(float fdt)
    {
        if (_dashCooldownTimer > 0f)
        {
            _dashCooldownTimer = Mathf.Max(0f, _dashCooldownTimer - fdt);
        }

        if (_dashTimer > 0f)
        {
            _dashTimer = Mathf.Max(0f, _dashTimer - fdt);
            entityRigidbody.linearVelocity = new Vector2(_dashDirection * dashSpeed, 0f);
            return;
        }

        var current = entityRigidbody.linearVelocity;

        if (movementMode == MovementMode.TopDown)
        {
            var input = _desiredDirection;
            var inputMag = Mathf.Clamp01(input.magnitude);
            var dir = (inputMag > 0f) ? (input / input.magnitude) : Vector2.zero;

            var target = dir * (entityBehavior.maxVelocity * inputMag);

            var rate = ((target.sqrMagnitude > 0f) ? entityBehavior.accel : entityBehavior.decel) * fdt;
            entityRigidbody.linearVelocity = Vector2.MoveTowards(current, target, rate);
            return;
        }

        var desiredX = Mathf.Clamp(_desiredDirection.x, -1f, 1f);
        var targetX = desiredX * entityBehavior.maxVelocity;
        var rateX = (Mathf.Abs(targetX) > 0.01f ? entityBehavior.accel : entityBehavior.decel) * fdt;
        var newX = Mathf.MoveTowards(current.x, targetX, rateX);

        var newY = current.y;
        if (newY < maxFallSpeed)
        {
            newY = maxFallSpeed;
        }

        if (Time.time - _lastJumpPressedTime <= jumpBuffer && Time.time - _lastGroundedTime <= coyoteTime)
        {
            newY = jumpVelocity;
            _lastJumpPressedTime = -999f;
            _lastGroundedTime = -999f;
        }

        entityRigidbody.linearVelocity = new Vector2(newX, newY);
    }

    public void Stop()
    {
        _desiredDirection = Vector2.zero;
        entityRigidbody.linearVelocity = Vector2.zero;
        entityRigidbody.angularVelocity = 0f;
    }

    public void RequestJump()
    {
        _lastJumpPressedTime = Time.time;
    }

    public void RequestDash(float direction)
    {
        if (_dashCooldownTimer > 0f) return;
        _dashDirection = Mathf.Approximately(direction, 0f) ? _lastFacingDirection : Mathf.Sign(direction);
        _dashTimer = dashDuration;
        _dashCooldownTimer = dashCooldown;
    }

    private void UpdateGrounded()
    {
        if (movementMode == MovementMode.TopDown)
        {
            IsGrounded = false;
            return;
        }

        if (groundCheck == null)
        {
            var found = transform.Find("GroundCheck");
            if (found != null) groundCheck = found;
        }

        if (groundCheck == null)
        {
            IsGrounded = false;
            return;
        }

        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers);
        if (IsGrounded)
        {
            _lastGroundedTime = Time.time;
        }
    }
}
