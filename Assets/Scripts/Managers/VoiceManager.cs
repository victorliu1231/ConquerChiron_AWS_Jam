using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Oculus.Voice;
using System.Reflection;
using Meta.WitAi.CallbackHandlers;

public class VoiceManager : MonoBehaviour
{
    [Header("Wit Configuration")]
    public AppVoiceExperience appVoiceExperience;
    public WitResponseMatcher responseMatcher;
    public TextMeshProUGUI transcriptionText;

    [Header("Voice Events")]
    public AmazonBedrockConnection amazonBedrockConnection;
    public UnityEvent utteranceDetected;
    public UnityEvent<string> completeTranscription;

    private bool _voiceCommandReady;

    private void Awake()
    {
        appVoiceExperience.VoiceEvents.OnRequestCompleted.AddListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);

        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(responseMatcher) is MultiValueEvent onMultiValueEvent)
        {
            onMultiValueEvent.AddListener(UtteranceDetected);
        }
    }

    private void OnDestroy(){
        appVoiceExperience.VoiceEvents.OnRequestCompleted.RemoveListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscription);

        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(responseMatcher) is MultiValueEvent onMultiValueEvent)
        {
            onMultiValueEvent.RemoveListener(UtteranceDetected);
        } 
    }

    private void ReactivateVoice() => appVoiceExperience.Activate();

    private void UtteranceDetected(string[] arg0){
        _voiceCommandReady = true;
        utteranceDetected?.Invoke();
    }

    private void OnPartialTranscription(string transcription){
        if (!_voiceCommandReady) return;
        transcriptionText.text = transcription;
    }

    private void OnFullTranscription(string transcription){
        if (!_voiceCommandReady) return;
        _voiceCommandReady = false;
        completeTranscription?.Invoke(transcription);
        amazonBedrockConnection.SendPrompt(transcription);
    }
}