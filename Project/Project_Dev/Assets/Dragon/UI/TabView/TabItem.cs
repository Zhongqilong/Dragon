using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TabItem : PoolableMono
{
    public Transform tra_background;
    public Transform tran_checkmask0;
    public Transform tran_checkmask1;
    public Transform tran_pop_checkmask0;
    public Transform tran_pop_checkmask1;
    public Transform tran_squ_checkmark0;
    public Transform tran_squ_checkmark1;

    public Text txt_label, txt_label_light;
    public Text txt_label_vertical, txt_label_vertical_highlight;
    public TabItemData data { private set; get; }

    private Toggle _tgl_item;
    public void SetGroup(ToggleGroup group)
    {
        _Init();
        _tgl_item.group = group;
    }

    void _Init()
    {
        if(_tgl_item==null)
        {
            _tgl_item = transform.GetComponent<Toggle>();
            _tgl_item.onValueChanged.AddListener(_OnValueChanged);
        }
    }

    public override void OnDespawn()
    {
        if(data.isSquare)
        {
            txt_label_vertical_highlight.gameObject.SetActive(false);
        }
        TabItemData.Release(data);
        data = null;
        if (_tgl_item != null)
        {
            _tgl_item.isOn = false;
            _tgl_item.group = null;
        }
    }

    static Vector3 rotate1 = new Vector3(0, 0, 90);
    static Vector3 rotate2 = new Vector3(0, 0, -90);
    void _OnValueChanged(bool isOn)
    {
        if (this.data == null)
        {
            txt_label_light.gameObject.SetActive(false);
            return;
        }
        if (this.data.useInPop)
        {
            if (isOn)
            {
                tran_pop_checkmask0.gameObject.SetActive(true);
                tran_pop_checkmask1.gameObject.SetActive(true);
                tran_pop_checkmask0.localEulerAngles = Vector3.zero;
                tran_pop_checkmask1.localEulerAngles = Vector3.zero;
                tran_pop_checkmask0.DOLocalRotate(rotate1, 0.4f).From();
                tran_pop_checkmask1.DOLocalRotate(rotate2, 0.4f).From();
                txt_label_light.gameObject.SetActive(true);
            }
            else
            {
                tran_pop_checkmask1.gameObject.SetActive(false);
                txt_label_light.gameObject.SetActive(false);
            }
        }
        else if(this.data.isSquare)
        {
            txt_label.gameObject.SetActive(false);
            txt_label_vertical.gameObject.SetActive(true);
            tran_squ_checkmark0.gameObject.SetActive(true);
            if(isOn)
            {
                tran_squ_checkmark1.gameObject.SetActive(true);
                txt_label_vertical_highlight.gameObject.SetActive(true);
            }
            else
            {
                tran_squ_checkmark1.gameObject.SetActive(false);
                txt_label_vertical_highlight.gameObject.SetActive(false);   
            }
        }
        else
        {
            if (isOn)
            {
                tran_checkmask0.gameObject.SetActive(true);
                tran_checkmask1.gameObject.SetActive(true);
                tran_checkmask0.localEulerAngles = Vector3.zero;
                tran_checkmask1.localEulerAngles = Vector3.zero;
                tran_checkmask0.DOLocalRotate(rotate1, 0.4f).From();
                tran_checkmask1.DOLocalRotate(rotate2, 0.4f).From();
                txt_label_light.gameObject.SetActive(true);
            }
            else
            {
                tran_checkmask1.gameObject.SetActive(false);
                txt_label_light.gameObject.SetActive(false);
            }
        }
    }

    public void SetData(TabItemData data)
    {
        tran_checkmask0.gameObject.SetActive(false);
        tran_checkmask1.gameObject.SetActive(false);
        tran_pop_checkmask0.gameObject.SetActive(data.useInPop);
        tran_pop_checkmask1.gameObject.SetActive(false);
        tran_squ_checkmark0.gameObject.SetActive(data.isSquare);
        tran_squ_checkmark1.gameObject.SetActive(false);

        txt_label.gameObject.SetActive(!data.isSquare);
        txt_label_vertical.gameObject.SetActive(data.isSquare);

        var rectBg = tra_background.transform.GetComponent<RectTransform>();
        rectBg.SetHeight(data.isSquare ? 141 : 100);


        _Init();
        this.data = data;
        txt_label.text = data.name;
        txt_label_light.text = data.name;
        txt_label_vertical.text = data.name;
        txt_label_vertical_highlight.text = data.name;
        // gameObject.SetActive(data.isShow);
    }   
}
