using System.Collections;
using System.Collections.Generic;
using Meta.Voice.Net.PubSub;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public bool fullScreen;
    public Slider soundtrackSlider;
    public Slider sfxSlider;
    public Slider mouseSensitivitySlider;
    public Toggle fullScreenToggle;
    public float maxMouseSensitivity = 11f;
    public float minMouseSensitivity = 1f;
    public DefaultSettings defaultSettings;
    
    void Awake(){
        Invoke("LoadDefaultSettings", 0f);
    }

    void LoadDefaultSettings(){
        LoadScreen(defaultSettings.fullScreen);
        LoadSliders(defaultSettings.soundtrackVolume, defaultSettings.sfxVolume, defaultSettings.mouseSensitivity);
        SetMouseSensitivity(defaultSettings.mouseSensitivity);
        AudioManager.Instance.SetSoundtrackLevel(defaultSettings.soundtrackVolume);
        AudioManager.Instance.SetSFXLevel(defaultSettings.sfxVolume);
        gameObject.SetActive(false);
    }

    void OnEnable(){
        fullScreenToggle.isOn = fullScreen;
        soundtrackSlider.value = AudioManager.Instance.GetSoundtrackLevel(out float soundtrackVolume);
        sfxSlider.value = AudioManager.Instance.GetSFXLevel(out float sfxVolume);
        mouseSensitivitySlider.value = GameManager.Instance.player.GetComponentInChildren<FirstPersonController>().RotationSpeed / maxMouseSensitivity;
    }

    public void LoadScreen(bool settingsFullScreen){
        if (settingsFullScreen){
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            fullScreen = true;
        } else {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            fullScreen = false;
        }
    }

    public void LoadSliders(float volumeBGM, float volumeSFX, float mouseSensitivity){
        soundtrackSlider.value = volumeBGM;
        sfxSlider.value = volumeSFX;
        mouseSensitivitySlider.value = mouseSensitivity;
    }

    public void SetFullScreen(){
        if( !fullScreen ) {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            fullScreen = true;
            fullScreenToggle.isOn = true;
        }
        else {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            fullScreen = false;
            fullScreenToggle.isOn = false;
        }
    }

    public void SetMouseSensitivity(float value){
        GameManager.Instance.player.GetComponentInChildren<FirstPersonController>().RotationSpeed = value * (maxMouseSensitivity - minMouseSensitivity) + minMouseSensitivity;
    }
}
