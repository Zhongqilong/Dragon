using UnityEngine;

public class InputUtils : MonoBehaviour
{

    public void Update()
    {
        var isBegan = Input.GetMouseButtonDown(0);
        var isEnded = Input.GetMouseButtonUp(0);
        var isDraged = Input.GetMouseButton(0);
        if (isBegan)
        {
            drag = true;
            pos = lastPos = Input.mousePosition;
        }
        if (isDraged)
        {
#if UNITY_EDITOR
            pos = Input.mousePosition;
#else
        if (Input.touchCount != 0) { pos = Input.GetTouch(0).position; }
#endif
        }
        if (isEnded)
        {
            drag = false;
        }

        if (drag)
        {
            Pitch = PitchParam * (pos.y - lastPos.y);
            Yaw = YawParam * (pos.x - lastPos.x);
        }
        lastPos = pos;
    }

    public void LateUpdate()
    {
        Pitch = 0;
        Yaw = 0;
    }

    #region 屏幕操作

    public float Pitch;
    public float Yaw;

    public float PitchParam = 1f;
    public float YawParam = 1f;

    private bool drag;
    private Vector3 pos;
    private Vector3 lastPos;

    #endregion

}