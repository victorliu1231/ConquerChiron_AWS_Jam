using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MeltedFuse : Interactable {
    public Item meltedFuse;
    public GameObject workingFuse;
    public bool canBeReplaced = false;

    public override void Interact() {
        base.Interact();
        if (!canBeReplaced){
            if (ItemManager.Instance.equippedItems.Contains(ItemManager.Instance.GetItemByName("Wrench"))){
                canBeReplaced = true;
                GameManager.Instance.sfxParent.Find("Unscrew").GetComponent<AudioSource>().Play();
                // Hand motion to unwrench fuse


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

    public override void SetText()
    {
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