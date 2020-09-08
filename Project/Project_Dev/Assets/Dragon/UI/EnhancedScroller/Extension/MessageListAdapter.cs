using EnhancedUI.EnhancedScroller;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 聊天信息类型的Scroller
/// </summary>
public class MessageListAdapter: BaseListAdapter , IEnhancedScrollerDelegate
{
    private IList _data;

    private Dictionary<System.Object, int> cellWidthDict;
    private System.Func<int, int> GetCellItemSize;
    private System.Func<int, String> GetCellViewName;
    private int defaultCellSize = 10;
    /// <summary>
    /// 在列表中,但是不会被显示的,减少单次压力;
    /// </summary>
    public int notShowInList { get; private set; }

    public MessageListAdapter(EnhancedUI.EnhancedScroller.EnhancedScroller scroller, IList list,
        Func<int, int> GetCellItemSize,
        Func<int, String> GetCellViewName,
        Dictionary<System.Object, int> cellWidthDict ,
        int initCount = 20,
        int defaultCellSize = 10)
    {
        this.scroller = scroller;
        this._data = list;
        this.cellWidthDict = cellWidthDict??new Dictionary<object, int>();
        this.GetCellItemSize = GetCellItemSize;
        this.GetCellViewName = GetCellViewName;
        this.defaultCellSize = defaultCellSize;
        this.notShowInList = Mathf.Max(0, _data.Count - initCount);
        this.scroller.Delegate = this;
    }


    /// <summary>
    /// Populates the data with some random Lorum Ipsum text
    /// </summary>
    public void LoadData(IList data, int notShowInList)
    {
        this._data = data;
        this.notShowInList = notShowInList;
        scroller.ReloadData();
    }

    public void InsertHistory(int addCount, int notShowInList)
    {
        this.notShowInList = notShowInList;
        scroller.ReloadData();
        scroller.JumpToDataIndex(addCount);
    }

    /// <summary>
    /// This function adds a new record, resizing the scroller and calculating the sizes of all cells
    /// </summary>
    public void AddNewRow()
    {
        if (Mathf.Abs(1-scroller.NormalizedScrollPosition) * scroller.ScrollSize < 10f )
        {
            scroller.ReloadData();
            scroller.JumpToDataIndex(_data.Count - notShowInList - 1, 1f, 1f);
        }
        else
        {
            var tmpSize = 80f;
            var nowPos = scroller.ScrollPosition;
            //需要记录当前的位置..
            var newRitio = (scroller.NormalizedScrollPosition * scroller.ScrollSize) / (scroller.ScrollSize + scroller.spacing + tmpSize);
            // first, clear out the cells in the scroller so the new text transforms will be reset
            scroller.ReloadData();
            //保持不动呢?
            scroller.ScrollPosition = nowPos;
        }
    }

    public float GetCellViewHeight(EnhancedScroller scroller, int dataIndex)
    {
        if (dataIndex >= _data.Count || dataIndex < 0)
        {
            return 0;
        }

        var tmpdata = _data[dataIndex];
        if (tmpdata == null)
            return 0;

        if (cellWidthDict.ContainsKey(tmpdata))
        {
            return cellWidthDict[tmpdata];
        }

        return 0;
    }

    #region EnhancedScroller Handlers

    override public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return _data != null ? Mathf.Max(0, _data.Count - notShowInList) : 0;
    }

    override public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        var newIndex = dataIndex + notShowInList;
        var tmpdata = _data[newIndex];
        if (tmpdata == null)
            return defaultCellSize;

        if ( cellWidthDict.ContainsKey(tmpdata))
        {
            return cellWidthDict[tmpdata];
        }
        else
        {
            if (GetCellItemSize != null)
            {
                cellWidthDict[tmpdata] = GetCellItemSize.Invoke(newIndex);
                return cellWidthDict[tmpdata];
            }
        }
        return defaultCellSize;
    }

    override public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var newIndex = dataIndex + notShowInList;
        var cellPrefabName = GetCellViewName(newIndex);
        //Uqee.Debug.LogError("GetCellView::" + dataIndex + ":::" + cellPrefabName);
        var cellView = scroller.GetCellView(cellPrefabName);
        updateAction?.Invoke(newIndex, cellView.transform);
        return cellView;
    }
    #endregion
}
