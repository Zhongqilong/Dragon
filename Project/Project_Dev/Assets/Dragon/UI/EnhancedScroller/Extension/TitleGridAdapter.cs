using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;
using Uqee.Pool;

public class TitleGridAdapter : BaseListAdapter
{
    private List<IList> list;
    private Vector2Int cellSize;
    private Vector2Int scrollerSize;
    private Vector2Int spaceSize;

    private int titleWidth;
    private int rowCount;
    public EnhancedGridCellView rowPrefab { get; private set; }
    private System.Action<int, int, Transform> updateAction;
    private System.Action<int, Transform> titleUpdateAction;
    private string cellPrefabName => prefabNames[0];
    private string titlePrefabName => prefabNames[1];
    public class CellData
    {
        public IList list;
        public int start;
        public int count;

        public void SetTitle(IList list)
        {
            this.list = list;
            this.start = -1;
            this.count = 0;
        }

        public bool IsTitle(IList list)
        {
            return this.list == list && this.start == -1;
        }
    }

    private List<CellData> tagList = new List<CellData>(12);
    //需要把Prefab释放出来;;
    public TitleGridAdapter(EnhancedScroller scroller, List<IList> list,
        Vector2Int cellSize, Vector2Int scrollerSize, Vector2Int spaceSize,
        int titleWidth, int rowCount,
        Action<int, int, Transform> updateAction, System.Action<int, Transform> titleUpdateAction)
    {
        this.scroller = scroller;
        this.list = list;
        this.cellSize = cellSize;
        this.scrollerSize = scrollerSize;
        this.spaceSize = spaceSize;
        this.titleWidth = titleWidth;
        this.rowCount = rowCount;
        this.updateAction = updateAction;
        this.titleUpdateAction = titleUpdateAction;

        var recyleNode = scroller.CreateOrGetRecycleNode();
        var gObj = CreateContainerCell(recyleNode.transform, "GridCellView");

        rowPrefab = gObj.GetComponent<EnhancedGridCellView>();
        InitRowCell(gObj, rowCount, cellSize, scrollerSize, spaceSize.x);
        UpdateDataList();
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
        InitRowCell(gObj, rowCount, cellSize, scrollerSize, spaceSize.x);
        CreateItemCell(gObj, rowCount, cellPrefabName);
        return gObj;
    }

    public void InitRowCell(GameObject gObj, int rowCount, Vector2Int cellSize, Vector2Int scrollerSize, int space)
    {
        var rowPrefab = gObj.GetComponent<EnhancedGridCellView>();
        var layout = gObj.GetOrAddComponent<GridLayoutGroup>();
        var rect = gObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(scrollerSize.x, cellSize.y);

        layout.spacing = new Vector2(space, 0);
        layout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        layout.constraintCount = 1;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.cellSize = cellSize;

        rowPrefab.rowCellViews = new EnhancedUI.EnhancedScroller.EnhancedScrollerCellView[rowCount];
    }

    public override int GetNumberOfCells(EnhancedUI.EnhancedScroller.EnhancedScroller scroller)
    {
        return tagList.Count;
    }

    public override float GetCellViewSize(EnhancedUI.EnhancedScroller.EnhancedScroller scroller, int dataIndex)
    {
        return tagList[dataIndex].start == -1 ? titleWidth : this.cellSize.y;
    }

    /// <summary>
    /// 获取Title的DataIndex;
    /// </summary>
    /// <param name="listIdx"></param>
    /// <returns></returns>
    public int GetTitleIndex(int listIdx)
    {
        if (list.IsNullOrEmpty())
            return -1;
        if (listIdx < 0 || listIdx >= this.list.Count)
            return -1;
        foreach(var tagData in tagList)
        {
            if(tagData.IsTitle(list[listIdx]))
            {
                return tagList.IndexOf(tagData);
            }
        }
        return -1;
    }

    /// <summary>
    /// 获取DataIndex
    /// </summary>
    /// <param name="tmpObj"></param>
    /// <returns></returns>
    public int GetDataIndex(System.Object tmpObj)
    {
        if (list.IsNullOrEmpty()||tmpObj==null)
            return -1;
        int tmpIdx = -1;
        IList targetList = FindList(tmpObj);
        if (targetList == null)
            return -1;
        var tmpSectIdx = targetList.IndexOf(tmpObj);

        for(int i = 0;i<tagList.Count;i++)
        {
            var tmpLst = tagList[i];
            if (tmpLst.list == targetList && tmpLst.start<=tmpSectIdx && tmpSectIdx<(tmpLst.start+tmpLst.count))
            {
                tmpIdx = i;
                break;
            }               
        }
        return tmpIdx;
    }

