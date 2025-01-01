using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampPanel : Interactable {
    public GameObject lampPanelOpen;
    public GameObject lampPanelClosed;
    public Interactable batteryInPanel;

    public override void Start(){
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
            GameManager.Instance.interactText.text = "Close";
        } else {
            GameManager.Instance.interactText.text = "Open";
        }
    }
}