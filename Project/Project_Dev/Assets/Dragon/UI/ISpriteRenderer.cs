using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpriteRenderer
{
    void SetSprite(Sprite sprite);
}

public static class SpriteRendererUtils
{
    public static ISpriteRenderer _NullRender = new NullImageRenderer();
    public static bool IsNullRender(ISpriteRenderer renderer)
    {
        return _NullRender == renderer;
    }

    public static ISpriteRenderer GetRenderer(SpriteRenderer image)
    {
        return new UISpriteRenderer(image);
    }

    public static ISpriteRenderer GetRenderer(UnityEngine.UI.Image image)
    {
        return new UIImageRenderer(image);
    }

    public static ISpriteRenderer GetNullRenderer()
    {
        return _NullRender;
    }

    internal class UISpriteRenderer : ISpriteRenderer
    {
        private SpriteRenderer image;
        public UISpriteRenderer(SpriteRenderer image)
        {
            this.image = image;
        }

        void ISpriteRenderer.SetSprite(Sprite sprite)
        {
            image.sprite = sprite;
        }
    }

    internal class UIImageRenderer : ISpriteRenderer
    {
        private UnityEngine.UI.Image image;
        public UIImageRenderer(UnityEngine.UI.Image image)
        {
            this.image = image;
        }

        void ISpriteRenderer.SetSprite(Sprite sprite)
        {
            image.sprite = sprite;
            image.rectTransform.SetAnchorX(Mathf.RoundToInt(image.rectTransform.anchoredPosition.x));
            image.SetLayoutDirty();
        }
    }

    internal class NullImageRenderer : ISpriteRenderer
    {
        public NullImageRenderer()
        {

        }

        void ISpriteRenderer.SetSprite(Sprite sprite)
        {

        }
    }
}
