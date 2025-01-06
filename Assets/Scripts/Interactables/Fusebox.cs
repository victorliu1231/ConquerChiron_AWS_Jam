using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fusebox : Interactable {
    public bool isOpen = false;

    public override void Interact() {
        base.Interact();
        if (isOpen){
            GetComponent<Animator>().Play("Close");
            isOpen = false;
        } else {
            GetComponent<Animator>().Play("Open");
            isOpen = true;
        }
    }

    public override void SetText()
    {
        if (isOpen)
        {
            GameManager.Instance.interactText.text = "Close Fusebox";
        }
        else
        {
            GameManager.Instance.interactText.text = "Open Fusebox";
        }
    }
}