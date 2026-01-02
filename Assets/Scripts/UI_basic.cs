using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_basic : MonoBehaviour
{
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main menu");
    }
    
}