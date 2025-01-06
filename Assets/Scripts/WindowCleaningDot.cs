using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class WindowCleaningDot : MonoBehaviour {
    public TextMeshProUGUI numClickedText;
    public Image dotImage;
    public int numClicked = 0;
    public int numClicksToComplete = 5;
    public float timeToFade = 0.5f;
    private bool _isComplete = false;

    void Start(){
        numClickedText.text = "";
    }

    void OnMouseOver()
    {
        if (GameManager.Instance.isWindowCleaningTaskOn){
            dotImage.color = Color.green;
            if (Input.GetMouseButtonDown(0) && !_isComplete)
            {
                if (numClicked < numClicksToComplete){
                    numClicked++;
                    numClickedText.text = numClicked.ToString();
                    GameManager.Instance.sfxParent.Find("SprayBottle").GetComponent<AudioSource>().Play();
                }
                if (numClicked == numClicksToComplete){
                    _isComplete = true;
                    GameManager.Instance.numDotsInWindowComplete++;
                    Invoke("Fade", 0.2f);
                }
            }
        }
    }

    void Fade(){
        dotImage.DOFade(0, timeToFade);
        numClickedText.DOFade(0, timeToFade).OnComplete(() => Destroy(this.gameObject));
    }

    void OnMouseExit()
    {
        dotImage.color = Color.white;
    }
}

