using System;
using Game_Management;
using UnityEngine;

/// <summary>
/// Detects if the player is within the light
/// </summary>
public class PlayerDetection : MonoBehaviour {
    private GameObject player;
    private Light spotlight; // Reference to the spotlight component
    
    /// <summary>
    /// Boolean to check if player is in range of the spotlight
    /// </summary>
    public bool playerInRange;

    private void Awake() {
        spotlight = GetComponentInChildren<Light>();
        player = Singleton<GameManager>.Instance.player;
    }

    private void Update() {
        playerInRange = IsPlayerInSpotlight();
        if (playerInRange) EndGame();
    }
    
    /// <summary>
    /// End the game if player is in spotlight
    /// </summary>
    private static void EndGame() {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Singleton<GameManager>.Instance.Death();
    }

    /// <summary>
    /// Return if player is in spotlight
    /// </summary>
    /// <returns></returns>
    private bool IsPlayerInSpotlight()
    {
        var distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (!(distanceToPlayer <= spotlight.range)) return false;
        var dirToPlayer = (player.transform.position - transform.position).normalized;
        var angleBetweenSpotlightAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        return angleBetweenSpotlightAndPlayer <= spotlight.spotAngle / 2f && LineOfSightCheck(player.transform.position);
    }

    /// <summary>
    /// Ensure there is line of sight between agent and playter
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    private bool LineOfSightCheck(Vector3 targetPosition) {
        var directionToTarget = targetPosition - transform.position;
        if (!Physics.Raycast(transform.position, directionToTarget, out var hit)) return false;
        return hit.transform.gameObject == player;
    }
}