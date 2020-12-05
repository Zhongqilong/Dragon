using UnityEngine;

public class AllObjectBase : MonoBehaviour {
    public int rounds = 0;
    public int obFlag = 0;
    protected void Start () {
        Init ();
    }

    public virtual void Init () { }
    public virtual void OnShow (object param = null) {
    }

    public virtual void RestartGame () {
    }
}