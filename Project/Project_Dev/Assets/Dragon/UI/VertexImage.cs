using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class VertexImage : MaskableGraphic
{
    public Vector3[] corners;
    private static UIVertex[] vertexList = new UIVertex[4];

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (corners != null && corners.Length == 4)
        {
            vh.Clear();
            var tmpSize = this.rectTransform.GetSize();
            var tmppivot = this.rectTransform.pivot;
            for (int i = 0; i < 4; i++)
            {
                vertexList[i].position = new Vector3((corners[i].x / 2f + 0.5f - tmppivot.x) * tmpSize.x, (corners[i].y / 2f + 0.5f - tmppivot.y) * tmpSize.y, 0);
                vertexList[i].normal = Vector3.forward;
                vertexList[i].color = this.color;
            }
            vh.AddUIVertexQuad(vertexList);
        }
        else
        {
            vh.Clear();
        }
    }
}
