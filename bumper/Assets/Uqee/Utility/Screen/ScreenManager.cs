using System;
using UnityEngine;
using UnityEngine.UI;
using Uqee.Resource;

public class ScreenManager : Singleton<ScreenManager>
{
    /// <summary>
    /// 是否刘海屏
    /// </summary>
    public bool hasNorth { get; private set; }
    /// <summary>
    /// 刘海屏显示区域偏移量
    /// </summary>
    public int safeAreaOffsetX = 0;
    /// <summary>
    /// 是否需要裁剪。屏幕比例小于16:9，高度需要裁剪
    /// </summary>
    public bool isCutMode = false;
    /// <summary>
    /// 裁剪相机的范围
    /// </summary>
    public Rect caRect = new Rect(0, 0, 1, 1);
    private float _widthScale = 1;

    protected void _FixFullScreen()
    {
        //刘海屏适配
#if !UNITY_EDITOR
#if UNITY_IPHONE
        // 维基百科中有最新的iPhone设备model表可查看：
        // https://www.theiphonewiki.com/wiki/Models
        // iPhoneX:"iPhone10,3","iPhone10,6"  iPhoneXR:"iPhone11,8"  iPhoneXS:"iPhone11,2"  iPhoneXS Max:"iPhone11,6"
        string modelStr = SystemInfo.deviceModel;
        int model = 0;
        if (modelStr.Equals("iPhone10,3") || modelStr.Equals("iPhone10,6"))
        {
            model = 11;
        }
        else
        {
            int.TryParse(modelStr.Substring(6, 2), out model);
        }
        if(model>=11)
        {
            ActiveNorthFace();
        }
#elif UNITY_ANDROID
        // if(SDKUtils.HasNorthFace())
        // {
        //     ActiveNorthFace();
        // }
#endif
#endif
        FixCameraRect(UIManager.I.cam_UICam);
    }

    public void ActiveNorthFace()
    {
        hasNorth = true;
        UpdateSaveArea();

        FixTransform(ViewCreator.I.transform as RectTransform);
    }

    /// <summary>
    /// 检查 CanvasScaler 比例，小于16:9 的进行模式切换
    /// </summary>
    /// <param name="canvasScale"></param>
    public void Fix(CanvasScaler canvasScale)
    {
        // float screenRate = (float)Screen.width / (float)Screen.height;
        // if (screenRate < 1.75f)
        // {
        //     //小于16:9的，留上下黑边，不然UI需要重做
        //     var rate = canvasScale.referenceResolution.y / ((canvasScale.referenceResolution.x / Screen.width) * Screen.height);
        //     isCutMode = true;
        //     caRect = new Rect(0, (1 - rate) * 0.5f, 1, rate);
        //     canvasScale.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        // }
        // _widthScale = Screen.width / canvasScale.referenceResolution.x;
        _FixFullScreen();
    }
    /// <summary>
    /// 对自己创建的相像进行显示区域的处理。
    /// 16:9以下比例，高度会裁剪掉一部分
    /// </summary>
    /// <param name="camera"></param>
    public void FixCameraRect(Camera camera)
    {
        if (isCutMode)
        {
            camera.rect = caRect;
        }
    }

    #region 刘海屏设置
    public void UpdateSaveArea()
    {
        if (hasNorth)
        {
            var h = Screen.safeArea.x;
#if !UNITY_EDITOR
#if UNITY_ANDROID
            //h = SDKUtils.GetNorthHeight();
            //UnityEngine.Debug.Log($"northWidth={SDKUtils.GetNorthWidth()}, northHeight={SDKUtils.GetNorthHeight()}, + safeArea: {Screen.safeArea.x}, scale={_widthScale}");
#else
#endif
#endif
            safeAreaOffsetX = (int)(h / _widthScale + 0.5f);

            if (safeAreaOffsetX < 44)
            {
                safeAreaOffsetX = 44;
            }
        }
    }
    public void FixTransform(RectTransform rect)
    {
        if (hasNorth)
        {
            //设置刘海屏全屏
            rect.offsetMin = new Vector2(safeAreaOffsetX, 0);
            rect.offsetMax = new Vector2(-safeAreaOffsetX, 0);
        }

    }
#endregion
}