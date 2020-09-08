using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public static class GraphicsExtensions
{
    public static void SetAlpha(this MaskableGraphic graphics,float alpha)
    {
        var color = graphics.color;
        color.a = alpha;
        graphics.color = color;
    }
    /// <summary>
    /// 设置成灰色
    /// </summary>
    /// <param name="graphic"></param>
    /// <param name="gray"></param>
    /// <param name="children"></param>
    public static void SetGray(this Graphic graphic, bool gray = true, bool children = false)
    {
        ResUtils.StartSetGray(graphic, gray, children);
    }

    /// <summary>
    /// 改RGB，不改Alpha
    /// </summary>
    /// <param name="graphic"></param>
    /// <param name="color"></param>
    public static void SetColor(this Graphic graphic, Color color)
    {
        color.a = graphic.color.a;
        graphic.color = color;
    }

    /// <summary>
    /// 改Alpha，不改RGB
    /// </summary>
    /// <param name="graphic"></param>
    /// <param name="alpha"></param>
    public static void SetAlpha(this Graphic graphic, float alpha)
    {
        var c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }

    public static void SetAlpha(this SpriteRenderer comp, float alpha)
    {
        if (comp == null) return;
        var c = comp.color;
        c.a = alpha;
        comp.color = c;
    }
}
