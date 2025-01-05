using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance;
    public AudioMixer soundtrackMixer;
    public AudioMixer sfxMixer;

    void Awake(){
        Instance = this;
    }

    public void SetSoundtrackLevel (float level) {
        // Convert level from 0 to 1 to decibels
        Debug.Log(level);
        if (level == 0) soundtrackMixer.SetFloat("Volume", -80);
        else {
            level = Mathf.Log10(level) * 20;
            soundtrackMixer.SetFloat("Volume", level);
        }
    }

    public void SetSFXLevel (float level) {
        // Convert level from 0 to 1 to decibels
        if (level == 0) sfxMixer.SetFloat("Volume", -80);
        else {
            level = Mathf.Log10(level) * 20;
            sfxMixer.SetFloat("Volume", level);
        }
    }

    public float GetSoundtrackLevel (out float level) {
        soundtrackMixer.GetFloat("Volume", out level);
        if (level == 0) return 0;
        // Convert decibels to level from 0 to 1
        level = Mathf.Pow(10, level / 20);
        return level;
    }

    public float GetSFXLevel (out float level) {
        sfxMixer.GetFloat("Volume", out level);
        if (level == 0) return 0;
        // Convert decibels to level from 0 to 1
        level = Mathf.Pow(10, level / 20);
        return level;
    }
}