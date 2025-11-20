using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider bckgSlider;
    [SerializeField] private Slider soundFXSlider;

    private void Start()
    {
        float background = PlayerPrefs.GetFloat("Background", 1f);
        float music = PlayerPrefs.GetFloat("Music", 1f);
        float soundFX = PlayerPrefs.GetFloat("SoundFX", 1f);

        bckgSlider.value = background;
        musicSlider.value = music;
        soundFXSlider.value = soundFX;

        SetBackgroundVolume(background);
        SetMusicVolume(music);
        SetSoundFXVolume(soundFX);
    }

    public void SetBackgroundVolume(float volume)
    {
        audioMixer.SetFloat("Background", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("Background", volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("Music", volume);
    }

    public void SetSoundFXVolume(float volume)
    {
        audioMixer.SetFloat("SoundFX", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("SoundFX", volume);
    }
}
