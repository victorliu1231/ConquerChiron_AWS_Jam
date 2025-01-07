using System;
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
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private TTSSpeaker ttsSpeaker;

    private AmazonBedrockRuntimeClient client;
    private const string ModelId = "amazon.nova-lite-v1:0";//"meta.llama3-8b-instruct-v1:0"; // Adjust llama model
    private static readonly RegionEndpoint RegionEndpoint = RegionEndpoint.USEast1; // Adjust server region
    private string lastResponse;
    public string context;

    private void Awake(){
        responseText.text = "";
        // var credentials = new BasicAWSCredentials(
        //     Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
        //     Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
        // );
        var credentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);
        client = new AmazonBedrockRuntimeClient(credentials, RegionEndpoint);

        inputField.onSubmit.AddListener((string prompt) => SendPrompt(prompt));
        //submitButton.onClick.AddListener(() => SendPrompt(inputField.text));
    }

    public async void SendPrompt(string prompt){
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
        var fullPrompt = $"{context}{prompt}";

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
        responseText.text = assistantResponse;
        lastResponse = assistantResponse;
        ttsSpeaker.Speak(assistantResponse);
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