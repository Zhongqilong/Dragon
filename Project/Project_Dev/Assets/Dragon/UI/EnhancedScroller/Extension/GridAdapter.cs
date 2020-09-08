using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using EnhancedUI.EnhancedScroller;

public class GridAdapter : BaseListAdapter
{
    private IList list;
    private int rowCount;

    private Vector2Int cellSize;
    private Vector2Int scrollerSize;
    private Vector2Int spaceSize;

    public EnhancedGridCellView rowPrefab { get; private set; }
    protected System.Action<int, Transform> updateAction;
    /// <summary>
    /// 为了分页;
    /// </summary>
    public int DataOffset = 0;
    //如果是从pool拿
    private string cellPrefabName => prefabNames[0];

    public GridLayoutGroup.Corner startCorner;
    public bool IsHorizental { get; private set; }

    public bool IsFreeLayout { get; private set; }

    public GridAdapter(EnhancedUI.EnhancedScroller.EnhancedScroller scroller,IList list,
        Vector2Int cellSize, Vector2Int scrollerSize, Vector2Int spaceSize,
        int rowCount, Action<int, Transform> updateAction, GridLayoutGroup.Corner startCorner,bool isFreeLayout)
    {
        this.IsFreeLayout = isFreeLayout;
        this.scroller = scroller;
        this.list = list;
        this.cellSize = cellSize;
        this.scrollerSize = scrollerSize;
        this.spaceSize = spaceSize;
        this.rowCount = rowCount;
        this.updateAction = updateAction;
        this.startCorner = startCorner;

        IsHorizental = scroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal;
        var recyleNode = scroller.CreateOrGetRecycleNode();
        _CreateRowPrefab(recyleNode.transform);
        InitRowCell(rowPrefab.gameObject, rowCount, cellSize, scrollerSize, spaceSize.x);
    }

    public void SetCallBack(System.Action<int, Transform> updateAction)
    {
        this.updateAction = updateAction;
    }

    protected override GameObject _OnInistatiate(string cellIdentifier, Transform parent)
    {
        if (cellIdentifier == this.rowPrefab.cellIdentifier)
        {
            return CreateRowCell(parent);
        }
        return null;
    }

    private GameObject CreateRowCell(Transform container)
    {
        var gObj = CreateContainerCell(container, "GridCellView");
        var rowPrefab = gObj.GetComponent<EnhancedGridCellView>();
        InitRowCell(gObj,  rowCount, cellSize, scrollerSize, spaceSize.x);
        CreateItemCell(gObj, rowCount, cellPrefabName);
        return gObj;
    }

    protected virtual void _CreateRowPrefab(Transform container)
    {
        var gObj = CreateContainerCell(container, "GridCellView");
        rowPrefab = gObj.GetComponent<EnhancedGridCellView>();
    }

    protected virtual void InitRowCell(GameObject gObj,int rowCount, Vector2Int cellSize, Vector2Int scrollerSize, int space)
    {
        var rowPrefab = gObj.GetComponent<EnhancedGridCellView>();
        var layout = gObj.GetOrAddComponent<GridLayoutGroup>();
        var rect = gObj.GetComponent<RectTransform>();

        layout.spacing = new Vector2(space, 0);
        layout.constraint = scroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal ? GridLayoutGroup.Constraint.FixedColumnCount : GridLayoutGroup.Constraint.FixedRowCount;
        layout.constraintCount = 1;
        layout.startAxis = GridLayoutGroup.Axis.Vertical;
        layout.childAlignment = TextAnchor.LowerLeft;
        layout.startCorner = startCorner;// GridLayoutGroup.Corner.LowerLeft;
        layout.cellSize = cellSize;

        rowPrefab.rowCellViews = new EnhancedUI.EnhancedScroller.EnhancedScrollerCellView[rowCount];
        rect.sizeDelta = new Vector2(scrollerSize.x, cellSize.y);
    }


    public override int GetNumberOfCells(EnhancedUI.EnhancedScroller.EnhancedScroller scroller)
    {
        return Mathf.CeilToInt(list.Count / (float)rowCount);
    }

    public override float GetCellViewSize(EnhancedUI.EnhancedScroller.EnhancedScroller scroller, int dataIndex)
    {
        return IsHorizental ? cellSize.x : cellSize.y;
    }

    public override EnhancedUI.EnhancedScroller.EnhancedScrollerCellView GetCellView(EnhancedUI.EnhancedScroller.EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var cellView = scroller.GetCellView(rowPrefab) as EnhancedGridCellView;
        // loop through the sub cells to display their data (or disable them if they are outside the bounds of the data)
        var startIndex = dataIndex * rowCount;
        for (var i = 0; i < cellView.rowCellViews.Length; i++)
        {
            bool inRange = startIndex + i < list.Count;
            cellView.rowCellViews[i].gameObject.SetActive(inRange);
            cellView.rowCellViews[i].dataIndex = DataOffset + startIndex + i;
            if (inRange)
            {
                try { 
                    updateAction?.Invoke(DataOffset + startIndex + i, cellView.rowCellViews[i].transform);
                }catch (Exception exce)
                {
                    Uqee.Debug.Log(exce.ToString());
                }
            }
        }
        return cellView;
    }

    public EnhancedUI.EnhancedScroller.EnhancedScrollerCellView GetActiveCellView(EnhancedUI.EnhancedScroller.EnhancedScroller scroller, object obj)
    {
        if (obj == null) return null;
        var dataIdx = this.list.IndexOf(obj);
        if (dataIdx < 0) return null;
        var rowIndex = dataIdx / rowCount;
        var rowCell = scroller.GetCellViewByDataIndex(rowIndex) as EnhancedGridCellView;
        if (rowCell == null)
            return null;

        var cellView = default(EnhancedScrollerCellView);
        foreach (var tmpCell in rowCell.rowCellViews)
        {
            if (tmpCell.dataIndex == dataIdx)
            {
                cellView = tmpCell;
                break;
            }
        }

        return cellView;
    }

    public override void UpdateListSize()
    {
        this.scroller.ReloadData();
    }

    public override void Dispose()
    {
        base.Dispose();
        list = null;
        if (rowPrefab != null)
        {
            GameObject.Destroy(rowPrefab.gameObject);
        }
        rowPrefab = null;
        updateAction = null;
    }
}