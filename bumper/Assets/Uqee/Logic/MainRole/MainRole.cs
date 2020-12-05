using System;
using System.Collections;
using UnityEngine;
using Uqee.Utility;

public class MainRole : PersonObjectBase {
    //与相机的距离
    public float disCaX, disCaZ;
    public Transform tra_alleffect;
    public Transform tra_Sphere;
    private Vector2 _lastMove;
    private Transform _curCaTrans;
    private Transform _curBridge;
    private Transform _curWater1, _curWater2;
    private Vector3 _initWaterPos;
    private Vector3 _caInitPos;
    private AndroidJavaObject _shakeActivity;
    private GameInfoData _initGameInfo;
    private AudioSource _audioHit;
    private AudioSource _audioWin;
    private bool _canPass = true;
    private bool _otherTouch = true;
    private bool _checkCameraPos = true;
    private bool _isBeBigger = false;
    private float _buffSeconds = 0;
    public override void OnShow (object param = null) {
        // UIManager.I.ShowView<ResultView> (true);
        var allAudio = GetComponents<AudioSource> ();
        _audioHit = allAudio[0];
        _audioHit.Stop ();
        _audioWin = allAudio[1];
        _audioWin.Stop ();
        _initGameInfo = param as GameInfoData;
        _curCaTrans = SceneLoadManager.I.GetSceneNodeTrans (SaveData.curCheckPoint, "Camera");
        _curBridge = SceneLoadManager.I.GetSceneNodeTrans (SaveData.curCheckPoint, "Bridge");
        _curWater1 = SceneLoadManager.I.GetSceneNodeTrans (SaveData.curCheckPoint, "Water1");
        _curWater2 = SceneLoadManager.I.GetSceneNodeTrans (SaveData.curCheckPoint, "Water2");
        _initWaterPos = _curWater1.localPosition;
        this.StartCoroutine (_TranslateWater ());
        _caInitPos = _curCaTrans.localPosition;
        _rigidbody = transform.GetComponent<Rigidbody> ();
        _myAnimator = transform.GetComponent<Animator> ();
        _initPos = new Vector3 (_initGameInfo.X, _initGameInfo.Y, _initGameInfo.Z);
        obFlag = 1;
        _InitInfo ();
        EventUtils.AddListener ("EnemyIsDie", _EnemyIsDie);
        EventUtils.AddListener ("Start", _CanStart);
        EventUtils.AddListener ("MainRoleBeForce", _MainRoleBeForce);
        EventUtils.AddListener ("InitMainPos", _InitMainPos);
#if UNITY_ANDROID
        if (_shakeActivity == null) {
            AndroidJavaClass jc = new AndroidJavaClass ("com.Uqee.LittleGame.ClickShake");
            _shakeActivity = jc.GetStatic<AndroidJavaObject> ("_mClickShake");
        }
#endif
    }

    public override void RestartGame () {
        gameObject.SetActive (true);
        _InitInfo ();
    }

    private void _InitInfo () {
        StopSomeCoroutines();
        transform.localPosition = _initPos;
        DataCache.curMainPos = transform.position;
        _speed = _initGameInfo.Speed;
        _curVolumeCoefficient = _initGameInfo.VolumeCoefficient;
        transform.localScale = Vector3.one * _curVolumeCoefficient;
        DataCache.mainRoleIsDie = false;
        DataCache.curMainRoleVolumeCoefficient = _curVolumeCoefficient;
        transform.forward = Vector3.forward;
        _myAnimator.Play ("role_idle02");
        _myAnimator.SetBool ("PrepareState", false);
        _myAnimator.SetBool ("BeHit", false);
        _myAnimator.SetBool ("CanUp", true);
        transform.localPosition = _initPos;
        _curCaTrans.localPosition = _caInitPos;
        disCaX = transform.localPosition.x - _caInitPos.x;
        disCaZ = transform.localPosition.z - _caInitPos.z;
        //_RotateCamera ();
        DataCache.startGame = false;
        _ShowOrHideBuff (false);
        this.StartCoroutine (_CheckIsDie ());
        //重开和复活都需要走这个初始化函数
        if (DataCache.rounds == 1) {
            _InitBridge ();
        }
        //测试
        //_MainRoleBeForce();
    }

