using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class AI_Blink : MonoBehaviour
{
    TextMeshProUGUI ai_text;
    Color originalColor;
    public Color aiQueryColor;
    public bool isBlinking = false;

    void Start()
    {
        ai_text = GetComponent<TextMeshProUGUI>();
        originalColor = ai_text.color;
        ResumeBlinking();
    }

    public void PauseBlinking(){
        isBlinking = false;
        DOTween.Kill(ai_text);
        ai_text.color = aiQueryColor;
    }

    public void ResumeBlinking(){
        isBlinking = true;
        ai_text.DOFade(0.0f, 1.5f).SetLoops(-1, LoopType.Yoyo);
        ai_text.color = originalColor;
    }
}
