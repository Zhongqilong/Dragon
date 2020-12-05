using System.Collections;
using UnityEngine;
using Uqee.Utility;

public class Enemy : PersonObjectBase {
    //力的方向单位向量
    private Vector3 _forceDir;
    //同一片区域再奔跑
    private bool _isRun = false;
    private bool _canStart = false;
    //第一，二，三个被撞的人
    public bool isOne, isTwo, isThree;
    public bool isDown = false;
    public ParticleSystem _particleSystem1;
    public ParticleSystem _particleSystem2;
    public Vector3 beForce;
    public override void OnShow (object param = null) {
        _speed = 1;
        _curVolumeCoefficient = 1;
        _initPos = new Vector3 (0, 0, 6);
        _curRidus = 15;
        if (param != null) {
            var info = (GameInfoData) param;
            _curVolumeCoefficient = info.VolumeCoefficient;
            var y = info.Y;
            _initPos = new Vector3 (info.X, info.Y, info.Z);
            _speed = info.Speed - 1;
            _curRidus = info.Radius;
            rounds = info.Rounds;
        }
        _rigidbody = transform.GetComponent<Rigidbody> ();
        _myAnimator = transform.GetComponent<Animator> ();
        _InitInfo ();
        EventUtils.AddListener ("Start", _CanStart);
    }

    public Animator GetAnimator () {
        return _myAnimator;
    }

    private void _InitInfo () {
        transform.forward = -Vector3.forward;
        _isDie = false;
        _canStart = false;
        transform.localScale = Vector3.one * _curVolumeCoefficient;
        transform.localPosition = _initPos;
        //_rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        _rigidbody.isKinematic = true;
        isThree = false;
        isOne = false;
        isTwo = false;
        isDown = false;
        _rigidbody.velocity = Vector3.zero;
        this.StopAllCoroutines ();
        _myAnimator.Play ("role_idle01", 0, 0f);
        _myAnimator.SetBool ("PrepareState", false);
        _myAnimator.SetBool ("BeHit", false);
        _myAnimator.SetBool ("CanUp", false);
    }

    public override void RestartGame () {
        gameObject.SetActive (true);
        _InitInfo ();
    }

    private void _CanStart () {
        //UnityEngine.Debug.LogError ("开始1111");
        if (!gameObject.activeSelf)
            return;
        if (rounds != DataCache.rounds)
            return;
        if (DataCache.isWin)
            DataCache.isWin= false;

        _myAnimator.SetBool ("PrepareState", true);
        _myAnimator.Play ("role_run");
        //_myAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        _canStart = true;
        if (DataCache.InMainSize (transform.localPosition, _curRidus)) 
        {
            this.StartCoroutine (_Run ());
            isDown = false;
        }
        _rigidbody.isKinematic = false;
    }
    private void FixedUpdate () {
        if (DataCache.isWin) {
            _StopRun ();
            return;
        }
        if (DataCache.mainRoleIsDie) {
            _StopRun ();
            return;
        }
        //_isRun用来防止重复添加_Run协程，因为在Update中所以需要谨慎使用
        if (DataCache.InMainSize (transform.localPosition, _curRidus)) {
            if (!_isRun && _canStart) {
                _myAnimator.Play ("role_run");
                _myAnimator.SetBool ("PrepareState", true);
                _myAnimator.SetBool ("BeHit", false);
                isThree = false;
                isOne = false;
                isTwo = false;
                this.StartCoroutine (_Run ());
                _isRun = true;
                isDown = false;
            }
        } else if(!isDown) {
            _StopRun ();
        }

        if (!_isDie && (_initPos.y - transform.localPosition.y > 2)) {
            gameObject.SetActive (false);
            EventUtils.Dispatch ("EnemyIsDie");
            EventUtils.Dispatch ("ShowCombo");
            _isDie = true;
        }

        if (!DataCache.mainRoleIsDown) {
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        } else if(!isDown){
            //主角倒地也可以被撞
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }
    }

    private void OnCollisionEnter (Collision other) {
        _AddReflectByMainRole (other.gameObject);
    }

