using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Drawer : Interactable {
    public bool isOpen = false;
    public Transform moveFromTransform;
    public Transform moveToTransform;

    public override void Interact() {
        base.Interact();
        if (isOpen) {
            isOpen = false;
            transform.DOMove(moveFromTransform.position, 0.5f);
        } else {
            isOpen = true;
            transform.DOMove(moveToTransform.position, 0.5f);
        }
    }

    public override void SetText() {
        if (isOpen) {
            GameManager.Instance.interactText.text = "Close";
        } else {
            GameManager.Instance.interactText.text = "Open";
        }
    }
}