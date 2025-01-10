using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurgeAirAlarm : Interactable {
    public override void Start() {
        base.Start();
        canInteract = false;
    }

    public override void Interact() {
        base.Interact();
        canInteract = false;
        //GameManager.Instance.CameraStaticMode();
        GameManager.Instance.MoveCamera(GameManager.Instance.pressPurgeButtonTransform, 0, MoveCameraMode.CameraStaticAndAnimOn, 0, GameManager.Instance.handPress, 0f, "ButtonPress");
        Invoke("DelayedOne", 0.25f); // weird bug where have to finagle the timing to get the sound to play at right time
        Invoke("DelayedTwo", GameManager.Instance.asteroidCameraTransitionTime + 0.625f + 0.25f); // 0.5s for waiting till player hand moves, 0.625s for animation to play, 0.25s to simulate latency between button press and air purge
        Invoke("PanBack", GameManager.Instance.asteroidCameraTransitionTime + 0.625f + 0.25f + 0.25f); // 0.5s for waiting till player hand moves, 0.625s for animation to play, 0.25s to simulate latency between button press and air purge, 0.25s for transition back to player
        // AI should mock player for pressing button
        // would be cool to have text dialogue show up at this point saying "Phew... that was close"
    }

    public override void SetText()
    {
        GameManager.Instance.interactText.text = "Press Button";
    }

    void PanBack(){
        GameManager.Instance.handPress.SetActive(false);
        GameManager.Instance.MoveCamera(GameManager.Instance.player.transform.Find("PlayerCameraRoot").transform, GameManager.Instance.asteroidCameraTransitionTime, MoveCameraMode.CameraFreeMode, 0.5f);
    }

    void DelayedOne(){
        GameManager.Instance.sfxParent.Find("MetalClick").GetComponent<AudioSource>().Play();
    }

    void DelayedTwo(){
        GameManager.Instance.sfxParent.Find("AirPurge").GetComponent<AudioSource>().Play();
        GameManager.Instance.TaskComplete(Task.PurgeAir);
    }
}