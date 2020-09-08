using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LocalRotationTransition : MonoBehaviour {
    public enum AXIS { x,y,z};
    public float speed;
    public AXIS axis;

    void Update()
    {
        transform.localRotation *= Quaternion.Euler(Convert.ToInt32(AXIS.x==axis)*speed, Convert.ToInt32(AXIS.y == axis) * speed, Convert.ToInt32(AXIS.z == axis) * speed);
    }
}
