using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressureGauge : Interactable {
    public override void Start(){
        base.Start();
        canInteract = false;
    }

    public override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Tab) && GameManager.Instance.pressureGaugeTutorial.activeSelf){
            GameManager.Instance.pressureGaugeTutorial.SetActive(false);
            GameManager.Instance.notepad.SetActive(false);
            GameManager.Instance.TurnOnPressureGaugeTask();
        }
    }

    public override void Interact(){
        canInteract = false;
        GameManager.Instance.notepad.SetActive(true);
        GameManager.Instance.pressureGaugeTutorial.SetActive(true);
        GameManager.Instance.CameraStaticMode();
    }

    public override void SetText(){
        GameManager.Instance.interactKeyGO.SetActive(true);
        GameManager.Instance.interactText.text = "Calibrate";
    }
}