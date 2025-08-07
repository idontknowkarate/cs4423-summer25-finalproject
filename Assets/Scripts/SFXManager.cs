using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    [SerializeField] private AudioSource sFXObject;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    private List<AudioSource> activeAudioSources = new List<AudioSource>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(sFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.outputAudioMixerGroup = sfxMixerGroup;
        audioSource.Play();

        activeAudioSources.Add(audioSource);
        Destroy(audioSource.gameObject, audioClip.length);
    }

    public void StopAllSFX()
    {
        foreach (AudioSource src in activeAudioSources)
        {
            if (src != null)
            {
                src.Stop();
                Destroy(src.gameObject);
            }
        }

        activeAudioSources.Clear();
    }

    // adjust exposed mixer params
    public void SetMasterVolume(float sliderValue)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
    }

    public void SetSFXVolume(float sliderValue)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(sliderValue) * 20);
    }
}
