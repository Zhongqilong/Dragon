using UnityEngine;
using Uqee.Utility;

public class EndGO : AllObjectBase
{
    public Transform effect_end;
    public override void OnShow(object param = null)
    {
        if (param != null)
        {
            var info = (GameInfoData) param;
            transform.localPosition = new Vector3(info.X, info.Y, info.Z);
            rounds = info.Rounds;
        }
        EventUtils.AddListener("IsWin", _IsWin);
    }

    private void _IsWin()
    {
        effect_end.gameObject.SetActive(true);
    }

    private void OnDestroy() {
        EventUtils.RemoveListener("IsWin", _IsWin);
    }
}