using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XEntity.InventoryItemSystem;

public class PurgeAirAlarm : Interactable {
    public override void Interact() {
        base.Interact();
        canInteract = false;
        // Hand motion to press button
        // Play alarm sound
        // Clean out air in spaceship
        Debug.Log("Purging air in spaceship");
    }

    public override void SetText()
    {
        GameManager.Instance.interactText.text = "Press Button";
    }
}