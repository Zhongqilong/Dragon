using UnityEngine;

public class DragUpdatableScrollRect : UnityEngine.UI.ScrollRect
{
    public void UpdateDragPoint(float v)
    {
        m_ContentStartPosition = new Vector2(0,v);
    }
}
