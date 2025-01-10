using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BatteryInPanel : Interactable {
    public Item usedBattery;

    public override void Interact() {
        base.Interact();
        if (ItemManager.Instance.inventory.ContainsItem(ItemManager.Instance.GetItemByName("Battery"))){
            ItemManager.Instance.UseItem(ItemManager.Instance.GetItemSlot(ItemManager.Instance.GetItemByName("Battery")));
            ItemManager.Instance.inventory.AddItem(usedBattery);
            canInteract = false;
            
            // 0.5s delay for waiting for animation to play, then 0.5s delay for waiting for animation to finish
            GameManager.Instance.transform.DOScale(1f, 0f).SetDelay(GameManager.Instance.asteroidCameraTransitionTime + 0.5f + 0.5f).OnComplete(() => {
                GameManager.Instance.sfxParent.Find("MetalClick").GetComponent<AudioSource>().Play();
                GameManager.Instance.TaskComplete(Task.ReplaceNightLampBattery);
            });
            GameManager.Instance.CameraStaticMode();
            GameManager.Instance.MoveCamera(GameManager.Instance.replaceBatteryTransform, GameManager.Instance.asteroidCameraTransitionTime, MoveCameraMode.CameraStaticAndAnimOn, 0, GameManager.Instance.genericHandInteract, 0.5f, "GenericInteract");

            // 0.5s delay for waiting for animation to play, then 0.5s delay for waiting for animation to finish, then another 0.5s delay to simulate waiting
            GameManager.Instance.transform.DOScale(1f, 0f).SetDelay(GameManager.Instance.asteroidCameraTransitionTime + 0.5f + 0.5f + 0.5f).OnComplete(() => {
                GameManager.Instance.MoveCamera(GameManager.Instance.player.transform.Find("PlayerCameraRoot").transform, GameManager.Instance.asteroidCameraTransitionTime, MoveCameraMode.CameraFreeMode);
                GameManager.Instance.genericHandInteract.SetActive(false);
            });
        } else {
            // Shake replaceableGO in GameManager
            GameManager.Instance.replaceableGO.transform.DOShakePosition(0.5f, new Vector3(20f, 0f, 0f), 10, 0, false, true);
        }
    }

    public override void SetText()
    {
        if (ItemManager.Instance.inventory.ContainsItem(ItemManager.Instance.GetItemByName("Battery"))){
            GameManager.Instance.replaceableKeyGO.SetActive(true);
            GameManager.Instance.replaceableText.text = "Replace Battery";
        } else {
            GameManager.Instance.replaceableKeyGO.SetActive(false);
            GameManager.Instance.replaceableText.text = "Missing New Battery";
        }
    }
}