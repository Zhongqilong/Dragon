using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using Uqee.Pool;

public class TabGroup : PoolableMono
{
    public override void OnDespawn()
    {
        VerticalLayoutGroup group = transform.GetComponent<VerticalLayoutGroup>();
        group.padding = new RectOffset(0,0,0,0);
        group.childAlignment = TextAnchor.UpperCenter;
        if (_itemList==null)
        {
            return ;
        }
        for(int i=0; i<_itemList.Count; i++)
        {
            if(_itemList[i] != null && _itemList[i].gameObject != null)
                ResManager.Despawn(_itemList[i].gameObject);
        }
        DataListFactory<TabItem>.Release(_itemList);
        _itemList = null;
    }

    private ToggleGroup _tgl_group;
    private List<TabItem> _itemList;
    void _Init() {        
        if (_itemList == null)
        {
            _itemList = DataListFactory<TabItem>.Get();
        }
        if(_tgl_group==null)
        {
            _tgl_group = transform.GetOrAddComponent<ToggleGroup>();
        }
    }
    // Use this for initialization
    void OnDestroy()
    {
        if(AppStatus.isApplicationQuit)
        {
            return ;
        }
        OnDespawn();
    }
    public void SetPadding(RectOffset padding )
    {
        if (padding == null) return;
        transform.GetComponent<VerticalLayoutGroup>().padding = padding;

    }
    public void SetData(TabItemData[] list, int defaultIndex = 0,bool needBtnSound = true)
    {
        _Init();
        // int len = 0;
        for(int i=0;i<list.Length;i++)
        {
            list[i].index = i;
            // if (list[i].isShow) len++;
            _CreateItem(list[i], needBtnSound);
        }
        // _SetDefaultIndex(defaultIndex);
        // _Clear(len);
    }

    private void _CreateItem(TabItemData data,bool needBtnSound = true)
    {   
        TabItem item = null;
        if (_itemList.Count>data.index)
        {
            item = _itemList[data.index];
            item.GetComponent<SoundButton>().enabled = needBtnSound;
        }
        else
        {
            item = UIManager.I.AddPrefabSync<TabItem>(transform);
            item.GetComponent<SoundButton>().enabled = needBtnSound;
            _itemList.Add(item);
            item.SetGroup( _tgl_group );
            item.name = "tab_" + data.index;
        }
        item.gameObject.SetActive(true);
        item.SetData(data);
        //item.isOn = false;
    }   
    public TabItem GetTabAt(int idx)
    {
        if(idx<_itemList.Count)
        {
            return _itemList[idx];
        }
        return null;
    }
}