    private void OnParticleCollision (GameObject other) {
        _AddReflectByParticle (other.gameObject);
    }

    /// <summary>
    /// 主角和敌人的碰撞
    /// </summary>
    /// <param name="go"></param>
    private void _AddReflectByMainRole (GameObject go) {
        // if (_rigidbody.velocity != Vector3.zero)
        // {
        //     return;
        // }
        if (go.tag == GameTag.Floor) {
            var info = _myAnimator.GetCurrentAnimatorStateInfo (0);
            if (info.IsName ("role_down") || info.IsName ("role_behit")) {
                _myAnimator.Play ("role_up");
                _myAnimator.SetBool ("CanUp", true);
                _rigidbody.velocity = Vector3.zero;
                isDown = false;
            }
            return;
        }
        if (go.tag == GameTag.MainRole && !DataCache.mainRoleIsDown && _isRun && !isDown) {
            //_UseCallBackEverySecond (() => { Handheld.Vibrate (); }, 0.1f, 0.1f);
            // _UseCallBackEverySecond (0.1f, 0.1f, ()=>{ Handheld.Vibrate (); }, ()=>{Handheld.();});
            isOne = true;
            isDown = true;
            _rigidbody.velocity = Vector3.zero;
            //倒地动画
            _myAnimator.SetBool ("BeHit", false);
            _myAnimator.Play ("role_behit");
            PlayEffect ();
            var dir_x = DataCache.curMainPos.x - transform.position.x;
            var dir_z = DataCache.curMainPos.z - transform.position.z;
            var enemy_force = DataCache.curMainRoleVolumeCoefficient * _curElasticForce;
            var role_force = 3 * _curElasticForce;
            _forceDir = new Vector3 (dir_x, -1, dir_z);
            //停止移动
            this.StopAllCoroutines ();
            this.StartCoroutine (_ContinueRun ());
            //UnityEngine.Debug.LogError ("方向向量1: " + go.transform.forward + "方向向量2: " + transform.forward + "夹角: " + Vector3.Angle (go.transform.forward, transform.forward));
            //UnityEngine.Debug.LogError ("主角坐标: " + go.transform.localPosition.y);
            var roleRigidbody = go.transform.GetComponent<Rigidbody> ();
            //经过计算主角的正面200度的范围是方向向量夹角在80~180度之间
            var anlge = Vector3.Angle (go.transform.forward, _forceDir.normalized);
            beForce = -enemy_force * _forceDir.normalized;
            //beForce = -enemy_force * new Vector3 (dir_x > 0 ? 1 : -1, 0, dir_z > 0 ? 1 : -1);
            beForce.y = 1500;
            //主角体积大于敌人并且正面撞，主角不受到力
            if (anlge >= 80 && anlge <= 180 && DataCache.curMainRoleVolumeCoefficient > _curVolumeCoefficient) {
                roleRigidbody.constraints = RigidbodyConstraints.FreezeAll;
                _rigidbody.AddForce (beForce, ForceMode.Acceleration);
                EventUtils.Dispatch ("MainRoleBeForce", true);
                return;
            }

            _rigidbody.AddForce (beForce);
            //反作用力
            var mainRoleBeForce = (new Vector3 (dir_x, 0, dir_z)).normalized;
            //roleRigidbody.WakeUp();
            roleRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            roleRigidbody.AddForce (role_force * mainRoleBeForce);
            EventUtils.Dispatch ("MainRoleBeForce");
            return;
        }

        //由于碰撞会检测两次。所以这里只判断当第一个人撞到我的时候，我自身被撞飞
        if (isOne && go.tag == GameTag.Enemy) {
            var enemy = go.GetComponent<Enemy> ();
            if (enemy.isTwo) {
                return;
            }
            var curRoleRigidbody = go.transform.GetComponent<Rigidbody> ();
            var palyer = enemy.GetAnimator ();
            //倒地动画
            palyer.SetBool ("BeHit", true);
            palyer.Play ("role_behit");
            enemy.isTwo = true;
            enemy.isDown = true;
            enemy.beForce = beForce;
            curRoleRigidbody.velocity = Vector3.zero;
            curRoleRigidbody.AddForce (new Vector3 (beForce.x, beForce.y, beForce.z));
            enemy.StopAllCoroutines ();
            enemy.StartCoroutine (enemy._ContinueRun ());
            return;
        }

        if (isTwo && go.tag == GameTag.Enemy) {
            var enemy = go.transform.GetComponent<Enemy> ();
            if (enemy.isThree) {
                return;
            }
            var curRoleRigidbody = go.transform.GetComponent<Rigidbody> ();
            var palyer = enemy.GetAnimator ();
            //倒地动画
            palyer.SetBool ("BeHit", true);
            _myAnimator.SetBool ("CanUp", true);
            palyer.Play ("role_behit");
            enemy.isThree = true;
            enemy.isDown = true;
            curRoleRigidbody.velocity = Vector3.zero;
            curRoleRigidbody.AddForce (new Vector3 (beForce.x, beForce.y / 2, beForce.z));
            enemy.StopAllCoroutines ();
            enemy.StartCoroutine (enemy._ContinueRun ());
        }
    }

