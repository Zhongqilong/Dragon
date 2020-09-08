using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;
using Uqee.Pool;

public class TitleRowGridAdapter : BaseListAdapter
{
    private List<IList> list;
    private Vector2Int cellSize;
    private Vector2Int spaceSize;
    public bool AutoReSize { get; private set; } = false;
    public int MaxSize { get; private set; } = 3;
    private int offset;
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
    public TitleRowGridAdapter(EnhancedScroller scroller,List<IList> list,
        EnhancedTitleGridCellView rowPrefab,
        Vector2Int cellSize,int rowCount)
    {
        this.scroller = scroller;
        scroller.Delegate = this;

        this.list = list;
        this.rowPrefab = rowPrefab;
        this.cellSize = cellSize;
        this.rowCount = rowCount;

        InitRowCell(rowPrefab.gameObject);
        UpdateDataList();
    }

    public TitleRowGridAdapter SetRowInfo(Vector2Int cellSize,int rowCount)
    {
        this.cellSize = cellSize;
        this.rowCount = rowCount;
        return this;
    }

    /// <summary>
    /// 设置回调;
    /// </summary>
    /// <param name="updateAction"></param>
    /// <param name="titleUpdateAction"></param>
    /// <returns></returns>
    public TitleRowGridAdapter SetCallBack(Action<int, int, Transform> updateAction, System.Action<int, Transform> titleUpdateAction)
    {
        this.updateAction = updateAction;
        this.titleUpdateAction = titleUpdateAction;
        return this;
    }

    /// <summary>
    /// 是否自动伸缩.
    /// </summary>
    /// <param name="autoResize"></param>
    /// <param name="maxCount"></param>
    /// <returns></returns>
    public TitleRowGridAdapter SetAutoResize(bool autoResize,int maxCount)
    {
        this.AutoReSize = autoResize;
        this.MaxSize = maxCount;
        return this;
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
        var gObj = GameObject.Instantiate(this.rowPrefab,container).gameObject;
        var rowPrefab = gObj.GetComponent<EnhancedTitleGridCellView>();
        InitRowCell(gObj);
        CreateItemCell(gObj, rowCount, cellPrefabName);
        var layoutElement = rowPrefab.titleCell.GetOrAddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;
        return gObj;
    }

    public void InitRowCell(GameObject gObj)
    {
        var rowPrefab = gObj.GetComponent<EnhancedTitleGridCellView>();
        var rect = gObj.GetComponent<RectTransform>();
        //rect.sizeDelta = new Vector2(scrollerSize.x, cellSize.y);

        if(rowPrefab.rowCellViews==null && rowPrefab.rowCellViews.Length != rowCount)
        {
            var tmpRow = rowPrefab.rowCellViews;
            rowPrefab.rowCellViews = new EnhancedUI.EnhancedScroller.EnhancedScrollerCellView[rowCount];
            if (tmpRow != null)
            {
                Array.Copy(tmpRow, rowPrefab.rowCellViews, Mathf.Min(rowPrefab.rowCellViews.Length, tmpRow.Length));
            }
        }
    }

    public override int GetNumberOfCells(EnhancedUI.EnhancedScroller.EnhancedScroller scroller)
    {
        return tagList.Count;
    }

    public override float GetCellViewSize(EnhancedUI.EnhancedScroller.EnhancedScroller scroller, int dataIndex)
    {
        return this.cellSize.y;
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
        //EnhancedScrollerCellView cellView;
        EnhancedTitleGridCellView rowCell;
        rowCell = scroller.GetCellView(rowPrefab) as EnhancedTitleGridCellView;
        // loop through the sub cells to display their data (or disable them if they are outside the bounds of the data)
        var startIndex = tagList[dataIndex].start;
        var tmpList = tagList[dataIndex].list;

        rowCell.titleCell?.gameObject.SetActive(startIndex == 0);
        if (startIndex == 0)
        {
            try {
                titleUpdateAction?.Invoke(list.IndexOf(tmpList), rowCell.titleCell.transform);
            }catch (Exception exce)
            {
                Uqee.Debug.Log(exce.ToString());
            }
    }
            
        for (var i = 0; i < rowCell.rowCellViews.Length; i++)
        {
            bool inRange = startIndex + i < tmpList.Count;
            rowCell.rowCellViews[i].gameObject.SetActive(inRange);
            rowCell.rowCellViews[i].dataIndex = startIndex + i;
            if (inRange)
            {
                updateAction?.Invoke(list.IndexOf(tagList[dataIndex].list), startIndex + i, rowCell.rowCellViews[i].transform);
            }
        }
        return rowCell;
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

    public void UpdateListSize( IList list)
    {
        if (this.scroller == null)
            return;
        this.list = list as List<IList>;
        UpdateDataList();
        if (this.AutoReSize)
        {
            AutoResizeRect(this.scroller);
        }
       
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
        {
            var dataIdx = GetDataIndex(subList[cellIndex]);
            if (dataIdx < 0) return null;
            var rowCell = scroller.GetCellViewByDataIndex(dataIdx) as EnhancedTitleGridCellView;
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

    protected void AutoResizeRect(EnhancedUI.EnhancedScroller.EnhancedScroller scroller)
    {
        var tmpCount = Math.Min(GetNumberOfCells(this.scroller), MaxSize);
        var rectTfm = scroller.GetComponent<RectTransform>();
        var tmpSize = rectTfm.sizeDelta;

        if (scroller.scrollDirection == EnhancedUI.EnhancedScroller.EnhancedScroller.ScrollDirectionEnum.Vertical)
        {
            var tmpH = tmpCount == 1 ? cellSize.y : (cellSize.y * tmpCount + (tmpCount - 1) * scroller.spacing);
            tmpH += (scroller.padding.top + scroller.padding.bottom);
            tmpSize.y = tmpH;
            rectTfm.sizeDelta = tmpSize;
        }
        else
        {
            var tmpW = tmpCount == 1 ? cellSize.x: (cellSize.x * tmpCount + (tmpCount - 1) * scroller.spacing);
            tmpW += (scroller.padding.left + scroller.padding.right);
            tmpSize.x = tmpW;
            rectTfm.sizeDelta = tmpSize;
        }
    }
}