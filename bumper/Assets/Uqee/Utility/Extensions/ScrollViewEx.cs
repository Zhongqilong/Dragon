using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;
using Uqee.Resource;

[RequireComponent(typeof(ScrollRect))]
public class ScrollViewEx : MonoBehaviour, IEndDragHandler, IBeginDragHandler
{
#if UNITY_EDITOR
    [MenuItem("GameObject/UI/Scroll View Ex(Vertical) &s", false, 1)]
    static void CreateVertical(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("Scroll View Ex");
        GameObject select = menuCommand.context as GameObject;
        if (select == null)
            select = Selection.activeTransform.gameObject;
        GameObjectUtility.SetParentAndAlign(go, select);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
        RectTransform rectTrans = go.AddComponent<RectTransform>();
        rectTrans.sizeDelta = new Vector3(200, 300);
        ScrollRect rect = go.AddComponent<ScrollRect>();
        ScrollViewEx ex = go.AddComponent<ScrollViewEx>();
        go.AddComponent<Mask>().showMaskGraphic = false;
        go.AddComponent<Image>();
        rect.horizontal = false;
        rect.vertical = true;
        rect.viewport = rectTrans;
        ex.direction = Direction.Vertical;
        ex.spacing = new Vector2(10, 10);
        ex.unit = 1;
        RectTransform root = (new GameObject("Root")).AddComponent<RectTransform>();
        GameObjectUtility.SetParentAndAlign(root.gameObject, go);
        root.localRotation = Quaternion.identity;
        root.localScale = Vector3.one;
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = new Vector2(0, 0);
        root.sizeDelta = new Vector2(200, 300);
        GameObject cell = new GameObject("Cell");
        GameObjectUtility.SetParentAndAlign(cell, root.gameObject);
        cell.transform.localPosition = new Vector3(0, 100, 0);
        cell.transform.localRotation = Quaternion.identity;
        cell.transform.localScale = Vector3.one;
        RectTransform rectCell = cell.AddComponent<RectTransform>();
        rectCell.sizeDelta = new Vector2(0, 0);
        GameObject temp1 = new GameObject("(Delete Me)");
        GameObjectUtility.SetParentAndAlign(temp1, cell);
        Text text = temp1.AddComponent<Text>();
        text.text = "<b>Insert\nYour\nCell\nHere</b>";
        text.fontSize = 23;
        Outline outline = temp1.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 1);
        GameObject temp2 = new GameObject("Image");
        GameObjectUtility.SetParentAndAlign(temp2, temp1);
        Image image = temp2.AddComponent<Image>();
        image.color = new Color(1, 1, 1, 0.5f);
        rect.content = root;
    }

    [MenuItem("GameObject/UI/Scroll View Ex(Horizontal) &a", false, 1)]
    static void CreateHorizontal(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("Scroll View Ex");
        GameObject select = menuCommand.context as GameObject;
        if (select == null)
            select = Selection.activeTransform.gameObject;
        GameObjectUtility.SetParentAndAlign(go, select);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
        RectTransform rectTrans = go.AddComponent<RectTransform>();
        rectTrans.sizeDelta = new Vector3(300, 200);
        ScrollRect rect = go.AddComponent<ScrollRect>();
        ScrollViewEx ex = go.AddComponent<ScrollViewEx>();
        go.AddComponent<Mask>().showMaskGraphic = false;
        go.AddComponent<Image>();
        rect.horizontal = true;
        rect.vertical = false;
        rect.viewport = rectTrans;
        ex.direction = Direction.Horizontal;
        ex.spacing = new Vector2(10, 10);
        ex.unit = 1;
        RectTransform root = (new GameObject("Root")).AddComponent<RectTransform>();
        GameObjectUtility.SetParentAndAlign(root.gameObject, go);
        root.localRotation = Quaternion.identity;
        root.localScale = Vector3.one;
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = new Vector2(0, 0);
        root.sizeDelta = new Vector2(300, 200);
        GameObject cell = new GameObject("Cell");
        GameObjectUtility.SetParentAndAlign(cell, root.gameObject);
        cell.transform.localPosition = new Vector3(-100, 0, 0);
        cell.transform.localRotation = Quaternion.identity;
        cell.transform.localScale = Vector3.one;
        RectTransform rectCell = cell.AddComponent<RectTransform>();
        rectCell.sizeDelta = new Vector2(0, 0);
        GameObject temp1 = new GameObject("(Delete Me)");
        GameObjectUtility.SetParentAndAlign(temp1, cell);
        Text text = temp1.AddComponent<Text>();
        text.text = "<b>Insert\nYour\nCell\nHere</b>";
        text.fontSize = 23;
        Outline outline = temp1.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 1);
        GameObject temp2 = new GameObject("Image");
        GameObjectUtility.SetParentAndAlign(temp2, temp1);
        Image image = temp2.AddComponent<Image>();
        image.color = new Color(1, 1, 1, 0.5f);
        rect.content = root;
    }
