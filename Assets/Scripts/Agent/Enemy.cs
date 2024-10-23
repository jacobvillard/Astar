using UnityEngine;
using static Interfaces.Interfaces;

public class Enemy : MonoBehaviour, IDamageable {
    public float health;
    
    public void TakeDamage(float amount) {
        health =- amount;
        if (health < 0) Die();
    }

    protected virtual void Die() {
        //Destroy the agent
        Destroy(gameObject);
    }
}
