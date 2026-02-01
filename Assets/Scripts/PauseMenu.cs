using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject container; 
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape key was pressed.");
            container.SetActive(true);
            Time.timeScale = 0f;    // Pause the game
        }
    }

    public void ResumeButton()
    {
        container.SetActive(false);
        Time.timeScale = 1f;    // Resume the game
    }

    public void MainMenuButton()
    {
        // TODO: navigate to starting screen
    }
}
