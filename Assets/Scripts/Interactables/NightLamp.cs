using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XEntity.InventoryItemSystem;

public class NightLamp : Interactable {
    public GameObject lampPanelOpen;
    public GameObject lampPanelClosed;
    public Interactable batteryInPanel;

    public override void Start() {
        canInteract = false;
        batteryInPanel.canInteract = false;
    }

    public override void Interact() {
        base.Interact();
        if (lampPanelOpen.activeSelf){
            lampPanelOpen.SetActive(false);
            lampPanelClosed.SetActive(true);
            batteryInPanel.canInteract = false;
        } else {
            lampPanelOpen.SetActive(true);
            lampPanelClosed.SetActive(false);
            batteryInPanel.canInteract = true;
        }
    }

    public override void SetText()
    {
        if (lampPanelOpen.activeSelf){
            GameManager.Instance.interactText.text = "Close Battery Box";
        } else {
            GameManager.Instance.interactText.text = "Open Battery Box";
        }
    }
}