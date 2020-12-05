using UnityEngine;

public class Buff : PersonObjectBase
{
    public override void OnShow(object param = null)
    {
        if (param != null)
        {
            var info = (GameInfoData) param;
            transform.localPosition = new Vector3(info.X, info.Y, info.Z);
            rounds = info.Rounds;
        }
        obFlag = 2;
    }
}