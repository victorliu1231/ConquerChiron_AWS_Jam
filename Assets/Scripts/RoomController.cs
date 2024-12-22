using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour {
    public void ChangeLighting(float newBrightness) {
        RenderSettings.ambientLight = new Color(newBrightness, newBrightness, newBrightness, 1.0f);
    }
}