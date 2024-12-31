using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Holdable : Interactable {
    public Vector3 rotationInHand;
    
    public override void Interact() {
        base.Interact();
        transform.position = GameManager.Instance.holdObjectTransform.position; 
        transform.rotation = Quaternion.Euler(rotationInHand);
        transform.SetParent(GameManager.Instance.holdObjectTransform);
        transform.localScale /= 2;
        GameManager.Instance.isHoldingObject = true;
        this.enabled = false;
    }

    public override void SetText()
    {
        GameManager.Instance.interactText.text = "Pick Up";
    }
}