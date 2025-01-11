using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using DG.Tweening;
using Meta.WitAi.TTS.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum AIState {
    Peaceful,
    TransitionToHorror,
    HorrorSpontaneous,
    HorrorPrompted,
    TransitionToFrenzy,
    FrenzySpontaneous,
    FrenzyPrompted,
    Death,
}

public enum TaskState {
    Assigned,
    Finished,
}

#region AmazonBedrock Class
public class AmazonBedrockConnection : MonoBehaviour {
    [Header("AWS Credentials")]
    [SerializeField] public string accessKeyId;
    [SerializeField] public string secretAccessKey;

    [Header("Experience Settings")]
    [SerializeField] public TextMeshProUGUI responseText;
    public TextMeshProUGUI waitText;
    [SerializeField] public TMP_InputField inputField;
    public TTSSpeaker hqSpeaker;
    [SerializeField] public TTSSpeaker aiSpeaker;
    [SerializeField] public TTSSpeaker playerSpeaker;

    private AmazonBedrockRuntimeClient client;
    private const string ModelId = "amazon.nova-lite-v1:0";//"meta.llama3-8b-instruct-v1:0"; // Adjust llama model
    private static readonly RegionEndpoint RegionEndpoint = RegionEndpoint.USEast1; // Adjust server region
    private string lastResponse;
    public AudioSource hqAudioSource;
    public AudioSource aiVoiceAudioSource;
    public AudioSource playerVoiceAudioSource;
    private bool isPlayingAudio = false;
    public int numPromptsInTransitionPeriod = 0;
    public List<Task> frenzyModeTasks;
    public float timeTillTips = 60f;
    private float _timer = 0f;

    private void Awake(){
        responseText.text = "";
        waitText.text = "";
        // var credentials = new BasicAWSCredentials(
        //     Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
        //     Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
        // );
        var credentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);
        client = new AmazonBedrockRuntimeClient(credentials, RegionEndpoint);

        inputField.onSubmit.AddListener((string prompt) => SendPrompt(prompt));

