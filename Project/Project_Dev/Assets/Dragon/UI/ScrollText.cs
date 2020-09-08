using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public class ScrollText : MonoBehaviour
{
    [SerializeField]
    [Tooltip("按最高位起始顺序设置每位数字Text（显示组）")]
    private List<Text> _numbers;
    [SerializeField]
    [Tooltip("按最高位起始顺序设置每位数字Text（替换组）")]
    private List<Text> _unactiveNumbers;
    /// <summary>
    /// 动画时长
    /// </summary>
    [SerializeField]
    private float _duration = 1.5f;
    /// <summary>
    /// 数字每次滚动时长
    /// </summary>
    [SerializeField]
    private float _rollingDuration = 0.05f;
    /// <summary>
    /// 数字每次变动数值
    /// </summary>
    private int _speed;
    /// <summary>
    /// 滚动延迟（每进一位增加一倍延迟，让滚动看起来更随机自然）
    /// </summary>
    [SerializeField]
    private float _delay = 0.008f;
    /// <summary>
    /// Text文字宽高
    /// </summary>
    private Vector2 _numberSize;
    /// <summary>
    /// 当前数字
    /// </summary>
    private int _curNumber;
    /// <summary>
    /// 起始数字
    /// </summary>
    private int _fromNumber;
    /// <summary>
    /// 最终数字
    /// </summary>
    private int _toNumber;
    /// <summary>
    /// 各位数字的缓动实例
    /// </summary>
    private List<Tweener> _tweener = new List<Tweener>();
    /// <summary>
    /// 是否处于数字滚动中
    /// </summary>
    private bool _isJumping;
    /// <summary>
    /// 滚动完毕回调
    /// </summary>
    public Action OnComplete;

    private void Awake()
    {
        if (_numbers.Count == 0 || _unactiveNumbers.Count == 0)
        {
            //MediaUnity.Debugger.LogError("[JumpingNumberTextComponent] 还未设置Text组件!");
            return;
        }
        _numberSize = _numbers[0].rectTransform.sizeDelta;
    }

    public float duration
    {
        get { return _duration; }
        set
        {
            _duration = value;
        }
    }

    private float _different;
    public float different
    {
        get { return _different; }
    }

    public int _letterCount { get; private set; }

    public void Change(int from, int to)
    {
        bool isRepeatCall = _isJumping && _fromNumber == from && _toNumber == to;
        if (isRepeatCall) return;

        bool isContinuousChange = (_toNumber == from) && ((to - from > 0 && _different > 0) || (to - from < 0 && _different < 0));
        if (_isJumping && isContinuousChange)
        {
        }
        else
        {
            _fromNumber = from;
            _curNumber = _fromNumber;
        }
        _toNumber = to;

        _different = _toNumber - _fromNumber;
        _speed = (int)Math.Ceiling(_different / (_duration * (1 / _rollingDuration)));
        _speed = _speed == 0 ? (_different > 0 ? 1 : -1) : _speed;
        _letterCount = Mathf.Max(from, to).ToString().Length;
        SetNumber(_curNumber, false, _letterCount);
        _isJumping = true;
        StopCoroutine("DoJumpNumber");
        StartCoroutine("DoJumpNumber");
    }

    public int number
    {
        get { return _toNumber; }
        set
        {
            if (_toNumber == value) return;
            Change(_curNumber, _toNumber);
        }
    }

    IEnumerator DoJumpNumber()
    {
        while (true)
        {
            if (_speed > 0)//增加
            {
                _curNumber = Math.Min(_curNumber + _speed, _toNumber);
            }
            else if (_speed < 0) //减少
            {
                _curNumber = Math.Max(_curNumber + _speed, _toNumber);
            }
            var tmpCount = _curNumber.ToString().Length;
            SetNumber(_curNumber, true, tmpCount);

            if (_curNumber == _toNumber)
            {
                StopCoroutine("DoJumpNumber");
                _isJumping = false;
                if (OnComplete != null) OnComplete();
                yield return null;
            }
            yield return new WaitForSeconds(_rollingDuration);
        }
    }

    /// <summary>
    /// 设置战力数字
    /// </summary>
    /// <param name="v"></param>
    /// <param name="isTween"></param>
    public void SetNumber(int v, bool isTween,int letterCount)
    {
        string s = v.ToString();

        if (!isTween)
        {
            for (int i = 0; i < _numbers.Count; i++)
            {
                _numbers[i].enabled = i < letterCount;
                if (i < s.Length)
                {
                    _numbers[i].text = s[i] + string.Empty;
                }
                else{
                    _numbers[i].text = "";
                }
            }
        }
        else
        {
            while (_tweener.Count > 0)
            {
                _tweener[0].Complete();
                _tweener.RemoveAt(0);
            }

            for (int i = 0; i < _numbers.Count; i++)
            {
                _unactiveNumbers[i].enabled = i < letterCount;
                if (i < s.Length)
                {
                    _unactiveNumbers[i].text = s[i] + string.Empty;
                }
                else
                {
                    _unactiveNumbers[i].text = "";
                }

                _unactiveNumbers[i].rectTransform.anchoredPosition = new Vector2(_unactiveNumbers[i].rectTransform.anchoredPosition.x, (_speed > 0 ? -1 : 1) * _numberSize.y);
                _numbers[i].rectTransform.anchoredPosition = new Vector2(_unactiveNumbers[i].rectTransform.anchoredPosition.x, 0);

                if (_unactiveNumbers[i].text != _numbers[i].text)
                {
                    DoTween(_numbers[i], (_speed > 0 ? 1 : -1) * _numberSize.y, _delay * i);
                    DoTween(_unactiveNumbers[i], 0, _delay * i);

                    Text tmp = _numbers[i];
                    _numbers[i] = _unactiveNumbers[i];
                    _unactiveNumbers[i] = tmp;
                }
            }
        }
    }

    public void DoTween(Text text, float endValue, float delay)
    {
        Tweener t = DOTween.To(() => text.rectTransform.anchoredPosition, (x) =>
        {
            text.rectTransform.anchoredPosition = x;
        }, new Vector2(text.rectTransform.anchoredPosition.x, endValue), _rollingDuration - delay).SetDelay(delay);
        _tweener.Add(t);
    }


    [ContextMenu("测试数字变化")]
    public void TestChange()
    {
        Change(UnityEngine.Random.Range(1, 1), UnityEngine.Random.Range(1, 100000));
    }

}