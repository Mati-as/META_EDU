using UnityEngine;
using System.Collections;

public class Dragonfly : MonoBehaviour
{
    private Animator dragonfly;
    void Start()
    {
        dragonfly = GetComponent<Animator>();
    }
    void Update()
    {
        if (dragonfly.GetCurrentAnimatorStateInfo(0).IsName("fly"))
        {
            dragonfly.SetBool("takeoff", false);
            dragonfly.SetBool("landing", false);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dragonfly.SetBool("idle", false);
            dragonfly.SetBool("idle2", false);
            dragonfly.SetBool("takeoff", true);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dragonfly.SetBool("landing", true);
            dragonfly.SetBool("fly", false);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            dragonfly.SetBool("walk", true);
            dragonfly.SetBool("idle", false);
            dragonfly.SetBool("idle2", false);
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            dragonfly.SetBool("walk", false);
            dragonfly.SetBool("idle", true);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            dragonfly.SetBool("die", true);
            dragonfly.SetBool("fly", false);
        }
    }
}


