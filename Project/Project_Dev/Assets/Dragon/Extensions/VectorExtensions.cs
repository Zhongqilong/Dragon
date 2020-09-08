using UnityEngine;

public static class VectorExtensions
{
    public static Vector2 ToVector2(this float val)
    {
        return new Vector2(val, val);
    }

    public static Vector3 ToVector3(this float val)
    {
        return new Vector3(val, val, val);
    }

    public static Vector4 ToVector4(this float val)
    {
        return new Vector4(val, val, val, val);
    }

    public static Vector2 ToVector2XY(this Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.y);
    }

    public static Vector2 ToVector2XZ(this Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.z);
    }

    public static Vector3 ToVector3XY(this Vector2 vector2)
    {
        return new Vector3(vector2.x, vector2.y);
    }

    public static Vector3 ToVector3XY(this Vector2 vector2, float z)
    {
        return new Vector3(vector2.x, vector2.y, z);
    }

    public static Vector3 ToVector3XZ(this Vector2 vector2,float y=0.0f)
    {
        return new Vector3(vector2.x, y, vector2.y);
    }

    public static Color ToColor(this Vector3 vector3)
    {
        return new Color(vector3.x, vector3.y, vector3.z);
    }

    public static Vector3 ToVector3(this Color color)
    {
        return new Vector3(color.r, color.g, color.b);
    }

    public static Vector4 ToVector4(this Vector3 vec3)
    {
        return new Vector4(vec3.x, vec3.y, vec3.z, 1f);
    }

    public static Vector4 ToVector4(this Vector3 vec3, float w)
    {
        return new Vector4(vec3.x, vec3.y, vec3.z, w);
    }

    public static Vector2 ToVector2XY(this Vector4 vec4)
    {
        return new Vector2(vec4.x, vec4.y);
    }

    public static Vector2 ToVector2ZW(this Vector4 vec4)
    {
        return new Vector2(vec4.z, vec4.w);
    }

    public static Vector3 ToVector3(this Vector4 vec4)
    {
        return new Vector3(vec4.x, vec4.y, vec4.z);
    }

    public static Vector3 ToVector3XY(this Vector4 vec4)
    {
        return new Vector3(vec4.x, vec4.y, 0f);
    }

    public static Vector3 Mul(this Vector3 this_vector3, Vector3 mul_vector3)
    {
        this_vector3.x *= mul_vector3.x;
        this_vector3.y *= mul_vector3.y;
        this_vector3.z *= mul_vector3.z;
        return this_vector3;
    }

    public static Vector3 Div(this Vector3 this_vector3, Vector3 div_vector3)
    {
        this_vector3.x /= div_vector3.x;
        this_vector3.y /= div_vector3.y;
        this_vector3.z /= div_vector3.z;
        return this_vector3;
    }

    public static float DistanceXZ(this Vector3 this_vector3, Vector3 to_vector3)
    {
        var diffXZ = new Vector2(this_vector3.x - to_vector3.x , this_vector3.z - to_vector3.z);
        return diffXZ.magnitude;
    }
}