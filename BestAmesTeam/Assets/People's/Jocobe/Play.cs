using UnityEngine;
using UnityEngine.SceneManagement;

public class Play : MonoBehaviour
{

    public void PlayGame()
    {
        int totalScenes = SceneManager.sceneCountInBuildSettings;
        if (totalScenes > 0)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            Debug.LogError("PlayGame: No scenes are configured in Build Settings.");
        }
    }
}
