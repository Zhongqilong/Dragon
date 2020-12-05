using UnityEngine;
/// <summary>
/// 让对象在刘海屏模式下显示全屏
/// </summary>
public class FixIPhoneXFullScreen : MonoBehaviour 
{
	public bool ignore;
	void Awake () {
		if (ScreenManager.I!=null && ScreenManager.I.hasNorth && !ignore) {
			var rectTransform = transform as RectTransform;
            if(rectTransform==null)
            {
                return;
            }
            rectTransform.offsetMin = new Vector2(rectTransform .offsetMin.x - ScreenManager.I.safeAreaOffsetX, rectTransform.offsetMin.y);
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x + ScreenManager.I.safeAreaOffsetX, rectTransform.offsetMax.y);
		}
	}

}