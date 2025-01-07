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
        canInteract = false;
        GameManager.Instance.aiBlink.PauseBlinking();
        foreach (TextMeshProUGUI text in typingTexts) {
            text.enabled = true;
        }
        GameManager.Instance.CameraStaticMode();
        GameManager.Instance.MoveCamera(GameManager.Instance.aiViewTransform, GameManager.Instance.asteroidCameraTransitionTime, true);
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
            Debug.Log("AI Monitor Closed");
            GameManager.Instance.aiBlink.ResumeBlinking();
            GameManager.Instance.MoveCamera(GameManager.Instance.player.transform.Find("PlayerCameraRoot"), GameManager.Instance.asteroidCameraTransitionTime, false);
        }
    }
}