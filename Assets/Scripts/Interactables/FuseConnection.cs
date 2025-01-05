using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XEntity.InventoryItemSystem;
using DG.Tweening;

public class FuseConnection : Interactable {
    public Interactable meltedFuseInteractable;

    public override void Start() {
        base.Start();
        meltedFuseInteractable.canInteract = false;
        meltedFuseInteractable.GetComponent<Collider>().enabled = false;
    }

    public override void Interact() {
        base.Interact();
        if (ItemManager.Instance.equippedItems.Contains(ItemManager.Instance.GetItemByName("Wrench"))){
            canInteract = false;
            meltedFuseInteractable.canInteract = true;
            meltedFuseInteractable.GetComponent<Collider>().enabled = true;
            // If have wrench in inventory, make hand motion to unscrew fuse            


        } else {
            // Shake replaceableGO in GameManager
            GameManager.Instance.replaceableGO.transform.DOShakePosition(0.5f, new Vector3(20f, 0f, 0f), 10, 0, false, true);
        }
    }

    public override void SetText()
    {
        if (ItemManager.Instance.equippedItems.Contains(ItemManager.Instance.GetItemByName("Wrench"))){
            GameManager.Instance.interactText.text = "Unscrew Fuse";
        } else {
            GameManager.Instance.interactText.text = "Must Equip Wrench";
        }
    }
}