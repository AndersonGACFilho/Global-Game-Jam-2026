using UnityEngine;

[RequireComponent(typeof(MovementBehavior))]
[RequireComponent(typeof(EntityBehavior))]
public class LifeComponent : MonoBehaviour
{
    [Header("Life Data")]
    public int maxHealth = 100;
    private int _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= _currentHealth <= 0 ? 0 : amount;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{GetComponent<EntityBehavior>().entityName} has died.");

        if (this.GetComponent<PlayerInputHandler>() != null)
        {
            Debug.Log("Player has died. Implement respawn or game over logic here.");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
}