    private void _InitBridge () {
        for (int i = 0; i < _curBridge.childCount; i++) {
            _curBridge.GetChild (i).gameObject.SetActive (false);
        }
    }

    private void _ShowBridgeByRounds () {
        _curBridge.Find ($"b{DataCache.rounds}")?.gameObject.SetActive (true);
    }

    private void _PlayBeBigger () {
        _isBeBigger = true;
        this.StartCoroutine (_BeBigger ());
    }

    //短振动
    private void OnCollisionEnter (Collision other) {
        var go = other.gameObject;
        //var info = go.GetComponent<Enemy>();
        switch (go.tag) {
            // case GameTag.Enemy:
            //     return;
            case GameTag.Wrap:
                if (!DataCache.mainRoleIsDie) {
                    _myAnimator.Play("role_down");
                    _myAnimator.SetBool ("CanUp", false);
                    _DieFunc ();
                }
                return;
            case GameTag.End:
                if (_canPass) {
                    _canPass = false;
                    DataCache.isWin = true;
                    _audioWin.Play ();
                    _RotateCamera ();
                }
                return;
            case GameTag.Gold:
            #if UNITY_ANDROID
                _shakeActivity.Call ("ClickShake");
            #endif
                return;
            case GameTag.Buff:
                //_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                //if (DataCache.can_eat_buff) {
                DataCache.can_eat_buff = false;
                other.gameObject.SetActive (false);
                EventUtils.Dispatch ("BuffCDIcon", 3.0f, 2.0f);
                _PalyEffect (1, 0.5f, () => { _PalyEffect (3, 5); });
                //}
                return;
            default:
                return;
        }
    }

    private void OnCollisionExit (Collision other) {
        var go = other.gameObject;
        if (other.gameObject.tag == GameTag.Buff) {
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }
    }

    private IEnumerator _ContinueRun () {
        while (true) {
            yield return new WaitForSeconds (0.5f);
            if (_myAnimator.GetCurrentAnimatorStateInfo (0).IsName ("role_idle02")) {
                _rigidbody.velocity = Vector3.zero;
                _myAnimator.SetBool ("BeHit", false);
                _rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                yield break;
            }
        }
    }

    private IEnumerator _BeBigger () {
        var scaleCount = 1;
        var scaleNum = 0.1f;
        var isBiggest = false;
        while (scaleCount > 0) {
            if (!isBiggest) {
                tra_Sphere.localScale += new Vector3 (scaleNum, scaleNum, scaleNum);
                if (scaleCount > 3) {
                    isBiggest = true;
                } else
                    scaleCount += 1;
            } else {
                tra_Sphere.localScale -= new Vector3 (scaleNum, scaleNum, scaleNum);
                scaleCount -= 1;
            }

            yield return new WaitForSeconds (0.01f);
        }
        _isBeBigger = false;
    }

    private IEnumerator _WaitAndRunCallBack (float seconds, Action call_back = null) {
        yield return new WaitForSeconds (seconds);
        call_back?.Invoke ();
    }

    private IEnumerator _PlayBuff (Action call_back = null) {
        while (true) {
            if (_buffSeconds < 0) {
                call_back?.Invoke ();
                yield break;
            }
            _buffSeconds -= Time.deltaTime;
            yield return new WaitForSeconds (Time.deltaTime);
        }
    }

    private IEnumerator _CheckIsDie () {
        while (true) {
            if ((_initPos.y - transform.localPosition.y > 1) && !DataCache.mainRoleIsDie) {
                _DieFunc ();
                yield break;
            }
            yield return new WaitForSeconds (0.1f);
        }
    }

    private IEnumerator _TranslateWater () {
        var timeDelta_Z = 0.08f;
        var min_z = _initWaterPos.z - 100;
        var curWater = _curWater1;
        while (true) {
            if (_curWater1.localPosition.z < min_z) {
                curWater = _curWater2;
                _curWater2.gameObject.SetActive (true);
                _curWater1.localPosition = _initWaterPos;
                _curWater1.gameObject.SetActive (false);
            }
            if (_curWater2.localPosition.z < min_z) {
                curWater = _curWater1;
                _curWater1.gameObject.SetActive (true);
                _curWater2.localPosition = _initWaterPos;
                _curWater2.gameObject.SetActive (false);
            }
            curWater.localPosition -= new Vector3 (0, 0, timeDelta_Z);
            yield return new WaitForSeconds (0.01f);
        }
    }

