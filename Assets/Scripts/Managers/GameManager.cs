using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public Checkpoint currentCheckpoint;
    public List<string> tasksCompleted;
    public Slider timerSlider;
    public float timer = 0f;
    public bool timerOn = false;
    public float timeToCompleteTasks = 60f; // in seconds
    public GameObject diedScreen;
    [Header("Asteroid Level")]
    public Transform asteroidsParent;
    public float asteroidSpeed = 0.5f;
    public float shipSpeed = 1f;
    public bool isAsteroidTaskOn;
    public float shipHealth = 100f;
    public float damagePerAsteroidHit = 20f;

    void Awake(){
        tasksCompleted = new List<string>();
        timerSlider.gameObject.SetActive(false);
        diedScreen.SetActive(false);
    }

    void Update(){
        if (timerOn) {
            timer += Time.deltaTime;
            timerSlider.value = (timeToCompleteTasks - timer) / timeToCompleteTasks;
            if (timer >= timeToCompleteTasks){
                GoToCheckpoint();
            }
        } else {
            timer = 0f;
        }

        if (isAsteroidTaskOn){
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
                asteroidsParent.position += Vector3.right * Time.deltaTime * shipSpeed;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
                asteroidsParent.position += Vector3.left * Time.deltaTime * shipSpeed;
            }
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
                asteroidsParent.position += Vector3.down * Time.deltaTime * shipSpeed;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
                asteroidsParent.position += Vector3.up * Time.deltaTime * shipSpeed;
            }
            asteroidsParent.position += Vector3.back * Time.deltaTime * asteroidSpeed;
        }
    }

    public void StartTimer(){
        timerOn = true;
        timerSlider.gameObject.SetActive(true);
    }

    // Restarts player at checkpoint
    public void GoToCheckpoint(){
        timerOn = false;
        diedScreen.SetActive(false);
    }

    public void ShipTakeDamage(){
        shipHealth -= damagePerAsteroidHit;
        if (shipHealth <= 0){
            GoToCheckpoint();
        }
    }
}