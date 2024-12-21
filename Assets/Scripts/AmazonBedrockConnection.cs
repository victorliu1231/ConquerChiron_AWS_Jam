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

    [Header("User Prompt Settings")]
    [SerializeField] private string userPrompt = "Answer in one sentence please:";

    private AmazonBedrockRuntimeClient client;
    private const string ModelId = "meta.llama3-8b-instruct-v1:0"; // Adjust llama model
    private static readonly RegionEndpoint RegionEndpoint = RegionEndpoint.USEast1; // Adjust server region

    private void Awake(){
        // var credentials = new BasicAWSCredentials(
        //     Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
        //     Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
        // );
        var credentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);
        client = new AmazonBedrockRuntimeClient(credentials, RegionEndpoint);

        submitButton.onClick.AddListener(() => SendPrompt(inputField.text));
    }

    public async void SendPrompt(string prompt){
        if (!InputValidator.ValidateInput(prompt)){
            responseText.text = "Invalid input detected";
            return;
        }
        prompt = InputSanitizer.SanitizeInput(prompt);
        promptText.text = $"User: {prompt}";
        var fullPrompt = $"user\n{userPrompt} {prompt}\n\nassistant\n";

        var request = new InvokeModelRequest(){
            ModelId = ModelId,
            Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new {
                prompt = fullPrompt,
                max_gen_len = 512,
                temperature = 0.5,
            }))),
            ContentType = "application/json",
        };

        var response = await client.InvokeModelAsync(request);
        var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
        var modelResponse = JObject.Parse(responseBody);

        var assistantResponse = modelResponse["generation"]?.ToString();
        responseText.text = assistantResponse;
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