using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityExtensions {
    public static float DistanceBetween(this GameObject one, GameObject two) {
        return Mathf.Abs((one.transform.position - two.transform.position).magnitude);
    }
    public static float DistanceBetweenSqr(this GameObject one, GameObject two) {
        return Mathf.Abs((one.transform.position - two.transform.position).sqrMagnitude);
    }
    public static float DistanceBetween(this GameObject one, Vector3 two) {
        return Mathf.Abs((one.transform.position - two).magnitude);
    }
    public static float DistanceBetweenSqr(this GameObject one, Vector3 two) {
        return Mathf.Abs((one.transform.position - two).sqrMagnitude);
    }
    public static Vector3 DirectionToObject(this GameObject one, GameObject two) {
        return two.transform.position - one.transform.position;
    }
}
