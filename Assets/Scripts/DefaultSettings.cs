using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultSettings", menuName = "DefaultSettings")]
public class DefaultSettings : ScriptableObject {
    public float soundtrackVolume = 0.5f;
    public float sfxVolume = 0.5f;
    public float mouseSensitivity = 0.75f;
    public float aiVoiceVolume = 0.5f;
    public bool fullScreen = true;
}