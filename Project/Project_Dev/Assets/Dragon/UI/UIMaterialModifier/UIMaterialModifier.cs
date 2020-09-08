using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UIMaterialModifier : MonoBehaviour
{
    private CommandBuffer zModifierBuff;
    private int ZModifier_KEY = Shader.PropertyToID("unity_GUIZTestMode");
    [SerializeField]private CompareFunction ztest = UnityEngine.Rendering.CompareFunction.LessEqual;
    public bool IsStated { get; private set; }
    public Camera _camera {get;private set;}

    public static UIMaterialModifier AddToCamera(Camera camera, CompareFunction ztest = UnityEngine.Rendering.CompareFunction.LessEqual)
    {
        var modifer = camera.gameObject.GetOrAddComponent<UIMaterialModifier>();
        modifer.ztest = ztest;
        modifer._camera = camera;
        if (modifer.IsStated)
        {
            modifer.ApplyModify();
        }
        return modifer;
    }

    private void Start()
    {
        ApplyModify();
        IsStated = true;
    }

    private void ApplyModify()
    {
        if (zModifierBuff == null)
        {
            zModifierBuff = new CommandBuffer();
        }

        if (_camera == null)
        {
            _camera = GetComponent<Camera>();
        }
        zModifierBuff.SetGlobalFloat(ZModifier_KEY, (int)ztest);
        _camera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, zModifierBuff) ;
        _camera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha,zModifierBuff);
    }

    private void OnDestroy()
    {
        if(_camera!=null && zModifierBuff != null)
        {
            _camera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, zModifierBuff);
            zModifierBuff = null;
        }
    } 

    [ContextMenu("APPLY")]
    private void test_Apply()
    {
        ApplyModify();
    }
}
