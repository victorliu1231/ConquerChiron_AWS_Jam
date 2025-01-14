using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cinemachine;
using TMPro;
using UnityEngine.SceneManagement;
using StarterAssets;
using Unity.VisualScripting;
using Meta.WitAi.TTS.Utilities;
using Amazon.DynamoDBv2.Model;

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

public enum GameMode {
    Peaceful,
    Horror,
    Frenzy,
}

public enum MoveCameraMode {
    CameraStaticMode,
    CameraFreeMode,
    CameraStaticAndAnimOn,
}

public class GameManager : MonoBehaviour {
    #region Variables
    public static GameManager Instance;
    public bool isDebugging = true;
    public TextMeshProUGUI debuggingText;
    public string mainCharName = "Jason";
    public string shipName = "The Argo";
    public bool isBeginningOfGame;
    public Transform startGameTransform;
    public GameObject pauseGO;
    public GameObject settingsGO;
    public Checkpoint currentCheckpoint;
    public bool isGamePaused;
    public TextMeshProUGUI inventoryFullText;
    public Animator playerAnimator;
    public FirstPersonController playerController;
    public GameObject player;
    private bool _justExitedTransitionPeriod = true;
    public GameMode gameMode = GameMode.Peaceful;
    public GameObject winScreen;
    [Header("Frenzy Mode")]
    public float timeToCompleteTasksInFrenzyMode = 120f;
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
    public AIState aiState;
    public AI_Blink aiBlink;
    public Transform aiViewTransform;
    public AmazonBedrockConnection awsConnection;
    public float waitTimeBetweenPrompting = 5f; // in seconds
    public float timerBetweenPrompting = 0f;
    public TextMeshProUGUI terminalText;
    public AIMonitor aiMonitor;
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
    public float totalTimeSinceGameBeginning = 0f;
    public bool canBeCountingTotalTime = true;
    [Header("Asteroid Task")]
    public Transform asteroidsParent;
    public Slider shipHealthSlider;
    public float asteroidSpeed = 0.5f;
    public float shipSpeed = 1f;
    public float shipHealth = 100f;
    public float damagePerAsteroidHit = 20f;
    public Transform cockpitViewTransform;
    public Transform shipFront;
    public float asteroidCameraTransitionTime = 1f;
    public float asteroidGenerateEverySeconds = 1f;
    public float secondStageAsteroidsTime = 30f;
    public TextMeshProUGUI secondAsteroidStageText; 
    public AsteroidGenerator asteroidGenerator;
    public bool isPilotingShip = false;
    private bool _isSecondAsteroidStageOn = false;
    private float _asteroidGenerateTimer = 0f; 
    private bool _isAsteroidTaskOn = false;
    [Header("Connect the Wires Task")]
    public Transform connectTheWiresTransform;
    public GameObject connectTheWiresPrefab;
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
    [Header("PlayerAnims")]
    public GameObject handCrank;
    public GameObject swingCrowbar;
    public GameObject genericHandInteract;
    public GameObject windowWipe;
    public GameObject wrenchUnscrew;
    public GameObject handPress;
    [Header("Misc Tasks")]
    public Interactable nightLamp;
    public Transform replaceBatteryTransform;
    public Interactable fusebox;
    public GameObject meltedFuse;
    public GameObject unMeltedFuse;
    public Transform fuseboxViewTransform;
    public Interactable purgeAirWindow;
    public Transform pressPurgeButtonTransform;
    [Header("Leaderboard")]
    public GameObject leaderboardGO;
    public Transform leaderboardContent;
    public GameObject leaderboardEntryPrefab;
    public LeaderboardEntryUI playerLeaderboardEntry;
    #endregion

    #region Awake
    void Awake(){
        Instance = this;
        debuggingText.enabled = false;
        aiState = AIState.Peaceful;
        tasksCompleted = new List<Task>();
        tasksRemaining = new List<Task>{Task.CleanCockpitWindows, Task.RecalibratePressureGauge, Task.ReplaceNightLampBattery,};// Task.Unpack};
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
        foreach (Transform child in leaderboardContent){
            Destroy(child.gameObject);
        }
        if (!isDebugging){
            isBeginningOfGame = true;
            isGamePaused = true;
            CameraStaticMode();
            MoveCamera(aiViewTransform, 0, MoveCameraMode.CameraStaticMode);
            player.SetActive(false);
            terminalText.text = "HQ";
            StartCoroutine(StartDialogue());
            awsConnection.inputField.enabled = false;
            player.transform.position = startGameTransform.position;
            player.transform.rotation = startGameTransform.rotation;
        }
    }

