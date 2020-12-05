using System;
using System.Collections;
using UnityEngine;

public struct ParamStruct {
    //位置坐标
    public Vector3 pos;
    //体积系数
    public float volumeCoefficient;
}

public class PersonObjectBase : AllObjectBase {
    //速度
    protected float _speed;
    //当前体积系数
    protected float _curVolumeCoefficient = 1.0f;
    //当前碰撞弹力: _curElasticForce * _curVolumeCoefficient
    protected int _curElasticForce = 500;
    //策划需求同一个区域内的怪才会朝向主角，这里定义一个区域
    protected float _curRidus;
    //刚体
    protected Rigidbody _rigidbody;
    //用于判断当前是否死亡
    //当前的动画控制器
    protected Animator _myAnimator;
    //初始位置，用于重新开始游戏
    protected Vector3 _initPos;
    protected bool _isDie = false;
    //1主角，2Buff
    // protected void Start () {
    //     Init ();
    // }

    // public virtual void Init () { }
    // public virtual void OnShow (object param = null) { 

    protected void _UseCallBackEverySecond (float all_time, float seconds, Action start_call_back, Action end_call_back = null) {
        CoroutineHelper.Start (deltaTimeFunc (all_time, seconds, start_call_back, end_call_back));
    }

    IEnumerator deltaTimeFunc (float all_time, float seconds, Action start_call_back, Action end_call_back = null) {
        while (true) {
            if (all_time > 0) {
                all_time -= seconds;
                start_call_back.Invoke ();
                yield return new WaitForSeconds (seconds);
            } else
            {
                end_call_back?.Invoke ();
                yield break;
            }
        }
    }
}