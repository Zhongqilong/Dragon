using EnhancedUI.EnhancedScroller;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UIListUtils;

public class MultiListAdapter: IEnhancedScrollerDelegate
{
    public IList list { get; private set; }
    public EnhancedScroller[] scrollRects { get; private set; }
    public int cellWidth { get; private set; }
    public Action<Transform, int> updateAction { get; private set; }
    private IList[] listDatas;
    private float[] scrollSize;
    private float spacing;
    private float maxSize;
    public string CellIdentifier { get; private set; }

    public MultiListAdapter(IList list, EnhancedScroller[] scrollers, 
        int cellWidth,int spacing, Action<Transform, int> updateAction)
    {
        this.list = list;
        this.scrollRects = scrollers;
        this.cellWidth = cellWidth;
        this.spacing = spacing;
        listDatas = new IList[scrollers.Length];
        scrollSize = new float[scrollers.Length];
        for (int i = 0; i < scrollers.Length; i++)
        {
            var tmpType = list.GetType();
            listDatas[i] = Activator.CreateInstance(tmpType) as IList;
        }
        UpdateDataList();
        for (int i = 0; i < scrollers.Length; i++)
        {
            scrollers[i].scrollerScrolled = _ScrollerScrolledDelegate;
            var scroller = scrollers[i];
            scroller.spacing = spacing;
            scrollers[i].Delegate = new DefaultListAdapter(scroller, listDatas[i], cellWidth, (idx, tfm) => UpdateView(scroller, idx, tfm));
        }
        this.updateAction = updateAction;
    }

    public MultiListAdapter AddCellActivator(Transform cellPrefab, int index = 0)
    {
        var view = cellPrefab.GetOrAddComponent<EnhancedScrollerCellView>();
        if (string.IsNullOrEmpty(view.cellIdentifier))
        {
            view.cellIdentifier = view.name;
        }
        System.Func<string,Transform,GameObject> d = (key, container) => { return (key == view.cellIdentifier) ? GameObject.Instantiate(cellPrefab, container).gameObject : null; };
        foreach (var scroller in scrollRects)
        {
            ((DefaultListAdapter)scroller.Delegate).AddCellActivator(view.cellIdentifier, d);
        }
        if(index == 0)
        {
            CellIdentifier = view.cellIdentifier;
        }
        return this;
    }

    public MultiListAdapter AddCellActivator(string prefabName, Func<string, Transform, GameObject> activator, int index = 0)
    {
        foreach (var scroller in scrollRects)
        {
            ((DefaultListAdapter)scroller.Delegate).AddCellActivator(prefabName, activator, index);
        }
        return this;
    }

    //Create Instance 
    public GameObject Instantiate(string cellIdentifier, Transform parent)
    {
        return null;
    }

    private void _ScrollerScrolledDelegate(EnhancedScroller scroller, Vector2 val, float scrollPosition)
    {
        for (int i = 0; i < scrollRects.Length; i++)
        {
            if (scroller != scrollRects[i])
            {
                scrollRects[i].ScrollPosition = Mathf.Clamp(scrollPosition,0,maxSize);
            }
        }
    }

    public EnhancedScrollerCellView GetActiveCellView(System.Object data)
    {
        return null;
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        // loop through the sub cells to display their data (or disable them if they are outside the bounds of the data)
        var cellView = scroller.GetCellView(CellIdentifier) as EnhancedScrollerCellView;
        try { 
            updateAction?.Invoke(cellView.transform, dataIndex);
        }
        catch (Exception exce)
        {
            Uqee.Debug.Log(exce.ToString());
        }
        return cellView;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return cellWidth;
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return list.Count;
    }

    private void UpdateView(EnhancedScroller scroller,int index, Transform tfm)
    {
        for (int i = 0; i < this.scrollRects.Length; i++)
        {
            if(scrollRects[i] == scroller)
            {
                UpdateView(this.scrollRects.Length*index + i,tfm);
            }
        }
    }

    public void UpdateView(int index,Transform tfm)
    {
        updateAction?.Invoke(tfm, index);
    }

    private void UpdateDataList()
    {
        var nCount = listDatas.Length;

        for (int i = 0; i < nCount; i++)
        {
            scrollSize[i] = scrollRects[i].padding.left;
            listDatas[i].Clear();
        }

        for (int i = 0; i < list.Count; i++)
        {
            var tmpIndex = i % nCount;
            scrollSize[tmpIndex] += (cellWidth + spacing);
            listDatas[tmpIndex].Add(list[i]);
        }

        if (list.Count > nCount)
        {
            for(int i = 0; i < nCount; i++)
            {
                scrollSize[i] -= (spacing);
            }
        }

        //为什么要-spacing/
        maxSize = scrollSize[0];
        for (int i = 1; i < nCount; i++)
        {
            if ((scrollSize[i]) > maxSize)
            {
                maxSize = scrollSize[i];
            }
        }

        for (int i = 0; i < nCount; i++)
        {
            var scroller = scrollRects[i];
            scrollRects[i].padding.right = Mathf.CeilToInt(maxSize - (float)scrollSize[i]);
        }
    }

    public void UpdateListSize()
    {
        UpdateDataList();
        for (int i = 0; i < scrollRects.Length; i++)
        {
            var scroller = scrollRects[i];
            scroller.ReloadData();
        }
    }

    public void Dispose()
    {

    }
    
}