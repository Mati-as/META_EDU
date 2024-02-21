using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Arctic_GlacierController : MonoBehaviour
{
    private enum Path
    {
        Start,
        Arrival,
        Max
    }
    private Transform[] _glaciers;

    private Vector3[] _path;
    private Vector3[] _distance;

    private float _interval =1f;
    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        MoveGlaciers();
    }

    private int CHILD_COUNT;
    private void Init()
    {
         CHILD_COUNT = transform.childCount;
        _glaciers = new Transform[CHILD_COUNT];

        for (var i = 0; i <= CHILD_COUNT; ++i)
        {
            _glaciers[i] = transform.GetChild(i);
        }

        _path = new Vector3[(int)Path.Max];

        var defaultPos = transform.position;
        _path[(int)Path.Start] = defaultPos + Vector3.forward * 40;
        _path[(int)Path.Arrival] = defaultPos + Vector3.back * 10;
    }

    private void MoveGlaciers()
    {
        for (var i = 0; i <= CHILD_COUNT; ++i)
        {
            _glaciers[i].DOMove(_path[(int)Path.Arrival], _interval + Random.Range(0.3f, 0.5f))
                .OnComplete(() =>
                {
                    _glaciers[i].position = _path[(int)Path.Arrival];
                });
        };
    }
}
