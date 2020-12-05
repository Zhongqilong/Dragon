using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Wrap : PersonObjectBase
{
    public override void OnShow(object param = null)
    {
        if (param != null)
        {
            var info = (GameInfoData) param;
            transform.localPosition = new Vector3(info.X, info.Y, info.Z);
            rounds = info.Rounds;
        }
    }
}