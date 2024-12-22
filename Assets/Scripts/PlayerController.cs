using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float jumpStrength = 10f;

    public void MovePosition(Vector3 newPosition){

    }

    public void Jump(){
        GetComponent<Rigidbody>().AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
    }

    public void Interact(){}

    public void GrabItem(InventoryItem inventoryItem){
        InventoryManager.Instance.AddItem(inventoryItem);
    }

    public void PlaceItem(){}
}