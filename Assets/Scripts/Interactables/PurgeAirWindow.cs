using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PurgeAirWindow : Interactable {
    public GameObject brokenWindow;
    public Interactable purgeAirButton;

    public override void Start(){
        base.Start();
        brokenWindow.SetActive(false);
        purgeAirButton.canInteract = false;
        purgeAirButton.GetComponent<Collider>().enabled = false;
    }

    public override void Update(){
        base.Update();
        if (ItemManager.Instance.inventory.ContainsItem(ItemManager.Instance.GetItemByName("Crowbar"))){
            canInteract = true;
        } else {
            canInteract = false;
        }
    }

    public override void Interact() {
        base.Interact();
        if (ItemManager.Instance.equippedItems.Contains(ItemManager.Instance.GetItemByName("Crowbar"))){
            canInteract = false;
            brokenWindow.SetActive(true);
            purgeAirButton.canInteract = true;
            purgeAirButton.GetComponent<Collider>().enabled = true;
            this.gameObject.SetActive(false);
            // If have crowbar in inventory, make hand motion to smash window            
            GameManager.Instance.sfxParent.Find("GlassSmash").GetComponent<AudioSource>().Play();

        } else {
            // Shake replaceableGO in GameManager
            GameManager.Instance.replaceableGO.transform.DOShakePosition(0.5f, new Vector3(20f, 0f, 0f), 10, 0, false, true);
        }
    }

    public override void SetText()
    {
        if (ItemManager.Instance.equippedItems.Contains(ItemManager.Instance.GetItemByName("Crowbar"))){
            GameManager.Instance.interactKeyGO.SetActive(true);
            GameManager.Instance.interactText.text = "Smash Window";
        } else {
            GameManager.Instance.interactKeyGO.SetActive(false);
            GameManager.Instance.interactText.text = "Must Equip Crowbar";
        }
    }
}