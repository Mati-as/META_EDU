using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour {

    public Transform target;
    public float rotationSpeed;
    public float distance;
    public float heightOffset;
    public float y;

    float x;

    void Update () {
        if (target == null)
            return;

        x += rotationSpeed;

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 awayVector = new Vector3(0, 0, -distance);
        Vector3 position = rotation * awayVector + target.position + Vector3.up * heightOffset;

        transform.rotation = rotation;
        transform.position = position;
    }
}
