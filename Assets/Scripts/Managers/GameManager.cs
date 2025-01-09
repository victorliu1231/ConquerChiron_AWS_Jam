using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cinemachine;
using TMPro;
using UnityEngine.SceneManagement;
using XEntity.InventoryItemSystem;
using StarterAssets;

public enum Task {
    Unpack,
    ReplaceNightLampBattery,
    RecalibratePressureGauge,
    CleanCockpitWindows,
    TalkToChiron,
    SurviveAsteroids,
    PurgeAir,
    ConnectWires,
    ReplaceFuse,
}

public class GameManager : MonoBehaviour {
    #region Variables
    public static GameManager Instance;
    public bool isDebugging = true;
    public GameObject pauseGO;
    public GameObject settingsGO;
    public Checkpoint currentCheckpoint;
    public bool isGamePaused;
    public TextMeshProUGUI inventoryFullText;
    public Animator playerAnimator;
    public bool horrorMode = false;
    public FirstPersonController playerController;
    private bool _justExitedTransitionPeriod = true;
    
    [Header("Sounds")]
    public Transform sfxParent;
    public Transform soundtrackParents;
    public float timeBetweenEerieNoises = 15f;
    private float _eerieNoiseTimer = 0f;
    private List<AudioSource> _footstepsSFX;
    [Header("Tasks")]
    public List<Task> tasksCompleted;
    public List<Task> tasksRemaining;
    public List<Task> assignedTasks;
    public List<string> playerPrompts;
    public TextMeshProUGUI tasksAssignedText;
    public Transform tasksPanelTransform;
    public Transform tasksPanelMoveToTransform;
    public bool allPeacefulTasksComplete = false;
    [Header("Tutorials")]
    public GameObject notepad;
    public GameObject windowCleaningTutorial;
    public GameObject pressureGaugeTutorial;
    public GameObject asteroidsTutorial;
    public GameObject connectWiresTutorial;
    [Header("AI")]
    public AI_Blink aiBlink;
    public Transform aiViewTransform;
    public AmazonBedrockConnection awsConnection;
    public float waitTimeBetweenPrompting = 5f; // in seconds
    public float timerBetweenPrompting = 0f;
    [Header("Interact")]
    public float interactDistance = 5f;
    public Transform holdObjectTransform;
    public bool isHoldingObject = false;
    public GameObject interactGO;
    public GameObject interactKeyGO;
    public TextMeshProUGUI interactText;
    public GameObject replaceableGO;
    public GameObject replaceableKeyGO;
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
    [Header("Pressure Gauge Task")]
    public Interactable pressureGauge;
    public Transform pressureGaugeViewTransform;
    public bool isPressureGaugeTaskOn;
    public Needle needle;
    [Header("Window Cleaning Task")]
    public Interactable cockpitWindow;
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
    [Header("Misc Tasks")]
    public Interactable nightLamp;
    public Interactable fusebox;
    public Interactable purgeAirWindow;
    #endregion

    #region Awake
    void Awake(){
        Instance = this;
        tasksCompleted = new List<Task>();
        tasksRemaining = new List<Task>{Task.CleanCockpitWindows, Task.RecalibratePressureGauge, Task.ReplaceNightLampBattery, Task.Unpack};
        assignedTasks = new List<Task>();
        playerPrompts = new List<string>();
        timerGO.SetActive(false);
        shipHealthSlider.gameObject.SetActive(false);
        diedScreen.SetActive(false);
        _footstepsSFX = new List<AudioSource>();
        tasksPanelTransform.localPosition = Vector3.zero;
        tasksPanelTransform.localScale = Vector3.zero;
        tasksAssignedText.text = "";
        foreach (Transform child in sfxParent){
            if (child.name.Contains("Step")){
                _footstepsSFX.Add(child.GetComponent<AudioSource>());
            }
        }
    }
    #endregion