    private IList FindList(System.Object tmpObj)
    {
        foreach(var tmpList in list)
        {
            if (tmpList.Contains(tmpObj))
                return tmpList;
        }
        return null;
    }

    public override EnhancedUI.EnhancedScroller.EnhancedScrollerCellView GetCellView(EnhancedUI.EnhancedScroller.EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        EnhancedScrollerCellView cellView;
        if (tagList[dataIndex].start == -1)
        {
            var tmpList = tagList[dataIndex].list;
            cellView = scroller.GetCellView(titlePrefabName) as EnhancedScrollerCellView;
            try { 
                titleUpdateAction?.Invoke(list.IndexOf(tmpList), cellView.transform);
            }
            catch (Exception exce)
            {
                Uqee.Debug.Log(exce.ToString());
            }
            return cellView;
        }
        else
        {
            EnhancedGridCellView rowCell;
            rowCell = scroller.GetCellView(rowPrefab) as EnhancedGridCellView;
            // loop through the sub cells to display their data (or disable them if they are outside the bounds of the data)
            var startIndex = tagList[dataIndex].start;
            var tmpList = tagList[dataIndex].list;
            for (var i = 0; i < rowCell.rowCellViews.Length; i++)
            {
                bool inRange = startIndex + i < tmpList.Count;
                rowCell.rowCellViews[i].gameObject.SetActive(inRange);
                rowCell.rowCellViews[i].dataIndex = startIndex + i;
                if (inRange)
                {
                    try { 
                        updateAction?.Invoke(list.IndexOf(tagList[dataIndex].list), startIndex + i, rowCell.rowCellViews[i].transform);
                    }
                    catch (Exception exce)
                    {
                        Uqee.Debug.Log(exce.ToString());
                    }
                }
            }
            return rowCell;
        }
    }

    private void ClearTags()
    {
        foreach (var tag in tagList)
        {
            DataFactory<CellData>.Release(tag);
        }
        tagList.Clear();
    }

    private void UpdateDataList()
    {
        var total = list.Count;
        ClearTags();
        CellData tmpCell;
        for (int i = 0; i < list.Count; i++)
        {
            tmpCell = DataFactory<CellData>.Get();
            tmpCell.SetTitle(list[i]);
            tagList.Add(tmpCell);

            var tmpCount = Mathf.CeilToInt(list[i].Count / (float)rowCount);
            total += tmpCount;
            for (int j = 0; j < tmpCount; j++)
            {
                tmpCell = DataFactory<CellData>.Get();
                tmpCell.list = list[i]; tmpCell.start = j * rowCount;
                tmpCell.count = Mathf.Min(list[i].Count - j * rowCount, rowCount);
                tagList.Add(tmpCell);
            }
        }
    }

    public void UpdateListSizeEx()
    {
        if (this.scroller == null)
            return;
        UpdateDataList();
        scroller.ReloadData();
    }

    public void UpdateListSize( IList list)
    {
        if (this.scroller == null)
            return;
        this.list = list as List<IList>;
        var rectTfm = scroller.GetComponent<RectTransform>();
        var tmpSize = rectTfm.sizeDelta;
        UpdateDataList();
        scroller.ReloadData();
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
        updateAction = null;
        titleUpdateAction = null;
    }

    /// <summary>
    /// 获取
    /// </summary>
    /// <param name="scroller"></param>
    /// <param name="dataIndex"></param>
    /// <param name="cellIndex"></param>
    /// <returns></returns>
    public EnhancedUI.EnhancedScroller.EnhancedScrollerCellView GetActiveCellView(EnhancedUI.EnhancedScroller.EnhancedScroller scroller, object obj)
    {
        if (obj == null) return null;
        EnhancedScrollerCellView cellView = null;
        var subList = FindList(obj);
        if (subList == null)
        {
            return null;
        }
        var listIdx = this.list.IndexOf(subList);
        var cellIndex = subList.IndexOf(obj);
        /////返回的title
        //if (cellIndex == -1)
        //{
        //    var dataIdx = GetTitleIndex(listIdx);
        //    cellView = scroller.GetCellViewByDataIndex(dataIdx);
        //}
        //else
        {
            var dataIdx = GetDataIndex(subList[cellIndex]);
            if (dataIdx < 0) return null;
            var rowCell = scroller.GetCellViewByDataIndex(dataIdx) as EnhancedGridCellView;
            if (rowCell == null) return null;
            foreach (var tmpCell in rowCell.rowCellViews)
            {
                if (tmpCell.dataIndex == cellIndex)
                {
                    cellView = tmpCell;
                    break;
                }
            }
        }

        return cellView;
    }
}