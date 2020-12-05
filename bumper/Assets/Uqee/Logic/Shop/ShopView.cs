using UnityEngine;
using UnityEngine.UI;
using Uqee.Utility;

public class ShopView : ViewBase
{
    public ScrollViewEx srv_skin;
    public Button btn_text;
    public Button btn_exit;
    public Image img_gold;
    public Text txt_gold;

    private int _maxCount = 8;
    public override void Init()
    {
        srv_skin.Init(_SetSkinSrv, _maxCount);
    }

    public override void OnShow(object param = null)
    {

    }

    private void _SetSkinSrv(Transform trans, int index)
    {
        if (index > _maxCount)
            return;

        
    }
}