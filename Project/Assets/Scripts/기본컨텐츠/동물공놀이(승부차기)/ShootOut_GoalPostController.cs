using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootOut_GoalPostController : MonoBehaviour
{
    public static event Action OnGoal;

    private void OnTriggerEnter(Collider other)
    {
        OnGoal?.Invoke();
    }
}

