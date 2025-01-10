using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AIMonitor : Interactable {
    public List<TextMeshProUGUI> typingTexts;

    public override void Start()
    {
        base.Start();
        foreach (TextMeshProUGUI text in typingTexts) {
            text.enabled = false;
        }
    }

    public override void Interact() {
        base.Interact();
        GameManager.Instance.awsConnection.inputField.enabled = true;
        canInteract = false;
        GameManager.Instance.aiBlink.PauseBlinking();
        foreach (TextMeshProUGUI text in typingTexts) {
            text.enabled = true;
        }
        GameManager.Instance.CameraStaticMode();
        GameManager.Instance.MoveCamera(GameManager.Instance.aiViewTransform, GameManager.Instance.asteroidCameraTransitionTime, MoveCameraMode.CameraStaticMode);
    }

    public override void SetText()
    {
        GameManager.Instance.interactText.text = "Talk";
    }

    public override void Update() {
        if (!GameManager.Instance.aiBlink.isBlinking && Input.GetKeyDown(KeyCode.Q)) {
            canInteract = true;
            foreach (TextMeshProUGUI text in typingTexts) {
                text.enabled = false;
            }
            if (GameManager.Instance.awsConnection.inputField.isFocused) {
                // Remove Q / q from input field
                GameManager.Instance.awsConnection.inputField.text = GameManager.Instance.awsConnection.inputField.text.Substring(0, GameManager.Instance.awsConnection.inputField.text.Length - 1);
            }
            GameManager.Instance.aiBlink.ResumeBlinking();
            GameManager.Instance.awsConnection.inputField.enabled = false;
            GameManager.Instance.MoveCamera(GameManager.Instance.player.transform.Find("PlayerCameraRoot"), GameManager.Instance.asteroidCameraTransitionTime, MoveCameraMode.CameraFreeMode);
        }
    }
}