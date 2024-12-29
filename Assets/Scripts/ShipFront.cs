using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipFront : MonoBehaviour {
    void OnTriggerEnter(Collider collider){
        if (collider.gameObject.tag == "Asteroid"){
            GameManager.Instance.ShipTakeDamage();
            Destroy(collider.gameObject);
        }
    }
}