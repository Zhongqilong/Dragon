using UnityEngine;
using UnityEngine.UI;

public class TestView1 : ViewBase
{
    public Text txt_opentime;

    public override void OnShow(object param = null)
    {
        txt_opentime.text = param.ToString();
    }
}