using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;

namespace MEET_AND_TALK {
public class CutsceneUtil : MonoBehaviour
{
    public static CutsceneUtil Instance;
    public DialogueContainerSO dialogueContainerSO;
    public GameObject speedUpGO;
    public TextMeshProUGUI speedUpText;
    public float speedMultiplier = 2f;
    public bool isSpedUp = false;

    private float _originalTypingSpeed;

    private void Awake(){
        Instance = this;
    }

    void Start(){
        speedUpGO.SetActive(false);
        _originalTypingSpeed = DialogueUIManager.Instance.typingSpeed;
        speedUpText.text = $"{speedMultiplier}x Speed";
        if (dialogueContainerSO != null) DialogueManager.Instance.StartDialogue(dialogueContainerSO);
    }   

    void Update(){
        if (Input.GetMouseButton(0)){
            DialogueUIManager.Instance.typingSpeed = _originalTypingSpeed * speedMultiplier;
            speedUpText.enabled = true;
            isSpedUp = true;
            speedUpGO.SetActive(true);
        } else {
            DialogueUIManager.Instance.typingSpeed = _originalTypingSpeed;
            speedUpText.enabled = false;
            isSpedUp = false;
            speedUpGO.SetActive(false);
        }
    }

    public void CameraBackgroundColorFadeOut(float duration){
        Camera.main.DOColor(new Color(1f, 1f, 1f, 0f), duration);
    }
}
}