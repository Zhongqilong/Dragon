using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHit : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }

    [Obsolete("Use OnPopulateMesh(VertexHelper vh) instead.", false)]
    protected override void OnPopulateMesh(Mesh m)
    {
        m.Clear();
    }
}
