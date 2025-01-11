using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PurgeAirWindow : Interactable {
    public GameObject brokenWindow;
    public Interactable purgeAirButton;

    public override void Start(){
        base.Start();
        canInteract = false;
        brokenWindow.SetActive(false);
        purgeAirButton.canInteract = false;
        purgeAirButton.GetComponent<Collider>().enabled = false;
    }

    public override void Update(){
        base.Update();
    }

    public override void Interact() {
        base.Interact();
        if (ItemManager.Instance.equippedItems.Contains(ItemManager.Instance.GetItemByName("Crowbar"))){
            GameManager.Instance.CameraStaticMode();
            GameManager.Instance.transform.DOScale(1f, 0f).SetDelay(GameManager.Instance.asteroidCameraTransitionTime).OnComplete(() => {
                GameManager.Instance.holdObjectTransform.Find("CrowbarEquippable").gameObject.SetActive(false);
            });
            GameManager.Instance.MoveCamera(GameManager.Instance.pressPurgeButtonTransform, GameManager.Instance.asteroidCameraTransitionTime, MoveCameraMode.CameraStaticAndAnimOn, 0, GameManager.Instance.swingCrowbar, 0.5f, "CrowbarSmash");
            Invoke("GlassSmash", GameManager.Instance.asteroidCameraTransitionTime + 0.5f + 0.467f); // 0.5s for waiting till player hand moves, 0.467s for animation to play
            Invoke("SetCrowbarSmashObjectFalse", GameManager.Instance.asteroidCameraTransitionTime + 0.5f + 0.467f + 0.25f); // 0.5s for waiting till player hand moves, 0.467s for animation to play + 0.25s to transition out
        } else {
            // Shake replaceableGO in GameManager
            GameManager.Instance.replaceableGO.transform.DOShakePosition(0.5f, new Vector3(20f, 0f, 0f), 10, 0, false, true);
        }
    }

    public override void SetText()
    {
        if (GameManager.Instance.assignedTasks.Contains(Task.PurgeAir)){
            Debug.Log("PurgeAirWindow SetText");
            if (ItemManager.Instance.equippedItems.Contains(ItemManager.Instance.GetItemByName("Crowbar"))){
                GameManager.Instance.interactKeyGO.SetActive(true);
                GameManager.Instance.interactText.text = "Smash Window";
            } else {
                GameManager.Instance.interactKeyGO.SetActive(false);
                GameManager.Instance.interactText.text = "Must Equip Crowbar";
            }
        }
    }

    void GlassSmash(){
        canInteract = false;
        brokenWindow.SetActive(true);
        purgeAirButton.canInteract = true;
        GetComponent<MeshRenderer>().enabled = false;
        purgeAirButton.GetComponent<Collider>().enabled = true;
        purgeAirButton.canInteract = true;
        GameManager.Instance.sfxParent.Find("GlassSmash").GetComponent<AudioSource>().Play();
    }

    void SetCrowbarSmashObjectFalse(){
        GameManager.Instance.swingCrowbar.SetActive(false);
    }
}