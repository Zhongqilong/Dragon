using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.U2D;
using Uqee.Resource;
using Uqee.Utility;
using Uqee.Pool;

public static class ResManager
{
    public static bool isValid { get; private set; }

    private static bool _fastLoad;
    public static void SetFastLoad(bool val)
    {
        if (val)
        {
            Application.backgroundLoadingPriority = ThreadPriority.High;
            QualitySettings.asyncUploadBufferSize = 32;
            QualitySettings.asyncUploadTimeSlice = 8;
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 60;
        }
        else
        {
            Application.backgroundLoadingPriority = ThreadPriority.Normal;
            QualitySettings.asyncUploadBufferSize = 16;
            QualitySettings.asyncUploadTimeSlice = 4;
            QualitySettings.vSyncCount = 2;
            Application.targetFrameRate = 30;
        }
        _fastLoad = val;
        ResourceProcessorManager.I.SetFastLoad(val);
    }
}