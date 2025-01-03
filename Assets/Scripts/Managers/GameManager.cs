using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cinemachine;
using TMPro;
using UnityEngine.SceneManagement;
using XEntity.InventoryItemSystem;

public class GameManager : MonoBehaviour {
    #region Variables
    public static GameManager Instance;
    public GameObject pauseGO;
    public GameObject settingsGO;
    public Checkpoint currentCheckpoint;
    public List<string> tasksCompleted;
    public bool isGamePaused;
    public TextMeshProUGUI inventoryFullText;
    public Animator playerAnimator;
    [Header("Interact")]
    public List<Item> equippedItems;
    public GameObject interactGO;
    public float interactDistance = 5f;
    public Transform holdObjectTransform;
    public bool isHoldingObject = false;
    public TextMeshProUGUI interactText;
    public GameObject replaceableGO;
    public TextMeshProUGUI replaceableText;
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
    private bool _isAsteroidTaskOn = false;
    [Header("Connect the Wires Task")]
    public GameObject connectTheWiresGO;
    [Header("Window Cleaning Task")]
    public GameObject cleaningDotPrefab;
    public List<Transform> windowCleaningCameraTransforms;
    public Transform dotsParent;
    public GameObject cleaningGOs;
    public Transform topLeftCleaningCanvas;
    public Transform bottomRightCleaningCanvas;
    public bool isWindowCleaningTaskOn = false;
    public int numDotsInWindowComplete = 0;
    public int numDotsPerWindowToComplete = 3;
    public int windowIndex = 0;
    #endregion

