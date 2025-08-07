using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public AudioClip buttonClickClip;
    public Transform sfxSpawnPoint; // Optional

    public string gameSceneName = "LevelOne"; // Replace with your real scene name if different

    public void PlayGame()
    {
        PlayButtonSFX();
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        PlayButtonSFX();
        Application.Quit();
    }

    public void OpenSettings()
    {
        PlayButtonSFX();
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        PlayButtonSFX();
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    void PlayButtonSFX()
    {
        if (buttonClickClip != null && SFXManager.instance != null)
        {
            Transform spawnAt = sfxSpawnPoint != null ? sfxSpawnPoint : transform;
            SFXManager.instance.PlaySFXClip(buttonClickClip, spawnAt, 1f);
        }
    }
}
