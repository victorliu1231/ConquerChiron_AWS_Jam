using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Holdable : Interactable {
    public override void Interact() {
        transform.position = GameManager.Instance.holdObjectTransform.position; 
        transform.SetParent(GameManager.Instance.holdObjectTransform);
        GameManager.Instance.isHoldingObject = true;
    }
}