using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game_Management {
    /// <summary>
    /// GameManager class that manages the game
    /// </summary>
    public class GameManager : Singleton<GameManager> {
    
        /// <summary>
        /// Reference to the player gameobject
        /// </summary>
        public GameObject player;
        /// <summary>
        /// Reference to the grid creator gameobject
        /// </summary>
        public GameObject gridCreator;
        [SerializeField] private GameObject agentPrefab;
        [SerializeField] private GameObject Hud;
        [SerializeField] private Text hudText;
        [SerializeField] private GameObject GameOverCanvas;
        [SerializeField] private Transform spawnPointsParent; 
        private int currentRound;
    
        private void Awake() {
            StartNextRound();
            Time.timeScale = 1;
        }
    


        /// <summary>
        /// Start the next round
        /// </summary>
        public void StartNextRound() {
            currentRound++;
            Hud.SetActive(true); //Turn on the HUD
            hudText.text = "Round " + currentRound; //Change the text
            StartCoroutine(HideHudAfterDelay(2)); //Hide the HUD after 2 seconds
            SpawnAgents((int)Math.Pow(2, currentRound - 1));
        }
    
        /// <summary>
        /// Hides hud after x seconds
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator HideHudAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            Hud.SetActive(false);
        }
    
        /// <summary>
        /// Sets death overlay gameobject visibility to true
        /// </summary>
        public void Death() {
            GameOverCanvas.SetActive(true);
        }
    
        /// <summary>
        /// Spawns agents
        /// </summary>
        /// <param name="numberOfAgents"></param>
        private void SpawnAgents(int numberOfAgents) {
            for (var i = 0; i < numberOfAgents; i++) {
                var spawnIndex = Random.Range(0, spawnPointsParent.childCount);
                var spawnPoint = spawnPointsParent.GetChild(spawnIndex);
                Instantiate(agentPrefab, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }
}
