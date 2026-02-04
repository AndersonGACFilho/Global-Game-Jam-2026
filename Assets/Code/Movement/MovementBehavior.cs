using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EntityBehavior))]
public class MovementBehavior : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D entityRigidbody;
    public EntityBehavior entityBehavior;
    
    [Header("Visual (optional)")]
    public TopDownWalkBob walkBob;
    public TopDownFacingSprite facingSprite;
    public AnimatorFacingDriver facingAnimator;

    private Vector2 _desiredDirection = Vector2.zero;
    
    private void Awake()
    {
        entityRigidbody = GetComponent<Rigidbody2D>();
        entityBehavior = GetComponent<EntityBehavior>();
        
        walkBob        = GetComponentInChildren<TopDownWalkBob>(true);
        facingSprite   = GetComponentInChildren<TopDownFacingSprite>(true);
        facingAnimator = GetComponentInChildren<AnimatorFacingDriver>(true);

        
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
        entityRigidbody.gravityScale = 0f;
        entityRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
    }
    
    public void SetMoveIntent(Vector2 direction, float? speed = null)
    {
        _desiredDirection = direction;
        if (walkBob != null) walkBob.SetMoveIntent(direction);
        if (facingSprite != null) facingSprite.ApplyFacing(direction);
        if (facingAnimator != null) facingAnimator.ApplyFacing(direction);
    }

    private void FixedUpdate()
    {
        ApplyMovement(Time.fixedDeltaTime);
    }

    private void ApplyMovement(float fdt)
    {
        var current = entityRigidbody.linearVelocity;

        var input = _desiredDirection;
        var inputMag = Mathf.Clamp01(input.magnitude);
        var dir = (inputMag > 0f) ? (input / input.magnitude) : Vector2.zero;

        var target = dir * (entityBehavior.maxVelocity * inputMag);

        var rate = ((target.sqrMagnitude > 0f) ? entityBehavior.accel : entityBehavior.decel) * fdt;
        entityRigidbody.linearVelocity = Vector2.MoveTowards(current, target, rate);
    }

    public void Stop()
    {
        _desiredDirection = Vector2.zero;
        entityRigidbody.linearVelocity = Vector2.zero;
        entityRigidbody.angularVelocity = 0f;
    }
}