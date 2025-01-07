using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fusebox : Interactable {
    public bool isOpen = false;
    public Interactable meltedFuse;

    public override void Start() {
        base.Start();
        meltedFuse.canInteract = false;
    }

    public override void Interact() {
        base.Interact();
        if (isOpen){
            GetComponent<Animator>().Play("Close");
            isOpen = false;
            meltedFuse.canInteract = false;
        } else {
            GetComponent<Animator>().Play("Open");
            isOpen = true;
            meltedFuse.canInteract = true;
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