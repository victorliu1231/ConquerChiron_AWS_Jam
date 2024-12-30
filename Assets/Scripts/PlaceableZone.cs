using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableZone : MonoBehaviour {
    public Vector3 rotation;
    public Vector3 position;
    
    void Start(){
        GetComponent<Renderer>().enabled = false;
    }

    void Update(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, GameManager.Instance.interactDistance, LayerMask.GetMask("PlaceableZone"))){
            if (hit.collider.gameObject == this.gameObject && GameManager.Instance.isHoldingObject){
                GetComponent<Renderer>().enabled = true;
            }
        } else {
            GetComponent<Renderer>().enabled = false;
        }
    }
}