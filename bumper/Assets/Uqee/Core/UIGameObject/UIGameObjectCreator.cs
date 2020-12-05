using UnityEngine;
using UnityEngine.UI;

namespace Uqee.Resource {
    /// <summary>
    /// UI层的GameObject（UI相机可见的所有UI）
    /// </summary>
    public class UIGameObjectCreator : AbstractUIGameObjectCreator<UIGameObjectCreator> {
        public UIGameObjectCreator () {
            _InitRoot ("UIRoot");
            //设置整体缩放
            var canvas = gameObject.GetOrAddComponent<Canvas> ();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

            var canvasScale = gameObject.GetComponent<CanvasScaler> ();
            if (canvasScale == null) {
                canvasScale = gameObject.AddComponent<CanvasScaler> ();
                canvasScale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScale.referencePixelsPerUnit = 100;
                canvasScale.referenceResolution = new Vector2 (750, 1334);
                canvasScale.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScale.matchWidthOrHeight = 1;
            }

            gameObject.GetOrAddComponent<GraphicRaycaster> ();
            transform = gameObject.transform;
        }
    }
}