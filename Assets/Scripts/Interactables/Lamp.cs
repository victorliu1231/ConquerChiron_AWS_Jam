using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : Holdable {
    public Interactable lampPanel;

    void Start() {
        lampPanel.enabled = false;
    }

    public override void Interact() {
        base.Interact();
        lampPanel.enabled = true;
    }
}