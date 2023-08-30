using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalController : MonoBehaviour
{

    private Rigidbody rb;
    private Animator anim;
    

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }


    void Update()
    {
     
       
   
       
        
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
