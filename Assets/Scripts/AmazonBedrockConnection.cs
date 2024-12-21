using System.IO;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;

using Meta.WitAi.TTS.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
        var credentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);
        client = new AmazonBedrockRuntimeClient(credentials, RegionEndpoint);

        submitButton.onClick.AddListener(() => SendPrompt(inputField.text));
    }

    public async void SendPrompt(string prompt){
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