using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PurgeAirAlarm : Interactable {
    public override void Start() {
        base.Start();
        canInteract = false;
    }

    public override void Interact() {
        if (GameManager.Instance.assignedTasks.Contains(Task.PurgeAir)) {
            base.Interact();
            canInteract = false;
            //GameManager.Instance.CameraStaticMode();
            GameManager.Instance.MoveCamera(GameManager.Instance.pressPurgeButtonTransform, 0, MoveCameraMode.CameraStaticAndAnimOn, 0, GameManager.Instance.handPress, 0f, "ButtonPress");
            Invoke("DelayedOne", 0.25f); // weird bug where have to finagle the timing to get the sound to play at right time
            Invoke("DelayedTwo", GameManager.Instance.asteroidCameraTransitionTime + 0.625f + 0.25f); // 0.5s for waiting till player hand moves, 0.625s for animation to play, 0.25s to simulate latency between button press and air purge
            Invoke("PanBack", GameManager.Instance.asteroidCameraTransitionTime + 0.625f + 0.25f + 0.25f); // 0.5s for waiting till player hand moves, 0.625s for animation to play, 0.25s to simulate latency between button press and air purge, 0.25s for transition back to player
        }
    }

    public override void SetText()
    {
        if (GameManager.Instance.assignedTasks.Contains(Task.PurgeAir)) {
            GameManager.Instance.interactText.text = "Press Button";
        }
    }

    void PanBack(){
        GameManager.Instance.handPress.SetActive(false);
        GameManager.Instance.MoveCamera(GameManager.Instance.player.transform.Find("PlayerCameraRoot").transform, GameManager.Instance.asteroidCameraTransitionTime, MoveCameraMode.CameraFreeMode, 0.5f);
        GameManager.Instance.transform.DOScale(1f, 0f).SetDelay(GameManager.Instance.asteroidCameraTransitionTime + 0.5f).OnComplete(() => GameManager.Instance.holdObjectTransform.Find("CrowbarEquippable").gameObject.SetActive(true));
    }

    void DelayedOne(){
        GameManager.Instance.sfxParent.Find("MetalClick").GetComponent<AudioSource>().Play();
    }

    void DelayedTwo(){
        GameManager.Instance.sfxParent.Find("AirPurge").GetComponent<AudioSource>().Play();
        GameManager.Instance.TaskComplete(Task.PurgeAir);
    }
}