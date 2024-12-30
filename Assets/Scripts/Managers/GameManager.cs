using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cinemachine;
using TMPro;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public GameObject settingsGO;
    public Checkpoint currentCheckpoint;
    public List<string> tasksCompleted;
    [Header("Timer")]
    public Image timerForegroundImage;
    public GameObject timerGO;
    public float timer = 0f;
    public float timeToCompleteTasks = 60f; // in seconds
    public float timeStartPulsing = 15f; // in seconds
    public GameObject diedScreen;
    private bool _timerOn = false;
    [Header("Asteroid Task")]
    public Transform asteroidsParent;
    public Slider shipHealthSlider;
    public float asteroidSpeed = 0.5f;
    public float shipSpeed = 1f;
    public bool isAsteroidTaskOn;
    public float shipHealth = 100f;
    public float damagePerAsteroidHit = 20f;
    public GameObject player;
    public Transform cockpitViewTransform;
    public Transform shipFront;
    public float asteroidCameraTransitionTime = 1f;
    public float asteroidGenerateEverySeconds = 1f;
    public float secondStageAsteroidsTime = 30f;
    public TextMeshProUGUI secondAsteroidStageText; 
    public AsteroidGenerator asteroidGenerator;
    private bool _isSecondAsteroidStageOn = false;
    private float _asteroidGenerateTimer = 0f; 
    [Header("Connect the Wires Task")]
    public GameObject connectTheWiresGO;

    void Awake(){
        Instance = this;
        tasksCompleted = new List<string>();
        timerGO.SetActive(false);
        shipHealthSlider.gameObject.SetActive(false);
        diedScreen.SetActive(false);
    }

    void Update(){
        if (_timerOn) {
            timer += Time.deltaTime;
            timerForegroundImage.fillAmount = timeToCompleteTasks - timer > 0 ? (timeToCompleteTasks - timer) / timeToCompleteTasks : 0f;
            if (timer >= timeToCompleteTasks){
                GoToCheckpoint();
            } 
            if (isAsteroidTaskOn){
                _asteroidGenerateTimer += Time.deltaTime;
                if (_asteroidGenerateTimer >= asteroidGenerateEverySeconds){
                    asteroidGenerator.GenerateAsteroids();
                    _asteroidGenerateTimer = 0f;
                }
                if (timer >= secondStageAsteroidsTime && !_isSecondAsteroidStageOn){
                    TurnOnSecondStageAsteroids();
                    timerForegroundImage.color = Color.red;
                    // wtf this shit is not looping
                    timerGO.transform.DOScale(1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                }
            } else {
                if (timer >= timeToCompleteTasks - timeStartPulsing && timerForegroundImage.color != Color.red){
                    timerForegroundImage.color = Color.red;
                    // wtf this shit is not looping
                    timerGO.transform.DOScale(1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                }
            } 
        } else {
            timer = 0f;
        }

        if (isAsteroidTaskOn){
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
                asteroidsParent.position += Vector3.left * Time.deltaTime * shipSpeed;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
                asteroidsParent.position += Vector3.right * Time.deltaTime * shipSpeed;
            }
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
                asteroidsParent.position += Vector3.down * Time.deltaTime * shipSpeed;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
                asteroidsParent.position += Vector3.up * Time.deltaTime * shipSpeed;
            }
            asteroidsParent.position += Vector3.forward * Time.deltaTime * asteroidSpeed;
        }

        if (Input.GetKeyDown(KeyCode.Tab)){
            if (isAsteroidTaskOn) TurnOffAsteroidTask(); else TurnOnAsteroidTask();
        }
        if (Input.GetKeyDown(KeyCode.Q)){
            if (connectTheWiresGO.activeSelf) {
                connectTheWiresGO.SetActive(false); 
            } else {
                StartTimer();
                connectTheWiresGO.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)){
            settingsGO.SetActive(true);
        }
    }

    public void TurnOnSecondStageAsteroids(){
        asteroidGenerator.numAsteroidsToGenerate = (int)(asteroidGenerator.numAsteroidsToGenerate*2f);
        _isSecondAsteroidStageOn = true;
        secondAsteroidStageText.gameObject.SetActive(true);
        secondAsteroidStageText.DOFade(0f, 1f).SetLoops(7, LoopType.Yoyo).OnComplete(() => {
            secondAsteroidStageText.gameObject.SetActive(false);
        });
    }

    [ContextMenu("Start Timer")]
    public void StartTimer(){
        _timerOn = true;
        timerGO.SetActive(true);
    }

    // Restarts player at checkpoint
    [ContextMenu("Go to Checkpoint")]
    public void GoToCheckpoint(){
        Debug.Log("Going to checkpoint");   
        _timerOn = false;
        diedScreen.SetActive(false);
    }

    public void ShipTakeDamage(){
        shipHealth -= damagePerAsteroidHit;
        Debug.Log("Ship Health: " + shipHealth);
        shipHealthSlider.value = shipHealth / 100f;
        if (shipHealth <= 0){
            GoToCheckpoint();
        }
    }

    [ContextMenu("Turn on Asteroid Task")]
    public void TurnOnAsteroidTask(){
        StartTimer();
        asteroidGenerator.GenerateAsteroids();
        isAsteroidTaskOn = true;
        shipHealthSlider.gameObject.SetActive(true);
        player.SetActive(false);
        Camera.main.GetComponent<CinemachineBrain>().enabled = false;
        Camera.main.transform.DORotateQuaternion(cockpitViewTransform.rotation, asteroidCameraTransitionTime);
        Camera.main.transform.DOMove(cockpitViewTransform.position, asteroidCameraTransitionTime);
    }

    [ContextMenu("Turn off Asteroid Task")]
    public void TurnOffAsteroidTask(){
        asteroidGenerator.ClearAsteroids();
        isAsteroidTaskOn = true;
        shipHealthSlider.gameObject.SetActive(true);
        player.SetActive(true);
        Camera.main.transform.DORotateQuaternion(player.transform.rotation, asteroidCameraTransitionTime);
        Camera.main.transform.DOMove(player.transform.position, asteroidCameraTransitionTime).OnComplete(() => {
            Camera.main.GetComponent<CinemachineBrain>().enabled = true;
        });
    }

    public void CameraShake(){
        Camera.main.DOShakePosition(0.5f, 0.5f);
    }
}