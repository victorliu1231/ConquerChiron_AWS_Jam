using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needle : MonoBehaviour {
    public float rotateSpeed = 1f;
    public float maxAngleFromVertical = 60f;
    public float angleOfVertical = -160f;

    void Update(){
        Debug.Log(transform.localRotation.eulerAngles);
        if (transform.localRotation.eulerAngles.y >= angleOfVertical - maxAngleFromVertical && transform.localRotation.eulerAngles.y <= angleOfVertical + maxAngleFromVertical){
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
                transform.Rotate(0,-rotateSpeed * Time.deltaTime,0);
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
                transform.Rotate(0,rotateSpeed * Time.deltaTime,0);
            }
        } else if (transform.localRotation.eulerAngles.y < angleOfVertical - maxAngleFromVertical){
            transform.localRotation = Quaternion.Euler(0,angleOfVertical - maxAngleFromVertical,0);
        } else if (transform.localRotation.eulerAngles.y > angleOfVertical + maxAngleFromVertical){
            transform.localRotation = Quaternion.Euler(0,angleOfVertical + maxAngleFromVertical,0);
        }
    }
}