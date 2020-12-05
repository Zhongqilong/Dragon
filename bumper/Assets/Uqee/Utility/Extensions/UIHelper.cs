using UnityEngine;
using UnityEngine.UI;

class UIHelper
{
    public static Bounds CalculateBounds(Transform transform)
    {
        LayoutElement layoutElement = transform.GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            if (layoutElement.flexibleWidth > 0 || layoutElement.flexibleHeight > 0)
            {
                Bounds bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(layoutElement.flexibleWidth, layoutElement.flexibleHeight, 0));
                return bounds;
            }
        }

        RectTransform[] transArray = transform.GetComponentsInChildren<RectTransform>();
        if (transArray.Length == 0)
        {
            return new Bounds(Vector3.zero, Vector3.zero);
        }
        //calculate the rect
        int count = 0;
        Vector3 min, max;
        float xMin = float.PositiveInfinity, xMax = float.NegativeInfinity, yMin = float.PositiveInfinity, yMax = float.NegativeInfinity;
        foreach (RectTransform trans in transArray)
        {
            Graphic graphic = trans.gameObject.GetComponent<Graphic>();
            if (!trans.gameObject.activeSelf || graphic == null || graphic.color.a == 0)
                continue;
            min = trans.TransformPoint(trans.rect.min);
            max = trans.TransformPoint(trans.rect.max);
            min = transform.InverseTransformPoint(min);
            max = transform.InverseTransformPoint(max);
            xMin = Mathf.Min(xMin, min.x);
            xMax = Mathf.Max(xMax, max.x);
            yMin = Mathf.Min(yMin, min.y);
            yMax = Mathf.Max(yMax, max.y);
            count++;
        }
        if (count == 0)
        {
            foreach (RectTransform trans in transArray)
            {
                if (!trans.gameObject.activeSelf)
                    continue;
                //Graphic graphic = trans.gameObject.GetComponent<Graphic>();
                min = trans.TransformPoint(trans.rect.min);
                max = trans.TransformPoint(trans.rect.max);
                min = transform.InverseTransformPoint(min);
                max = transform.InverseTransformPoint(max);
                xMin = Mathf.Min(xMin, min.x);
                xMax = Mathf.Max(xMax, max.x);
                yMin = Mathf.Min(yMin, min.y);
                yMax = Mathf.Max(yMax, max.y);
            }
        }
        Bounds b = new Bounds(new Vector3(xMin, yMin, 0), Vector3.zero);
        b.Encapsulate(new Vector3(xMax, yMax, 0));
        return b;
    }

    public static Rect GetChildRect(Transform transform)
    {
        RectTransform[] transArray = transform.GetComponentsInChildren<RectTransform>();
        if (transArray.Length <= 1)
        {
            return new Rect();
        }
        //calculate the rect
        Vector3 min, max;
        float xMin = float.PositiveInfinity, xMax = float.NegativeInfinity, yMin = float.PositiveInfinity, yMax = float.NegativeInfinity;
        foreach (RectTransform trans in transArray)
        {
            if (trans == transform)
                continue;
            min = trans.TransformPoint(trans.rect.min);
            max = trans.TransformPoint(trans.rect.max);
            min = transform.InverseTransformPoint(min);
            max = transform.InverseTransformPoint(max);
            xMin = Mathf.Min(xMin, min.x);
            xMax = Mathf.Max(xMax, max.x);
            yMin = Mathf.Min(yMin, min.y);
            yMax = Mathf.Max(yMax, max.y);
        }
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    public static Rect GetChildRaycastRect(RectTransform transform)
    {
        MaskableGraphic[] mgArray = transform.GetComponentsInChildren<MaskableGraphic>();
        if (mgArray.Length <= 0)
        {
            return new Rect();
        }
        //calculate the rect
        Vector3 min, max, min1, max1;
        float xMin = float.PositiveInfinity, xMax = float.NegativeInfinity, yMin = float.PositiveInfinity, yMax = float.NegativeInfinity;
        foreach (MaskableGraphic mg in mgArray)
        {
            if (!mg.raycastTarget)
                continue;
            RectTransform trans = mg.rectTransform;
            min = trans.TransformPoint(trans.rect.min);
            max = trans.TransformPoint(trans.rect.max);
            min = transform.InverseTransformPoint(min);
            max = transform.InverseTransformPoint(max);
            min1 = new Vector3(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y));
            max1 = new Vector3(Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
            xMin = Mathf.Min(xMin, min1.x);
            xMax = Mathf.Max(xMax, max1.x);
            yMin = Mathf.Min(yMin, min1.y);
            yMax = Mathf.Max(yMax, max1.y);
        }
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }
}
