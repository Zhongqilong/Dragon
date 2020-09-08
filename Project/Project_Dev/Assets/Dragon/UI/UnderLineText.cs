using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnderLineText : MonoBehaviour {

    private Text linkText;
    private Text underline;

    void Awake()
    {
        linkText = GetComponent<Text>();
    }
    public string text
    {
        get { return linkText.text; }
        set
        {
            linkText.text = value;
            CreateLink();
        }
    }
    public void CreateLink()
    {
        if (linkText == null)
            return;

        //克隆Text，获得相同的属性  
        if (underline == null)
        {
            underline = Instantiate(linkText);
            underline.name = "Underline";
            underline.transform.SetParent(linkText.transform, false);
            Destroy(underline.transform.GetComponent<Button>());
            Destroy(underline.transform.GetComponent<UnderLineText>());
        }
        else
        {
            underline.color = linkText.color;
            linkText.fontSize = linkText.fontSize;
        }


        RectTransform rt = underline.rectTransform;

        //设置下划线坐标和位置  
        rt.anchoredPosition3D = Vector3.zero;
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.anchorMin = Vector2.zero;

        underline.text = "_";
        float perlineWidth = underline.preferredWidth;      //单个下划线宽度  

        float width = linkText.preferredWidth;
        int lineCount = (int)Mathf.Round(width / perlineWidth);
        for (int i = 1; i < lineCount; i++)
        {
            underline.text += "_";
        }
    }
}
