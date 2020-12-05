using System.Collections;
using UnityEngine;
using Uqee.Utility;

public class Coin : PersonObjectBase {
    public ParticleSystem p1, p2;
    public GameObject coin;
    public override void OnShow (object param = null) {
        if (param != null) {
            var info = (GameInfoData) param;
            transform.localPosition = new Vector3 (info.X, info.Y, info.Z);
            rounds = info.Rounds;
        }
        p1.gameObject.SetActive (false);
        p2.gameObject.SetActive (false);
    }

    private void OnCollisionEnter (Collision other) {
        if (other.gameObject.tag == GameTag.MainRole) {
            DataCache.roundsCount -= 1;
            SaveData.gold_num += 1;
            SaveData.eatGold += 1;
            EventUtils.Dispatch ("EatCoin");
            p1.gameObject.SetActive (true);
            p2.gameObject.SetActive (true);
            coin.GetComponent<MeshCollider> ().enabled = false;
            this.StartCoroutine (_BeJump ());
        }
    }

    private IEnumerator _BeJump () {
        var scaleCount = 1;
        var scaleNum = 0.2f;
        var isBiggest = false;
        while (scaleCount > 0) {
            if (!isBiggest) {
                transform.localPosition += new Vector3 (0, scaleNum, 0);
                if (scaleCount > 15) {
                    isBiggest = true;
                } else
                    scaleCount += 1;
            } else {
                transform.localPosition -= new Vector3 (0, scaleNum * 2, 0);
                scaleCount -= 1;
            }

            yield return new WaitForSeconds (0.01f);
        }
        transform.gameObject.SetActive (false);
    }
}