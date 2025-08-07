using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public TMP_Dropdown resolutionDropdown;
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle fullscreenToggle;
    public GameObject confirmPopup;
    public AudioClip buttonClickClip;

    private Resolution[] resolutions;
    private float originalMasterVol;
    private float originalSFXVol;
    private int originalResIndex;
    private bool originalFullscreen;

    void Start()
    {
        // ------- RESOLUTION SETUP -------
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        HashSet<string> seenResolutions = new HashSet<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;

            if (!seenResolutions.Contains(option))
            {
                seenResolutions.Add(option);
                options.Add(option);
            }

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = options.Count - 1;
            }
        }

        resolutionDropdown.AddOptions(options);

        int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        resolutionDropdown.value = savedResIndex;
        resolutionDropdown.RefreshShownValue();
        SetResolution(savedResIndex);

        // ------- VOLUME SETUP -------
        float defaultVolume = 0.5f;

        float savedMaster = PlayerPrefs.GetFloat("MasterVolume", defaultVolume);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", defaultVolume);

        audioMixer.SetFloat("MasterVolume", Mathf.Log10(savedMaster) * 20);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(savedSFX) * 20);

        if (masterVolumeSlider) masterVolumeSlider.value = savedMaster;
        if (sfxVolumeSlider) sfxVolumeSlider.value = savedSFX;

        // ------- FULLSCREEN SETUP -------
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = savedFullscreen;
        if (fullscreenToggle) fullscreenToggle.isOn = savedFullscreen;

        originalMasterVol = masterVolumeSlider.value;
        originalSFXVol = sfxVolumeSlider.value;
        originalResIndex = resolutionDropdown.value;
        originalFullscreen = fullscreenToggle.isOn;
    }

    bool SettingsHaveChanged()
    {
        return
            masterVolumeSlider.value != originalMasterVol ||
            sfxVolumeSlider.value != originalSFXVol ||
            resolutionDropdown.value != originalResIndex ||
            fullscreenToggle.isOn != originalFullscreen;
    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void ResetToDefault()
    {
        PlayButtonSFX();

        PlayerPrefs.SetFloat("MasterVolume", 0.5f);
        PlayerPrefs.SetFloat("SFXVolume", 0.5f);
        PlayerPrefs.SetInt("ResolutionIndex", 0);
        PlayerPrefs.SetInt("Fullscreen", 1);

        SetMasterVolume(0.5f);
        SetSFXVolume(0.5f);
        SetResolution(0);
        SetFullscreen(true);

        if (masterVolumeSlider) masterVolumeSlider.value = 0.5f;
        if (sfxVolumeSlider) sfxVolumeSlider.value = 0.5f;
        if (resolutionDropdown) resolutionDropdown.value = 0;
        if (fullscreenToggle) fullscreenToggle.isOn = true;
    }

    public void ShowConfirmPopup()
    {
        if (SettingsHaveChanged())
        {
            confirmPopup.SetActive(true);
        }
        else
        {
            CloseSettingsPanel();
        }
    }

    public void ConfirmSettings()
    {
        // apply all settings
        SetMasterVolume(masterVolumeSlider.value);
        SetSFXVolume(sfxVolumeSlider.value);
        SetResolution(resolutionDropdown.value);
        SetFullscreen(fullscreenToggle.isOn);

        // save new values as "original" to prevent unnecessary prompts next time
        originalMasterVol = masterVolumeSlider.value;
        originalSFXVol = sfxVolumeSlider.value;
        originalResIndex = resolutionDropdown.value;
        originalFullscreen = fullscreenToggle.isOn;

        PlayerPrefs.SetFloat("MasterVolume", originalMasterVol);
        PlayerPrefs.SetFloat("SFXVolume", originalSFXVol);
        PlayerPrefs.SetInt("ResolutionIndex", originalResIndex);
        PlayerPrefs.SetInt("Fullscreen", originalFullscreen ? 1 : 0);

        // hide popup and close settings
        confirmPopup.SetActive(false);
        CloseSettingsPanel();
    }

    public void CancelConfirm()
    {
        confirmPopup.SetActive(false);
    }

    public GameObject settingsPanel;
    public GameObject mainPanel;

    void CloseSettingsPanel()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void PlayButtonSFX()
    {
        if (buttonClickClip != null && SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXClip(buttonClickClip, transform, 1f);
        }
    }
}