    private void _AddReflectByParticle (GameObject go) {
        if (go.tag == GameTag.MainRole) {
            if (!DataCache.mainRoleIsDown) {
                isOne = true;
                _rigidbody.velocity = Vector3.zero;
                //倒地动画
                _myAnimator.SetBool ("BeHit", false);
                _myAnimator.Play ("role_behit");
                PlayEffect ();
                var dir_x = DataCache.curMainPos.x - transform.position.x;
                var dir_z = DataCache.curMainPos.z - transform.position.z;
                var enemy_force = DataCache.curMainRoleVolumeCoefficient * _curElasticForce * 2;
                var role_force = 3 * _curElasticForce;
                _forceDir = new Vector3 (dir_x, -1, dir_z);
                //停止移动
                this.StopAllCoroutines ();
                this.StartCoroutine (_ContinueRun ());
                beForce = -enemy_force * _forceDir.normalized;
                beForce.y = 250;
                _rigidbody.AddForce (beForce);
                return;
            }
        }
    }

    private void _StopRun () {
        _isRun = false;
        _myAnimator.SetBool ("PrepareState", false);
        _myAnimator.Play ("role_idle01");
        this.StopAllCoroutines ();
    }

    public IEnumerator _ContinueRun () {
        while (true) {
            yield return new WaitForSeconds (Time.deltaTime);
            var info = _myAnimator.GetCurrentAnimatorStateInfo (0);
            if (info.IsName ("role_up") && info.normalizedTime >= 1.0f) {
                _myAnimator.SetBool ("BeHit", false);
                _myAnimator.Play ("role_run");
                _myAnimator.SetBool ("PrepareState", true);
                _myAnimator.SetBool ("CanUp", false);
                _rigidbody.velocity = Vector3.zero;
                isDown = false;
                isThree = false;
                isOne = false;
                isTwo = false;
                beForce = Vector3.zero;
                this.StartCoroutine (_Run ());
                yield break;
            }
        }
    }

    public IEnumerator _Run () {
        while (true) {
            yield return new WaitForSeconds (0.001f);
            AI ();
        }
    }

    private void AI () {
        var dir_x = DataCache.curMainPos.x - transform.position.x;
        var dir_z = DataCache.curMainPos.z - transform.position.z;
        var pos = dir_z > 0 ? transform.forward : -transform.forward;
        transform.Translate (pos.normalized * _speed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.LookRotation (new Vector3 (dir_x, 0, dir_z)), Time.deltaTime);
    }

    public void PlayEffect () {
        _particleSystem1.gameObject.SetActive (true);
        _particleSystem2.gameObject.SetActive (true);
        _particleSystem1.Play ();
        _particleSystem2.Play ();
    }

    private void _Dispose () {
        CoroutineHelper.Stop (gameObject);
        GameObject.Destroy (gameObject);
    }

    private void OnDestroy () {
        EventUtils.RemoveListener ("Start", _CanStart);
    }
}