    #region Update
    void Update(){
        timerBetweenPrompting += Time.deltaTime;

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

        if (horrorMode){
            if (aiBlink.isBlinking && _justExitedTransitionPeriod){
                _justExitedTransitionPeriod = false;
                Invoke("AssignHorrorTask", 10f);
            }

            _eerieNoiseTimer += Time.deltaTime;
            if (_eerieNoiseTimer >= timeBetweenEerieNoises){
                int random = Random.Range(0, 4);
                switch (random){
                    case 0:
                        sfxParent.Find("SpaceEcho1").GetComponent<AudioSource>().Play();
                        break;
                    case 1:
                        sfxParent.Find("SpaceEcho2").GetComponent<AudioSource>().Play();
                        break;
                    case 2:
                        sfxParent.Find("SpaceEcho3").GetComponent<AudioSource>().Play();
                        break;
                    case 3:
                        sfxParent.Find("SpaceEcho4").GetComponent<AudioSource>().Play();
                        break;
                }
                sfxParent.Find("HeavyBreathing").GetComponent<AudioSource>().PlayDelayed(5f);
                _eerieNoiseTimer = 0f;
            }
        }

        if (!playerController.Grounded){
            foreach (AudioSource audioSource in _footstepsSFX){
                audioSource.Stop();
            }
        }

        if (playerController.GetComponent<CharacterController>().velocity != Vector3.zero){
            if (horrorMode){
                sfxParent.Find("HeavyBreathing").GetComponent<AudioSource>().Play();
            }
            
            bool isFootstepsSFXPlaying = false;
            foreach (AudioSource audioSource in _footstepsSFX){
                if (audioSource.isPlaying) {
                    if (playerController._input.sprint){
                        audioSource.pitch = 1.5f;
                    } else {
                        audioSource.pitch = 1f;
                    }
                    isFootstepsSFXPlaying = true;
                }
            }

            if (!isFootstepsSFXPlaying){
                _footstepsSFX[Random.Range(0, 3)].Play();
            }
        }
    }
    #endregion