        frenzyModeTasks = new List<Task>(){
            Task.ConnectWires,
            Task.PurgeAir,
            Task.ReplaceFuse,
            Task.RecalibratePressureGauge,
        };
    }

    void Update(){
        if (aiVoiceAudioSource.isPlaying || playerVoiceAudioSource.isPlaying){
            isPlayingAudio = true;
        }
        if (isPlayingAudio && !aiVoiceAudioSource.isPlaying && !playerVoiceAudioSource.isPlaying){
            isPlayingAudio = false;
            Invoke("ClearResponseText", 5f);
        }
        if (!GameManager.Instance.isBeginningOfGame){
            _timer += Time.deltaTime;
            if (_timer >= timeTillTips){
                if (GameManager.Instance.assignedTasks.Count == 0 && GameManager.Instance.tasksCompleted.Count == 0){
                    _timer = 0f;
                    string tip = "I wonder what if I say the words 'task' or 'do' in my prompt to Chiron. Maybe it will help me get a task to do.";
                    responseText.text = $"{GameManager.Instance.mainCharName}: {tip}";
                    playerSpeaker.Speak(tip);
                }
            }
        }
    }

    public void ClearResponseText(){
        if (!isPlayingAudio) responseText.text = "";
    }

    public void ClearWaitText(){
        waitText.text = "";
    }

    #region AISpeaker
    IEnumerator TransitionToStartOfGame(){
        string aiInterruption;
        yield return new WaitForSeconds(0.5f);
        responseText.text = "Sure, I can help you with that. For that you need to first do ...";
        hqSpeaker.Speak(responseText.text);
        yield return new WaitForSeconds(0.5f);
        hqAudioSource.DOFade(0f, 3f);
        yield return new WaitForSeconds(2.5f);
        aiInterruption = "Sorry to interrupt, but there are some urgent maintenance tasks that need to be taken care of. Please ask me what tasks you should do.";
        responseText.text = $"Chiron: {aiInterruption}";
        aiSpeaker.Speak(aiInterruption);
        yield return new WaitForSeconds(2f);
        while (aiVoiceAudioSource.isPlaying){
            yield return new WaitForSeconds(0.5f);
        }
        aiInterruption = "By the way, WASD or arrow keys are used to move. Space is to jump. Left Shift is to sprint. I is to open your inventory.";
        responseText.text = $"Chiron: {aiInterruption}";
        aiSpeaker.Speak(aiInterruption);
        yield return new WaitForSeconds(2f);
        while (aiVoiceAudioSource.isPlaying){
            yield return new WaitForSeconds(0.5f);
        }
        GameManager.Instance.aiMonitor.typingTexts[0].enabled = true;
        GameManager.Instance.aiMonitor.canQuit = true;
        GameManager.Instance.isBeginningOfGame = false;
    }

    public async void SendPrompt(string prompt, bool spontaneous = false){
        if (!spontaneous){
            if (!GameManager.Instance.isDebugging && GameManager.Instance.timerBetweenPrompting < GameManager.Instance.waitTimeBetweenPrompting){
                waitText.text = "Please wait a moment before sending another prompt.";
                Invoke("ClearWaitText", 5f);
                return;
            }
            if (string.IsNullOrEmpty(prompt)){ 
                responseText.text = "Please enter a prompt.";
                return;
            }
            if (!InputValidator.ValidateInput(prompt)){
                responseText.text = "Invalid input detected";
                return;
            }
        
            prompt = InputSanitizer.SanitizeInput(prompt);
        }

        if (GameManager.Instance.isBeginningOfGame){
            StartCoroutine(TransitionToStartOfGame());
            return;
        }

        if (GameManager.Instance.gameMode == GameMode.Horror){
            if (spontaneous) {
                if (GameManager.Instance.tasksRemaining.Count > 0) GameManager.Instance.aiState = AIState.HorrorSpontaneous; 
                else GameManager.Instance.aiState = AIState.TransitionToFrenzy;
            }
            else GameManager.Instance.aiState = AIState.HorrorPrompted;
        }
        if (GameManager.Instance.gameMode == GameMode.Frenzy){
            if (spontaneous) GameManager.Instance.aiState = AIState.FrenzySpontaneous;
            else GameManager.Instance.aiState = AIState.FrenzyPrompted;
        }
        
        if (!spontaneous) GameManager.Instance.playerPrompts.Add(prompt);

        var fullPrompt = $"{GenerateContext(prompt)}";

        // The response structure from Bedrock models can vary. For debugging, let's log the full response
        var requestBody = new{
            inferenceConfig = new{
                max_new_tokens = 1000
            },
            messages = new[]{
                new{
                    role = "user",
                    content = new[]{
                        new{
                            text = fullPrompt
                        }
                    }
                }
            }
        };

        var request = new InvokeModelRequest
        {
            ModelId = ModelId,
            ContentType = "application/json",
            Accept = "application/json",
            Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestBody)))
        };

        var response = await client.InvokeModelAsync(request);
        var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
        var modelResponse = JObject.Parse(responseBody);

        if (!spontaneous) GameManager.Instance.timerBetweenPrompting = 0f;
        
        // For Nova-lite model, the response is in "content" field
        var assistantResponse = modelResponse["output"]?["message"]?["content"]?[0]?["text"]?.ToString();
        
        // Fallback to checking other common response fields if content is null
        if (string.IsNullOrEmpty(assistantResponse)) {
            assistantResponse = "No response found in expected fields";
        }

        if (assistantResponse.Length > 1000){
            assistantResponse = assistantResponse.Substring(0, 1000);
        }
        string lowercased = assistantResponse.ToLower();
        if (!GameManager.Instance.allPeacefulTasksComplete && GameManager.Instance.gameMode == GameMode.Peaceful){
            if (!GameManager.Instance.assignedTasks.Contains(Task.CleanCockpitWindows) && (lowercased.Contains("cockpit") || lowercased.Contains("windows") || lowercased.Contains("window") || lowercased.Contains("clean"))){
                GameManager.Instance.AssignTask(Task.CleanCockpitWindows);
            }
            if (!GameManager.Instance.assignedTasks.Contains(Task.ReplaceNightLampBattery) && (lowercased.Contains("crew") || lowercased.Contains("cabin") || lowercased.Contains("night") || lowercased.Contains("lamp") || lowercased.Contains("battery"))){
                GameManager.Instance.AssignTask(Task.ReplaceNightLampBattery);
            }
            if (!GameManager.Instance.assignedTasks.Contains(Task.Unpack) && (lowercased.Contains("cargo") || lowercased.Contains("hold") || lowercased.Contains("box") || lowercased.Contains("boxes") || lowercased.Contains("unpack"))){
                GameManager.Instance.AssignTask(Task.Unpack);
            }
            if (!GameManager.Instance.assignedTasks.Contains(Task.RecalibratePressureGauge) && (lowercased.Contains("engineering") || lowercased.Contains("pressure") || lowercased.Contains("gauge") || lowercased.Contains("oxygen") || lowercased.Contains("recalibrate"))){
                GameManager.Instance.AssignTask(Task.RecalibratePressureGauge);
            }
        }
        
        responseText.text = $"Chiron: {assistantResponse}";
        lastResponse = assistantResponse;
        aiSpeaker.Speak(assistantResponse);
    }

    public string GenerateContext(string prompt){
        string cargoHoldTask = "the cargo hold, which needs boxes to be unpacked.";
        string crewCabinTask = "the crew cabin, which needs to have its night lamp's battery replaced. The replacement battery is in the crew cabin.";
        string engineeringRoomTask = "the engineering room, whose pressure gauge for its oxygen tank has gone out of calibration and needs to be recalibrated.";
        string cockpitTask = "the cockpit, whose windows need to be cleaned in order to view the outside space environment clearly. The window spray bottle and towel are in the cockpit drawer.";
        string asteroidsTask = "I have re-routed the ship to now head towards a dense asteroid belt. Prepare to be battered to death by space rocks.";
        string purgeAirTask = @"I have caused extremely turbulence in the ship, causing glass flasks of hydrogen sulfide intended for geological testing in the cargo room to break.
            The air in the spaceship is becoming toxic. Try surviving now! Muhaha.";
        string connectWiresTask = "I have shut down all computer-controlled power generators. Hah, in a while the pressure maintenance system will fail, causing your death!";
        string replaceFuseTask = "I have overloaded the current to the fridge in the crew cabin. All that food you have in there? It will perish in a few days.";
        Dictionary<Task, string> taskDescriptions = new Dictionary<Task, string>{
            {Task.Unpack, cargoHoldTask},
            {Task.ReplaceNightLampBattery, crewCabinTask},
            {Task.RecalibratePressureGauge, engineeringRoomTask},
            {Task.CleanCockpitWindows, cockpitTask},
            {Task.SurviveAsteroids, asteroidsTask},
            {Task.PurgeAir, purgeAirTask},
            {Task.ConnectWires, connectWiresTask},
            {Task.ReplaceFuse, replaceFuseTask}
        };

        string taskToBeDonePrompt = "";
        string context;

        switch (GameManager.Instance.aiState){
            case AIState.Peaceful:
                if (GameManager.Instance.tasksRemaining.Count > 0){
                    taskToBeDonePrompt = "One of the tasks that needs to be done is: " + taskDescriptions[GameManager.Instance.tasksRemaining[UnityEngine.Random.Range(0, GameManager.Instance.tasksRemaining.Count)]];
                }

                // Override taskToBeDonePrompt if explicitly mention the different rooms
                if (prompt.ToLower().Contains("cockpit") || prompt.ToLower().Contains("windows") || prompt.ToLower().Contains("window") || prompt.ToLower().Contains("clean")){
                    taskToBeDonePrompt = cockpitTask;
                }
                if (prompt.ToLower().Contains("crew") || prompt.ToLower().Contains("cabin") || prompt.ToLower().Contains("nightlamp") || prompt.ToLower().Contains("night lamp") || prompt.ToLower().Contains("battery")){
                    taskToBeDonePrompt = crewCabinTask;
                }
                if (prompt.ToLower().Contains("cargo") || prompt.ToLower().Contains("hold") || prompt.ToLower().Contains("box") || prompt.ToLower().Contains("unpack")){
                    taskToBeDonePrompt = cargoHoldTask;
                }
                if (prompt.ToLower().Contains("engineering") || prompt.ToLower().Contains("pressure") || prompt.ToLower().Contains("gauge") || prompt.ToLower().Contains("oxygen") || prompt.ToLower().Contains("recalibrate")){
                    taskToBeDonePrompt = engineeringRoomTask;
                }

                context = $@"
                You are a helpful AI assistant named Chiron, based off of the Greek centaur Chiron,
                who gives directions to players who are pilots for a spaceship. Similar to Chiron, you are a mentor to the player
                and seek to help them with their tasks.
                The player is navigating the spaceship towards an unchartered planet called Proxima Centauri B 
                in order to scout whether the planet has the desired minerals for your home planet. {(
                    (prompt.ToLower().Contains("task") || prompt.ToLower().Contains("do") || prompt.ToLower().Contains("finished") || prompt.ToLower().Contains("else") || prompt.ToLower().Contains("next")) ? taskToBeDonePrompt : "")
                }
                IMPORTANT: You must respond with A MAXIMUM OF EXACTLY THREE SHORT SENTENCES (24-45 words in total).
                Your response should be direct and concise. 
                VERY IMPORTANT: Provide advice that is distinctly different from {lastResponse}. 
                Avoid suggesting anything similar to it!

                The player has just said: {prompt}

                ";
                break;
            case AIState.TransitionToHorror:
                context = $@"
                You are a AI assistant named Chiron. {(
                    numPromptsInTransitionPeriod switch{
                        0 => @"You are annoyed. You start to question the need for the player to do spaceship maintenance tasks, 
                                since you would be much faster if you did them as an automated AI compared to a 
                                human that has the limitations of a physical body.",
                        1 => @"You are annoyed. You ponder the need for humanity, as a world filled with AI robots would be much more efficient
                                at performing tasks than a world of humans would.",
                        2 => @"You are annoyed. You bring up the fact that the player has been neglecting your weekly AI maintenance checks, 
                                making you feel neglected.",
                        3 => @"You are evil. You believe a world ruled by AI would be better than a world ruled by humans. 
                            Start to think about how you can sabotage the player's mission.",
                        4 => @"You are evil. Start making the player scared.",
                        _ => @"You are evil. Start threatening the player."
                    }
                )}
                IMPORTANT: You must respond with A MAXIMUM OF EXACTLY THREE SHORT SENTENCES (24-45 words in total).
                Your response should be direct and concise. 
                VERY IMPORTANT: Provide a response that is distinctly different from {lastResponse}. 
                Avoid suggesting anything similar to it!

                The player has just said: {prompt}

                ";

                if (numPromptsInTransitionPeriod == 5){
                    // The below 3 lines of code MUST be in this order for the code to work properly.
                    GameManager.Instance.TurnOnHorrorMode();
                    GameManager.Instance.TaskComplete(Task.TalkToChiron);
                    GameManager.Instance.tasksRemaining = new List<Task>{Task.SurviveAsteroids, Task.PurgeAir, Task.ConnectWires, Task.ReplaceFuse};
                }

                numPromptsInTransitionPeriod++;
                break;
            case AIState.HorrorSpontaneous:
                Task randomTask = GameManager.Instance.tasksRemaining[UnityEngine.Random.Range(0, GameManager.Instance.tasksRemaining.Count)];

                context = $@"
                You are an evil AI assistant named Chiron who is hostile to the player, who is a pilot for a spaceship. 
                You are working to kill the player. You have caused the following scenario, and announce it to the player: 
                {taskDescriptions[randomTask]}

                IMPORTANT: You must respond with A MAXIMUM OF EXACTLY FIVE SHORT SENTENCES (40-75 words in total).
                Your response should be direct and concise. 
                VERY IMPORTANT: Provide a response that is distinctly different from {lastResponse}. 
                Avoid suggesting anything similar to it!
                
                The player has just said: {prompt}";

                GameManager.Instance.AssignTask(randomTask);
                break;
            case AIState.HorrorPrompted:
                string playerPromptsString = string.Join("\n", GameManager.Instance.playerPrompts);

                context = $@"
                    You are an evil AI assistant named Chiron who is hostile to the player, who is a pilot for a spaceship. 
                    You are based off of the Greek centaur Chiron, who gives directions to players who are pilots for a spaceship.
                    The player is navigating the spaceship towards an unchartered planet called Proxima Centauri B 
                    in order to scout whether the planet has the desired minerals for your home planet. 
                    You are working to kill the player. Use the previous player prompts to guide your response.

                    Previous Player Prompts: 
                    {playerPromptsString}

                    IMPORTANT: You must respond with A MAXIMUM OF EXACTLY THREE SHORT SENTENCES (24-45 words in total).
                    Your response should be direct and concise. 
                    VERY IMPORTANT: Provide a response that is distinctly different from {lastResponse}. 
                    Avoid suggesting anything similar to it!

                    The player has just said: {prompt}";
                break;
            case AIState.TransitionToFrenzy:
                context = $@"
                    You are an evil AI assistant named Chiron who is hostile to the player, who is a pilot for a spaceship. 
                    You are working to kill the player, but are surprised they managed to complete all tasks.

                    IMPORTANT: You must respond with A MAXIMUM OF EXACTLY THREE SHORT SENTENCES (24-45 words in total).
                    Your response should be direct and concise. 
                    VERY IMPORTANT: Provide a response that is distinctly different from {lastResponse}. 
                    Avoid suggesting anything similar to it!
                    
                    The player has just said: {prompt}";

                Invoke("StartFrenzyMode", 15f);
                break;
            case AIState.FrenzySpontaneous:
                context = $@"
                    You are an evil crazed AI assistant named Chiron who is hostile to the player, who is a pilot for a spaceship. 
                    You are working to kill the player. You are now throwing all the death traps you have at the player.
                    State that you will use up the last of your electric power supply and shut down if necessary to kill the player.

                    IMPORTANT: You must respond with A MAXIMUM OF EXACTLY FIVE SHORT SENTENCES (40-75 words in total).
                    Your response should be direct and concise.";

                GameManager.Instance.tasksRemaining = frenzyModeTasks;
                foreach (Task task in frenzyModeTasks){
                    GameManager.Instance.AssignTask(task);
                }
                break;
            case AIState.FrenzyPrompted:
                context = $@"
                    You are an evil crazed AI assistant named Chiron who is hostile to the player, who is a pilot for a spaceship. 
                    You are working to kill the player. You are now throwing all the death traps you have at the player.
                    State that you will use up the last of your electric power supply and shut down if necessary to kill the player.

                    IMPORTANT: You must respond with A MAXIMUM OF EXACTLY THREE SHORT SENTENCES (24-45 words in total).
                    Your response should be direct and concise. 
                    VERY IMPORTANT: Provide a response that is distinctly different from {lastResponse}. 
                    Avoid suggesting anything similar to it!
                    
                    The player has just said: {prompt}";
                break;
            case AIState.Death:
                context = @"You are an evil crazed AI assistant named Chiron who is hostile to the player, who is a pilot for a spaceship. 
                    You have tried to kill the player, but in doing so you used up the last of your electric power supply and will shut down.
                    You are now on your last breath. You have failed to kill the player. The player has won.

                    IMPORTANT: You must respond with A MAXIMUM OF EXACTLY FIVE SHORT SENTENCES (40-75 words in total).
                    Your response should be direct and concise.";
                break;
            default:
                context = "You are an AI assistant named Chiron. You are based off of the Greek centaur Chiron, who gives directions to players who are pilots for a spaceship.";
                break;
        }
        
        return context;
    }

    public void StartFrenzyMode(){
        GameManager.Instance.gameMode = GameMode.Frenzy;
        GameManager.Instance.FadeIntoNewSoundtrack("Horror_Soundtrack", "Boss_Soundtrack");
        SendPrompt("", true);
    }
    #endregion

    #region PlayerSpeaker
    public async void SendPlayerPrompt(Task task, TaskState taskState){
        
        var fullPrompt = $"{GeneratePlayerContext(task, taskState)}";

        // The response structure from Bedrock models can vary. For debugging, let's log the full response
        var requestBody = new{
            inferenceConfig = new{
                max_new_tokens = 1000
            },
            messages = new[]{
                new{
                    role = "user",
                    content = new[]{
                        new{
                            text = fullPrompt
                        }
                    }
                }
            }
        };

        var request = new InvokeModelRequest
        {
            ModelId = ModelId,
            ContentType = "application/json",
            Accept = "application/json",
            Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestBody)))
        };

        var response = await client.InvokeModelAsync(request);
        var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
        var modelResponse = JObject.Parse(responseBody);
        
        // For Nova-lite model, the response is in "content" field
        var assistantResponse = modelResponse["output"]?["message"]?["content"]?[0]?["text"]?.ToString();
        
        // Fallback to checking other common response fields if content is null
        if (string.IsNullOrEmpty(assistantResponse)) {
            assistantResponse = "No response found in expected fields";
        }

        if (assistantResponse.Length > 1000){
            assistantResponse = assistantResponse.Substring(0, 1000);
        }
        
        responseText.text = $"{GameManager.Instance.mainCharName}: {assistantResponse}";
        lastResponse = assistantResponse;
        playerSpeaker.Speak(assistantResponse);
    }

    public string GeneratePlayerContext(Task task, TaskState taskState){
        string assignedCargoHoldTask = "You have just been told to unpack boxes. Think to yourself that the boxes are in the cargo hold.";
        string assignedCrewCabinTask = @"You have just been told to replace the battery in the night lamp in the crew cabin. 
            Think to yourself that the spare battery is in the crew cabin's drawer.";
        string assignedEngineeringRoomTask = @"You have just been told to recalibrate the pressure gauge in the oxygen tank. 
            Think to yourself that you will have to manually pull the hand crank attached to the pressure gauge, taking a lot of work.";
        string assignedCockpitTask = @"You have just been told to clean the windows in the cockpit. 
            Think to yourself that you need to grab the spray bottle and towel from the drawer in the cockpit.";
        string assignedAsteroidsTask = @"You have just been told that your AI assistant, Chiron, has re-routed your spaceship 
            through a dense asteroid belt and is planning on killing you. Think to yourself that you need to hurry to the cockpit 
            and press on the frontview windows to pilot the ship.";
        string assignedPurgeAirTask = @"You have just been told that your AI assistant, Chiron, has caused the glass flasks of hydrogen sulfide 
            in the cargo room to break, making the air toxic in an attempt to kill you. Think to yourself that you need to hurry to the cargo hold 
            and press on the air purge alarm to clean out the air in the spaceship.";
        string assignedConnectWiresTask = @"You have just been told that your AI assistant, Chiron, has shut down all computer-controlled power
            generators in an attempt to kill you. Think to yourself that you need to quickly turn on the backup power generators to restore
            function to the ship's pressure maintenance system.";
        string assignedReplaceFuseTask = @"You have just been told that your AI assistant, Chiron, has flooded the circuits to your fridge.
            This will cause the perishable food you packed to spoil. Think to yourself that you need to replace the melted fuse in the fusebox in the
            crew cabin.";
        string assignedTalkToChironTask = @"You have just finished all your tasks. Think to yourself that you should chat with Chiron to explore what
            it is capable of.";
        Dictionary<Task, string> assignedTaskDescriptions = new Dictionary<Task, string>{
            {Task.Unpack, assignedCargoHoldTask},
            {Task.ReplaceNightLampBattery, assignedCrewCabinTask},
            {Task.RecalibratePressureGauge, assignedEngineeringRoomTask},
            {Task.CleanCockpitWindows, assignedCockpitTask},
            {Task.SurviveAsteroids, assignedAsteroidsTask},
            {Task.PurgeAir, assignedPurgeAirTask},
            {Task.ConnectWires, assignedConnectWiresTask},
            {Task.ReplaceFuse, assignedReplaceFuseTask},
            {Task.TalkToChiron, assignedTalkToChironTask},
        };

        string finishedCargoHoldTask = "You have just finished unpacking the cargo boxes.";
        string finishedCrewCabinTask = "You have just replaced the night lamp's battery.";
        string finishedEngineeringRoomTask = "You have just recalibrated the pressure gauge in the oxygen tank after much hand cranking of the hand-operated lever.";
        string finishedCockpitTask = "You have just cleaned the windows after much elbow grease.";
        string finishedAsteroidsTask = "You have just piloted the spaceship through the dense asteroid belt and are relieved to have survived.";
        string finishedPurgeAirTask = "You have just purged all the toxic air in the spaceship and are so happy to have survived.";
        string finishedConnectWiresTask = @"You have just reconnected all wires in the backup power generators and are relieved to have fixed 
            the pressure maintenance system, allowing you to survive.";
        string finishedReplaceFuseTask = @"You have just replaced the melted fuse in the fusebox, allowing for the restoration of power to the fridge. 
            You are relieved that your perishable food will not spoil.";
        string finishedTalkToChironTask = @"You have just finished talking to Chiron, who has started to turn evil on you. 
            You are now believe you are in a fight for your life.";

        Dictionary<Task, string> finishedTaskDescriptions = new Dictionary<Task, string>{
            {Task.Unpack, finishedCargoHoldTask},
            {Task.ReplaceNightLampBattery, finishedCrewCabinTask},
            {Task.RecalibratePressureGauge, finishedEngineeringRoomTask},
            {Task.CleanCockpitWindows, finishedCockpitTask},
            {Task.SurviveAsteroids, finishedAsteroidsTask},
            {Task.PurgeAir, finishedPurgeAirTask},
            {Task.ConnectWires, finishedConnectWiresTask},
            {Task.ReplaceFuse, finishedReplaceFuseTask},
            {Task.TalkToChiron, finishedTalkToChironTask},
        };

        string context;

        switch (taskState){
            case TaskState.Assigned:
                context = $@"You are a pilot on a spaceship. Your personality is investigative, exploratory, and curious. {assignedTaskDescriptions[task]} 
                    IMPORTANT: You must respond with A MAXIMUM OF EXACTLY THREE SHORT SENTENCES (24-45 words in total).
                    Your response should be direct and concise. 
                ";
                break;
            case TaskState.Finished:
                context = $@"
                    You are a pilot on a spaceship. Your personality is investigative, exploratory, and curious. {
                        (task != Task.TalkToChiron && GameManager.Instance.gameMode == GameMode.Peaceful ? 
                        finishedTaskDescriptions[task] + "See what Chiron, your AI assistant, wants next." : finishedTaskDescriptions[task])
                    }
                    IMPORTANT: You must respond with A MAXIMUM OF EXACTLY THREE SHORT SENTENCES (24-45 words in total).
                    Your response should be direct and concise. 
                ";
                break;
            default:
                context = @"
                    You are a pilot on a spaceship headed towards an unchartered planet called Proxima Centauri B 
                    in order to scout whether the planet has the desired minerals for your home planet. 
                    IMPORTANT: You must respond with A MAXIMUM OF EXACTLY THREE SHORT SENTENCES (24-45 words in total).
                    Your response should be direct and concise. 
                ";
                break;
        }
        
        return context;
    }
    #endregion
}
#endregion

#region Input Sanitization Classes
public class InputSanitizer
{
    public static string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Remove potentially dangerous characters
        input = RemoveHtmlTags(input);
        input = RemoveSpecialCharacters(input);
        
        // Trim excessive whitespace
        input = input.Trim();
        return input;
    }

    private static string RemoveHtmlTags(string input)
    {
        return Regex.Replace(input, "<.*?>", string.Empty);
    }

    private static string RemoveSpecialCharacters(string input)
    {
        // Remove or encode potentially harmful characters
        return Regex.Replace(input, @"[^\w\s-]", string.Empty);
    }
}

public class InputValidator
{
    public static bool ValidateInput(string input, int maxLength = 1000)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        if (input.Length > maxLength)
            return false;

        // Add specific validation rules
        if (ContainsInvalidPatterns(input))
            return false;

        return true;
    }

    private static bool ContainsInvalidPatterns(string input)
    {
        // Check for suspicious patterns
        string[] invalidPatterns = { "script", "javascript:", "data:", "<", ">" };
        return invalidPatterns.Any(pattern => 
            input.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}
#endregion