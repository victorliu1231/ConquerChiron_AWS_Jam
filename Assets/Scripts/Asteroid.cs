using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour {
    void Update() {
        float rotationMultiplier = 0.2f;
        // Rotate randomly
        transform.Rotate(new Vector3(Random.Range(0, 360)*rotationMultiplier, Random.Range(0, 360)*rotationMultiplier, Random.Range(0, 360)*rotationMultiplier) * Time.deltaTime);
        // Destroy object if past ship front
        if (transform.position.z > GameManager.Instance.shipFront.position.z){
            Destroy(gameObject);
        }
    }
}