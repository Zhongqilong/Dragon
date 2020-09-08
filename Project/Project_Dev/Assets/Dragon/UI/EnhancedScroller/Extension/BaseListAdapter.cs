using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

public class BaseListAdapter : EnhancedUI.EnhancedScroller.IEnhancedScrollerDelegate
{
    public EnhancedScroller scroller { get; protected set; }
    private Dictionary<string, Func<string, Transform, GameObject>> dict = new Dictionary<string, Func<string, Transform, GameObject>>();
    protected SafeGetList<string> prefabNames = new SafeGetList<string>();

    protected System.Action<int, Transform> updateAction;

    public void SetUpdateCall(Action<int, Transform> updateAction)
    {
        this.updateAction = updateAction;
    }

    protected GameObject CreateContainerCell(Transform container,string cellIdentifier,bool isFreeLayout = false)
    {
        var types = isFreeLayout ? new Type[] { typeof(RectTransform), typeof(EnhancedGridCellView) } : new Type[] { typeof(RectTransform), typeof(EnhancedGridCellView), typeof(GridLayoutGroup) };
        var gObj = new GameObject(cellIdentifier, types);
        gObj.transform.SetParent(container, false);
        var rowPrefab = gObj.GetComponent<EnhancedGridCellView>();
        rowPrefab.cellIdentifier = cellIdentifier;
        return gObj;
    }

    protected virtual void CreateItemCell(GameObject gObj,int rowCount ,string cellIdentifier)
    {
        var rowPrefab = gObj.GetComponent<EnhancedGridCellView>();

        for (int i = 0; i < rowCount; i++)
        {
            if (rowPrefab.rowCellViews[i] == null)
                rowPrefab.rowCellViews[i] = this.Instantiate(cellIdentifier, gObj.transform).GetOrAddComponent<EnhancedScrollerCellView>();
        }
    }

    protected virtual GameObject _OnInistatiate(string cellIdentifier, Transform parent)
    {
        return null;
    }

    public virtual EnhancedScrollerCellView GetActiveCellView(System.Object data)
    {
        return null;
    }

    public virtual EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        throw new System.NotImplementedException();
    }

    public virtual float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 1;
    }

    public virtual int GetNumberOfCells(EnhancedScroller scroller)
    {
        return 0;
    }

    public virtual void UpdateListSize()
    {
        
    }

    public GameObject Instantiate(string cellIdentifier, Transform parent)
    {
        if (dict.ContainsKey(cellIdentifier))
        {
            return dict[cellIdentifier].Invoke(cellIdentifier, parent);
        }
        return _OnInistatiate(cellIdentifier, parent);
    }

    public BaseListAdapter AddCellActivator(Transform cellPrefab, int index = 0)
    {
        var view = cellPrefab.GetOrAddComponent<EnhancedScrollerCellView>();
        if (string.IsNullOrEmpty(view.cellIdentifier))
        {
            view.cellIdentifier = view.name;
        }
        AddCellActivator(view.cellIdentifier, (key, container) => { return (key == view.cellIdentifier) ? GameObject.Instantiate(cellPrefab, container).gameObject : null; }, index);
        return this;
    }

    public BaseListAdapter AddCellActivator(string prefabName, Func<string, Transform, GameObject> activator, int index = 0)
    {
        dict[prefabName] = activator;
        prefabNames[index] = prefabName;
        return this;
    }

    public virtual void Dispose()
    {
        dict.Clear();
        prefabNames.Clear();
        updateAction = null;
    }
}