    private void Update () {
        //不能触屏表示当前状态是倒下或者死亡状态
        DataCache.mainRoleIsDown = !_CanTouch ();
        if (!DataCache.mainRoleIsDown && Input.touchCount > 0) {
            var touch_info = Input.GetTouch (0);
            _myAnimator.Play ("role_run");
            _myAnimator.SetBool ("PrepareState", true);
            if (touch_info.phase == TouchPhase.Began) {
                _lastMove = new Vector2 (touch_info.position.x, touch_info.position.y);
            }
            if (touch_info.phase == TouchPhase.Moved) {
                var curTouch = new Vector2 (touch_info.position.x, touch_info.position.y);
                var result = curTouch - _lastMove;
                if (result != Vector2.zero) {
                    //平滑取插值
                    transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.LookRotation (new Vector3 (result.x, 0, result.y)), Time.deltaTime * (float) _speed);
                    transform.Translate (Vector3.forward * Time.deltaTime * (float) _speed);
                } else {
                    transform.Translate (Vector3.forward * Time.deltaTime * (float) _speed);
                }
            }
            if (touch_info.phase == TouchPhase.Stationary)
                transform.Translate (Vector3.forward * Time.deltaTime * (float) _speed);

            if (touch_info.phase == TouchPhase.Ended) {
                _myAnimator.Play ("role_idle02");
                _myAnimator.SetBool ("PrepareState", false);
            }
        }
        if (!DataCache.mainRoleIsDown) {
            if (Input.GetKey (KeyCode.W)) {
                transform.Translate (Vector3.forward * Time.deltaTime * _speed);
            }
            if (Input.GetKey (KeyCode.S)) {
                transform.Translate (-Vector3.forward * Time.deltaTime * _speed);
            }
            if (Input.GetKey (KeyCode.A)) {
                transform.Translate (Vector3.left * Time.deltaTime * _speed);
            }
            if (Input.GetKey (KeyCode.D)) {
                transform.Translate (Vector3.right * Time.deltaTime * _speed);
            }
        }

