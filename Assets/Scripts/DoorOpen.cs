using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DoorOpen : MonoBehaviour {
    public Transform moveToTransform;
    public Transform moveBackTransform;
    public Transform door;

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            door.DOMove(moveToTransform.position, 0.75f);
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.tag == "Player") {
            door.DOMove(moveBackTransform.position, 0.75f);
        }
    }
}