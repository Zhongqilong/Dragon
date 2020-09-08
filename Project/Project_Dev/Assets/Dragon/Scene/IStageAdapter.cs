using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IStageAdapter 
{
    Camera GetCamera(Scene scene, string sceneName);
    void InitScene(Scene scene, string sceneName, Camera camera);
    void LoadLod(string sceneName);
    void OnSceneUnload(string sceneName);
    void OnSceneUnload(List<string> sceneName);
    void OnSceneLoad(string sceneName);
    void OnEmptySceneLoaded();
    void OnAllSceneLoaded();
    void BeginLoad(string sceneName);
    void EndLoad(string sceneName);
}