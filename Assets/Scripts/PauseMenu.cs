using UnityEngine;
using UnityEngine.SceneManagement; 

public class PauseMenu : MonoBehaviour
{
    public GameObject container; 
    public static bool isPaused = false;
    public int escapePressed = 0;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape key was pressed.");
            isPaused = !isPaused;
            container.SetActive(isPaused);
            Time.timeScale = isPaused ? 0f : 1f;
            escapePressed += 1; 
        }
    }

    public void PauseButton()
    {
        container.SetActive(true);
        Time.timeScale = 0f;    
        isPaused = true; 
    }

    public void ResumeButton()
    {
        container.SetActive(false);
        Time.timeScale = 1f;    
        isPaused = false;
    }

    public void MainMenuButton()
    {
        isPaused = false; 
        Time.timeScale = 1f;

        if (container != null)
        {
            container.SetActive(false); 
        }

        SceneManager.LoadScene("TitleScreen"); 
    }
}
