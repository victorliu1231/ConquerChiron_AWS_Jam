using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cinemachine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    #region Variables
    public static GameManager Instance;
    public GameObject pauseGO;
    public GameObject settingsGO;
    public Checkpoint currentCheckpoint;
    public List<string> tasksCompleted;
    public bool isGamePaused;
    [Header("Interact")]
    public GameObject interactGO;
    public float interactDistance = 5f;
    public Transform holdObjectTransform;
    public bool isHoldingObject = false;
    public TextMeshProUGUI interactText;
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
    #endregion

    #region Awake
    void Awake(){
        Instance = this;
        tasksCompleted = new List<string>();
        timerGO.SetActive(false);
        shipHealthSlider.gameObject.SetActive(false);
        diedScreen.SetActive(false);
    }
    #endregion

    #region Update
    void Update(){
        if (_timerOn) {
            timer += Time.deltaTime;
            timerForegroundImage.fillAmount = timeToCompleteTasks - timer > 0 ? (timeToCompleteTasks - timer) / timeToCompleteTasks : 0f;
            if (timer >= timeToCompleteTasks){
                GoToCheckpoint();
            } 
            HandleAsteroidsTask();
        } else {
            timer = 0f;
        }
        HandleInput();
        HandleInteract();
        HandlePlacing();
    }
    #endregion

    #region Handle Functions
    public void HandleAsteroidsTask(){
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
    }

    public void HandleInput(){
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
            if (settingsGO.activeSelf){
                settingsGO.SetActive(false);
            } else {
                if (pauseGO.activeSelf){
                    pauseGO.SetActive(false);
                    ResumeGame();
                } else {
                    pauseGO.SetActive(true);
                    PauseGame();
                }
            }
        }
    }

    public void HandleInteract(){
        // Raycast from player position in direction of mouse to screen and see if it hits any objects with the layer "Interactable"
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactDistance, LayerMask.GetMask("Interactable"))){
            // Ugh raycast hits the lamp first...
            Interactable interactable = hit.collider.gameObject.GetComponent<Interactable>();
            Holdable holdable = hit.collider.gameObject.GetComponent<Holdable>();
            if (holdable != null && isHoldingObject) {
                interactGO.SetActive(false);
                return; // Cannot pick up another object while holding one
            }
            if (interactable != null){
                interactGO.SetActive(true);
                interactable.SetText();
                if (Input.GetKeyDown(KeyCode.E)){
                    interactable.Interact();
                    interactGO.SetActive(false);
                }
            } else {
                interactGO.SetActive(false);
            }
        } else {
            interactGO.SetActive(false);
        }
    }

    public void HandlePlacing(){
        if (Input.GetMouseButtonDown(0)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, interactDistance, LayerMask.GetMask("PlaceableZone"))){
                PlaceableZone placeableZone = hit.collider.gameObject.GetComponent<PlaceableZone>();
                Transform objectInHand = holdObjectTransform.GetChild(0);
                objectInHand.position = placeableZone.position;
                objectInHand.rotation = Quaternion.Euler(placeableZone.rotation);
                objectInHand.localScale *= 2;
                objectInHand.SetParent(placeableZone.transform);
                objectInHand.GetComponent<Interactable>().enabled = true;
                placeableZone.GetComponent<Renderer>().enabled = false;
                isHoldingObject = false;

                Lamp lamp = objectInHand.GetComponent<Lamp>();
                if (lamp != null) lamp.lampPanel.enabled = false;
            }
        }
    }
    #endregion

    #region Misc Functions
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
    #endregion

    #region Asteroid Functions
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
    public void TurnOnSecondStageAsteroids(){
        asteroidGenerator.numAsteroidsToGenerate = (int)(asteroidGenerator.numAsteroidsToGenerate*2f);
        _isSecondAsteroidStageOn = true;
        secondAsteroidStageText.gameObject.SetActive(true);
        secondAsteroidStageText.DOFade(0f, 1f).SetLoops(7, LoopType.Yoyo).OnComplete(() => {
            secondAsteroidStageText.gameObject.SetActive(false);
        });
    }

    public void CameraShake(){
        Camera.main.DOShakePosition(0.5f, 0.5f);
    }
    #endregion

    #region Pause Menu
    public void PauseGame(){
        Time.timeScale = 0f;
        isGamePaused = true;
        // Add stuff later
    }

    public void ResumeGame(){
        Time.timeScale = 1f;
        isGamePaused = false;
        // Add stuff later
    }

    public void SaveToMenu(){
        PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
        PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
        PlayerPrefs.SetFloat("PlayerZ", player.transform.position.z);
        PlayerPrefs.SetFloat("PlayerRotX", player.transform.rotation.x);
        PlayerPrefs.SetFloat("PlayerRotY", player.transform.rotation.y);
        PlayerPrefs.SetFloat("PlayerRotZ", player.transform.rotation.z);
        PlayerPrefs.SetFloat("PlayerRotW", player.transform.rotation.w);
        SceneManager.LoadScene("MainMenu");
    }
    #endregion
}