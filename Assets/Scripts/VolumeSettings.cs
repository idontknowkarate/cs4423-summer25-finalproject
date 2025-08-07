using UnityEngine;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    public Slider masterSlider;
    public Slider sfxSlider;

    void Start()
    {
        // optionally load saved values
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMasterVolume(float value)
    {
        SFXManager.instance.SetMasterVolume(value);
    }

    public void SetSFXVolume(float value)
    {
        SFXManager.instance.SetSFXVolume(value);
    }
}
