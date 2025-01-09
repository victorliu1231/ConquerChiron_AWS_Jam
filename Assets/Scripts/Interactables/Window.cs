using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XEntity.InventoryItemSystem;
using DG.Tweening;

public class Window : Interactable {
    public override void Start(){
        base.Start();
        canInteract = false;
    }

    public override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Tab)){
            if (GameManager.Instance.windowCleaningTutorial.activeSelf){
                GameManager.Instance.windowCleaningTutorial.SetActive(false);
                GameManager.Instance.notepad.SetActive(false);
                GameManager.Instance.TurnOnWindowCleaningTask();
            }
            if (GameManager.Instance.asteroidsTutorial.activeSelf){
                GameManager.Instance.asteroidsTutorial.SetActive(false);
                GameManager.Instance.notepad.SetActive(false);
                Time.timeScale = 1f;
                GameManager.Instance.CameraStaticMode();
                GameManager.Instance.MoveCamera(GameManager.Instance.cockpitViewTransform, GameManager.Instance.asteroidCameraTransitionTime, true);
            }
        }
    }

    public override void Interact(){
        canInteract = false;
        if (GameManager.Instance.horrorMode){
            GameManager.Instance.notepad.SetActive(true);
            GameManager.Instance.asteroidsTutorial.SetActive(true);
            GameManager.Instance.CameraStaticMode();
            Time.timeScale = 0f;
        } else {
            if (ItemManager.Instance.equippedItems.Contains(ItemManager.Instance.GetItemByName("Spindex")) && ItemManager.Instance.equippedItems.Contains(ItemManager.Instance.GetItemByName("Towel"))){
                GameManager.Instance.notepad.SetActive(true);
                GameManager.Instance.windowCleaningTutorial.SetActive(true);
                GameManager.Instance.CameraStaticMode();
            } else {
                GameManager.Instance.replaceableGO.transform.DOShakePosition(0.5f, new Vector3(20f, 0f, 0f), 10, 0, false, true);
            }
        }
    }

    public override void SetText(){
        if (GameManager.Instance.horrorMode){
            GameManager.Instance.interactKeyGO.SetActive(true);
            GameManager.Instance.interactText.text = "Pilot Ship";
        } else {
            if (ItemManager.Instance.equippedItems.Contains(ItemManager.Instance.GetItemByName("Spindex")) && ItemManager.Instance.equippedItems.Contains(ItemManager.Instance.GetItemByName("Towel"))){
                GameManager.Instance.interactKeyGO.SetActive(true);
                GameManager.Instance.interactText.text = "Start Cleaning";
            } else {
                GameManager.Instance.interactKeyGO.SetActive(false);
                GameManager.Instance.interactText.text = "Must Equip Spindex and Towel";
            }
        }
    }
}