using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Uqee.Utility;

public class ResultView : ViewBase {
    /// <summary>
    /// 三种Transform对应胜利失败界面(失败有两个界面)
    /// </summary>
    public Transform tra_lose1;
    public Transform tra_lose2;
    public Transform tra_win;
    public Image img_bar;
    public Button btn_free_revive, btn_exit;
    public Button btn_again;
    public Button btn_next;
    public float bar_percent = 1;

    public override void Init () {
        btn_next.onClick.AddListener (_OnClickBtnNext);
        btn_free_revive.onClick.AddListener(_OnClickFreeRevive);
        btn_exit.onClick.AddListener(_OnClickBtnExit);
        btn_again.onClick.AddListener(_OnClickBtnAgain);
    }

    public override void OnShow (object param = null) {
        bool isWin = (bool) param;
        if (isWin) {
            _ShowWin ();
            return;
        }
        _ShowLose1 ();
    }

    private void _ShowLose1 () {
        tra_lose1.gameObject.SetActive (true);
        _ShowLoseCountDown ();
    }

    private void _ShowLose2 () {
        tra_lose1.gameObject.SetActive (false);
        tra_lose2.gameObject.SetActive (true);
    }

    private void _ShowWin () {
        tra_win.gameObject.SetActive (true);
    }

    private void _ShowLoseCountDown () {
        this.StartCoroutine (BarCountDown ());
    }

    private IEnumerator BarCountDown () {
        while (true) {
            yield return new WaitForSeconds (0.01f);
            bar_percent -= 0.005f;
            if (bar_percent < 0) {
                _ShowLose2 ();
                yield break;
            }
            img_bar.fillAmount = bar_percent;
        }
    }

    private void _OnClickBtnNext () {
        EventUtils.Dispatch("PlayClickBtnMusic");
        UIManager.I.HideView("WelcomeView");
        UIManager.I.HideView("ResultView");
        SaveData.max_cp += 1;
        SaveData.eatGold = 0;
        // if (SaveData.curCheckPointNum > DataCache.max_cp)
        // {
        //     SaveData.curCheckPointNum = 1;
        // }
        SceneLoadManager.I.ShowScene (SaveData.curCheckPoint, () => {
            UIManager.I.ShowView<WelcomeView> ();
            SceneLoadManager.I.AddPerfabToSceneInit (()=>{
                EventUtils.Dispatch("ShowView");
            });
            //SceneLoadManager.I.ShowGameObjectByRounds (DataCache.rounds);
        }, true);

        //EventUtils.Dispatch("OnClickNextCheckPoint");
    }

    private void _OnClickFreeRevive()
    {
        EventUtils.Dispatch("PlayClickBtnMusic");
        UIManager.I.HideView("ResultView");
        ViewBase welcome = null;
        if (UIManager.I.GetViewCache("WelcomeView", out welcome))
        {
            welcome.GetComponent<WelcomeView>().HideBuffIcon();
        }
        DataCache.mainRoleIsDie = false;
        SceneLoadManager.I.ReStartSceneGame ();
        EventUtils.Dispatch("InitMainPos");
        EventUtils.Dispatch ("Start");
    }

    private void _OnClickBtnExit()
    {
        EventUtils.Dispatch("PlayClickBtnMusic");
        _ShowLose2();
    }

    private void _OnClickBtnAgain()
    {
        EventUtils.Dispatch("PlayClickBtnMusic");
        UIManager.I.HideView("ResultView");
        DataCache.mainRoleIsDie = false;
        DataCache.rounds = 1;
        SceneLoadManager.I.ReStartSceneGame ();
        EventUtils.Dispatch ("Start");
    }
}