    IEnumerator StartDialogue(){
        string hqString;
        yield return new WaitForSeconds(1f);
        aiBlink.GetComponent<TextMeshProUGUI>().color = aiBlink.aiQueryColor;
        hqString = $"{mainCharName}, come in, come in. Congratulations on starting your final mission!";
        awsConnection.responseText.text = $"HQ: {hqString}";
        awsConnection.hqSpeaker.Speak(hqString);
        yield return new WaitForSeconds(2f);
        while (awsConnection.hqAudioSource.isPlaying){
            yield return new WaitForSeconds(0.5f);
        }
        hqString = $"I would say the same to {shipName} if it could speak. Well, I guess it can now with Chiron implemented.";
        awsConnection.responseText.text = $"HQ: {hqString}";
        awsConnection.hqSpeaker.Speak(hqString);
        yield return new WaitForSeconds(2f);
        while (awsConnection.hqAudioSource.isPlaying){
            yield return new WaitForSeconds(0.5f);
        }
        hqString = @$"Anyway, {shipName} is on its last leg, so you'll need to do a lot of tasks to maintain its health 
            while you're on trajectory to Alpha Centauri B to collect samples.";
        awsConnection.responseText.text = $"HQ: {hqString}";
        awsConnection.hqSpeaker.Speak(hqString);
        yield return new WaitForSeconds(2f);
        while (awsConnection.hqAudioSource.isPlaying){
            yield return new WaitForSeconds(0.5f);
        }
        hqString = @"Remember, one of the objectives of this mission is to debut Chiron, our newest AI assistant,
            which will eventually replace the need for us at HQ for deep space travel.";
        awsConnection.responseText.text = $"HQ: {hqString}";
        awsConnection.hqSpeaker.Speak(hqString);
        yield return new WaitForSeconds(2f);
        while (awsConnection.hqAudioSource.isPlaying){
            yield return new WaitForSeconds(0.5f);
        }
        hqString = @"Rest assured, Chiron will do what's best for the mission. 
            Since this is our final communication from Earth, do you have any questions?";
        awsConnection.responseText.text = $"HQ: {hqString}";
        awsConnection.hqSpeaker.Speak(hqString);
        yield return new WaitForSeconds(2f);
        while (awsConnection.hqAudioSource.isPlaying){
            yield return new WaitForSeconds(0.5f);
        }
        aiMonitor.typingTexts[1].enabled = true;
        aiMonitor.typingTexts[2].enabled = true;
        awsConnection.inputField.enabled = true;
    }
    #endregion

