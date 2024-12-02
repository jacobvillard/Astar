using UnityEngine;
using static Interfaces.Interfaces;

/// <summary>
/// Enemy class that can take damage
/// </summary>
public class Enemy : MonoBehaviour, IDamageable {
    
    /// <summary>
    /// Health of the agent
    /// </summary>
    public float health;
    
    /// <summary>
    /// Reduces the health of the agent
    /// </summary>
    /// <param name="amount"></param>
    public void TakeDamage(float amount) {
        health =- amount;
        if (health < 0) Die();
    }

    /// <summary>
    /// Destroys the agent
    /// </summary>
    protected virtual void Die() {
        //Destroy the agent
        Destroy(gameObject);
    }
}
