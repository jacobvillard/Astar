using Game_Management;
using UnityEngine;

public class Agent : Enemy {
    /// <summary>
    /// Boid prefab to instantiate when agent dies
    /// </summary>
    public GameObject boidPrefab; 
    /// <summary>
    /// number of boids to instantiate when agent dies
    /// </summary>
    public int numberOfBoids = 10;
    
    /// <summary>
    /// Overridden die method
    /// </summary>
    protected override void Die() {
        for (var i = 0; i < numberOfBoids; i++)Instantiate(boidPrefab, transform.position, Random.rotation);
        base.Die();
        CheckForRemainingAgents();
        
    }
    
    /// <summary>
    /// Check if there is any agents left, starts next round if not
    /// </summary>
    private static void CheckForRemainingAgents() {
        var remainingAgents = FindObjectsOfType<Agent>();
        if (remainingAgents.Length <= 1) Singleton<GameManager>.Instance.StartNextRound();
    }
        

}
