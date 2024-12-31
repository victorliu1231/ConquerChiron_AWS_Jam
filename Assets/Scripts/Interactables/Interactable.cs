using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {
    public bool canInteract = true;
    
    public virtual void Interact() {}
    public virtual void SetText() {
        GameManager.Instance.interactText.text = "Interact";
    }
}