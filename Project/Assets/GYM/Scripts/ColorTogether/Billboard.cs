using Cinemachine;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public CinemachineVirtualCamera VirtualCamera;

    void Update()
    {
        if (VirtualCamera != null)
        {
            Transform lookAtTarget = VirtualCamera.LookAt;
            if (lookAtTarget != null)
            {
                Vector3 lookDirection = lookAtTarget.position - transform.position;
                transform.rotation = Quaternion.LookRotation(lookDirection.normalized);
            }
            else
            {
                Vector3 fallbackDirection = VirtualCamera.transform.forward;
                transform.rotation = Quaternion.LookRotation(fallbackDirection);
            }
        }
    }
}