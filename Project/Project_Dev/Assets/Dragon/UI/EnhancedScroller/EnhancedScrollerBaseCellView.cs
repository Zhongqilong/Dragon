using UnityEngine;
using System.Collections;
using EnhancedUI.EnhancedScroller;

public class EnhancedScrollerBaseCellView : EnhancedScrollerCellView
{

    protected Data _data;

    public virtual void SetData(Data data)
    {
        _data = data;
    }

}