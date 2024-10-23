using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Reloads the current scene
/// </summary>
public class ReloadScene : MonoBehaviour {
    private string currentSceneName;
    
    public void ReloadCurrentScene() {
        Time.timeScale = 1;
        currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
    
}
