using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XEntity.InventoryItemSystem;

public class PurgeAirAlarm : Interactable {
    public override void Start() {
        base.Start();
        canInteract = false;
    }

    public override void Interact() {
        base.Interact();
        canInteract = false;
        GameManager.Instance.sfxParent.Find("AirPurge").GetComponent<AudioSource>().Play();
        // Hand motion to press button
        GameManager.Instance.TaskComplete(Task.PurgeAir);
        // AI should mock player for pressing button
        // would be cool to have text dialogue show up at this point saying "Phew... that was close"
        Debug.Log("Purging air in spaceship");
    }

    public override void SetText()
    {
        GameManager.Instance.interactText.text = "Press Button";
    }
}