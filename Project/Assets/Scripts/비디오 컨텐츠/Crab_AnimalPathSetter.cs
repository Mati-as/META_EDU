using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class Crab_AnimalPathSetter : MonoBehaviour
{

    public Transform crab;
    public Transform lobster;
    public Transform fish;
    
    public Transform[] pathCrab;
    public Transform[] pathLobster;
    public Transform[] pathFish;
    
    private Vector3[] _pathVecCrab;
    private Vector3[] _pathVecLobster;
    private Vector3[] _pathVecFish;

    public float moveDurationCrab;
    public float moveDurationLobster;
    public float moveDurationFish;


    private void Awake()
    {
        DOTween.Init();
        
    }

    private void Start()
    {
        Init();
    }

    private void OnDestroy()
    {
        SetSubscription(isSubscribe: false);
    }

    private void SetSubscription(bool isSubscribe = true)
    {
        if (isSubscribe)
        {
            CrabVideoGameManager.onRewind -=OnReplay;
            CrabVideoGameManager.onRewind +=OnReplay;
        }
        else
        {
            CrabVideoGameManager.onRewind -=OnReplay;
        }
       
    }


    private void Init()
    {
        SetSubscription();
        
        _pathVecCrab = new Vector3[pathCrab.Length];
        _pathVecLobster = new Vector3[pathLobster.Length];
        _pathVecFish = new Vector3[pathFish.Length];
        
        for (int i = 0; i < pathCrab.Length; i++)
        {
            _pathVecCrab[i] = pathCrab[i].position;
        }
        
        for (int i = 0; i < pathLobster.Length; i++)
        {
            _pathVecLobster[i] = pathLobster[i].position;
        }
        
        for (int i = 0; i < pathFish.Length; i++)
        {
            _pathVecFish[i] = pathFish[i].position;
        }
    }

    private void OnReplay()
    {
        Move(crab, _pathVecCrab,moveDurationCrab);
        Move(lobster, _pathVecLobster,moveDurationLobster);
        Move(fish, _pathVecFish,moveDurationFish);
    }

    private void Move(Transform trans,Vector3[] path,float duration=4.5f)
    {
        trans.position = path[0];
        
        trans.DOPath(path, duration,PathType.CatmullRom).OnComplete(() =>
        {
            trans.position = path[0];
        });
    }
}