    #region Handle Functions
    public void HandleInput(){
        if (aiBlink.isBlinking){
            if (ItemManager.Instance.inventory.isUIInitialized) ItemManager.Instance.inventory.CheckForUIToggleInput();
            if (isDebugging){
                if (Input.GetKeyDown(KeyCode.O)){
                    if (horrorMode) TurnOffHorrorMode(); else TurnOnHorrorMode();
                }
                if (Input.GetKeyDown(KeyCode.K)){
                    if (connectTheWiresGO.activeSelf) {
                        connectTheWiresGO.SetActive(false); 
                    } else {
                        StartTimer();
                        connectTheWiresGO.SetActive(true);
                    }
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
    }
    
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
                timerGO.transform.DOScale(1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            }
            if (timer >= timeToCompleteTasks){
                if (shipHealth > 0) TaskComplete(Task.SurviveAsteroids);
                else GoToCheckpoint();
            }
        }
    }

    public void HandleWindowCleaningTask(){
        if (isWindowCleaningTaskOn){
            if (windowIndex < windowCleaningCameraTransforms.Count){
                if (numDotsInWindowComplete == numDotsPerWindowToComplete){
                    numDotsInWindowComplete = 0;
                    windowIndex++;
                    MoveCamera(windowCleaningCameraTransforms[windowIndex], asteroidCameraTransitionTime, true, asteroidCameraTransitionTime);
                    Invoke("GenerateCleaningDots", asteroidCameraTransitionTime*2);
                }
            } else {
                TaskComplete(Task.CleanCockpitWindows);
            }
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
                sfxParent.Find("ItemPickup").GetComponent<AudioSource>().Play();
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

    #region Horror Mode
    public void TurnOnHorrorMode(){
        horrorMode = true;
        soundtrackParents.Find("Peaceful_Soundtrack").GetComponent<AudioSource>().DOFade(0f, 2f).OnComplete(() => {
            soundtrackParents.Find("Peaceful_Soundtrack").GetComponent<AudioSource>().Stop();
        });
        soundtrackParents.Find("Horror_Soundtrack").GetComponent<AudioSource>().Play();
        soundtrackParents.Find("Horror_Soundtrack").GetComponent<AudioSource>().volume = 0f;
        soundtrackParents.Find("Horror_Soundtrack").GetComponent<AudioSource>().DOFade(1f, 2f).SetDelay(2f);
    }

    public void TurnOffHorrorMode(){
        horrorMode = false;
        soundtrackParents.Find("Horror_Soundtrack").GetComponent<AudioSource>().DOFade(0f, 2f).OnComplete(() => {
            soundtrackParents.Find("Horror_Soundtrack").GetComponent<AudioSource>().Stop();
        });
        soundtrackParents.Find("Peaceful_Soundtrack").GetComponent<AudioSource>().Play();
        soundtrackParents.Find("Peaceful_Soundtrack").GetComponent<AudioSource>().volume = 0f;
        soundtrackParents.Find("Peaceful_Soundtrack").GetComponent<AudioSource>().DOFade(1f, 2f).SetDelay(2f);
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
        sfxParent.Find("ItemPickup").GetComponent<AudioSource>().Play();
        GameObject equippedItem = Instantiate(item.prefab, holdObjectTransform, false);
        if (equippedItem.GetComponent<Equippable>() != null){
            equippedItem.transform.localPosition = equippedItem.GetComponent<Equippable>().equippedPosition;
        }
    }

    public void UnequipItem(Item item){
        sfxParent.Find("ItemPickup").GetComponent<AudioSource>().Play();
        // Find a way to find which child is the item and destroy it
        Destroy(holdObjectTransform.GetChild(0).gameObject);
    }
    #endregion

    #region Tasks
    public void AssignTask(Task task){
        if (assignedTasks.Count == 0){
            tasksPanelTransform.DOScale(1, 1f).SetDelay(2f).OnComplete(() => StartCoroutine(MoveTasksPanel()));
        }
        assignedTasks.Add(task);
        if (task == Task.CleanCockpitWindows){
            tasksAssignedText.text += "- Clean cockpit windows\n";
            cockpitWindow.canInteract = true;
        }
        if (task == Task.RecalibratePressureGauge){
            tasksAssignedText.text += "- Recalibrate pressure gauge in engineering room\n";
            pressureGauge.canInteract = true;
        }
        if (task == Task.ReplaceNightLampBattery){
            tasksAssignedText.text += "- Replace night lamp battery in crew cabin\n";
            nightLamp.canInteract = true;
        }
        if (task == Task.Unpack){
            tasksAssignedText.text += "- Unpack boxes in cargo hold\n";
        }
        if (task == Task.TalkToChiron){
            tasksAssignedText.text += "- Talk to Chiron for a while\n";
        }
        if (task == Task.SurviveAsteroids){
            tasksAssignedText.text += "- Pilot the spaceship from cockpit and survive the onslaught of asteroids\n";
            TurnOnAsteroidTask();
        }
        if (task == Task.PurgeAir){
            tasksAssignedText.text += "- Purge the air in the spaceship. Button is in cargo hold.\n";
            TurnOnAirPurgeTask();
        }
        if (task == Task.ConnectWires){
            tasksAssignedText.text += "- Turn on backup generators in engineering room\n";
            connectTheWiresGO.SetActive(true);
            StartTimer();
        }
        if (task == Task.ReplaceFuse){
            tasksAssignedText.text += "- Replace the melted fuse in the fusebox in the crew cabin\n";
            fusebox.canInteract = true;
            StartTimer();
        }
    }

    IEnumerator MoveTasksPanel(){
        while (Vector3.Distance(tasksPanelTransform.position, tasksPanelMoveToTransform.position) > 0.01f){
            tasksPanelTransform.position = Vector3.MoveTowards(tasksPanelTransform.position, tasksPanelMoveToTransform.position, 0.01f);
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void TaskComplete(Task task){
        tasksCompleted.Add(task);
        if (tasksRemaining.Contains(task)) tasksRemaining.Remove(task);
        if (tasksRemaining.Contains(task)) assignedTasks.Remove(task);
        // Remove the task string from tasksAssignedText
        if (task == Task.CleanCockpitWindows){
            tasksAssignedText.text = tasksAssignedText.text.Replace("- Clean cockpit windows\n", "");
            TurnOffWindowCleaningTask();
        }
        if (task == Task.RecalibratePressureGauge){
            TurnOffPressureGaugeTask();
            tasksAssignedText.text = tasksAssignedText.text.Replace("- Recalibrate pressure gauge in engineering room\n", "");
        }
        if (task == Task.ReplaceNightLampBattery){
            tasksAssignedText.text = tasksAssignedText.text.Replace("- Replace night lamp battery in crew cabin\n", "");
        }
        if (task == Task.Unpack){
            tasksAssignedText.text = tasksAssignedText.text.Replace("- Unpack boxes in cargo hold\n", "");
        }
        if (task == Task.TalkToChiron){
            tasksAssignedText.text = tasksAssignedText.text.Replace("- Talk to Chiron for a while\n", "");
        }
        if (horrorMode){
            if (!_justExitedTransitionPeriod) Invoke("AssignHorrorTask", 5f);

            if (task == Task.SurviveAsteroids){
                tasksAssignedText.text = tasksAssignedText.text.Replace("- Pilot the spaceship from cockpit and survive the onslaught of asteroids\n", "");
                TurnOffAsteroidTask();
            }
            if (task == Task.PurgeAir){
                tasksAssignedText.text = tasksAssignedText.text.Replace("- Purge the air in the spaceship. Button is in cargo hold.\n", "");
                TurnOffAirPurgeTask();
            }
            if (task == Task.ConnectWires){
                tasksAssignedText.text = tasksAssignedText.text.Replace("- Turn on backup generators in engineering room\n", "");
                connectTheWiresGO.SetActive(false);
            }
            if (task == Task.ReplaceFuse){
                tasksAssignedText.text = tasksAssignedText.text.Replace("- Replace the melted fuse in the fusebox in the crew cabin\n", "");
            }
        }
        if (assignedTasks.Count == 0 && !horrorMode){
            if (tasksRemaining.Count == 0){
                AssignTask(Task.TalkToChiron);
                allPeacefulTasksComplete = true;
            } else {
                tasksAssignedText.text += "- Ask Chiron for another task\n";
            }
        }

        // Play some sound effect
        StopTimer();
    }

    public void AssignHorrorTask(){
        Debug.Log("tracing path...");
        if (GameManager.Instance.tasksRemaining.Count > 0) awsConnection.SendPrompt("I survived what you just threw at me. What next?", true);
        else awsConnection.SendPrompt("I survived all the assassination attempts you just threw at me. I'm ready to go home.", true);
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
        StartTimer();
        CameraStaticMode();
        MoveCamera(cockpitViewTransform, asteroidCameraTransitionTime, true);
    }

    public void TurnOffAsteroidTask(){
        asteroidGenerator.ClearAsteroids();
        _isAsteroidTaskOn = false;
        shipHealthSlider.gameObject.SetActive(false);
        MoveCamera(player.transform.Find("PlayerCameraRoot"), asteroidCameraTransitionTime, false);
    }
    public void TurnOnSecondStageAsteroids(){
        sfxParent.Find("AlarmNonLoop").GetComponent<AudioSource>().Play();
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
        StartTimer();
        CameraStaticMode();
        MoveCamera(windowCleaningCameraTransforms[0], asteroidCameraTransitionTime, true);
        Invoke("GenerateCleaningDots", asteroidCameraTransitionTime);
    }

    public void TurnOffWindowCleaningTask(){
        isWindowCleaningTaskOn = false;
        cleaningGOs.SetActive(false);
        MoveCamera(player.transform.Find("PlayerCameraRoot"), asteroidCameraTransitionTime, false);
    }

    public void GenerateCleaningDots(){
        Vector3 lastPosition = Vector3.zero;
        for (int i = 0; i < numDotsPerWindowToComplete; i++){
            Vector3 position = new Vector3(Random.Range(topLeftCleaningCanvas.position.x, bottomRightCleaningCanvas.position.x),
                                            Random.Range(topLeftCleaningCanvas.position.y, bottomRightCleaningCanvas.position.y),
                                            Random.Range(topLeftCleaningCanvas.position.z, bottomRightCleaningCanvas.position.z));
            if (i == 0) lastPosition = position;
            else if (Vector3.Distance(lastPosition, position) < 0.075f){
                i--;
                continue;
            } else {
                lastPosition = position;
            }
            Instantiate(cleaningDotPrefab, position, Quaternion.identity, dotsParent);
        }
    }
    #endregion

    #region Air Purge Functions
    public void TurnOnAirPurgeTask(){
        StartTimer();
        purgeAirWindow.canInteract = true;
        sfxParent.Find("Shake").GetComponent<AudioSource>().Play();
        sfxParent.Find("Alarm").GetComponent<AudioSource>().PlayDelayed(5f);
    }

    public void TurnOffAirPurgeTask(){
        sfxParent.Find("Alarm").GetComponent<AudioSource>().DOFade(0f, 2f).OnComplete(() => {
            sfxParent.Find("Alarm").GetComponent<AudioSource>().Stop();
        });
    }
    #endregion

    #region Pressure Gauge Functions
    public void TurnOnPressureGaugeTask(){
        isPressureGaugeTaskOn = true;
        needle.Restart();
        CameraStaticMode();
        MoveCamera(pressureGaugeViewTransform, asteroidCameraTransitionTime, true);
    }

    public void TurnOffPressureGaugeTask(){
        isPressureGaugeTaskOn = false;
        MoveCamera(player.transform.Find("PlayerCameraRoot"), asteroidCameraTransitionTime, false);
    }
    #endregion

    #region Camera Functions
    public void CameraStaticMode(){
        player.GetComponent<FirstPersonController>().isCameraFree = false;
        Camera.main.GetComponent<CinemachineBrain>().enabled = false;
    }

    public void MoveCamera(Transform moveToTransform, float transitionTime, bool cameraStaticMode, float delay = 0f){
        Debug.Log(cameraStaticMode);
        Camera.main.transform.DORotateQuaternion(moveToTransform.rotation, transitionTime).SetDelay(delay);
        if (cameraStaticMode) Camera.main.transform.DOMove(moveToTransform.position, transitionTime).SetDelay(delay);
        else Camera.main.transform.DOMove(moveToTransform.position, transitionTime).SetDelay(delay).OnComplete(() => {
            Camera.main.GetComponent<CinemachineBrain>().enabled = true;
            player.GetComponent<FirstPersonController>().isCameraFree = true;
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
        player.GetComponent<FirstPersonController>().isCameraFree = false;
        ItemManager.Instance.inventory.containerInteractor.canInteract = false;
        // Add stuff later
    }

    public void ResumeGame(){
        Time.timeScale = 1f;
        isGamePaused = false;
        player.GetComponent<FirstPersonController>().isCameraFree = true;
        ItemManager.Instance.inventory.containerInteractor.canInteract = true;
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