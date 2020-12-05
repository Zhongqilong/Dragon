using UnityEngine.UI;
using Uqee.Utility;

public class TestView : ViewBase {
    public Text txt_opentime;
    public Button btn_start;
    public Button btn_restart;
    public Button btn_next;
    public Button btn_buff;
    public override void OnShow (object param = null) {
        txt_opentime.text = param.ToString ();
        //UIManager.I.ShowView<TestView1>("111222");

        btn_start.onClick.AddListener (_OnclickBtnStart);
        btn_restart.onClick.AddListener (_OnclickBtnRestart);
        btn_next.onClick.AddListener(_OnclickBtnNext);

        EventUtils.AddListener("OnClickNextCheckPoint", _OnClickNextCheckPoint);
    }

    private void _OnclickBtnStart () {
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
    }

    public override void OnHide()
    {
        base.OnHide();
        EventUtils.RemoveListener("OnClickNextCheckPoint", _OnClickNextCheckPoint);
    }
}