using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sandwich_AnimalController : MonoBehaviour
{
    public static readonly int EAT = Animator.StringToHash("Eat"); 
    private enum Animal
    {
        Pig,
        Cow,
        Horse,
        Max
    }

    private Transform[] _animals;
    private Animator[] _controllers;
    private void Awake()
    {
        _animals = new Transform[(int)Animal.Max];
        _controllers = new Animator[(int)Animal.Max];

        for (int i = (int)Animal.Pig; i < (int)Animal.Max; i++)
        {
            _animals[i] = transform.GetChild(i);
            _controllers[i] = transform.GetChild(i).GetComponent<Animator>();

        }

        Sandwitch_GameManager.onSandwichArrive -= OnSandwichArrive;
        Sandwitch_GameManager.onSandwichArrive += OnSandwichArrive;
    }

    private void OnSandwichArrive()
    {
#if UNITY_EDITOR
        Debug.Log($"Sandwich Arrived");
#endif
        foreach (var controller in _controllers)
        {
            controller.SetBool(EAT,true);
        }
    }
}
