using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour
{
    [SerializeField] GameObject soundOn;
    [SerializeField] GameObject soundOff;
    [SerializeField] Slider volumeSliderSFX;
    [SerializeField] Slider volumeSliderMusic;
    [SerializeField] AudioMixer audioMixer;
    private bool muted = false;

    void Start()
    {
        if (!PlayerPrefs.HasKey("muted"))
        {
            PlayerPrefs.SetInt("muted", 0);
            Load();
        }
        else
        {
            Load();
        }
        UpdateButtonIcon();
        AudioListener.pause = muted;

        if (!PlayerPrefs.HasKey("sfxVolume"))
        {
            PlayerPrefs.SetFloat("sfxVolume", 0);
            LoadSFXVolume();
        }

        else
        {
            LoadSFXVolume();
        }

        if (!PlayerPrefs.HasKey("musicVolume"))
        {
            PlayerPrefs.SetFloat("musicVolume", 0);
            LoadMusicVolume();
        }

        else
        {
            LoadMusicVolume();
        }
    }

    public void ChangeVolumeSFX()
    {
        audioMixer.SetFloat("sfxVolume", volumeSliderSFX.value * 5);
        SaveSFXVolume();
    }

    public void LoadSFXVolume()
    {
        volumeSliderSFX.value = PlayerPrefs.GetFloat("sfxVolume");
    }

    public void AddSFX()
    {
        volumeSliderSFX.value += 1;
        ChangeVolumeSFX();
    }

    public void SubtractSFX()
    {
        volumeSliderSFX.value -= 1;
        ChangeVolumeSFX();
    }

    public void SaveSFXVolume()
    {
        PlayerPrefs.SetFloat("sfxVolume", volumeSliderSFX.value);
    }

    public void ChangeVolumeMusic()
    {
        audioMixer.SetFloat("musicVolume", volumeSliderMusic.value * 5);
        SaveMusicVolume();
    }

    public void LoadMusicVolume()
    {
        volumeSliderMusic.value = PlayerPrefs.GetFloat("musicVolume");
    }

    public void AddMusic()
    {
        volumeSliderMusic.value += 1;
        ChangeVolumeSFX();
    }

    public void SubtractMusic()
    {
        volumeSliderMusic.value -= 1;
        ChangeVolumeSFX();
    }

    public void SaveMusicVolume()
    {
        PlayerPrefs.SetFloat("musicVolume", volumeSliderMusic.value);
    }


    public void SoundOnOff()
    {
        if (!muted)
        {
            muted = true;
            AudioListener.pause = true;
        }
        else
        {
            muted = false;
            AudioListener.pause = false;
        }

        Save();
        UpdateButtonIcon();
    }

    private void UpdateButtonIcon()
    {
        if (!muted)
        {
            soundOn.SetActive(true);
            soundOff.SetActive(false);
        }
        else
        {
            soundOn.SetActive(false);
            soundOff.SetActive(true);
        }
    }

    private void Load()
    {
        muted = PlayerPrefs.GetInt("muted") == 1;
    }

    private void Save()
    {
        PlayerPrefs.SetInt("muted", muted ? 1 : 0);
    }
}
