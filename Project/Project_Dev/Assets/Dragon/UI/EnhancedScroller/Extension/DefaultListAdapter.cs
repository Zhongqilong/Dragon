using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using EnhancedUI.EnhancedScroller;

public class DefaultListAdapter : BaseListAdapter
{
    public IList list { get; protected set; }
    public int cellWidth { get; protected set; }
   

    public bool AutoReSize { get; private set; } = false;
    public int MaxSize { get; private set; } = 3;
    public float MaxSizeInPt { get; private set; } = 20000;

    //如果是从pool拿
    private string cellPrefabName => prefabNames[0];
    public DefaultListAdapter(EnhancedScroller scroller, IList list, int cellWidth, Action<int, Transform> updateAction)
    {
        this.scroller = scroller;
        this.scroller.Delegate = this;
        this.list = list;
        this.cellWidth = cellWidth;
        this.updateAction = updateAction;
    }


    /// <summary>
    /// 是否自动伸缩.
    /// </summary>
    /// <param name="autoResize"></param>
    /// <param name="maxCount"></param>
    /// <returns></returns>
    public DefaultListAdapter SetAutoResize(bool autoResize, int maxCount,float maxSizeInPt = 20000)
    {
        this.AutoReSize = autoResize;
        this.MaxSize = maxCount;
        this.MaxSizeInPt = maxSizeInPt;
        return this;
    }

    public override int GetNumberOfCells(EnhancedUI.EnhancedScroller.EnhancedScroller scroller)
    {
        return list.Count;
    }

    public override float GetCellViewSize(EnhancedUI.EnhancedScroller.EnhancedScroller scroller, int dataIndex)
    {
        return cellWidth;
    }

    public override EnhancedScrollerCellView GetActiveCellView(object obj)
    {
        if (obj == null) return null;
        var dataIdx = this.list.IndexOf(obj);
        if (dataIdx < 0) return null;
        return scroller.GetCellViewByDataIndex(dataIdx);
    }

    public override EnhancedUI.EnhancedScroller.EnhancedScrollerCellView GetCellView(EnhancedUI.EnhancedScroller.EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        EnhancedScrollerCellView cellView;
        cellView = scroller.GetCellView(cellPrefabName);
        try
        {
            updateAction?.Invoke(dataIndex, cellView.transform);
        }catch(Exception exce)
        {
            Uqee.Debug.Log(exce.ToString());
        }
        return cellView;
    }

    public override void UpdateListSize()
    {
        if (this.scroller == null)
            return;
        this.list = list;
        if (this.AutoReSize)
        {
            AutoResizeRect(scroller);
        }

        scroller.ReloadData();
    }

    private void AutoResizeRect(EnhancedUI.EnhancedScroller.EnhancedScroller scroller)
    {
        var tmpCount = Math.Min(list.Count, MaxSize);
        var rectTfm = scroller.GetComponent<RectTransform>();
        var tmpSize = rectTfm.sizeDelta;

        if (scroller.scrollDirection == EnhancedUI.EnhancedScroller.EnhancedScroller.ScrollDirectionEnum.Vertical)
        {
            var tmpH = tmpCount == 1 ? cellWidth : (cellWidth * tmpCount + (tmpCount - 1) * scroller.spacing);
            tmpH += (scroller.padding.top + scroller.padding.bottom);
            tmpSize.y = Math.Min(MaxSizeInPt, tmpH);
            rectTfm.sizeDelta = tmpSize;
        }
        else
        {
            var tmpW = tmpCount == 1 ? cellWidth : (cellWidth * tmpCount + (tmpCount - 1) * scroller.spacing);
            tmpW += (scroller.padding.left + scroller.padding.right);
            tmpSize.x = Math.Min(MaxSizeInPt, tmpW);
            rectTfm.sizeDelta = tmpSize;
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        list = null;
        updateAction = null;
    }
}
