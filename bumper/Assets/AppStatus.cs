public static class AppStatus
{
    public static volatile bool isApplicationQuit;
    /// <summary>
    /// <para>对应 Time.realtimeSinceStartup </para>
    /// <para>在UpdateManager.Update中更新</para>
    /// <para>用于多线程无法访问Time.realtimeSinceStartup</para>
    /// </summary>
    /// <param name=""></param>
    /// <param name=""></param>
    /// <returns></returns>
    public static volatile float realtimeSinceStartup;
    public static bool gameInited;
    public static bool resInited;
    public static bool cgFinish;

    public static void Clear()
    {
        resInited = false;
    }
}