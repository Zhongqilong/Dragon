using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class ImageResourceBinder : MonoBehaviour
{
    private Image image;
    private string _spriteName;
    private UI2DSpriteAnimation _animation;
    /// <summary>加载精灵图片方法</summary>
    public static EmotionSpriteProvider funLoadSprite;

    public void UpdateImageRef()
    {
        if (image == null && this.gameObject != null)
        {
            image = this.GetComponent<Image>();
        }
    }

    public void Reset()
    {
        _spriteName = null;
        if (image != null)
        {
            image.sprite = null;
        }
    }

    bool _isStarted = false;
    private void Start()
    {
        _isStarted = true;
        _UpdateSprite(_spriteName);
    }

    /// <summary>
    /// 1. 单帧
    /// 2. 多帧
    /// </summary>
    /// <param name="spriteName"></param>
    public void SetSource(string spriteName)
    {
        bool changed = _spriteName != spriteName;
        _spriteName = spriteName;
        if (funLoadSprite == null)
            return;
        if (!_isStarted)
        {
            return;
        }
        if (changed)
        {
            _UpdateSprite(_spriteName);
        }
    }

    private void _UpdateSprite(string spriteName)
    {
        if (_animation != null)
        {
            _animation.enabled = false;
        }
        if (_spriteName != null)
        {
            if (funLoadSprite.IsStatic(spriteName))
            {
                funLoadSprite.GetSprite(spriteName, OnLoadSprite);
            }
            else
            {
                funLoadSprite.GetSprites(spriteName, OnLoadAtlas);
            }
        }
        else
        {
            if (image != null)
            {
                image.sprite = null;
            }
        }
    }

    private void OnLoadSprite(Sprite sprite)
    {
        UpdateImageRef();
        if (image != null)
        {
            image.sprite = sprite;
        }
    }

    private void OnLoadAtlas(Sprite[] sprites,int frameRate)
    {
        if (gameObject == null)
            return;
        UpdateImageRef();
        Uqee.Debug.Log(StrOpe.i + "OnLoadAtlas::" + (sprites != null ? sprites.Length : 0) + ":" + this.image + ":" + gameObject.name  + "::"+ transform.parent+ "++");
        if (sprites != null)
        {
            _animation = this.GetOrAddComponent<UI2DSpriteAnimation>();
            _animation.ForceUpdateRender();
            _animation.enabled = true;
            _animation.loop = true;
            _animation.framesPerSecond = frameRate;
            _animation.frames = sprites;
            _animation.ResetToBeginning();
            _animation.Play();
            if (image != null)
            {
                Utility.Reactive(image);
            }
        }
    }
}
