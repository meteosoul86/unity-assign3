using System;
using System.IO;
using UnityEngine;
using Python.Runtime;

public class Gesticulator : MonoBehaviour
{
    private VariablesManager _variablesManager; // 변수 저장 클래스
    private AvatarHandler _avatarHandler; // 아바타 클래스
    private string _gesticulationJson; // Gesticulation 실행 결과

    private void Start()
    {
        this._variablesManager = FindObjectOfType<VariablesManager>();
        this._avatarHandler = FindObjectOfType<AvatarHandler>();
    }

    /**
     * Gesticulator 실행.
     */
    public void RunGesticulator(string localWavFilePath, string inputText, AudioClip generatedAudioClip)
    {
        // Pythonnet 세팅
        var pyVenvPath = this._variablesManager.GetPyVenvPath();
        var pyDllFile = this._variablesManager.GetPyDllFileName();
        var unityProjectPath = this._variablesManager.GetUnityProjectPath();
        
        Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pyVenvPath + "\\" + pyDllFile, EnvironmentVariableTarget.Process);
        PythonEngine.PythonHome = pyVenvPath;
        var pythonPath = string.Join(
            Path.PathSeparator.ToString(),
            new string[] {
                pyVenvPath + "\\Lib\\site-packages",
                pyVenvPath + "\\Lib",
                pyVenvPath + "\\DLLs",
                unityProjectPath + "\\Assets\\gesticulator\\gesticulator\\visualization",
                unityProjectPath + "\\Assets\\gesticulator"
            }
        );
        PythonEngine.PythonPath = pythonPath;
        
        // Gesticulatrion 실행
        PythonEngine.Initialize();
        using (Py.GIL())
        {
            dynamic demo = Py.Import("demo.demo_custom");
            dynamic geticulatorJson = demo.main(localWavFilePath, inputText, Path.Combine(Application.persistentDataPath));
            
            // Gesticulatrion 실행 결과
            this._gesticulationJson = (string) geticulatorJson;
        }
        PythonEngine.Shutdown();
        
        Debug.Log("(3/4) Gesticulation 실행 완료.");
        
        // 아바타 재생
        this._avatarHandler.PlayAvatar(generatedAudioClip, this._gesticulationJson, localWavFilePath);
    }
}
