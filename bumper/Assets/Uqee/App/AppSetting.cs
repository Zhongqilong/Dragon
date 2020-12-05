using UnityEngine;
using Uqee.Resource;

public class AppSetting : MonoBehaviourSingleton<AppSetting>
{
    [UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitOnLoad()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        // Screen.autorotateToPortrait = false;
        // Screen.autorotateToPortraitUpsideDown = false;
    }
    public int shaderLodMax = 1500;
    public bool lunarConsole;
    public bool showFps;
    public int defaultQuality = 3;
    public float startTime = 3;
    public float endTime = 2;

    public float attackDelay = 0.1f;
    public float deathDelay = 22;


    protected override void Awake()
    {
        Shader.globalMaximumLOD = shaderLodMax;
    }
}