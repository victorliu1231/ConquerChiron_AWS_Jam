using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipFront : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D collider){
        if (collider.gameObject.tag == "Asteroid"){
            Destroy(collider.gameObject);
            GameManager.Instance.ShipTakeDamage();
        }
    }
}