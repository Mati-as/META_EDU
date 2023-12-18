using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainstickRotater : MonoBehaviour
{ 
    public float rotationSpeed;

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float rotationThisFrame = rotationSpeed * Time.deltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotationThisFrame,0, 0));
    }
}
