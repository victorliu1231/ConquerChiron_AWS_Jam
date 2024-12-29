using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cinemachine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public Checkpoint currentCheckpoint;
    public List<string> tasksCompleted;
    [Header("Timer")]
    public Image timerForegroundImage;
    public GameObject timerGO;
    public float timer = 0f;
    public bool timerOn = false;
    public float timeToCompleteTasks = 60f; // in seconds
    public float timeStartPulsing = 15f; // in seconds
    public GameObject diedScreen;
    [Header("Asteroid Level")]
    public Transform asteroidsParent;
    public Slider shipHealthSlider;
    public float asteroidSpeed = 0.5f;
    public float shipSpeed = 1f;
    public bool isAsteroidTaskOn;
    public float shipHealth = 100f;
    public float damagePerAsteroidHit = 20f;
    public GameObject player;
    public Transform cockpitViewTransform;
    public float asteroidCameraTransitionTime = 1f;
    public AsteroidGenerator asteroidGenerator;

    void Awake(){
        Instance = this;
        tasksCompleted = new List<string>();
        timerGO.SetActive(false);
        shipHealthSlider.gameObject.SetActive(false);
        diedScreen.SetActive(false);
    }

    void Update(){
        if (timerOn) {
            timer += Time.deltaTime;
            timerForegroundImage.fillAmount = timeToCompleteTasks - timer > 0 ? (timeToCompleteTasks - timer) / timeToCompleteTasks : 0f;
            if (timer >= timeToCompleteTasks){
                GoToCheckpoint();
            } else if (timer >= timeToCompleteTasks - timeStartPulsing){
                timerForegroundImage.color = Color.red;
                // wtf this shit is not looping
                timerGO.transform.DOScale(1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            } else {
                timerForegroundImage.color = Color.blue;
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
                asteroidsParent.position += Vector3.up * Time.deltaTime * shipSpeed;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
                asteroidsParent.position += Vector3.down * Time.deltaTime * shipSpeed;
            }
            asteroidsParent.position += Vector3.forward * Time.deltaTime * asteroidSpeed;
        }

        if (Input.GetKeyDown(KeyCode.Tab)){
            TurnOnAsteroidTask();
        }
    }

    [ContextMenu("Start Timer")]
    public void StartTimer(){
        timerOn = true;
        timerGO.SetActive(true);
    }

    // Restarts player at checkpoint
    [ContextMenu("Go to Checkpoint")]
    public void GoToCheckpoint(){
        timerOn = false;
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
}