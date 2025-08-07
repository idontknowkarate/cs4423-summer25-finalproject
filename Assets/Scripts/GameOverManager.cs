using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public Animator fadeAnimator;
    public GameObject gameOverUI;
    public float delayBeforeFade = 2f;
    public float delayAfterFade = 1.5f;

    private bool gameHasEnded = false;

    public void TriggerGameOver()
    {
        if (gameHasEnded) return;
        gameHasEnded = true;
        StartCoroutine(HandleGameOverSequence());
    }

    private IEnumerator HandleGameOverSequence()
    {
        yield return new WaitForSeconds(delayBeforeFade);

        // start fade animation
        fadeAnimator.SetTrigger("FadeOut");

        yield return new WaitForSeconds(delayAfterFade);

        Time.timeScale = 0f; // freeze the game
        SFXManager.instance.StopAllSFX();
        StopAllAudioInScene();
        gameOverUI.SetActive(true);
    }

    void StopAllAudioInScene()
    {
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (AudioSource src in allAudioSources)
        {
            src.Stop();
        }
    }

    public void Retry()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public GameObject missionCompleteUI; // assign a "Mission Complete" screen

    public void TriggerMissionComplete()
    {
        if (gameHasEnded) return;
        gameHasEnded = true;
        StartCoroutine(HandleMissionCompleteSequence());
    }

    private IEnumerator HandleMissionCompleteSequence()
    {
        yield return new WaitForSeconds(delayBeforeFade);

        fadeAnimator.SetTrigger("FadeOut");

        yield return new WaitForSeconds(delayAfterFade);

        Time.timeScale = 0f;
        SFXManager.instance.StopAllSFX();
        StopAllAudioInScene();
        missionCompleteUI.SetActive(true);
    }

    public void OpenSettings()
    {
        Debug.Log("Settings button clicked!");
        // TODO: Show settings UI    
    }
}