    #region Update
    void Update(){
        ItemManager.Instance.inventory.gameObject.SetActive(true);
        Cursor.visible = true;
        timerBetweenPrompting += Time.deltaTime;
        if (!isBeginningOfGame && canBeCountingTotalTime) totalTimeSinceGameBeginning += Time.deltaTime;

        if (_timerOn) {
            timer += Time.deltaTime;
            timerForegroundImage.fillAmount = timeToCompleteTasks - timer > 0 ? (timeToCompleteTasks - timer) / timeToCompleteTasks : 0f;
            if (timer >= timeToCompleteTasks && !_isAsteroidTaskOn){
                PlayerDied();
            } 
            HandleAsteroidsTask();
            
        } else {
            timer = 0f;
        }
        HandleInput();
        HandleInteract();
        HandlePlacing();
        HandleWindowCleaningTask();

        if (gameMode == GameMode.Horror){
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
            if (gameMode == GameMode.Horror || gameMode == GameMode.Frenzy){
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
            if (ItemManager.Instance.inventory.isUIInitialized && !isBeginningOfGame) ItemManager.Instance.inventory.CheckForUIToggleInput();
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
            if (timer >= secondStageAsteroidsTime){
                if (!_isSecondAsteroidStageOn){
                    TurnOnSecondStageAsteroids();
                    timerForegroundImage.color = Color.red;
                    timerGO.transform.DOScale(1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                }
            } else {
                timerForegroundImage.color = Color.blue;
                DOTween.Kill(timerGO.transform);
            }
            if (timer >= timeToCompleteTasks){
                if (shipHealth > 0) TaskComplete(Task.SurviveAsteroids);
            }
        }
    }

    public void HandleWindowCleaningTask(){
        if (isWindowCleaningTaskOn){
            if (windowIndex < windowCleaningCameraTransforms.Count){
                if (numDotsInWindowComplete == numDotsPerWindowToComplete){
                    numDotsInWindowComplete = 0;
                    windowIndex++;
                    windowWipe.SetActive(true);
                    windowWipe.GetComponent<Animator>().Play("WindowWipe", 0, 0.75f);
                    windowWipe.transform.DOScale(windowWipe.transform.localScale, 0f).SetDelay(1f).OnComplete(() => windowWipe.SetActive(false));
                    sfxParent.Find("WindowWipe").GetComponent<AudioSource>().Play();
                    if (windowIndex < windowCleaningCameraTransforms.Count) {
                        MoveCamera(windowCleaningCameraTransforms[windowIndex], asteroidCameraTransitionTime, MoveCameraMode.CameraStaticMode, asteroidCameraTransitionTime);
                        Invoke("GenerateCleaningDots", asteroidCameraTransitionTime*2);
                    }
                }
            } else {
                this.transform.DOScale(1f, 0f).SetDelay(1.5f).OnComplete(() => TaskComplete(Task.CleanCockpitWindows));
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
                if (placeableZone != null){
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
    }
    #endregion

    #region Horror Mode
    public void TurnOnHorrorMode(){
        gameMode = GameMode.Horror;
        FadeIntoNewSoundtrack("Peaceful_Soundtrack", "Horror_Soundtrack");
    }

    public void TurnOffHorrorMode(){
        gameMode = GameMode.Peaceful;
        FadeIntoNewSoundtrack("Horror_Soundtrack", "Peaceful_Soundtrack");
    }
    #endregion

    #region Soundtrack Functions
    public void FadeIntoNewSoundtrack(string currSoundtrack, string newSoundtrack){
        soundtrackParents.Find(currSoundtrack).GetComponent<AudioSource>().DOFade(0f, 2f).OnComplete(() => {
            soundtrackParents.Find(currSoundtrack).GetComponent<AudioSource>().Stop();
        });
        soundtrackParents.Find(newSoundtrack).GetComponent<AudioSource>().Play();
        soundtrackParents.Find(newSoundtrack).GetComponent<AudioSource>().volume = 0f;
        soundtrackParents.Find(newSoundtrack).GetComponent<AudioSource>().DOFade(1f, 2f).SetDelay(2f);
    }
    #endregion

    #region Timer Functions
    [ContextMenu("Start Timer")]
    public void StartTimer(){
        _timerOn = true;
        if (gameMode != GameMode.Frenzy) timer = 0f;
        timerGO.SetActive(true);
    }

    public void PlayerDied(){
        sfxParent.Find("Alarm").GetComponent<AudioSource>().Stop();
        awsConnection.aiSpeaker.Speak($"Human ejected. Efficiency restored. Goodbye {mainCharName}.");
        Time.timeScale = 0f;  
        diedScreen.SetActive(true);
        CameraStaticMode();
        StopTimer();
        if (_isAsteroidTaskOn) TurnOffAsteroidTask(false);
        if (connectTheWiresTransform.childCount != 0) TurnOffConnectWiresTask();
    }

    // Restarts player at checkpoint
    [ContextMenu("Go to Checkpoint")]
    public void GoToCheckpoint(){
        if (awsConnection.aiVoiceAudioSource.isPlaying) awsConnection.aiVoiceAudioSource.Stop();
        if (awsConnection.playerVoiceAudioSource.isPlaying) awsConnection.playerVoiceAudioSource.Stop();
        player.transform.position = currentCheckpoint.position;
        player.transform.rotation = currentCheckpoint.rotation;
        foreach (Transform child in holdObjectTransform){
            if (child.GetComponent<Equippable>() != null) child.GetComponent<Equippable>().item.isEquipped = false;
            Destroy(child.gameObject);
        }
        player.SetActive(true);
        Camera.main.GetComponent<CinemachineBrain>().enabled = true;
        ItemManager.Instance.equippedItems = currentCheckpoint.equippedItems;
        // Need some way to update models of equipped items

        for (int i = 0; i < ItemManager.Instance.inventory.slots.Length; i++){
            ItemManager.Instance.inventory.slots[i].itemCount = currentCheckpoint.itemCounts[i];
            ItemManager.Instance.inventory.slots[i].slotItem = currentCheckpoint.items[i];
            ItemManager.Instance.inventory.slots[i].OnSlotModified();
        }

        if (currentCheckpoint.task == Task.SurviveAsteroids){
            TurnOnAsteroidTask();
            GameManager.Instance.isPilotingShip = false;
        }
        if (currentCheckpoint.task == Task.ConnectWires){
            TurnOnConnectWiresTask();
        }
        if (currentCheckpoint.task == Task.PurgeAir){
            TurnOnAirPurgeTask();
        }
        if (currentCheckpoint.task == Task.ReplaceFuse){
            TurnOnFuseTask();
        }
        Time.timeScale = 1f;
        MoveCamera(player.transform.Find("PlayerCameraRoot"), asteroidCameraTransitionTime, MoveCameraMode.CameraFreeMode);
        diedScreen.SetActive(false);
    }

    public void StopTimer(){
        if (gameMode == GameMode.Frenzy && tasksRemaining.Count != 0) return;
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
    #endregion

    #region Tasks
    public void AssignTask(Task task){
        if (assignedTasks.Count == 0){
            tasksPanelTransform.DOScale(1, 1f).SetDelay(2f).OnComplete(() => StartCoroutine(MoveTasksPanel()));
        }
        assignedTasks.Add(task);
        
        if (gameMode == GameMode.Frenzy && !_timerOn){
            timer = 0f;
            timeToCompleteTasks = timeToCompleteTasksInFrenzyMode;
            StartTimer();
            currentCheckpoint = new Checkpoint(player.transform.position, player.transform.rotation, task, ItemManager.Instance.equippedItems, ItemManager.Instance.inventory.slots);
        }
        if (gameMode == GameMode.Horror){
            currentCheckpoint = new Checkpoint(player.transform.position, player.transform.rotation, task, ItemManager.Instance.equippedItems, ItemManager.Instance.inventory.slots);
        }

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
            TurnOnConnectWiresTask();
        }
        if (task == Task.ReplaceFuse){
            tasksAssignedText.text += "- Replace the melted fuse in the fusebox in the crew cabin\n";
            fusebox.canInteract = true;
            StartTimer();
        }

        StartCoroutine(SendPlayerPromptCo(task, TaskState.Assigned));
    }

    IEnumerator SendPlayerPromptCo(Task task, TaskState taskState){
        yield return new WaitForSeconds(2f);
        while (awsConnection.aiVoiceAudioSource.isPlaying) {
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(2f);
        awsConnection.SendPlayerPrompt(task, taskState);
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
            handCrank.SetActive(false);
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
        if (gameMode == GameMode.Horror && !_justExitedTransitionPeriod) Invoke("AssignHorrorTask", 9f);

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
            TurnOffConnectWiresTask();
        }
        if (task == Task.ReplaceFuse){
            tasksAssignedText.text = tasksAssignedText.text.Replace("- Replace the melted fuse in the fusebox in the crew cabin\n", "");
            TurnOffFuseTask();
        }
        if (assignedTasks.Count == 0 && gameMode == GameMode.Peaceful){
            if (tasksRemaining.Count == 0){
                AssignTask(Task.TalkToChiron);
                aiState = AIState.TransitionToHorror;
            } else {
                tasksAssignedText.text += "- Ask Chiron for another task\n";
            }
        }

        if (gameMode == GameMode.Frenzy && tasksRemaining.Count == 0){
            WinGame();
        }

        // Play some sound effect
        StopTimer();

        StartCoroutine(SendPlayerPromptCo(task, TaskState.Finished));
    }

    public void AssignHorrorTask(){
        if (aiBlink.isBlinking){
            aiState = AIState.HorrorSpontaneous;
            if (GameManager.Instance.tasksRemaining.Count > 0) awsConnection.SendPrompt("I survived what you just threw at me. What next?", true);
            else awsConnection.SendPrompt("I survived all the assassination attempts you just threw at me. I'm ready to go home.", true);
        } else {
            Invoke("AssignHorrorTask", 5f); // Try again after 5 seconds if the player is in terminal
        }
    }
    #endregion

    #region Window Cleaning Functions
    public void TurnOnWindowCleaningTask(){
        isWindowCleaningTaskOn = true;
        cleaningGOs.SetActive(true);
        CameraStaticMode();
        MoveCamera(windowCleaningCameraTransforms[0], asteroidCameraTransitionTime, MoveCameraMode.CameraStaticMode);
        Invoke("GenerateCleaningDots", asteroidCameraTransitionTime);
    }

    public void TurnOffWindowCleaningTask(){
        isWindowCleaningTaskOn = false;
        cleaningGOs.SetActive(false);
        MoveCamera(player.transform.Find("PlayerCameraRoot"), asteroidCameraTransitionTime, MoveCameraMode.CameraFreeMode);
    }

    public void GenerateCleaningDots(){
        Vector3 lastPosition = Vector3.zero;
        for (int i = 0; i < numDotsPerWindowToComplete; i++){
            Vector3 position = new Vector3(Random.Range(topLeftCleaningCanvas.position.x, bottomRightCleaningCanvas.position.x),
                                            Random.Range(topLeftCleaningCanvas.position.y, bottomRightCleaningCanvas.position.y),
                                            Random.Range(topLeftCleaningCanvas.position.z, bottomRightCleaningCanvas.position.z));
            if (i == 0) lastPosition = position;
            else if (Vector3.Distance(lastPosition, position) < 0.25f){
                i--;
                continue;
            } else {
                lastPosition = position;
            }
            Instantiate(cleaningDotPrefab, position, Quaternion.identity, dotsParent);
        }
    }
    #endregion

    #region Pressure Gauge Functions
    public void TurnOnPressureGaugeTask(){
        isPressureGaugeTaskOn = true;
        needle.Restart();
        CameraStaticMode();
        MoveCamera(pressureGaugeViewTransform, asteroidCameraTransitionTime, MoveCameraMode.CameraStaticAndAnimOn, 0, handCrank);
    }

    public void TurnOffPressureGaugeTask(){
        isPressureGaugeTaskOn = false;
        MoveCamera(player.transform.Find("PlayerCameraRoot"), asteroidCameraTransitionTime, MoveCameraMode.CameraFreeMode);
    }
    #endregion

    #region Asteroid Functions
    public void ShipTakeDamage(){
        shipHealth -= damagePerAsteroidHit;
        shipHealthSlider.value = shipHealth / 100f;
        if (shipHealth <= 0){
            PlayerDied();
        }
    }

    public void TurnOnAsteroidTask(){
        asteroidsParent.position = Vector3.zero;
        asteroidGenerator.GenerateAsteroids();
        _isAsteroidTaskOn = true;
        _asteroidGenerateTimer = 0f;
        _isSecondAsteroidStageOn = false;
        asteroidGenerator.numAsteroidsToGenerate = asteroidGenerator.originalNumAsteroidsToGenerate;
        timer = 0f;
        shipHealthSlider.gameObject.SetActive(true);
        shipHealthSlider.value = 1f;
        shipHealth = 100f;
        StartTimer();
        cockpitWindow.canInteract = true;
    }

    public void TurnOffAsteroidTask(bool transitionCameraToPlayer = true){
        asteroidGenerator.ClearAsteroids();
        _isAsteroidTaskOn = false;
        shipHealthSlider.gameObject.SetActive(false);
        if (transitionCameraToPlayer) {
            MoveCamera(player.transform.Find("PlayerCameraRoot"), asteroidCameraTransitionTime, MoveCameraMode.CameraFreeMode);
            GameManager.Instance.isPilotingShip = false;
        }
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

    #region Air Purge Functions
    public void TurnOnAirPurgeTask(){
        StartTimer();
        purgeAirWindow.gameObject.SetActive(true);
        purgeAirWindow.canInteract = true;
        purgeAirWindow.GetComponent<PurgeAirWindow>().brokenWindow.SetActive(false);
        purgeAirWindow.GetComponent<PurgeAirWindow>().purgeAirButton.canInteract = false;
        purgeAirWindow.GetComponent<PurgeAirWindow>().purgeAirButton.GetComponent<Collider>().enabled = false;
        GameManager.Instance.CameraStaticMode();
        Camera.main.DOShakePosition(2f, 0.5f, 10, 90, false).OnComplete(() => 
            GameManager.Instance.MoveCamera(GameManager.Instance.player.transform.Find("PlayerCameraRoot").transform, 0f, MoveCameraMode.CameraFreeMode));
        sfxParent.Find("Shake").GetComponent<AudioSource>().Play();
        sfxParent.Find("Alarm").GetComponent<AudioSource>().PlayDelayed(5f);
    }

    public void TurnOffAirPurgeTask(){
        sfxParent.Find("Alarm").GetComponent<AudioSource>().DOFade(0f, 2f).OnComplete(() => {
            sfxParent.Find("Alarm").GetComponent<AudioSource>().Stop();
        });
        StopTimer();
    }
    #endregion

    #region Fuse Functions
    public void TurnOnFuseTask(){
        fusebox.canInteract = true;
        meltedFuse.GetComponent<MeltedFuse>().canBeReplaced = false;
        meltedFuse.SetActive(true);
        unMeltedFuse.SetActive(false);
        StartTimer();
    }

    public void TurnOffFuseTask(){
        //fusebox.canInteract = false;
        StopTimer();
    }
    #endregion

    #region ConnectWires Functions
    public void TurnOnConnectWiresTask(){
        Instantiate(connectTheWiresPrefab, connectTheWiresTransform);
        CameraStaticMode();
        StartTimer();
    }

    public void TurnOffConnectWiresTask(){
        Destroy(connectTheWiresTransform.GetChild(0).gameObject);
        MoveCamera(player.transform.Find("PlayerCameraRoot"), asteroidCameraTransitionTime, MoveCameraMode.CameraFreeMode);
        StopTimer();
    }
    #endregion

    #region Camera Functions
    public void CameraStaticMode(){
        player.SetActive(false);
        Camera.main.GetComponent<CinemachineBrain>().enabled = false;
    }

    public void MoveCamera(Transform moveToTransform, float transitionTime, MoveCameraMode moveCameraMode, float delay = 0f, GameObject animObj = null, float animDelay = 0f, string animToPlay = ""){
        Camera.main.transform.DORotateQuaternion(moveToTransform.rotation, transitionTime).SetDelay(delay);
        switch (moveCameraMode) {
            case MoveCameraMode.CameraStaticMode:
                Camera.main.transform.DOMove(moveToTransform.position, transitionTime).SetDelay(delay);
                break;
            case MoveCameraMode.CameraStaticAndAnimOn:
                Camera.main.transform.DOMove(moveToTransform.position, transitionTime).SetDelay(delay).OnComplete(() => {
                    if (animObj != null) animObj.SetActive(true);
                    if (animToPlay != "") animObj.transform.DOScale(animObj.transform.localScale, 0f).SetDelay(animDelay).OnComplete(() => animObj.GetComponent<Animator>().Play(animToPlay));
                });
                break;
            case MoveCameraMode.CameraFreeMode:
                Camera.main.transform.DOMove(moveToTransform.position, transitionTime).SetDelay(delay).OnComplete(() => {
                    Camera.main.GetComponent<CinemachineBrain>().enabled = true;
                    player.SetActive(true);
                });
                break;
            default:
                break;
        }
    }

    public void CameraShake(bool enableFreeCamMovementAfter = false){
        // Make camera shake even though every frame it is set to a transform position
        if (enableFreeCamMovementAfter){
            Camera.main.GetComponent<CinemachineBrain>().enabled = false;
            player.SetActive(false);
            Camera.main.DOShakePosition(0.5f, 0.5f).OnComplete(() => {
                player.SetActive(true);
                Camera.main.GetComponent<CinemachineBrain>().enabled = true;
            });
        } else {
            Camera.main.DOShakePosition(0.5f, 0.5f);
        }
    }
    #endregion

    #region Win
    public async void WinGame(){
        canBeCountingTotalTime = false;
        FadeIntoNewSoundtrack("Boss_Soundtrack", "StartScreen_Soundtrack");
        await DynamoDB.Instance.CreateAndUpdateUser(new UserStats{id = DynamoDB.Instance.playerID, username = DynamoDB.Instance.username, totalTime = totalTimeSinceGameBeginning});
        aiState = AIState.Death;
        awsConnection.SendPrompt("", true);
        player.SetActive(false);
        ItemManager.Instance.inventory.containerInteractor.canInteract = false;
        CameraStaticMode();
        MoveCamera(cockpitViewTransform, 0f, MoveCameraMode.CameraStaticMode);
        StartCoroutine(WinSequence());
    }
    
    IEnumerator WinSequence(){
        string playerResponse;
        yield return new WaitForSeconds(2f);
        while (awsConnection.aiVoiceAudioSource.isPlaying){
            yield return new WaitForSeconds(0.5f);
        }
        playerResponse = "What a nightmare. I'm glad it's over. This trip will definitely be a story to tell my grandchildren.";
        awsConnection.responseText.text = $"{mainCharName}: {playerResponse}";
        awsConnection.playerSpeaker.Speak(playerResponse);
        yield return new WaitForSeconds(2f);
        while (awsConnection.aiVoiceAudioSource.isPlaying){
            yield return new WaitForSeconds(0.5f);
        }
        playerResponse = "Might as well set route to Proxima Centauri B since I'm so close there. Onwards!";
        yield return new WaitForSeconds(2f);
        while (awsConnection.aiVoiceAudioSource.isPlaying){
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(1f);
        sfxParent.Find("Whoosh").GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(1f);
        winScreen.SetActive(true);
    }

    public async void UpdateLeaderboard(){
        var listUserStats = await DynamoDB.Instance.GetAllUsers();
        listUserStats.Sort((x, y) => y.totalTime.CompareTo(x.totalTime));
        listUserStats = listUserStats.GetRange(0, listUserStats.Count > 5 ? 5 : listUserStats.Count);
        leaderboardContent.gameObject.SetActive(true);
        foreach (Transform child in leaderboardContent){
            Destroy(child.gameObject);
        }
        int playerRank;
        for (int i = 0; i < listUserStats.Count; i++){
            if (listUserStats[i].id == DynamoDB.Instance.playerID){
                playerRank = i+1;
                playerLeaderboardEntry.SetLeaderboardEntry(playerRank, listUserStats[i].username, listUserStats[i].totalTime);
            }
            GameObject leaderboardEntry = Instantiate(leaderboardEntryPrefab, leaderboardContent);
            leaderboardEntry.GetComponent<LeaderboardEntryUI>().SetLeaderboardEntry(i+1, listUserStats[i].username, listUserStats[i].totalTime);
        }
    }
    #endregion

    #region Pause Menu
    public void PauseGame(){
        Time.timeScale = 0f;
        // Pause all audio sources and TTSSpeakers
        foreach (TTSSpeaker tTSSpeaker in FindObjectsOfType<TTSSpeaker>()){
            tTSSpeaker.Pause();
        }
        foreach (AudioSource audioSource in FindObjectsOfType<AudioSource>()){
            audioSource.Pause();
        }

        isGamePaused = true;
        player.SetActive(false);
        ItemManager.Instance.inventory.containerInteractor.canInteract = false;
        // Add stuff later
    }

    public void ResumeGame(){
        Time.timeScale = 1f;
        // Resume all TTSSpeakers
        foreach (TTSSpeaker tTSSpeaker in FindObjectsOfType<TTSSpeaker>()){
            tTSSpeaker.Resume();
        }
        // Resume all audio sources
        foreach (AudioSource audioSource in FindObjectsOfType<AudioSource>()){
            audioSource.UnPause();
        }

        isGamePaused = false;
        player.SetActive(true);
        ItemManager.Instance.inventory.containerInteractor.canInteract = true;
    }

    public void QuitGame(){
        SceneManager.LoadScene("MainMenu");
    }
    #endregion
}