using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageLine : MaskableGraphic
{
    public LinkImageText imageText;
    private static VertexHelper helper = new VertexHelper();
    private static UIVertex[] quadVertexs = new UIVertex[4];
    protected override void Start()
    {
        base.Start();
        UpdateDirty();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        this.SetVerticesDirty();
    }

    override public void OnCullingChanged()
    {
        base.OnCanvasGroupChanged();
        this.SetAllDirty();
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        if (!isActiveAndEnabled)
            return;
        this.SetVerticesDirty();
    }

    public void UpdateMesh()
    {
        UpdateGeometry();
    }

    private void UpdateDirty()
    {
        this.gameObject.SetActive(false);
        this.SetAllDirty();
        this.gameObject.SetActive(true);
    }

    /// <summary>
    /// 换行问题
    /// 
    /// </summary>
    protected override void UpdateGeometry()
    {
        if (imageText != null && imageText.HrefBounds != null && imageText.HrefBounds.Count > 0)
        {
            OnPopulateMesh(helper);
            if (helper.currentVertCount > 0)
            {
                canvasRenderer.Clear();
                workerMesh.Clear();
                helper.FillMesh(workerMesh);
                canvasRenderer.SetMesh(workerMesh);
            }
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (imageText != null && imageText.HrefBounds != null && imageText.HrefBounds.Count > 0)
        {
            for (int i = 0; i < imageText.HrefBounds.Count; i++)
            {
                foreach (var box in imageText.HrefBounds[i].boxes)
                {
                    _UnderLineBound(helper, box, imageText.HrefBounds[i].color);
                }
            }
        }
    }

    protected void _UnderLineBound(VertexHelper vh, Rect bounds, Color color = default(Color))
    {
        var currentIdx = vh.currentVertCount;
        var tmpY = bounds.min.y;
        var worldPos = imageText.rectTransform.TransformPoint(new Vector3(bounds.min.x, tmpY));
        worldPos.z = 0;
        var minPos = rectTransform.InverseTransformPoint(worldPos);

        worldPos = imageText.rectTransform.TransformPoint(new Vector3(bounds.max.x, tmpY));
        worldPos.z = 0;
        var maxPos = rectTransform.InverseTransformPoint(worldPos);

        quadVertexs[0] = new UIVertex() { position = new Vector3(minPos.x, minPos.y + 2), color = color, uv0 = Vector2.zero };
        quadVertexs[1] = new UIVertex() { position = new Vector3(maxPos.x, minPos.y + 2), color = color, uv0 = new Vector2(0, 1) };
        quadVertexs[2] = new UIVertex() { position = new Vector3(maxPos.x, minPos.y + 0), color = color, uv0 = new Vector2(1, 1) };
        quadVertexs[3] = new UIVertex() { position = new Vector3(minPos.x, minPos.y + 0), color = color, uv0 = new Vector2(1, 0) };

        //quadVertexs[0] = new UIVertex() { position = new Vector3(bounds.min.x, tmpY + 2), color = color, uv0 = Vector2.zero };
        //quadVertexs[1] = new UIVertex() { position = new Vector3(bounds.max.x, tmpY + 2), color = color, uv0 = new Vector2(0, 1) };
        //quadVertexs[2] = new UIVertex() { position = new Vector3(bounds.max.x, tmpY + 0), color = color, uv0 = new Vector2(1, 1) };
        //quadVertexs[3] = new UIVertex() { position = new Vector3(bounds.min.x, tmpY + 0), color = color, uv0 = new Vector2(1, 0) };
        vh.AddUIVertexQuad(quadVertexs);
    }
}
