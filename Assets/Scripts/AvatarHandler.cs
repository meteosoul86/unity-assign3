using System;
using System.Collections;
using System.IO;
using UnityEngine;

// JSON 데이터를 담을 구조체 또는 클래스 정의
[Serializable]
public class GesticulationData
{
    public string xRotationCsvPath;
    public string yRotationCsvPath;
    public string zRotationCsvPath;
    public int frameRate;
    public int frameCount;
}

public class AvatarHandler : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource; // wav 재생 AudioSource
    private bool _isPlaying = false; // wav 재생중 여부
    private bool _isWavPlaying = false; // wav 재생중 여부
    private bool _isGesturePlaying = false; // 제스처 재생중 여부
    
    // 제스처 데이터
    private const int JointCount = 15;
    private float[,,] _jointAngles;
    private int _frameCount;
    private int _currentFrame;
    private int _motionFramerate;
    private string[] _jointNames;
    private Transform[] _jointTransforms;
    private Quaternion[] _defaultRotations;
    private float _deltaTime = 0;
    private Quaternion[] _interpolationTargetPose;

    private void Awake()
    {
        this._jointNames = GetJointNames();
        this._jointTransforms = new Transform[JointCount];
        this._defaultRotations = new Quaternion[JointCount];
        this._interpolationTargetPose = new Quaternion[JointCount];
        this.SetDefaultPose();
    }

    /**
     * Joint 이름.
     */
    private static string[] GetJointNames()
    {
        return new string[]
        {
            "mixamorig:Hips/mixamorig:Spine",
            "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1",
            "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2",
            "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck", // 임시 Joint, Gesticualtor의 Spine3가 아바타에 없음
            "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck", 
            "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head", // 임시 Joint, Gesticualtor의 Neck2가 아바타에 없음
            "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head",
            "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder",
            "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm",
            "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm",
            "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand",
            "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder",
            "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm",
            "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm",
            "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand"
        };
    }

    /**
     * 기본 포즈 세팅.
     */
    private void SetDefaultPose()
    {
        this._jointTransforms = new Transform[JointCount];
        this._defaultRotations = new Quaternion[JointCount];

        for (var i = 0; i < JointCount; i++)
        {
            this._jointTransforms[i] = this.transform.Find(this._jointNames[i]);
            this._defaultRotations[i] = this._jointTransforms[i].localRotation;
        }
    }
    
    /**
     * 아바타 재생.
     */
    public void PlayAvatar(AudioClip generatedAudioClip, string geticulatorJson, string localWavFilePath)
    {
        // 아바타가 재생중인 경우
        if (this.GetisPlaying()) return;

        this._isPlaying = true;
        
        // JSON 데이터를 Unity 데이터 구조로 변환
        var geticulatorData = JsonUtility.FromJson<GesticulationData>(geticulatorJson);
        
        // 제스처 데이터 세팅
        this._motionFramerate = geticulatorData.frameRate;
        this._frameCount = geticulatorData.frameCount;
        this._jointAngles = new float[3, JointCount, this._frameCount];
        this.SetJointAngles(geticulatorData);
            
        // 스테레오 출력으로 믹싱 설정
        this.audioSource.spatialBlend = 0;

        // Audio Source에 AudioClip 동적 생성
        this.audioSource.clip = generatedAudioClip;
        
        // wav, 제스처 재생 실행
        Debug.Log("(4/4) 아바타 실행 시작.");
        StartCoroutine(this.PlayAudio(localWavFilePath));
        StartCoroutine(this.PlayGesture());
        
        this._isPlaying = false;
    }

    /**
     * CSV에 저장된 Gesticulator가 생성한 Joint 모션 데이터 로드.
     */
    private void SetJointAngles(GesticulationData gesticulationData)
    {   
        var csvPaths = new string[] {
            gesticulationData.xRotationCsvPath,
            gesticulationData.yRotationCsvPath,
            gesticulationData.zRotationCsvPath    
        };
        
        for (var axis = 0; axis < 3; axis++)
        {
            using var reader = new StreamReader(csvPaths[axis]);
            for (var frame = 0; frame < this._frameCount; frame++)
            {
                var line = reader.ReadLine();
                if (line != null)
                {
                    var values = line.Split(',');

                    for (int jointIdx = 0; jointIdx < JointCount; jointIdx++)
                    {
                        this._jointAngles[axis, jointIdx, frame] = float.Parse(values[jointIdx]);
                    }
                }
            }
        }
        
        // CSV 파일 삭제
        File.Delete(gesticulationData.xRotationCsvPath);
        File.Delete(gesticulationData.yRotationCsvPath);
        File.Delete(gesticulationData.zRotationCsvPath);
    }

    /**
     * 오디오 재생.
     */
    private IEnumerator PlayAudio(string localWavFilePath)
    {
        this._isWavPlaying = true;
            
        // 재생 실행
        this.audioSource.Play();
        
        // 재생이 끝날 때까지 대기
        yield return new WaitForSeconds(this.audioSource.clip.length);
        
        // wav 파일 삭제
        File.Delete(localWavFilePath);
        
        this._isWavPlaying = false;
    }
        
    /**
     * 제스처 재생.
     */
    private IEnumerator PlayGesture()
    {
        this._isGesturePlaying = true;
        
        // 제스처 시작
        this.StartGesture();

        yield return new WaitForSeconds(0.5f);

        // Joint 각도 업데이트
        float freq = 1f / this._motionFramerate;
        InvokeRepeating(nameof(UpdateJointAngles), 0, freq);
        
        this._isGesturePlaying = false;
    }

    /**
     * 제스처 시작.
     */
    private void StartGesture()
    {
        for (int jointIdx = 0; jointIdx < JointCount; jointIdx++)
        {
            var x = this._jointAngles[0, jointIdx, 0];
            var y = this._jointAngles[1, jointIdx, 0];
            var z = this._jointAngles[2, jointIdx, 0];

            var rot = Quaternion.Euler(-x, y, z);
            rot.w *= -1;

            this._interpolationTargetPose[jointIdx] = rot;
        }

        InvokeRepeating(nameof(InterpolateJointAngles), 0.0f, 0.05f);
    }

    /**
     * 각 Joint 각도를 프레임 단위로 업데이트.
     */
    private void UpdateJointAngles()
    {
        for (var jointIdx = 0; jointIdx < JointCount; jointIdx++)
        {
            var x = this._jointAngles[0, jointIdx, this._currentFrame];
            var y = this._jointAngles[1, jointIdx, this._currentFrame];
            var z = this._jointAngles[2, jointIdx, this._currentFrame];
            
            var rot = Quaternion.Euler(-x, y, z);
            rot.w *= -1;

            var jointTransform = transform.Find(this._jointNames[jointIdx]);
            jointTransform.localRotation = rot;
        }

        if (++this._currentFrame == this._frameCount)
        {
            this._currentFrame = 0;
            CancelInvoke(nameof(UpdateJointAngles));
            this._defaultRotations.CopyTo(this._interpolationTargetPose, 0);
            InvokeRepeating(nameof(InterpolateJointAngles), 0.0f, 0.05f);
        }
    }

    /**
     * 한 프레임씩 interpolationTargetPose에 저장된 포즈(회전) 쪽으로 보간.
     */
    private void InterpolateJointAngles()
    {
        this._deltaTime += 0.05f;

        for (var i = 0; i < JointCount; i++)
        {
            this._jointTransforms[i].localRotation = Quaternion.Slerp(
                this._jointTransforms[i].localRotation, 
                this._interpolationTargetPose[i], 
                this._deltaTime
            );
        }
        
        if (Math.Abs(this._deltaTime - 0.5f) < 1E-5f)
        {
            this._deltaTime = 0.0f;
            CancelInvoke(nameof(InterpolateJointAngles));
        }
    }

    /**
     * 아바타 재생 상태.
     */
    public bool GetisPlaying()
    {
        return this._isPlaying || this._isWavPlaying || this._isGesturePlaying;
    }
}
