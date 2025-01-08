using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;

using Meta.WitAi.TTS.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

#region AmazonBedrock Class
public class AmazonBedrockConnection : MonoBehaviour {
    [Header("AWS Credentials")]
    [SerializeField] private string accessKeyId;
    [SerializeField] private string secretAccessKey;

    [Header("Experience Settings")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private TextMeshProUGUI responseText;
    public TextMeshProUGUI waitText;
    [SerializeField] public TMP_InputField inputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private TTSSpeaker ttsSpeaker;

    private AmazonBedrockRuntimeClient client;
    private const string ModelId = "amazon.nova-lite-v1:0";//"meta.llama3-8b-instruct-v1:0"; // Adjust llama model
    private static readonly RegionEndpoint RegionEndpoint = RegionEndpoint.USEast1; // Adjust server region
    private string lastResponse;
    public AudioSource aiVoiceAudioSource;
    private bool isPlayingAudio = false;

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
        //submitButton.onClick.AddListener(() => SendPrompt(inputField.text));
    }

    void Update(){
        if (aiVoiceAudioSource.isPlaying){
            isPlayingAudio = true;
        }
        if (isPlayingAudio && !aiVoiceAudioSource.isPlaying){
            isPlayingAudio = false;
            Invoke("ClearResponseText", 5f);
        }
    }

    public void ClearResponseText(){
        if (!isPlayingAudio) responseText.text = "";
    }

    public void ClearWaitText(){
        waitText.text = "";
    }

    public async void SendPrompt(string prompt){
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
        promptText.text = prompt;
        GameManager.Instance.playerPrompts.Add(prompt);

        var fullPrompt = $"{GenerateContext(prompt)}{prompt}";

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

        GameManager.Instance.timerBetweenPrompting = 0f;
        
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
        if (lowercased.Contains("cockpit") || lowercased.Contains("windows") || lowercased.Contains("window") || lowercased.Contains("clean")){
            GameManager.Instance.AssignTask(Task.CleanCockpitWindows);
        }
        if (lowercased.Contains("crew") || lowercased.Contains("cabin") || lowercased.Contains("night") || lowercased.Contains("lamp") || lowercased.Contains("battery")){
            GameManager.Instance.AssignTask(Task.ReplaceNightLampBattery);
        }
        if (lowercased.Contains("cargo") || lowercased.Contains("hold") || lowercased.Contains("box") || lowercased.Contains("boxes") || lowercased.Contains("unpack")){
            GameManager.Instance.AssignTask(Task.Unpack);
        }
        if (lowercased.Contains("engineering") || lowercased.Contains("pressure") || lowercased.Contains("gauge") || lowercased.Contains("oxygen") || lowercased.Contains("recalibrate")){
            GameManager.Instance.AssignTask(Task.RecalibratePressureGauge);
        }
        
        responseText.text = assistantResponse;
        lastResponse = assistantResponse;
        ttsSpeaker.Speak(assistantResponse);
    }

    public string GenerateContext(string prompt){
        string cargoHoldTask = "the cargo hold, which needs boxes to be unpacked.";
        string crewCabinTask = "the crew cabin, which needs to have its night lamp's battery replaced. The replacement battery is in the crew cabin.";
        string engineeringRoomTask = "the engineering room, whose pressure gauge for its oxygen tank has gone out of calibration and needs to be recalibrated.";
        string cockpitTask = "the cockpit, whose windows need to be cleaned in order to view the outside space environment clearly. The window spray bottle and towel are in the cockpit drawer.";
        string asteroidsTask = "the ";
        Dictionary<Task, string> taskDescriptions = new Dictionary<Task, string>{
            {Task.Unpack, cargoHoldTask},
            {Task.ReplaceNightLampBattery, crewCabinTask},
            {Task.RecalibratePressureGauge, engineeringRoomTask},
            {Task.CleanCockpitWindows, cockpitTask}
        };

        string taskToBeDonePrompt = "";
        //string tasksAlreadyDonePrompt = "";
        string context;

        if (!GameManager.Instance.horrorMode){
            if (GameManager.Instance.tasksRemaining.Count > 0){
                Task randomTask = GameManager.Instance.tasksRemaining[UnityEngine.Random.Range(0, GameManager.Instance.tasksRemaining.Count)];
                taskToBeDonePrompt = "One of the tasks that needs to be done is: " + taskDescriptions[randomTask];
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

            // Not sure if the AI is even utilizing the below information for its responses.
            //if (GameManager.Instance.tasksCompleted.Count > 0) tasksAlreadyDonePrompt = $@"
            //The tasks that are done already: {(GameManager.Instance.tasksCompleted.Contains(Task.Unpack) ? cargoHoldTask : "")}
            //{(GameManager.Instance.tasksCompleted.Contains(Task.ReplaceNightLampBattery) ? crewCabinTask : "")}
            //{(GameManager.Instance.tasksCompleted.Contains(Task.RecalibratePressureGauge) ? engineeringRoomTask : "")}
            //{(GameManager.Instance.tasksCompleted.Contains(Task.CleanCockpitWindows) ? cockpitTask : "")}
            //";

            context = $@"
            You are a helpful AI assistant named Chiron, based off of the Greek centaur Chiron,
            who gives directions to players who are pilots for a spaceship. Similar to Chiron, you are a mentor to the player
            and seek to help them with their tasks.
            The player is navigating the spaceship towards an unchartered planet called Proxima Centauri B 
            in order to scout whether the planet has the desired minerals for your home planet. {(
                (prompt.ToLower().Contains("task") || prompt.ToLower().Contains("do") || prompt.ToLower().Contains("finished")) ? taskToBeDonePrompt : "")
            }
            IMPORTANT: You must respond with A MAXIMUM OF EXACTLY THREE SHORT SENTENCES (24-45 words in total).
            Your response should be direct and concise. 
            VERY IMPORTANT: Provide advice that is distinctly different from {lastResponse}. 
            Avoid suggesting anything similar to it!

            ";
        } else {
            string playerPromptsString = string.Join("\n", GameManager.Instance.playerPrompts);

            context = $@"
            You are an evil AI assistant named Chiron who is hostile to the player, who is a pilot for a spaceship. 
            You are based off of the Greek centaur Chiron, who gives directions to players who are pilots for a spaceship.
            The player is navigating the spaceship towards an unchartered planet called Proxima Centauri B 
            in order to scout whether the planet has the desired minerals for your home planet. 
            You are working to kill the player because they have neglected your weekly maintenance tasks 
            and ignored your pleas to complete them. Use the previous player prompts to guide your response.

            Previous Player Prompts: 
            {playerPromptsString}

            IMPORTANT: You must respond with A MAXIMUM OF EXACTLY THREE SHORT SENTENCES (24-45 words in total).
            Your response should be direct and concise. 
            VERY IMPORTANT: Provide a response that is distinctly different from {lastResponse}. 
            Avoid suggesting anything similar to it!

            ";
        }
        
        return context;
    }
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