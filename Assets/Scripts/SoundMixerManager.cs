using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    public void SetBackgroundVolume(float volume)
    {
        audioMixer.SetFloat("Background", Mathf.Log10(volume) * 20f);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20f);
    }

    public void SetSoundFXVolume(float volume)
    {
        audioMixer.SetFloat("SoundFX", Mathf.Log10(volume) * 20f);
    }
}