#endif

    public enum Direction
    {
        Horizontal,
        Vertical,
    }
    public Direction direction;
    public Vector2 spacing;
    public int unit = 1;
    private bool _isPageMode = false;
    RectTransform _root;
    RectTransform _cell;
    private int _bestFitSize;
    private List<Transform> _cellList = new List<Transform>();
    private Dictionary<int, int> _cellIndexDict = new Dictionary<int, int>();

    private float _height, _width;
    private float _offsetHeight, _offsetWidth;
    private float _top, /*bot,*/ _left, /*right,*/ _centerX, _centerY;
    private int _totalCount;
    private int _totalLength;
    private Vector3 _cellOffsetPos;
    private Action<Transform, int> _OnSpawnFunc;
    private bool _hadInit;
    private bool _isDirty;
    private ScrollRect _scrollRect;
    private string _cellName;
    private Scrollbar _verticalBar;
    private Scrollbar _horizontalBar;
    private Tweener _tweener;

    private float _srvWidth;
    private float _srvHeight;
    private RectTransform _rectTransform;
    private Action<int> _onPageChangedAction;

    public void Init(Action<Transform, int> func, Action<int> pageChangedAction = null)
    {
        Init(func, 0, pageChangedAction);
    }
    /// <summary>
    /// 初始化并将显示区大小设置到可完整显示 direction 方向的所有内容
    /// </summary>
    /// <param name="func"></param>
    /// <param name="fitCount"></param>
    public void InitAndFit(Action<Transform, int> func, int fitCount)
    {
        _Init();
        var rect = GetComponent<RectTransform>();
        if (direction == Direction.Horizontal)
        {
            rect.SetWidth(fitCount * (_cell.sizeDelta.x + spacing.x) - spacing.x);
        }
        else
        {
            rect.SetHeight(fitCount * (_cell.sizeDelta.y + spacing.y) - spacing.y);
        }
        //Uqee.Debug.Log("rect.GetWidth " + rect.GetWidth());
        if (_hadInit)
        {
            ResetToBegin();
        }
        _hadInit = false;
        Init(func, 0);
    }
    public void SetCount(int count, bool autoFitCount)
    {
        _totalCount = count < 0 ? 0 : count;
        if (count <= 0)
        {
            count = autoFitCount ? _bestFitSize * unit : 0;
        }

        _totalLength = count / unit;
        if (count % unit != 0)
        {
            _totalLength++;
        }
    }

    void _Init()
    {
        if(_rectTransform!=null)
        {
            return ;
        }
        
        _scrollRect = GetComponent<ScrollRect>();
        _rectTransform = transform as RectTransform;

        _horizontalBar = _scrollRect.horizontalScrollbar;
        if (_horizontalBar != null)
        {
            if (direction == Direction.Horizontal)
            {
                EventTriggerListener.Get(_horizontalBar.gameObject).onDown = _OnHorizontalBar;
                EventTriggerListener.Get(_horizontalBar.handleRect.gameObject).onDown = _OnHorizontalBar;
            }
        }
        _verticalBar = _scrollRect.verticalScrollbar;
        if (_verticalBar != null)
        {
            if (direction == Direction.Vertical)
            {
                EventTriggerListener.Get(_verticalBar.gameObject).onDown = _OnVerticalBar;
                EventTriggerListener.Get(_verticalBar.handleRect.gameObject).onDown = _OnVerticalBar;
            }
        }
            
        if (transform.Find("Root") != null)
            _root = transform.Find("Root") as RectTransform;
        else if (transform.childCount != 0)
            _root = transform.GetChild(0) as RectTransform;
        _pivot = _root.pivot;
        if (_root == null)
        {
            //Uqee.Debug.LogError("[ScrollViewEx]No Root Found!");
            return;
        }
        Transform cell=null;
        if (_root.Find("Cell") != null)
            cell = _root.Find("Cell");
        else if (_root.childCount > 0)
            cell = _root.GetChild(0);
        if (cell == null)
        {
            //Uqee.Debug.LogError("[ScrollViewEx]No Cell Found!");
            return;
        }

        _cell = cell.GetComponent<RectTransform>();//_root.childCount > 1 ? _root.GetChild(1) : _cell;
        _cell.gameObject.SetActive(false);
        if (_cell==null || _cell.sizeDelta.x==0 || _cell.sizeDelta.y==0)
        {
            //Uqee.Debug.LogError("[ScrollViewEx]Cell size=0");
            return;
        }

        var rect = _cell.rect;
        _height = _cell.sizeDelta.y;
        _width = _cell.sizeDelta.x;
        _top = rect.yMax;
        _left = -rect.xMin;
        _centerX = rect.center.x;
        _centerY = rect.center.y;
    }
    public void Init(Action<Transform, int> func, int count, Action<int> pageChangedAction = null)
    {
        if (_hadInit) return;
        if (!Application.isPlaying) return;
        _Init();
        _cellOffsetPos = _cell.localPosition.ToVector2XY();

        _srvWidth = _rectTransform.sizeDelta.x;
        _srvHeight = _rectTransform.sizeDelta.y;

        if (direction == Direction.Horizontal)
        {
            _cellOffsetPos.x = _rectTransform.rect.xMin + _left;
        }
        else
        {
            _cellOffsetPos.y = _rectTransform.rect.yMax - _top;
        }
        if (direction == Direction.Horizontal)
            _bestFitSize = Mathf.CeilToInt(_rectTransform.rect.width / (_width + spacing.x)) + 1;
        else
            _bestFitSize = Mathf.CeilToInt(_rectTransform.rect.height / (_height + spacing.y)) + 1;

        _OnSpawnFunc = func;

        if (unit <= 0)
            unit = 1;

        SetCount(count, true);
        if (direction == Direction.Horizontal)
        {
            _scrollRect.horizontal = true;
            _scrollRect.vertical = false;
        }
        else
        {
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;
        }

        ResetRootRect();
        
        Vector3 pos = Vector3.zero;
        _cell.SetParent(_root);
        _cellName = _cell.name;

        var cacheSize = _bestFitSize * unit;
        while (cacheSize > _cellList.Count)
        {
            _CreateCell();
        }
        for(int i=_cellList.Count-1; i>=cacheSize; i--)
        {
            var cell = _cellList[i];
            _cellList.RemoveAt(i);
            InstantiateCache.I.DespawnChildren(cell);
            Destroy(cell.gameObject);
        }
        
        _offsetHeight = _root.localPosition.y;
        _offsetWidth = _root.localPosition.x;

        _hadInit = true;
        _onPageChangedAction = pageChangedAction;
        _isPageMode = _onPageChangedAction != null;
        Refresh();
    }
    public void ResetToBegin()
    {
        if(!_hadInit)
        {
            return;
        }
        MoveToLine(0);
    }

    private void _OnHorizontalBar(GameObject go, params object[] pars)
    {
        Refresh();
    }

    private void _OnVerticalBar(GameObject go, params object[] pars)
    {
        Refresh();
    }

    private void _CreateCell()
    {
        var index = _cellList.Count;
        var curCell = Instantiate(_cell);
        curCell.gameObject.SetActive(true);
        curCell.name = $"{_cellName}_{index}";
        curCell.SetParent(_root);
        curCell.localScale = _cell.localScale;
        curCell.localRotation = _cell.localRotation;
        _SetCellPos(curCell, index, 0, true);

        _cellList.Add(curCell);
    }
    public void Clear()
    {
        _isDirty = false;
        for(int i=_cellList.Count-1; i>=0; i--)
        {
            var cell = _cellList[i];
            InstantiateCache.I.DespawnChildren(cell);
        }
    }
    private int _firstIdx = 0;
    private Dictionary<int, Transform> _tmpCellDict = new Dictionary<int, Transform>();
    private Queue<Transform> _tmpCellList = new Queue<Transform>();
    private void LateUpdate()
    {
        int firstIdx = GetVisibleFirstIndex();
        if(_firstIdx!=firstIdx)
        {
            _firstIdx = firstIdx;
            _isDirty = true;
        }
        if (_isDirty)
        {
            _ForceUpdate(firstIdx);
        }
    }

    private void _ForceUpdate(int firstIdx)
    {
        int i = 0;
        if (_cellIndexDict.Count > 0)
        {
            for (i = 0; i < _cellList.Count; i++)
            {
                foreach (var indexPairs in _cellIndexDict)
                {
                    if (indexPairs.Value == firstIdx + i)
                    {
                        _tmpCellDict[indexPairs.Value] = _cellList[indexPairs.Key];
                        _cellList[indexPairs.Key] = null;
                        break;
                    }
                }
            }
            _cellIndexDict.Clear();
        }
        for (i = 0; i < _cellList.Count; i++)
        {
            if (_cellList[i] != null)
            {
                _tmpCellList.Enqueue(_cellList[i]);
                _cellList[i] = null;
            }
        }
        var firstCol = firstIdx / unit;
        for (i = 0; i < _bestFitSize; i++)
        {
            for (int j = 0; j < unit; j++)
            {
                int cellIdx = i * unit + j;
                int dataIdx = firstIdx + cellIdx;
                bool outSize = dataIdx < 0 || dataIdx >= _totalCount;
                Transform cell = null;
                if (_tmpCellDict.ContainsKey(dataIdx))
                {
                    cell = _tmpCellDict[dataIdx];
                    _tmpCellDict.Remove(dataIdx);
                    _SetCell(cell, outSize, cellIdx, i + firstCol, j, dataIdx);
                }
                else
                {
                    if (!outSize)
                    {
                        if (_tmpCellList.Count == 0)
                        {
                            //Uqee.Debug.LogError("out size:" + dataIdx + "," + cellIdx);
                        }
                        else
                        {
                            cell = _tmpCellList.Dequeue();
                            _SetCell(cell, outSize, cellIdx, i + firstCol, j, dataIdx);
                            _OnSpawn(cell, dataIdx);
                        }
                    }
                }
            }
        }
        i = 0;
        while (_tmpCellList.Count > 0)
        {
            for (; i < _cellList.Count; i++)
            {
                if (_cellList[i] == null)
                {
                    _cellList[i] = _tmpCellList.Dequeue();
                    _SetCellPos(_cellList[i], i, 0, true);
                    break;
                }
            }
            if (i >= _cellList.Count)
            {
                break;
            }
        }
        foreach (var cellPairs in _tmpCellDict)
        {
            for (; i < _cellList.Count; i++)
            {
                if (_cellList[i] == null)
                {
                    _cellList[i] = cellPairs.Value;
                    _SetCellPos(_cellList[i], i, 0, true);
                    break;
                }
            }
            if (i >= _cellList.Count)
            {
                break;
            }
        }
        _tmpCellList.Clear();
        _tmpCellDict.Clear();
        _isDirty = false;
    }

    private void _SetCell(Transform cell, bool outSize,int cellIdx, int col, int row, int dataIdx)
    {
        _cellList[cellIdx] = cell;
        if (outSize)
        {
            if(cell!=null)
            {
                _SetCellPos(cell, col, row, true);
            }
        }
        else
        {
            _cellIndexDict[cellIdx] = dataIdx;
            cell.localScale = _cell.localScale;
            cell.localRotation = _cell.localRotation;
            _SetCellPos(cell, col, row);
        }
    }
    private void _SetCellPos(Transform cell, int col, int row, bool outSize=false)
    {
        if (direction == Direction.Horizontal)
        {
            _tmpPos.x = col * (_width + spacing.x) + _cellOffsetPos.x;
            _tmpPos.y = -row * (_height + spacing.y) + _cellOffsetPos.y;
        }
        else
        {
            _tmpPos.x = row * (_width + spacing.x) + _cellOffsetPos.x;
            _tmpPos.y = -col * (_height + spacing.y) + _cellOffsetPos.y;
        }
        if(outSize)
        {
            _tmpPos.x += 5000;
            _tmpPos.y += 5000;
        }
        cell.localPosition = _tmpPos;

    }
    public bool cellInited {get ;private set;} = false;
    Vector2 _tmpPos = new Vector2();
    private bool _OnSpawn(Transform tran, int index)
    {
        cellInited = true;
        if (_OnSpawnFunc != null)
        {
            _OnSpawnFunc?.Invoke(tran, index);
            return true;
        }
        return false;
    }

    public void DisableScroll()
    {
        ScrollRect rect = GetComponent<ScrollRect>();
        rect.horizontal = false;
        rect.vertical = false;
    }
    
    public void EnableScroll()
    {
        ScrollRect rect = GetComponent<ScrollRect>();
        if (direction == Direction.Horizontal)
            rect.horizontal = true;
        else if (direction == Direction.Vertical)
            rect.vertical = true;
    }

    public void Refresh()
    {
        _isDirty = true;
    }

    public void Refresh(int count, int idx = -1,bool instant=true)
    {
        if (!_hadInit) {
            //Uqee.Debug.LogError("scrollViewEx 未初始化");
            return;
        } 
        _cellIndexDict.Clear();
        SetCount(count, false);
        ResetRootRect();
        if (idx != -1)
        {
            MoveToIndex(idx, instant);
        }
        Refresh();
    }
    
    private void ResetRootRect()
    {
        if(_scrollRect==null)
        {
            //Uqee.Debug.LogError("ScrollViewEx._scrollRect == null;");
            return;
        }
        if (_totalCount == 0)
        {
            _scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            _root.sizeDelta = Vector2.zero;
            return;
        }
        _scrollRect.movementType = ScrollRect.MovementType.Elastic;
        if (direction == Direction.Horizontal)
        {
            _root.sizeDelta = new Vector2(Mathf.Max(_width * _totalLength + (_totalLength - 1) * spacing.x, _rectTransform.rect.width),
                Mathf.Max(_height * unit + (unit - 1) * spacing.y, _rectTransform.rect.height));

            _pivot.x = (-_cellOffsetPos.x + _left) / _root.sizeDelta.x;
        }
        else
        {
            _root.sizeDelta = new Vector2(Mathf.Max(_width * unit + (unit - 1) * spacing.x, _rectTransform.rect.width),
                Mathf.Max(_height * _totalLength + (_totalLength - 1) * spacing.y, _rectTransform.rect.height));

            _pivot.y = 1 - (_cellOffsetPos.y + _top) / _root.sizeDelta.y;
        }
        _root.pivot = _pivot;
    }
    Vector2 _pivot;

    private int _index = -1;
    public void MoveToIndex(int index, bool instant = true, float time = 0.3f)
    {
        if (index < 0 || index >= _totalCount)
            return;
        _index = index;
        int line = index / unit;
        MoveToLine(line, instant, time);
    }

    public void MoveToLine(int line)
    {
        MoveToLine(line, true);
    }

    public void MoveToLine(int line, bool instant, float time = 0.3f)
    {
        if (direction == Direction.Horizontal)
        {
            MoveRelativeTo(-line * _width + -line * spacing.x + _offsetWidth, instant, time);
        }
        else
        {
            MoveRelativeTo(line * _height + line * spacing.y + _offsetHeight, instant, time);
        }
    }
    public void MoveToNextPage(bool isRight = true, bool instant = true)
    {
        if (direction == Direction.Horizontal)
        {
            MoveRelative(isRight ? _srvWidth : -_srvWidth, instant);
        }
        else
        {
            MoveRelative(isRight ? _srvHeight : -_srvHeight, instant);
        }
    }

    public void MoveRelativeTo(float To)
    {
        MoveRelativeTo(To, true);
    }

    public void MoveRelativeTo(float To, bool instant, float time = 0.3f)
    {
        float relative;
        if (direction == Direction.Horizontal)
            relative = To - _root.localPosition.x;
        else
            relative = To - _root.localPosition.y;
        MoveRelative(relative, instant, time);
    }

    private void MoveRelative(float relative, bool instant = true, float time = 0.3f)
    {
        _scrollRect?.StopMovement();
        if(_root==null)
        {
            return;
        }
        if (instant)
        {
            if (direction == Direction.Horizontal)
            {
                _root.localPosition += Vector3.right * relative;
            }
            else
            {
                _root.localPosition += Vector3.up * relative;
            }
            if (_hadInit)
            {
                _ForceUpdate(GetVisibleFirstIndex());
            }
        }
        else
        {
            if (direction == Direction.Horizontal)
            {
                _tweener = _root.DOLocalMoveX(_root.localPosition.x + relative, time);
            }
            else
            {
                var targetPos = _root.localPosition.y + relative;
                if (targetPos < 0) targetPos = 0;
                if (targetPos > _root.sizeDelta.y - _srvHeight) targetPos = _root.sizeDelta.y - _srvHeight;
                 _tweener = _root.DOLocalMoveY(targetPos, time);
            }
            _tweener.SetEase(Ease.OutCubic);
        }
    }

    public int GetVisibleFirstIndex()
    {
        if(!_hadInit || _root == null)
        {
            return 0;
        }
        int line = 0;
        if (direction == Direction.Horizontal)
        {
            line = Mathf.FloorToInt((-_root.localPosition.x + _offsetWidth - spacing.x) / (_width + spacing.x));
        }
        else
        {
            line = Mathf.FloorToInt((_root.localPosition.y - _offsetHeight + spacing.y) / (_height + spacing.y));
        }
        return Math.Max(0, line * unit);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(_root==null)
        {
            return;
        }
        if (_isPageMode)
        {
            var offset = direction == Direction.Horizontal ? (eventData.pressPosition.x - eventData.position.x) : (eventData.pressPosition.y - eventData.position.y);
            var targetIndex = Mathf.Abs(offset) < 15 ? _index : (offset < 0 ? (_index - 1) : (_index + 1));
            if (targetIndex < 0 || targetIndex >= _totalCount)
                targetIndex = _index;
            MoveToIndex(targetIndex, false);
            _onPageChangedAction?.Invoke(targetIndex);
        }
        else
        {
            int line = 0;
            if (direction == Direction.Horizontal)
            {
                line = Mathf.FloorToInt((-_root.localPosition.x + _offsetWidth - spacing.x) / (_width + spacing.x));
            }
            else
            {
                line = Mathf.FloorToInt((_root.localPosition.y - _offsetHeight + spacing.y) / (_height + spacing.y));
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _tweener?.Kill();
    }
    
    public Transform GetCellByIndex(int index)
    {
        if (index < _firstIdx * unit || index >= (_firstIdx + _bestFitSize) * unit)
        {
            return null;
        }
        return _cellList[index - _firstIdx * unit];
    }

    uint _timerId = 0;
    //处理领取item奖励,把item缩放消失，item后面的对象移动到item的位置
    public void BackMoveTo(int index, float time, float offset = 0, Action<int> callBack = null)
    {
        var curItem = GetCellByIndex(index).GetChild(0);
        if (curItem == null) return;
        //Uqee.Debug.Log("Ex BackMoveTo()!!!!!!!!!!!!!!!!!!!!!!");
        var alphaGroup = curItem.GetOrAddComponent<CanvasGroup>();
        alphaGroup.DOFade(0f, time);
        curItem.DOScale(0.1f, time).OnComplete(() =>
        {
            //Uqee.Debug.Log("index:" + index + " _totalCount:" + _totalCount);
            if (index < _totalCount - 1)
            {
                if (direction == Direction.Horizontal)
                {
                    float offsetX = curItem.GetComponent<RectTransform>().GetSize().x + offset;
                    for (int i = index + 1; i < _totalCount; i++)
                    {
                        var item = GetCellByIndex(i)?.GetChild(0);
                        if (item != null)
                            item.DOLocalMoveX(-offsetX, time);
                    }
                }
                else
                {
                    float offsetY = curItem.GetComponent<RectTransform>().GetSize().y + offset;
                    for (int i = index + 1; i < _totalCount; i++)
                    {
                        var item = GetCellByIndex(i)?.GetChild(0);
                        if (item != null)
                            item.DOLocalMoveY(offsetY, time);
                    }
                }

                if (_timerId != 0) JobScheduler.I.ClearTimer(_timerId);

                _timerId = JobScheduler.I.SetTimeOut(() =>
                {
                    //Uqee.Debug.Log("ex item finish move over!!!!!!!!!!!!!");
                    alphaGroup.alpha = 1;
                    curItem.transform.localScale = Vector3.one;
                    for (int i = index + 1; i < _totalCount; i++)
                    {
                        var item = GetCellByIndex(i)?.GetChild(0);
                        if (item != null)
                            item.localPosition = Vector2.zero;
                    }
                    callBack?.Invoke(index);
                    JobScheduler.I.ClearTimer(_timerId);
                }, time);
            }
            else
            {
                alphaGroup.alpha = 1;
                curItem.transform.localScale = Vector3.one;
                callBack?.Invoke(index);
            }
        });
    }

    public float GetNormalizedPosition()
    {
        if (_scrollRect == null)
            return 0;

        if (direction == Direction.Horizontal)
            return _scrollRect.horizontalNormalizedPosition;
        else
            return _scrollRect.verticalNormalizedPosition;
    }

#if UNITY_EDITOR

    /// <summary>
    /// Draw a visible orange outline of the bounds.
    /// </summary>

    void OnDrawGizmos()
    {
            var _rectTransform = transform as RectTransform;
        if (!Application.isPlaying)
        {
            Transform root = null;
            if (transform.Find("Root") != null)
                root = transform.Find("Root");
            else if (transform.childCount > 0)
                root = transform.GetChild(0);
            if (root == null)
                return;
            Transform cell = null;
            if (root.Find("Cell") != null)
                cell = root.Find("Cell");
            else if (root.childCount > 0)
                cell = root.GetChild(0);
            if (cell == null)
                return;
            var rect = cell.GetComponent<RectTransform>();
            if (rect==null || rect.sizeDelta.x == 0 || rect.sizeDelta.y == 0)
                return;
                
            int height = Mathf.RoundToInt(rect.sizeDelta.y);
            int width = Mathf.RoundToInt(rect.sizeDelta.x);

            int top = Mathf.RoundToInt(rect.rect.yMax);
            int left = Mathf.RoundToInt(-rect.rect.xMin);
            int centerX = Mathf.RoundToInt(rect.rect.center.x);
            int centerY = Mathf.RoundToInt(rect.rect.center.y);
            Vector3 offsetCell = cell.localPosition;
            if (direction == Direction.Horizontal)
            {
                float dif = (_rectTransform.rect.xMin - (offsetCell.x - left)) / (width + spacing.x);
                if (dif > 0.5f)
                {
                    offsetCell.x += (width + spacing.x) * Mathf.FloorToInt(dif + 0.5f);
                }
                else if (dif < -0.5f)
                {
                    offsetCell.x -= (width + spacing.x) * Mathf.FloorToInt(-dif + 0.5f);
                }
            }
            else
            {
                float dif = (_rectTransform.rect.yMax - (offsetCell.y + top)) / (height + spacing.y);
                if (dif > 0.5f)
                {
                    offsetCell.y += (height + spacing.y) * Mathf.FloorToInt(dif + 0.5f);
                }
                else if (dif < -0.5f)
                {
                    offsetCell.y -= (height + spacing.y) * Mathf.FloorToInt(-dif + 0.5f);
                }
            }
            float temp1 = -height * 0.5f + top;
            float temp2 = width * 0.5f - left;
            int bestFit = 0;
            if (direction == Direction.Horizontal)
                bestFit = Mathf.CeilToInt(_rectTransform.rect.width / (width + spacing.x)) + 2;
            else
                bestFit = Mathf.CeilToInt(_rectTransform.rect.height / (height + spacing.y)) + 2;

            Gizmos.matrix = root.localToWorldMatrix;

            Vector3 pos = Vector3.zero;
            for (int i = 0; i < bestFit; i++)
            {
                for (int j = 0; j < unit; j++)
                {
                    if (direction == Direction.Horizontal)
                    {
                        pos = Vector3.right * ((i - 1) * (width + spacing.x) + centerX) + Vector3.up * (temp1 - j * (height + spacing.y)) + offsetCell;
                    }
                    else
                    {
                        pos = -Vector3.up * ((i - 1) * (height + spacing.y) - centerY) + Vector3.right * (temp2 + j * (width + spacing.x)) + offsetCell;
                    }
                    if (i == 1 && j == 0)
                        Gizmos.color = new Color(0.3f, 0.6f, 1f);
                    else
                        Gizmos.color = new Color(0.6f, 1f, 1f);
                    Gizmos.DrawWireCube(pos, new Vector3(width, height, 0f));
                }
            }
        }
        else
        {
            if (_cell == null)
                return;
            Gizmos.matrix = _root.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0.8f, 0f);
            foreach (var c in _cellList)
            {
                Gizmos.DrawWireCube(c.localPosition + new Vector3(_centerX, _centerY, 0f), new Vector3(_width, _height, 0f));
            }
        }

        Gizmos.matrix = _rectTransform.localToWorldMatrix;
        Gizmos.color = new Color(0.5f, 0f, 0.5f);
        Gizmos.DrawWireCube(_rectTransform.rect.center, _rectTransform.rect.size);
    }


#endif
}

