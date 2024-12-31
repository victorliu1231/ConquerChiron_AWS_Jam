using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XEntity.InventoryItemSystem;
using DG.Tweening;

public class BatteryInPanel : Interactable {
    public Item usedBattery;

    public override void Interact() {
        base.Interact();
        if (ItemManager.Instance.inventory.ContainsItem(ItemManager.Instance.GetItemByName("Battery"))){
            ItemManager.Instance.UseItem(ItemManager.Instance.GetItemSlot(ItemManager.Instance.GetItemByName("Battery")));
            ItemManager.Instance.inventory.AddItem(usedBattery);
            canInteract = false;
            // If have battery in inventory, make hand motion to replace battery in lamp            


        } else {
            // Shake replaceableGO in GameManager
            GameManager.Instance.replaceableGO.transform.DOShakePosition(0.5f, new Vector3(20f, 0f, 0f), 10, 0, false, true);
        }
    }

    public override void SetText()
    {
        if (ItemManager.Instance.inventory.ContainsItem(ItemManager.Instance.GetItemByName("Battery"))){
            GameManager.Instance.replaceableText.text = "Replace Battery";
        } else {
            GameManager.Instance.replaceableText.text = "Missing Battery";
        }
    }
}