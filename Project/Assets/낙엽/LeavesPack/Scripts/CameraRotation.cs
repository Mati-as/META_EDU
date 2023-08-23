using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed;
    public float distance;
    public float heightOffset;
    public float y;

    private float x;

    private void Update()
    {
        if (target == null)
            return;

        x += rotationSpeed;

        var rotation = Quaternion.Euler(y, x, 0);
        var awayVector = new Vector3(0, 0, -distance);
        var position = rotation * awayVector + target.position + Vector3.up * heightOffset;

        transform.rotation = rotation;
        transform.position = position;
    }
}