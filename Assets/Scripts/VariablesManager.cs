using UnityEngine;

public class VariablesManager : MonoBehaviour
{
    [SerializeField] private string azureSubscriptionKey; // Azure Speech API 구독 키
    [SerializeField] private string azureServiceRegion; // Azure Speech API 서비스 리전
    [SerializeField] private string unityProjectPath; // 유니티 프로젝트 경로
    [SerializeField] private string pyVenvPath; // 파이썬 가상환경 경로
    [SerializeField] private string pyDllFileName; // 파이썬 가상환경 파이썬 DLL 파일 이름

    /* 이하 Getter */
    public string GetAzureSubscriptionKey()
    {
        return this.azureSubscriptionKey;
    }

    public string GetAzureServiceRegion()
    {
        return this.azureServiceRegion;
    }
    
    public string GetUnityProjectPath()
    {
        return this.unityProjectPath;
    }
    
    public string GetPyVenvPath()
    {
        return this.pyVenvPath;
    }
    
    public string GetPyDllFileName()
    {
        return this.pyDllFileName;
    }
}
