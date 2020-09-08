using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Uqee.Utility;
using UnityEngine.UI;

public interface EmotionSpriteProvider
{
    bool IsStatic(string spriteName);
    void GetSprite(string spriteName, Action<Sprite> completeCall);
    void GetSprites(string atlas, Action<Sprite[], int> completeCall);
}


//请立即装备<a href=mujian>[木剑]</a><quad name=xb_b size=25 width=1 />武器
/// <summary>
/// 文本控件，支持超链接、图片
/// </summary>
[AddComponentMenu("UI/LinkImageText", 10)]
public class LinkImageText : Text, IPointerClickHandler
{
    /// <summary>
    /// 图片池
    /// </summary>
    protected readonly List<Image> m_ImagesPool = new List<Image>();

    /// <summary>
    /// 图片的最后一个顶点的索引
    /// </summary>
    private readonly List<int> m_ImagesVertexIndex = new List<int>();

    /// <summary>
    /// 超链接信息列表
    /// </summary>
    [SerializeField]
    private List<HrefInfo> m_HrefInfos = new List<HrefInfo>();
    public List<HrefInfo> HrefBounds { get { return m_HrefInfos; } }

    /// <summary>
    /// 下划线
    /// </summary>
    private ImageLine m_underLine;

    /// <summary>
    /// 文本构造器
    /// </summary>
    protected static readonly StringBuilder s_TextBuilder = new StringBuilder();

    [SerializeField]
    private int m_LineCount;
    public int LineCount => m_LineCount;
    private static List<UILineInfo> lineInfos = new List<UILineInfo>();
    private static List<UICharInfo> charInfos = new List<UICharInfo>();
    [SerializeField]
    private bool oneLineWrap = false;

    [Serializable]
    public class HrefClickEvent : UnityEvent<string> { }

    [SerializeField]
    private HrefClickEvent m_OnHrefClick = new HrefClickEvent();

    public Action<Vector2> OnTotalClick { get; set; }
    public string LinkTextColor = "#00b911";
    /// <summary>
    /// 超链接点击事件
    /// </summary>
    public HrefClickEvent onHrefClick
    {
        get { return m_OnHrefClick; }
        set { m_OnHrefClick = value; }
    }

    public static LinkImageText ActiveText { get; private set; }

    /// <summary>
    /// 正则取出所需要的属性
    /// </summary>
    private static readonly Regex s_ImageRegex =
        new Regex(@"<quad name=(.+?) size=(\d*\.?\d+%?) width=(\d*\.?\d+%?) />", RegexOptions.Singleline);

    /// <summary>
    /// 超链接正则
    /// 嵌套会导致很大问题..
    /// 嵌套需要
    /// </summary>
    private static readonly Regex s_HrefRegex =
        new Regex(@"<a href=([^>\n\s]+)>(.*?)(</a>)", RegexOptions.Singleline);

    private static readonly Regex s_ColorRegex =
       new Regex(@"<color='([^>\n\s]+)'>(.*?)(</color>)", RegexOptions.Singleline);

    public override void SetVerticesDirty()
    {
        UpdateQuadImage();
        base.SetVerticesDirty();
    }

    /// <summary>重池子中拿,可能出现表情不刷新问题</summary>
    public void OnReset()
    {
        m_ImagesPool.RemoveNull();
        GetComponentsInChildren<Image>(true, m_ImagesPool);
        foreach(var e in m_ImagesPool)
        {
            var binder = e.GetComponent<ImageResourceBinder>();
            binder.Reset();
        }
    }

