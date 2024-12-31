using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryInPanel : Interactable {
    public override void Interact() {
        base.Interact();
        Debug.Log("Battery in panel");
        // If have battery in inventory, replace battery in lamp, else shake camera
    }

    public override void SetText()
    {
        GameManager.Instance.replaceableText.text = "Replace Battery";
        // if no battery in inventory make text message that you need battery in hand
    }
}