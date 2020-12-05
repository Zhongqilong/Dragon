using System;
using UnityEngine.UI;
using UnityEngine;
using Uqee.Utility;
using System.Collections;

public class WelcomeView : ViewBase {
    public Text txt_gold;
    public Text txt_combo;
    public Image img_buffbg;
    public Image img_buff;
    public Image img_combo;
    public Image img_shake;
    public Button btn_start;
    public RectTransform tra_checkpoint;
    private AudioSource _audioBg;
    private AudioSource _audioButton;
    private float _barPercent = 1.0f;
    private int _comboNum = 1;
    private bool _isComboing = false;
    private bool _isBeShake = false;
    private float _duration = 3.0f;
    private Vector3 shakeInitPos;
    public override void Init()
    {
        var allAudio = GetComponents<AudioSource>();
        txt_gold.text = SaveData.gold_num.ToString();
        _audioBg = allAudio[0];
        _audioButton = allAudio[1];
        _audioButton.Stop();
        btn_start.onClick.AddListener(_OnclickBtnStart);
        shakeInitPos = img_combo.transform.localPosition;
        gameObject.SetActive(false);
        EventUtils.AddListener("OnClickNextCheckPoint", _GoToNextScene);
        EventUtils.AddListener("BuffCDIcon", _BuffCDIcon);
        EventUtils.AddListener("PlayClickBtnMusic", _PlayClickBtnMusic);
        EventUtils.AddListener("EatCoin", _EatCoin);
        EventUtils.AddListener("ShowCombo", _ShowCombo);
        //EventUtils.AddListener("ShakeScreen", _ShakeScreen);
        EventUtils.AddListener("ShowView", _ShowView);
    }

    public override void OnShow (object param = null) {
        _audioBg.Play();
        _audioBg.loop = true;
        _InitCheckPoint();
    }

    public void HideBuffIcon()
    {
        img_buffbg.gameObject.SetActive(false);
        this.StopCoroutine(BarCountDown(0.0f));
    }

    private void _InitCheckPoint()
    {
        var img_green = tra_checkpoint.Find("img_green");
        var img_cp = tra_checkpoint.Find("img_cp");
        var tra_cp = tra_checkpoint.Find("tra_cp");
        var cp = SaveData.max_cp;
        if (cp > 5)
        {
            cp = SaveData.curCheckPointNum % 6;
        }
        var txt_cp = tra_cp.Find($"txt_cp{cp}");
        float next_pos = 74.4f;
        float next_percent = 0.215f;
        int cp_index = 0;
        cp_index = (int)Math.Floor(((double)SaveData.max_cp) / 5.0);
        if (SaveData.max_cp % 5 == 0)
        {
            cp_index = cp_index - 1;
        }
        for (int i = 0; i < tra_cp.childCount; i++)
        {
            var cur_cp = tra_cp.GetChild(i);
            var txt = cur_cp.GetComponent<Text>();
            txt.fontSize = 40;
            txt.text = (cp_index * 5 + i + 1).ToString();
            if (cp_index * 5 + i + 1 > SaveData.max_cp)
            {
                txt.color = new Color(0.427f, 0.419f, 0.419f);
                cur_cp.GetComponent<Shadow>().enabled = false;
            }else
            {
                txt.color = new Color(1, 1, 1);
                cur_cp.GetComponent<Shadow>().enabled = true;
            }
        }
        txt_cp.GetComponent<Text>().fontSize = 52;
        img_cp.localPosition = new Vector3(-147.8f + next_pos * (cp - 1), -2, 0);
        img_green.GetComponent<Image>().fillAmount = next_percent * (cp - 1);
    }

    private void _GoToNextScene()
    {
        img_combo.gameObject.SetActive(false);
        _InitCheckPoint();
    }

    private void _OnclickBtnStart () {
        _audioButton.gameObject.SetActive(true);
        _audioButton.Play();
        EventUtils.Dispatch ("Start");
        btn_start.gameObject.SetActive (false);
    }

    private void _OnclickBtnRestart () {
        btn_start.gameObject.SetActive (true);
        //SceneLoadManager.I.RemoveAllScene();
        //AppInit.OpenGame();
        DataCache.rounds = 1;
        SceneLoadManager.I.ReStartSceneGame ();
    }

    private void _OnclickBtnNext () {
        SaveData.curCheckPointNum += 1;
        DataCache.rounds = 1;
        SceneLoadManager.I.ShowScene (SaveData.curCheckPoint, () => {
            SceneLoadManager.I.AddPerfabToSceneInit();
            SceneLoadManager.I.ShowGameObjectByRounds(DataCache.rounds);
        }, true);
        btn_start.gameObject.SetActive (true);
    }