    #region Awake
    void Awake(){
        Instance = this;
        tasksCompleted = new List<string>();
        timerGO.SetActive(false);
        shipHealthSlider.gameObject.SetActive(false);
        diedScreen.SetActive(false);
        equippedItems = new List<Item>();
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
            HandleWindowCleaningTask();
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
        if (_isAsteroidTaskOn){
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

    public void HandleWindowCleaningTask(){
        if (isWindowCleaningTaskOn){
            if (windowIndex < windowCleaningCameraTransforms.Count){
                if (numDotsInWindowComplete == numDotsPerWindowToComplete){
                    numDotsInWindowComplete = 0;
                    windowIndex++;
                    MoveCamera(windowCleaningCameraTransforms[windowIndex], asteroidCameraTransitionTime, true);
                    Invoke("GenerateCleaningDots", asteroidCameraTransitionTime);
                }
            } else {
                TurnOffWindowCleaningTask();
            }
        }
    }

    public void HandleInput(){
        if (Input.GetKeyDown(KeyCode.Tab)){
            if (_isAsteroidTaskOn) TurnOffAsteroidTask(); else TurnOnAsteroidTask();
        }
        if (Input.GetKeyDown(KeyCode.P)){
            if (isWindowCleaningTaskOn) TurnOffWindowCleaningTask(); else TurnOnWindowCleaningTask();
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
        if (Input.GetKeyDown(KeyCode.Z)){
            playerAnimator.SetTrigger("Walking");
        }
        if (Input.GetKeyDown(KeyCode.X)){
            playerAnimator.SetTrigger("Running");
        }
        if (Input.GetKeyDown(KeyCode.C)){
            playerAnimator.SetTrigger("PullLever");
        }
    }

    public void HandleInteract(){
        // Raycast from player position in direction of mouse to screen and see if it hits any objects with the layer "Interactable"
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactDistance, LayerMask.GetMask("Interactable"))){
            Interactable interactable = hit.collider.gameObject.GetComponent<Interactable>();
            Holdable holdable = hit.collider.gameObject.GetComponent<Holdable>();
            if (holdable != null && isHoldingObject) {
                interactGO.SetActive(false);
                return; // Cannot pick up another object while holding one
            }
            if (interactable != null && interactable.canInteract){
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
        if (Physics.Raycast(ray, out hit, interactDistance, LayerMask.GetMask("Replaceable"))){
            Interactable interactable = hit.collider.gameObject.GetComponent<Interactable>();
            if (interactable != null && interactable.canInteract){
                replaceableGO.SetActive(true);
                interactable.SetText();
                if (Input.GetKeyDown(KeyCode.R)){
                    interactable.Interact();
                    replaceableGO.SetActive(false);
                }
            } else {
                replaceableGO.SetActive(false);
            }
        } else {
            replaceableGO.SetActive(false);
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
                objectInHand.GetComponent<Holdable>().canInteract = true;
                placeableZone.GetComponent<Renderer>().enabled = false;
                isHoldingObject = false;
            }
        }
    }
    #endregion

    #region Timer Functions
    [ContextMenu("Start Timer")]
    public void StartTimer(){
        _timerOn = true;
        timer = 0f;
        timerGO.SetActive(true);
    }

    // Restarts player at checkpoint
    [ContextMenu("Go to Checkpoint")]
    public void GoToCheckpoint(){
        Debug.Log("Going to checkpoint");   
        StopTimer();
    }

    public void TaskComplete(){
        Debug.Log("Task complete");
        // Play some sound effect
        StopTimer();
    }

    public void StopTimer(){
        _timerOn = false;
        timerGO.SetActive(false);
    }
    #endregion

    #region Inventory Functions
    public void InventoryFull(){
        inventoryFullText.DOFade(1, 0);
        inventoryFullText.enabled = true;
        inventoryFullText.DOFade(0, 2).OnComplete(() => inventoryFullText.enabled = false);
    }

    public void EquipItem(Item item){
        equippedItems.Add(item);
        GameObject equippedItem = Instantiate(item.prefab, holdObjectTransform, false);
        if (equippedItem.GetComponent<Equippable>() != null){
            equippedItem.transform.localPosition = equippedItem.GetComponent<Equippable>().equippedPosition;
        }
    }

    public void UnequipItem(Item item){
        equippedItems.Remove(item);
        // Find a way to find which child is the item and destroy it
        Destroy(holdObjectTransform.GetChild(0).gameObject);
    }

    [ContextMenu("Equip Crowbar")]
    public void EquipCrowbar(){
        ItemManager.Instance.inventory.AddItem(ItemManager.Instance.GetItemByName("Crowbar"));
        EquipItem(ItemManager.Instance.GetItemByName("Crowbar"));
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

    public void TurnOnAsteroidTask(){
        asteroidGenerator.GenerateAsteroids();
        _isAsteroidTaskOn = true;
        shipHealthSlider.gameObject.SetActive(true);
        CameraStaticMode();
        MoveCamera(cockpitViewTransform, asteroidCameraTransitionTime, true);
    }

    public void TurnOffAsteroidTask(){
        asteroidGenerator.ClearAsteroids();
        _isAsteroidTaskOn = false;
        shipHealthSlider.gameObject.SetActive(false);
        TaskComplete();
        MoveCamera(player.transform, asteroidCameraTransitionTime, false);
    }
    public void TurnOnSecondStageAsteroids(){
        asteroidGenerator.numAsteroidsToGenerate = (int)(asteroidGenerator.numAsteroidsToGenerate*2f);
        _isSecondAsteroidStageOn = true;
        secondAsteroidStageText.gameObject.SetActive(true);
        secondAsteroidStageText.DOFade(0f, 1f).SetLoops(7, LoopType.Yoyo).OnComplete(() => {
            secondAsteroidStageText.gameObject.SetActive(false);
        });
    }
    #endregion

    #region Window Cleaning Functions
    public void TurnOnWindowCleaningTask(){
        isWindowCleaningTaskOn = true;
        cleaningGOs.SetActive(true);
        CameraStaticMode();
        MoveCamera(windowCleaningCameraTransforms[0], asteroidCameraTransitionTime, true);
        Invoke("GenerateCleaningDots", asteroidCameraTransitionTime);
    }

    public void TurnOffWindowCleaningTask(){
        isWindowCleaningTaskOn = false;
        cleaningGOs.SetActive(false);
        TaskComplete();
        MoveCamera(player.transform, asteroidCameraTransitionTime, false);
    }

    public void GenerateCleaningDots(){
        Vector3 lastPosition = Vector3.zero;
        for (int i = 0; i < numDotsPerWindowToComplete; i++){
            Vector3 position = new Vector3(Random.Range(topLeftCleaningCanvas.position.x, bottomRightCleaningCanvas.position.x),
                                            Random.Range(topLeftCleaningCanvas.position.y, bottomRightCleaningCanvas.position.y),
                                            Random.Range(topLeftCleaningCanvas.position.z, bottomRightCleaningCanvas.position.z));
            if (i == 0) lastPosition = position;
            else if (Vector3.Distance(lastPosition, position) < 0.05f){
                i--;
                continue;
            } else {
                lastPosition = position;
            }
            Instantiate(cleaningDotPrefab, position, Quaternion.identity, dotsParent);
        }
    }
    #endregion

    #region Camera Functions
    public void CameraStaticMode(){
        StartTimer();
        player.SetActive(false);
        Camera.main.GetComponent<CinemachineBrain>().enabled = false;
    }

    public void MoveCamera(Transform moveToTransform, float transitionTime, bool cameraStaticMode){
        Camera.main.transform.DORotateQuaternion(moveToTransform.rotation, transitionTime);
        if (cameraStaticMode) Camera.main.transform.DOMove(moveToTransform.position, transitionTime);
        else Camera.main.transform.DOMove(moveToTransform.position, transitionTime).OnComplete(() => {
            Camera.main.GetComponent<CinemachineBrain>().enabled = true;
            player.SetActive(true);
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