using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ImageProgressItem : MonoBehaviour
{
    private RectTransform _rect_progress;
    private float _widthMax = 0;
    public void SetWidthMax(float width)
    {
        
        _widthMax = width;
    }
    public void SetProgress(float progress) {
        if (progress < 0) progress = 0;
        if (progress > 1) progress = 1;
        if (_widthMax == 0) {
            _widthMax = _rect_progress.transform.parent.GetComponent<RectTransform>().GetWidth();
            Uqee.Debug.LogWarning("未设置 progress进度条的最大宽度 暂取父节点（背景）宽度");
        }

        var img_process = transform.Find("img_progress");
        if (img_process == null)
        {
            Uqee.Debug.LogError("子节点未找到 img_progress 请改名或者添加");
            return;
        }
        _rect_progress = img_process.GetComponent<RectTransform>();
        var width = progress * _widthMax;
        _rect_progress.sizeDelta = new Vector2(width, _rect_progress.sizeDelta.y);
    }
   
}