    private void _OnClickNextCheckPoint()
    {
        btn_start.gameObject.SetActive (true);
        this.StopCoroutine(ShowCombo());
        this.StopCoroutine(ShowComboAction());
    }

    public override void OnHide()
    {
        base.OnHide();
        EventUtils.RemoveListener("OnClickNextCheckPoint", _OnClickNextCheckPoint);
        EventUtils.RemoveListener("OnClickNextCheckPoint", _GoToNextScene);
        EventUtils.RemoveListener("BuffCDIcon", _BuffCDIcon);
        EventUtils.RemoveListener("PlayClickBtnMusic", _PlayClickBtnMusic);
        EventUtils.RemoveListener("EatCoin", _EatCoin);
        EventUtils.RemoveListener("ShowCombo", _ShowCombo);
        EventUtils.RemoveListener("ShakeScreen", _ShakeScreen);
        EventUtils.RemoveListener("ShowView", _ShowView);
    }

    private void _PlayClickBtnMusic()
    {
        //_audioButton.gameObject.SetActive(true);
        _audioButton.Play();
    }

    private void _BuffCDIcon(object param1 = null, object param2 = null)
    {
        img_buffbg.gameObject.SetActive(true);
        float buff_id = (float) param1;
        float buff_second = (float) param2;
        _barPercent = 1.0f;
        this.StartCoroutine (BarCountDown (0.01f / buff_second));
    }

    private void _EatCoin()
    {
        txt_gold.text = SaveData.gold_num.ToString();
    }

    private void _ShakeScreen()
    {
        if (!_isBeShake)
        {
            this.StartCoroutine(_BeShake());
        }
    }

    private void _ShowCombo()
    {
        img_combo.gameObject.SetActive(true);
        if (_isComboing)
        {
            _comboNum += 1;
            txt_combo.text = _comboNum.ToString();
            _duration = 3;
            //img_combo.fillAmount = 1;
            return;
        }
        img_combo.fillAmount = 0;
        _comboNum = 1;
        txt_combo.text = _comboNum.ToString();
        this.StartCoroutine(ShowCombo());
    }

    private void _ComboAction()
    {
        if (_isComboing)
            return;

        this.StartCoroutine(ShowComboAction());
    }

    private void _ShowView()
    {
        gameObject.SetActive(true);
    }

    private IEnumerator _BeShake () {
        var scaleCount = 1;
        var scaleNum = 0.0f;
        var isBiggest = false;
        _isBeShake = true;
        while (scaleCount > 0) {
            if (!isBiggest) {
                img_shake.color = new Vector4 (1, 1, 1, scaleNum);
                scaleNum += 0.17f;
                if (scaleCount > 6) {
                    isBiggest = true;
                }else
                    scaleCount += 1;
            } else
            {
                img_shake.color = new Vector4 (1, 1, 1, scaleNum);
                scaleNum -= 0.2f;
                scaleCount -= 1;
            }

            yield return new WaitForSeconds (0.01f);
        }
        _isBeShake = false;
    }

    private IEnumerator BarCountDown (float delta) {
        while (true) {
            yield return new WaitForSeconds (0.01f);
            _barPercent -= delta;
            if (_barPercent < 0) {
                img_buffbg.gameObject.SetActive(false);
                DataCache.can_eat_buff = true;
                yield break;
            }
            img_buff.fillAmount = _barPercent;
        }
    }

    private IEnumerator ShowCombo () {
        _isComboing = true;
        this.StartCoroutine(ShowComboAction());
        while (_duration > 0)
        {
            yield return new WaitForSeconds(0.5f);
            _duration -= 1;
        }
        this.StopCoroutine(ShowComboAction());
        _isComboing = false;
        img_combo.gameObject.SetActive(false);
        _duration = 3;
    }

    private IEnumerator ShowComboAction () {
        var timeDelta = 5;
        while(true)
        {
            img_combo.transform.localPosition = new Vector3(GetX(timeDelta), GetY(timeDelta), 0);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private float GetX(float change_x)
    {
        return shakeInitPos.x + UnityEngine.Random.Range(-1 * change_x, change_x);
    }

    private float GetY(float change_y)
    {
        return shakeInitPos.y + UnityEngine.Random.Range(-1 * change_y, change_y);
    }
}