using System;
using UnityEngine;

public class PackageGridView : MonoBehaviour
{

    private int _count;
    private ScrollViewEx _scrollviewEx;
    private Action<Transform, int> _action;
    private int ROW_COUNT;
    private int PAGE_ROW_COUNT;

    public void Init(ScrollViewEx scrollviewEx, int pageRowCount, int rowCount, Action<Transform, int> func, Action<int> pageChangedAction = null)
    {
        _action = func;
        _scrollviewEx = scrollviewEx;
        _scrollviewEx.Init(_InitItem, 0, pageChangedAction);
        ROW_COUNT = rowCount;
        PAGE_ROW_COUNT = pageRowCount;
    }

    public void Refresh(int count = 0)
    {
        _count = count;
        _scrollviewEx.Refresh(CellToGridCount(count));
    }

    public void Clear()
    {
        _scrollviewEx.Clear();
    }

    public void ResetToBegin()
    {
        _scrollviewEx.ResetToBegin();
    }

    private void _InitItem(Transform cell, int index)
    {
        var child = cell.childCount > 0 ? cell.GetChild(0) : null;

        Transform item;
        if (child == null) item = UIManager.I.AddPrefabToSync("PackageGridItem", cell);
        else item = child;
        if (index < _count)
        {
            _action?.Invoke(item.transform, index);
            if (item.transform.childCount > 0) item.transform.GetChild(0).gameObject.SetActive(true);
        }
        else if (item.transform.childCount > 0)
        {
            item.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    private int CellToGridCount(int cellCount)
    {
        float tmp = (float)cellCount / (float)ROW_COUNT;
        int count = Mathf.CeilToInt(tmp);
        count = Mathf.Max(PAGE_ROW_COUNT, count);
        return count * ROW_COUNT;
    }

}