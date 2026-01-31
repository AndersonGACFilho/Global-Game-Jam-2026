using System;
using UnityEngine;

[DisallowMultipleComponent]
public class EntityBehavior : MonoBehaviour
{
    [Header("Identification")]
    [SerializeField] private string entityId;
    public string EntityId => entityId;
    
    [Header("Metadata")]
    public string entityName;
    
    [Header("Components")]
    public Collider2D entityWorldCollider;
    public Collider2D entityInteractionCollider;
    
    
    [Header("Movement Stats")]
    public float maxVelocity = 5f;
    public float accel = 33f;
    public float decel = 45f;

    private void Start()
    {
        GenerateIdIfIsEmpty();
    }
    
    private void Awake()
    {
        entityWorldCollider = transform.Find("WorldCollider")?.GetComponent<Collider2D>();
        entityInteractionCollider = transform.Find("InteractionCollider")?.GetComponent<Collider2D>();
        
        
        if (entityWorldCollider != null && entityInteractionCollider != null) return;

        var message = "";
        if(entityWorldCollider == null) message += "WorldCollider";
        if(entityWorldCollider == null && entityInteractionCollider == null) message += " and ";
        if(entityInteractionCollider == null) message += "InteractionCollider";
        
        message = $"EntityBehavior: {message} not found on entity {entityName} (ID: {entityId}). " +
                  $"Make sure to add the missing collider as a a child GameObject.";
        
        Debug.LogError(message, this);
        throw new Exception(message);
    }
    
    #if UNITY_EDITOR
        private void OnValidate()
        {
            GenerateIdIfIsEmpty();
        }
    #endif
    
    private void GenerateIdIfIsEmpty()
    {
        if (string.IsNullOrEmpty(entityId))
        {
            entityId = Guid.NewGuid().ToString();
        }
    }

    public void SetTargetRotation(float angle)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}