using System;
using DG.Tweening;
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

        for (var i = (int)Animal.Pig; i < (int)Animal.Max; i++)
        {
            _animals[i] = transform.GetChild(i);
            _controllers[i] = transform.GetChild(i).GetComponent<Animator>();
        }

        Sandwitch_GameManager.onSandwichArrive -= OnSandwichArrive;
        Sandwitch_GameManager.onSandwichArrive += OnSandwichArrive;
    }

    public static event Action onFinishEating;
    public static event Action onAllFinishAnimOver;

    private void OnSandwichArrive()
    {

        foreach (var controller in _controllers) controller.SetBool(EAT, true);

        DOVirtual.Float(0, 0, 5, _ => { }).OnComplete(() =>
        {
            foreach (var controller in _controllers) controller.SetBool(EAT, false);
            DOVirtual.Float(0, 0, 2, _ => { }).OnComplete(() => { onFinishEating?.Invoke(); });
            DOVirtual.Float(0, 0, 10, _ => { }).OnComplete(() =>
            {
#if UNITY_EDITOR
Debug.Log("ALL ANIM FINISHED... ROUND IS BEING READY AGAIN");
#endif
                onAllFinishAnimOver?.Invoke();
            });
        });
    }
}