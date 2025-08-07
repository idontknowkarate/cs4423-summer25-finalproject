using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public Animator fadeAnimator;
    public GameObject gameOverUI;
    public GameObject settingsUI;
    public float delayBeforeFade = 2f;
    public float delayAfterFade = 1.5f;
    public AudioClip buttonClickClip;
    public Transform sfxSpawnPoint; // Optional

    [Header("Medal System")]
    public Image medalImage;
    public Sprite bronzeMedal;
    public Sprite silverMedal;
    public Sprite goldMedal;
    public GameObject missionCompleteUI;

    public int bronzeScore = 4;
    public int silverScore = 8;
    public int goldScore = 12;

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

        fadeAnimator.SetTrigger("FadeOut");

        yield return new WaitForSeconds(delayAfterFade);

        Time.timeScale = 0f;
        SFXManager.instance.StopAllSFX();
        StopAllAudioInScene();

        missionCompleteUI?.SetActive(false);

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
        PlayButtonSFX();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        PlayButtonSFX();
        SceneManager.LoadScene("MainMenu");
    }

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

    public void PlayButtonSFX()
    {
        if (buttonClickClip != null && SFXManager.instance != null)
        {
            Transform spawnAt = sfxSpawnPoint != null ? sfxSpawnPoint : transform;
            SFXManager.instance.PlaySFXClip(buttonClickClip, spawnAt, 1f);
        }
    }

    public void OpenSettings()
    {
        Debug.Log("OpenSettings() called from Game Over");
        PlayButtonSFX();
        settingsUI.SetActive(true);
    }

    public void CloseSettings()
    {
        PlayButtonSFX();
        settingsUI.SetActive(false);
    }

    public void ShowMedalForScore()
    {
        if (medalImage == null) return;

        int finalScore = ScoreManager.Instance.GetScore();

        if (finalScore >= goldScore)
            medalImage.sprite = goldMedal;
        else if (finalScore >= silverScore)
            medalImage.sprite = silverMedal;
        else if (finalScore >= bronzeScore)
            medalImage.sprite = bronzeMedal;
        else
            medalImage.enabled = false; // hide if no medal earned
    }
}