        DataCache.curMainPos = transform.position;
        if (_checkCameraPos)
            _ChangeCameraPos ();
        else if (transform.localPosition.y - _initPos.y > 1) {
            transform.Translate (0, _initPos.y - transform.localPosition.y, 0);
            _rigidbody.velocity = Vector3.zero;
        }
    }

    private void _DieFunc () {
        DataCache.mainRoleIsDie = true;
        UIManager.I.ShowView<ResultView> (false);
    }

    private bool _CanTouch () {
        var curAnimator = _myAnimator.GetCurrentAnimatorStateInfo (0);
        return _otherTouch && !DataCache.mainRoleIsDie && DataCache.startGame && (curAnimator.IsName ("role_run") || curAnimator.IsName ("role_idle02"));
    }

    //特写镜头
    private void _RotateCamera () {
        _checkCameraPos = false;
        _otherTouch = false;
        EventUtils.Dispatch ("IsWin");
        _UseCallBackEverySecond (0.75f, 0.01f, () => {
            _curCaTrans.RotateAround (transform.localPosition, Vector3.up, 2.4f);
        }, () => {
            var pos = transform.localPosition - _curCaTrans.localPosition;
            _UseCallBackEverySecond (0.36f, 0.01f, () => { _TranslateCamera (pos); }, () => {
                this.StartCoroutine (_WaitAndRunCallBack (1, () => {
                    UIManager.I.ShowView<ResultView> (true);
                }));
            });
        });
    }

    private void _TranslateCamera (Vector3 pos) {
        _curCaTrans.localPosition += pos / 90;
        _curCaTrans.Rotate (-0.32f, 0, 0);
    }

    private void _ChangeCameraPos (float pos_y = 0) {
        var localtion_pos = transform.localPosition;
        _curCaTrans.localPosition = new Vector3 (localtion_pos.x - disCaX, _curCaTrans.localPosition.y + pos_y, localtion_pos.z - disCaZ);
    }
    private void _CanStart () {
        DataCache.startGame = true;
    }

    private void _EnemyIsDie () {
        UnityEngine.Debug.LogError("DataCache.roundsCoun" + DataCache.roundsCount + ", " + DataCache.restCount);
        if (DataCache.roundsCount > 0)
            DataCache.roundsCount -= 1;

        DataCache.curMainRoleVolumeCoefficient = _curVolumeCoefficient;
        if (DataCache.roundsCount < DataCache.restCount) {
            _ShowBridgeByRounds ();
            DataCache.rounds += 1;
            SceneLoadManager.I.ShowGameObjectByRounds (DataCache.rounds, () => {
                // if (DataCache.roundsCount == 0) {
                //     _canPass = true;
                // }
                DataCache.rounds -= 1;
            });
        }
    }

    private void _MainRoleBeForce (object lock_state = null) {
        _audioHit.Play ();
        if (!_isBeBigger) {
            _PlayBeBigger ();
        }
        //EventUtils.Dispatch ("ShakeScreen");
#if UNITY_ANDROID
	    _shakeActivity.Call ("ClickShake");
#endif
        if (lock_state != null && (bool) lock_state) {
            this.StartCoroutine (_WaitAndRunCallBack (1, () => {
                _rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            }));
            return;
        }
        _myAnimator.Play ("role_behit");
        _myAnimator.SetBool ("BeHit", true);
        this.StartCoroutine (_ContinueRun ());
    }

    private void _InitMainPos () {
        var pos_info = DataCache.reviveInfo[new Tuple<string, int> (SaveData.curCheckPoint, DataCache.rounds)];
        transform.localPosition = new Vector3 (pos_info.X, pos_info.Y, pos_info.Z);
        DataCache.mainRoleIsDie = false;
        gameObject.SetActive (true);
    }

    /// <summary>
    /// 1是吃buff特效,2是吃金币,3是冲击波
    /// </summary>
    /// <param name="buff"></param>
    /// <param name="seconds"></param>
    /// <param name="call_back"></param>
    private void _PalyEffect (int buff, float seconds, Action call_back = null) {
        Transform buff_parent = tra_alleffect.Find ($"tra_effect{buff}");
        _ShowOrHideBuff (true);
        for (int i = 0; i < buff_parent.childCount; i++) {
            var particleSys = buff_parent.GetChild (i).GetComponent<ParticleSystem> ();
            var main = particleSys.main;
            main.loop = true;
            particleSys.gameObject.SetActive (true);
        }
        _buffSeconds = seconds;
        this.StartCoroutine (_PlayBuff (() => {
            for (int i = 0; i < buff_parent.childCount; i++) {
                var particleSys = buff_parent.GetChild (i).GetComponent<ParticleSystem> ();
                var main = particleSys.main;
                main.loop = false;
                particleSys.gameObject.SetActive (false);
            }
            call_back?.Invoke ();
        }));
    }

    private void _ShowOrHideBuff (bool show_or_hide) {
        for (int i = 0; i < tra_alleffect.childCount; i++) {
            var particleSys = tra_alleffect.GetChild (i);
            particleSys.gameObject.SetActive (show_or_hide);
        }
    }

    private void StopSomeCoroutines () {
        this.StopCoroutine ("_ContinueRun");
        this.StopCoroutine ("_BeBigger");
        this.StopCoroutine ("_WaitAndRunCallBack");
        this.StopCoroutine ("_WaitAndRunCallBack");
        this.StopCoroutine ("_PlayBuff");
        this.StopCoroutine ("_CheckIsDie");
    }

    private void OnDestroy () {
        this.StopAllCoroutines ();
        EventUtils.RemoveListener ("Start", _CanStart);
        EventUtils.RemoveListener ("EnemyIsDie", _EnemyIsDie);
        EventUtils.RemoveListener ("MainRoleBeForce", _MainRoleBeForce);
        EventUtils.RemoveListener ("InitMainPos", _InitMainPos);
    }
}