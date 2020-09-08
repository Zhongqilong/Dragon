using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using Uqee.Utility;

public class ScrollViewWrap : MonoBehaviour
{
    public enum Direction
    {
        Horizontal,
        Vertical,
    }
    public Direction direction;//方向
    public Vector2 spacing;//间隔
    public Vector2 cellSize;//子物体宽高
    public int unit;//每行或者每列单元格数
    public int maxItemNum;
    private Action<Transform, int> _onRefresh;
    private RectTransform _rect;
    private int _totalCount;
    private int _fitCount;
    private List<Vector3> _itemPosList;
    private Vector3[] _woldCorners = new Vector3[4];
    private List<Transform> _list;
    public System.Func<Transform> onCreate;
    private Action<GameObject> _onRemove;
    public Vector2 outPos = new Vector2(10000, 10000);
  
    void Start()
    {
       
    }

    public void Init(Action<Transform,int> onRefresh, Action<GameObject> onRemove)
    {
        _list = new List<Transform>();
        _onRefresh = onRefresh;
        _onRemove = onRemove;
    }

    public void RefreshView(int count)
    {
        _totalCount = count;
        _fitCount =(int) Math.Ceiling(_totalCount / (double)unit);
        _SetContentSize();
    }

    private void _SetContentSize()
    {
        if(_rect==null)
        {
            _rect = GetComponent<RectTransform>();
        }
        if (direction == Direction.Horizontal)
        {
            _rect.SetWidth(_fitCount * (cellSize.x + spacing.x) - spacing.x);
        }
        else
        {
            _rect.SetHeight(_fitCount * (cellSize.y + spacing.y) - spacing.y);
        }
        _itemPosList = new List<Vector3>();
        for (int i=0;i<_totalCount;i++)
        {
            Vector3 pos = new Vector3();
            pos.x = cellSize.x / 2f + spacing.x + (cellSize.x + spacing.x) * (i%unit);
            pos.y = -cellSize.y / 2f + spacing.x - (cellSize.y + spacing.y) * (i/unit);
            _itemPosList.Add(pos);
        }
        _lastPosition = transform.position;
    }
  
    public void RefreshInView()
    {
        
        //_RefreshInView();
    }
    private void _RefreshInView()
    {
        
        int showIndex = 0;
        for (int i = 0; i < _itemPosList.Count; i++)
        {
            bool inView1 = _IsInView(_itemPosList[i]);
            //Uqee.Debug.Log(string.Format("name:{0},i:{1},inView1:{2},showIndex:{3}", transform.name, i,inView1, showIndex));

          
            if (inView1)
            {
                Transform item = null;
                if(_list.Count> showIndex)
                {
                    item = _list[showIndex];
                }
                else
                {
                    item = onCreate();
                    _list.Add(item);
                }
                _onRefresh?.Invoke(item, i);
                item.transform.localPosition = _itemPosList[i];
                showIndex++;
                
            }
        }
        for(int i= _list.Count-1; i>=showIndex;i--)
        {
            //_list[i].localPosition = outPos;
            _onRemove?.Invoke(_list[i].gameObject);
            _list.RemoveAt(i);
        }
    }

    private  bool _IsInView(Vector3 worldPos)
    {
        return UIUtils.IsUIChildShowInCamera(UIManager.I.cam_UICam, UIManager.I.canvasScale, _rect, worldPos.x, worldPos.y, cellSize.x, cellSize.y);
        /***
        if (UIManager.I.cam_UICam == null) return false;
        Vector2 viewPos = UIManager.I.cam_UICam.WorldToViewportPoint(worldPos);
        if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1)
            return true;
        else
            return false;
        ***/
    }
  
    private Vector3 _lastPosition;
    private bool _isChange;
    void LateUpdate()
    {   
        if(_lastPosition!=null)
        {
            float distance = (_lastPosition - transform.position).magnitude;
            Uqee.Debug.Log("distance:" + distance);
            if (distance != 0)
            {
                _lastPosition = transform.position;
                _RefreshInView();


            }
        }
        
    }
 
}
