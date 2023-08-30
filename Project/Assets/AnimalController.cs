using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalController : MonoBehaviour
{

    private Rigidbody rb;
    private Animator anim;
    private int velocityID = Animator.StringToHash("Velocity");

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }


    void Update()
    {
       float velocity= Mathf.Max(rb.velocity.x, rb.velocity.y);
       
       anim.SetFloat(velocityID,velocity);
       
        
    }
    // private void OnTrigger(Collider other)
    // {
    //     if (other.CompareTag("isRoundReady"))
    //     {
    //         Debug.Log("isRoundReady");
    //         GameManager.isRoundReady = true;
    //     }
    // }
}
