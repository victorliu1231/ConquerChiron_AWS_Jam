using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MeltedFuse : Interactable {
    public Item meltedFuse;
    public GameObject workingFuse;
    public bool canBeReplaced = false;

    void OnEnable(){
        transform.localPosition = new Vector3(0.975f, -2.522f, 0.677f);
    }

    public override void Interact() {
        if (GameManager.Instance.assignedTasks.Contains(Task.ReplaceFuse)){
            base.Interact();
            if (!canBeReplaced){
                if (ItemManager.Instance.equippedItems.Contains(ItemManager.Instance.GetItemByName("Wrench"))){
                    canBeReplaced = true;
                    GameManager.Instance.sfxParent.Find("Unscrew").GetComponent<AudioSource>().Play();
                    GameManager.Instance.CameraStaticMode();
                    GameManager.Instance.MoveCamera(GameManager.Instance.fuseboxViewTransform, GameManager.Instance.asteroidCameraTransitionTime, MoveCameraMode.CameraStaticAndAnimOn, 0f, GameManager.Instance.wrenchUnscrew, 0.5f, "WrenchPivot");
                    GameManager.Instance.transform.DOScale(1f, 0f).SetDelay(GameManager.Instance.asteroidCameraTransitionTime + 1f).OnComplete(() => {
                        GameManager.Instance.wrenchUnscrew.SetActive(false);
                        GameManager.Instance.MoveCamera(GameManager.Instance.player.transform.Find("PlayerCameraRoot").transform, GameManager.Instance.asteroidCameraTransitionTime, MoveCameraMode.CameraFreeMode);
                    });
                    transform.localPosition += new Vector3(0f, -2.575f + 2.522f, 0f);
                } else {
                    // Shake replaceableGO in GameManager
                    GameManager.Instance.replaceableGO.transform.DOShakePosition(0.5f, new Vector3(20f, 0f, 0f), 10, 0, false, true);
                }
            } else {
                if (ItemManager.Instance.inventory.ContainsItem(ItemManager.Instance.GetItemByName("Fuse"))){
                    ItemManager.Instance.UseItem(ItemManager.Instance.GetItemSlot(ItemManager.Instance.GetItemByName("Fuse")));
                    ItemManager.Instance.inventory.AddItem(meltedFuse);
                    
                    workingFuse.SetActive(true);
                    GameManager.Instance.sfxParent.Find("MetalClick").GetComponent<AudioSource>().Play();
                    GameManager.Instance.TaskComplete(Task.ReplaceFuse);
                    // Hand motion to replace fuse

                    gameObject.SetActive(false);
                } else {
                    // Shake replaceableGO in GameManager
                    GameManager.Instance.replaceableGO.transform.DOShakePosition(0.5f, new Vector3(20f, 0f, 0f), 10, 0, false, true);
                }
            }
        }
    }

    public override void SetText()
    {
        if (GameManager.Instance.assignedTasks.Contains(Task.ReplaceFuse)){
            if (!canBeReplaced){
                if (ItemManager.Instance.equippedItems.Contains(ItemManager.Instance.GetItemByName("Wrench"))){
                    GameManager.Instance.replaceableKeyGO.SetActive(true);
                    GameManager.Instance.replaceableText.text = "Unscrew Fuse";
                } else {
                    GameManager.Instance.replaceableKeyGO.SetActive(false);
                    GameManager.Instance.replaceableText.text = "Must Equip Wrench";
                }
            } else {
                if (ItemManager.Instance.inventory.ContainsItem(ItemManager.Instance.GetItemByName("Fuse"))){
                    GameManager.Instance.replaceableKeyGO.SetActive(true);
                    GameManager.Instance.replaceableText.text = "Replace Fuse";
                } else {
                    GameManager.Instance.replaceableKeyGO.SetActive(false);
                    GameManager.Instance.replaceableText.text = "Missing New Fuse";
                }
            }
        }
    }
}