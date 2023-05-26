using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MicHandler : MonoBehaviour
{
    // UI
    [SerializeField] private Button micBtn; // 마이크 버튼 오브젝트
    
    // 입력
    private string _deviceName; // 마이크 디바이스 이름
    private AudioClip _mic; // 마이크 AudioClip
    private int _micChannelCount; // 마이크 채널 수
    private readonly int _sampleRate = 44100; // 샘플링 주기 - 1초에 SampleRate만큼 
    
    // 읽기
    private bool _isReading; // 읽기 진행중 여부
    private int _readingEndSamplePos; // 읽기 완료된 샘플링 위치
    private float[] _sampleDatas; // 샘플링 데이터
    private float[] _collectedSampleDatas; // 누적 샘플링 데이터
    
    // 저장
    private bool _isSaving; // 저장 진행중 여부
    private SttManager _sttManager; // STT 클래스
    private AvatarHandler _avatarHandler; // 아바타 클래스
    
    private void Start()
    {
        // UI
        this.micBtn.onClick.AddListener(MicTurnOnOff);
        
        // 입력
        this._deviceName = Microphone.devices[0]; // 첫 번째 마이크 사용
        // Debug.Log("deviceName : " + this._deviceName);
        this._mic = Microphone.Start(this._deviceName, true, 1, this._sampleRate);
        this._micChannelCount = this._mic.channels;
        
        // 읽기
        this._isReading = false; // 읽기 진행중 여부
        this._readingEndSamplePos = 0; // 읽기 완료된 샘플링 위치
        this._sampleDatas = null; // 샘플링 데이터
        this._collectedSampleDatas = Array.Empty<float>();

        // 저장
        this._isSaving = false;
        this._sttManager = FindObjectOfType<SttManager>();
        this._avatarHandler = FindObjectOfType<AvatarHandler>();
    }

    private void Update()
    {
        // 저장 진행중이거나 아바타가 재생중인 경우
        if (this._isSaving || this._avatarHandler.GetisPlaying()) return;
        
        // 읽기 진행중인 경우
        if (this._isReading)
        {
            // 매 프레임마다 마이크 입력 읽기
            ReadMicInput();
        }
        // 읽기 진행중이 아닌 경우
        else
        {
            // 누적 샘플링 데이터에 값이 있는 경우
            if (this._collectedSampleDatas.Length <= 0) return;
            
            // 저장 실행
            this._isSaving = true;
            this.SaveToWav();
        }
    }
    
    /**
     * 마이크 ON/OFF.
     */
    private void MicTurnOnOff()
    {
        // 아바타가 재생중인 경우
        if (this._avatarHandler.GetisPlaying()) return;
        
        // 읽기 진행중이 아닌 경우
        if (!this._isReading)
        {
            this._isReading = true;
            micBtn.GetComponentInChildren<TextMeshProUGUI>().text = "MIC OFF";
        }
        // 읽기 진행중인 경우
        else
        {
            this._isReading = false;
            micBtn.GetComponentInChildren<TextMeshProUGUI>().text = "MIC ON";
        }
    }

    /**
     * 마이크 입력 읽기.
     */
    private void ReadMicInput()
    {
        // 현재 샘플링 위치
        var currentSamplingPos = Microphone.GetPosition(this._deviceName);
        // Debug.Log("currentSamplingPos : " + currentSamplingPos);
        // Debug.Log("currentSamplingPos : " + Math.Round(currentSamplingPos / (double) SampleRate, 1) + "s");

        // 현재 샘플링 위치가 읽기 완료된 샘플링 위치 이후인 경우
        var diff = currentSamplingPos - this._readingEndSamplePos;
        if (diff > 0)
        {
            // 샘플링 데이터 배열 크기 설정
            this._sampleDatas = new float[diff * this._micChannelCount];
            // 샘플링 데이터에 읽기 완료된 샘플링 위치 이후의 데이터 전부 삽입
            this._mic.GetData(this._sampleDatas, this._readingEndSamplePos);
            
            /* 샘플링 데이터 누적 시작 */
            var oldLeng = this._collectedSampleDatas.Length;
            var addLeng = this._sampleDatas.Length;
            var newLeng = oldLeng + addLeng;
            
            // 기존에 누적된 샘플링 데이터가 있는 경우
            if (oldLeng > 0)
            {
                // 기존에 누적된 샘플링 데이터를 oldSampleDatas에 임시로 복사
                var oldSampleDatas = new float[oldLeng];
                this._collectedSampleDatas.CopyTo(oldSampleDatas, 0);
            
                // _collectedSampleDatas를 새로 선언 + oldSampleDatas의 샘플링 데이터 삽입
                this._collectedSampleDatas = new float[newLeng];
                for (var i = 0; i < oldLeng; i++)
                {
                    this._collectedSampleDatas[i] = oldSampleDatas[i];
                }
            }
            // 기존에 누적된 샘플링 데이터가 없는 경우
            else
            {
                // _collectedSampleDatas를 새로 선언
                this._collectedSampleDatas = new float[newLeng];
            }

            // GetData로 얻은 샘플링 데이터 삽입
            for (var j = oldLeng; j < newLeng; j++)
            {
                this._collectedSampleDatas[j] = this._sampleDatas[j - oldLeng];
            }
            /* 샘플링 데이터 누적 끝 */
        }

        // 현재 샘플링 위치를 읽기 완료된 샘플링 위치로 저장
        this._readingEndSamplePos = currentSamplingPos;
    }

    /**
     * wav 파일로 저장.
     */
    private void SaveToWav()
    {
        // 누적 샘플링 데이터를 AudioClip으로 생성
        var audioClip = AudioClip.Create("Mic_Recording", this._collectedSampleDatas.Length,
            this._micChannelCount, this._sampleRate, false);
        audioClip.SetData(this._collectedSampleDatas, 0);
                
        // wav 파일 저장
        var wavFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".wav";
        SavWav.Save(wavFileName, audioClip);
        
        // AudioClip 동적 생성
        var generatedAudioClip = AudioClip.Create("Mic_Recording", this._collectedSampleDatas.Length,
            this._micChannelCount, this._sampleRate, false);
        generatedAudioClip.SetData(this._collectedSampleDatas, 0);
        
        Debug.Log("(1/4) 마이크 입력 데이터 생성 완료.");
        
        // STT 실행
        var localWavFilePath = Path.Combine(Application.persistentDataPath, wavFileName);
        _sttManager.RunStt(localWavFilePath, generatedAudioClip);

        // 누적 샘플링 데이터 초기화
        this._collectedSampleDatas = Array.Empty<float>();

        // 저장 진행중 false로 설정
        this._isSaving = false;
    }
}
