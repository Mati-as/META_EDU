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

    private float _interval =38f;
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

#if UNITY_EDITOR
        Debug.Log($"glaciers count : {CHILD_COUNT}");
#endif
        for (var i = 0; i < CHILD_COUNT; i++)
        {
            _glaciers[i] = transform.GetChild(i);
        }

        _path = new Vector3[(int)Path.Max];

        var defaultPos = transform.position;
        _path[(int)Path.Start] = defaultPos + Vector3.back * 30;
        _path[(int)Path.Arrival] = GameObject.Find("GlacierArrivalPoint" +gameObject.name).transform.position;
    }

    private void MoveGlaciers()
    {
        for (var i = 0; i < CHILD_COUNT; i++)
        {
            var i1 = i;
            _glaciers[i]
                .DOMove(_path[(int)Path.Arrival], (i+1) * _interval + Random.Range(0.3f, 0.5f))
                .OnComplete(() =>
                {
                    _glaciers[i1].position = _path[(int)Path.Start];
                });
        };
    }
}
