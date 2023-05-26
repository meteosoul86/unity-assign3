using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using UnityEngine;

public class SttManager : MonoBehaviour
{
    private VariablesManager _variablesManager; // 변수 저장 클래스
    private string _azureSubscriptionKey; // Azure Speech API 구독 키
    private string _azureServiceRegion; // Azure Speech API 서비스 리전
    private SpeechConfig _config; // Azure Speech SDK Config
    private Gesticulator _gesticulator; // Gesticulator 클래스

    private void Start()
    {
        this._variablesManager = FindObjectOfType<VariablesManager>();
        this._azureSubscriptionKey = this._variablesManager.GetAzureSubscriptionKey();
        this._azureServiceRegion = this._variablesManager.GetAzureServiceRegion();
        
        // 필수값 입력 체크
        if (string.IsNullOrEmpty(this._azureSubscriptionKey) || string.IsNullOrEmpty(this._azureServiceRegion))
        {
            Debug.LogError("Variables에 Azure Speech API 구독 키와 서비스 리전을 입력해주세요.");
        }
        
        // Azure STT
        this._config = SpeechConfig.FromSubscription(this._azureSubscriptionKey, this._azureServiceRegion);
        this._config.SpeechRecognitionLanguage = "en-US"; // 영어로 설정

        this._gesticulator = FindObjectOfType<Gesticulator>();
    }

    /**
     * STT 실행.
     */
    public async void RunStt(string localWavFilePath, AudioClip generatedAudioClip)
    {
        // 필수값 입력 체크
        if (string.IsNullOrEmpty(this._azureSubscriptionKey) || string.IsNullOrEmpty(this._azureServiceRegion))
        {
            Debug.LogError("Variables에 Azure Speech API 구독 키와 서비스 리전을 입력해주세요.");
            return;
        }
        
        // Azure STT 실행
        using var audioInput = AudioConfig.FromWavFileInput(localWavFilePath);
        using var recognizer = new SpeechRecognizer(this._config, audioInput);
        var result = await recognizer.RecognizeOnceAsync();

        // STT가 성공한 경우
        if (result.Reason == ResultReason.RecognizedSpeech) 
        {
            Debug.Log("(2/4) STT 실행 완료.");
            
            // Gesticulator 실행
            this._gesticulator.RunGesticulator(localWavFilePath, result.Text, generatedAudioClip);
        }
        // STT가 성공하지 못한 경우
        else
        {
            Debug.LogError("STT 실패 : " + result.Reason);
        }
    }
}
