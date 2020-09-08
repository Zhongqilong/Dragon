using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageController : MonoBehaviour
{
    /// <summary>原始数据</summary>
    public IList Data { get; private set; }
    /// <summary>分页后的数据</summary>
    public IList PagedData { get; private set; }
    public int PageIndex { get; private set; }
    public int DataOffset { get { return PageIndex * PagePerCount; } }
    public int PagePerCount { get; private set; }
    public int PageCount => Data != null ? Mathf.CeilToInt((float)Data.Count / PagePerCount) : 0;
    public delegate void PageChange();
    public PageChange OnPageChanged;

    public void Init(IList Data, int PagePerCount)
    {
        this.Data = Data;
        this.PagePerCount = PagePerCount;
        PagedData = System.Activator.CreateInstance(Data.GetType()) as IList;
        _OnResetPageIdx();
    }

    private void _OnResetPageIdx()
    {
        PageIndex = 0;
        FillData(0);
        OnPageChanged?.Invoke();
    }

    private void FillData(int pageIdx)
    {
        var start = pageIdx * PagePerCount;
        var end = Mathf.Min(start + PagePerCount, Data.Count);
        PagedData.Clear();
        for (int i = start; i < end; i++)
        {
            PagedData.Add(Data[i]);
        }
    }

    public void SetPage(int pageIdx)
    {
        if (pageIdx < PageCount)
        {
            _SetPageIndex(pageIdx);
        }
    }

    private bool HasData()
    {
        return Data != null && Data.Count > 0;
    }

    public bool HasNext()
    {
        return HasData() && PageIndex < PageCount-1;
    }

    public bool HasPrev()
    {
        return HasData()&&PageIndex > 0;
    }

    public void GoNext()
    {
        if (HasNext())
        {
            _SetPageIndex(PageIndex + 1);
        }
        
    }

    public void GoPrev()
    {
        if (HasPrev())
        {
            _SetPageIndex( PageIndex - 1);
        }
    }

    private void _SetPageIndex(int pageIdx)
    {
        this.PageIndex = pageIdx;
        FillData(PageIndex);
        OnPageChanged?.Invoke();
    }

    private void OnDestroy()
    {
        OnPageChanged = null;
    }
}
