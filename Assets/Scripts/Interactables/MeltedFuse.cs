using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XEntity.InventoryItemSystem;
using DG.Tweening;

public class MeltedFuse : Interactable {
    public Item meltedFuse;
    public GameObject workingFuse;

    public override void Interact() {
        base.Interact();
        if (ItemManager.Instance.inventory.ContainsItem(ItemManager.Instance.GetItemByName("Fuse"))){
            ItemManager.Instance.UseItem(ItemManager.Instance.GetItemSlot(ItemManager.Instance.GetItemByName("Fuse")));
            ItemManager.Instance.inventory.AddItem(meltedFuse);
            Instantiate(workingFuse, transform.position, transform.rotation, transform.parent);
            // Hand motion to replace fuse

            Destroy(gameObject);
        } else {
            // Shake replaceableGO in GameManager
            GameManager.Instance.replaceableGO.transform.DOShakePosition(0.5f, new Vector3(20f, 0f, 0f), 10, 0, false, true);
        }
    }

    public override void SetText()
    {
        if (ItemManager.Instance.inventory.ContainsItem(ItemManager.Instance.GetItemByName("Fuse"))){
            GameManager.Instance.replaceableText.text = "Replace Fuse";
        } else {
            GameManager.Instance.replaceableText.text = "Missing New Fuse";
        }
    }
}