    [SerializeField]
    private string m_rawText = string.Empty;
    public string m_OutputText => m_Text;
    public string RawText => m_rawText;
    public override string text
    {
        get
        {
            return m_Text;
        }
        set
        {
            if (String.IsNullOrEmpty(value))
            {
                if (String.IsNullOrEmpty(m_Text))
                    return;
                m_Text = "";
                m_rawText = "";
                SetVerticesDirty();
                if(m_underLine != null)
                {
                    m_underLine.gameObject.SetActive(false);
                }
            }
            else if (m_rawText != value)
            {
                m_rawText = value;
                m_Text = GetOutputText(m_rawText);
                Vector2 extents = rectTransform.rect.size;
                var settings = GetGenerationSettings(extents);
                cachedTextGenerator.PopulateWithErrors(m_Text, settings, gameObject);
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }

    private static Type[] imgsType = new Type[3] { typeof(RectTransform), typeof(Image) ,typeof(Canvas)};
    protected void UpdateQuadImage()
    {
#if UNITY_EDITOR
        if ((UnityEditor.PrefabUtility.GetPrefabAssetType(this) == UnityEditor.PrefabAssetType.Model
            || (UnityEditor.PrefabUtility.GetPrefabAssetType(this) == UnityEditor.PrefabAssetType.Variant))
            && UnityEditor.PrefabUtility.GetPrefabInstanceStatus(this)== UnityEditor.PrefabInstanceStatus.Connected)
        {

        }
#endif
        this.cachedTextGenerator.GetLines(lineInfos);
        m_LineCount = 0;
        if (lineInfos.Count > 0)
        {
            for (int i = lineInfos.Count - 1; i >= 0; i--)
            {
                if (lineInfos[i].startCharIdx < m_Text.Length)
                {
                    m_LineCount = i + 1;//不成功的那一个;
                    break;
                }
            }
        }

        if (oneLineWrap && m_LineCount > 0)
        {
            cachedTextGenerator.GetCharacters(charInfos);
            if (m_Text.Length > charInfos.Count)
            {
                s_TextBuilder.Length = 0;
                s_TextBuilder.Append(m_Text);
                m_Text = TextUtility.ReplaceOver(m_Text, charInfos.Count, s_TextBuilder);
            }
        }

        m_ImagesVertexIndex.Clear();
        //当前表情数量
        var emotionCnt = 0;
        //quad占据字符总数
        var quadCnt = 0;
        var noColorTxt = StringUtils.ClearAllColor(m_Text);
        foreach (Match match in s_ImageRegex.Matches(noColorTxt))
        {
            //quad字符串在渲染时候是不会创建的，所以我们算三角形顶点的时候要扣除这些字符的占位
            //（1个字符渲染生成4个顶点），否则此处算出的endindex是远远超过实际UIMesh的顶点数量的
            var picIndex = match.Index - quadCnt+emotionCnt;
            quadCnt += match.Length;
            emotionCnt++;
            var endIndex = picIndex * 4 + 3;
            m_ImagesVertexIndex.Add(endIndex);
            m_ImagesPool.RemoveNull();
            if (m_ImagesPool.Count == 0)
            {
                GetComponentsInChildren<Image>(m_ImagesPool);
            }
            var spriteName = match.Groups[1].Value;
            var size = float.Parse(match.Groups[2].Value);
            if (m_ImagesVertexIndex.Count > m_ImagesPool.Count)
            {
                //var resources = new DefaultControls.Resources();
                //var go = DefaultControls.CreateImage(resources);
                var go = new GameObject("emotion", imgsType);
                go.layer = gameObject.layer;
                var rt = go.transform as RectTransform;
                if (rt)
                {
                    rt.SetParent(rectTransform);                   
                    rt.localRotation = Quaternion.identity;
                    rt.localScale = Vector3.one;
                }
                m_ImagesPool.Add(go.GetComponent<Image>());
            }

           
            var img = m_ImagesPool[m_ImagesVertexIndex.Count - 1];
            img.rectTransform.sizeDelta = new Vector2(size, size);
            Vector2 parentSize = rectTransform.sizeDelta; 
            Vector2 offset = new Vector2(parentSize.x * 0.5f, -parentSize.y * 0.5f);
            img.rectTransform.localPosition = offset;
            

            PlayImage(img, spriteName);
            img.enabled = true;
        }

        for (var i = m_ImagesVertexIndex.Count; i < m_ImagesPool.Count; i++)
        {
            if (m_ImagesPool[i])
            {
                m_ImagesPool[i].enabled = false;
            }
        }

        if (m_HrefInfos.Count > 0)
        {
            if (!showUnderLine)
                return;
            if (m_underLine == null)
            {
                m_underLine = GetComponentInChildren<ImageLine>();
                if (m_underLine == null)
                {
                    var g = new GameObject("line", typeof(ImageLine));
                    m_underLine = g.GetComponent<ImageLine>();
                }
            }

            var rt = m_underLine.transform as RectTransform;
            if (rt)
            {
                rt.SetParent(rectTransform);
                rt.localPosition = Vector3.zero;
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
                rt.sizeDelta = Vector2.zero;
            }
            m_underLine.raycastTarget = false;
            m_underLine.imageText = this;
            m_underLine.enabled = true;
            m_underLine.gameObject.SetActive(true);
        }
        else
        {
            if(m_underLine!=null)
                m_underLine.enabled = false;
        }
    }

    bool showUnderLine = true;
    public void SetUnderLineState(bool show)
    {
        showUnderLine = show;
        if (m_underLine != null)
            m_underLine.enabled = show;
    }

    public void ForceUpdateLine()
    {
        if (m_underLine != null)
        {
            m_underLine.rectTransform.SetAnchorX(Mathf.RoundToInt(m_underLine.rectTransform.anchoredPosition.x));
            m_underLine.SetAllDirty();
        }
    }

    public void ForceEmotion()
    {
        if (m_ImagesPool != null)
        {
            for (int i = 0; i < m_ImagesPool.Count; i++)
            {
                if (m_ImagesPool[i] != null && m_ImagesPool[i].enabled)
                {
                    Utility.Reactive(m_ImagesPool[i]);
                }
            }
        }
    }

    private void PlayImage(Image img, string spriteName)
    {
        var binder = img.GetOrAddComponent<ImageResourceBinder>();
        binder.UpdateImageRef();
        binder.SetSource(spriteName);
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);
        UIVertex vert = new UIVertex();
        UIVertex endVert = new UIVertex();
        
        for (var i = 0; i < m_ImagesVertexIndex.Count; i++)
        {
            var endIndex = m_ImagesVertexIndex[i];
            var rt = m_ImagesPool[i].rectTransform;
            var size = rt.sizeDelta;
            List<UIVertex> listUIVertex = new List<UIVertex>();
            toFill.GetUIVertexStream(listUIVertex);
            if (endIndex < toFill.currentVertCount)
            {
                toFill.PopulateUIVertex(ref vert, endIndex);
                var worldPos = rectTransform.TransformPoint(vert.position);
                worldPos.z = 0;
                var locPos = rectTransform.InverseTransformPoint(worldPos);
                //Debug.Log(vert.position + "+_+"  + locPos + "++"+RectTransformUtility.WorldToScreenPoint(UIManager.I.cam_UICam, worldPos));

                rt.localPosition = locPos + new Vector3(size.x / 2, size.x / 3, 0);
                //rt.position = worldPos + new Vector3(size.x/2,size.x/2,0);
                //rt.anchoredPosition = new Vector2(vert.position.x + size.x / 2, vert.position.y + size.y / 2);
                // 抹掉左下角的小黑点
                toFill.PopulateUIVertex(ref vert, endIndex - 3);
                var pos = vert.position;
                for (int j = endIndex, m = endIndex - 3; j > m; j--)
                {
                    toFill.PopulateUIVertex(ref vert, j);
                    vert.position = pos;
                    toFill.SetUIVertex(vert, j);
                }
            }
        }

        // 处理超链接包围框
        foreach (var hrefInfo in m_HrefInfos)
        {
            hrefInfo.boxes.Clear();
            if (hrefInfo.startIndex >= toFill.currentVertCount)
            {
                continue;
            }

            // 将超链接里面的文本顶点索引坐标加入到包围框
            toFill.PopulateUIVertex(ref vert, hrefInfo.startIndex);
            var pos = vert.position;
            var bounds = new Bounds(pos, Vector3.zero);

            if (hrefInfo.endIndex > hrefInfo.startIndex)
            {
                toFill.PopulateUIVertex(ref endVert, hrefInfo.startIndex + 2);
                bounds.Encapsulate(endVert.position);
                //UnityEngine.Debug.Log(":::bounds:" + bounds + ":" + pos + "=>" + endVert.position + "|" + (m_OutputText.Length > (hrefInfo.startIndex / 4) ? m_OutputText[hrefInfo.startIndex / 4].ToString() : "|"));
            }

            for (int i = hrefInfo.startIndex+1, m = hrefInfo.endIndex; i < m; i+=4)
            {
                if (i >= toFill.currentVertCount)
                {
                    break;
                }

                toFill.PopulateUIVertex(ref vert, i);
                toFill.PopulateUIVertex(ref endVert, i + 2);

                pos = vert.position;
                if (pos.x < bounds.min.x) // 换行重新添加包围框
                {
                    hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                    bounds = new Bounds(pos, Vector3.zero);
                    bounds.Encapsulate(endVert.position);
                }
                else
                {
                    bounds.Encapsulate(pos); // 扩展包围框
                    bounds.Encapsulate(endVert.position);
                }
            }
            hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
        }

        if (m_underLine != null)
        {
            m_underLine.UpdateMesh();
        }
    }

    public static Color FromRGBString(string s)
    {
        if (s.Contains("#"))
        {
            s = s.Replace("#", "0x");
        }
        var val = Convert.ToUInt32(s, 16);
        return new Color((val >> 16 & 0xff) / 255f, (val >> 8 & 0xff) / 255f, (val & 0xff) / 255f, 1);
    }

    /// <summary>
    /// 获取超链接解析后的最后输出文本
    /// </summary>
    /// <returns></returns>
    protected virtual string GetOutputText(string outputText)
    {
        return GetString(outputText, LinkTextColor, m_HrefInfos);
    }

    public static string GetString(string outputText,string LinkTextColor, List<HrefInfo> hrefInfos)
    {
        s_TextBuilder.Length = 0;
        hrefInfos?.Clear();
        var indexText = 0;
        foreach (Match match in s_HrefRegex.Matches(outputText))
        {
            s_TextBuilder.Append(outputText.Substring(indexText, match.Index - indexText));
            s_TextBuilder.Append("<color=");  // 超链接颜色
            s_TextBuilder.Append(LinkTextColor);  // 超链接颜色
            s_TextBuilder.Append(">");  // 超链接颜色

            var group = match.Groups[1];
            var hrefInfo = new HrefInfo
            {
                startIndex = s_TextBuilder.Length * 4, // 超链接里的文本起始顶点索引
                endIndex = (s_TextBuilder.Length + match.Groups[2].Length - 1) * 4 + 3,
                name = group.Value.Replace("\"", string.Empty)
            };

            if (match.Groups[2].Value.Contains("color"))
            {
                var colorMatch = s_ColorRegex.Match(match.Groups[2].Value);
                if (colorMatch.Success)
                {
                    hrefInfo.color = FromRGBString(colorMatch.Groups[1].Value);
                }
                else
                {
                    hrefInfo.color = FromRGBString(LinkTextColor);
                }
            }
            else
            {
                hrefInfo.color = FromRGBString(LinkTextColor);
            }

            hrefInfos?.Add(hrefInfo);

            s_TextBuilder.Append(match.Groups[2].Value);
            s_TextBuilder.Append("</color>");
            indexText = match.Index + match.Length;
        }
        s_TextBuilder.Append(outputText.Substring(indexText, outputText.Length - indexText));
        return s_TextBuilder.ToString();
    }

    protected HrefInfo CheckHref(Vector2 lp)
    {
        foreach (var hrefInfo in m_HrefInfos)
        {
            var boxes = hrefInfo.boxes;
            for (var i = 0; i < boxes.Count; ++i)
            {
                if (boxes[i].Contains(lp))
                {
                    return hrefInfo;
                }
            }
        }
        return null;
    }

    virtual protected void OnClickAt(Vector2 lp)
    {
        var href = CheckHref(lp);
        if (href != null)
        {
            m_OnHrefClick.Invoke(href.name);
        }
        else
        {
            OnTotalClick?.Invoke(lp);
        }
    }

    /// <summary>
    /// 点击事件检测是否点击到超链接文本
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out lp);
        ActiveText = this;
        OnClickAt(lp);
    }

    protected override void OnDestroy()
    {
        this.onHrefClick?.RemoveAllListeners();
        this.OnTotalClick = null;
        if (ActiveText == this)
        {
            ActiveText = null;
        }
        base.OnDestroy();
    }

    /// <summary>
    /// 超链接信息类
    /// </summary>
    [Serializable]
    public class HrefInfo
    {
        public int startIndex;

        public int endIndex;

        public string name;

        public Color color;

        public List<Rect> boxes = new List<Rect>();
    }
}
