using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject infoPanel;
    public static bool isHardMode = false;

    public void OnInfoClicked()
    {
        if (infoPanel != null)
            infoPanel.SetActive(true);
    }

    public void OnCloseInfoClicked()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
        Debug.Log("Game quit.");
    }

    public void OnEasyModeClicked()
    {
        MainMenu.isHardMode = false;
        Debug.Log("Starting game in Easy Mode.");
        SceneManager.LoadScene("DeckBuilder");
    }

    public void OnHardModeClicked()
    {
        MainMenu.isHardMode = true;
        Debug.Log("Starting game in Hard Mode.");
        SceneManager.LoadScene("DeckBuilder");
    }
}