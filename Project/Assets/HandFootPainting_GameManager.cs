using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class HandFootPainting_GameManager : IGameManager
{
    public static bool isInit { get; private set; }
    
    private Queue<GameObject> _printPool;
    private GameObject[] _prints;

    private float _poolSize = 10;
    private SpriteRenderer _bgSprite;
   

    public static event Action onRoundFinished;
    
    protected override void Init()
    {
        base.Init();
        SetPool();
        _bgSprite = GameObject.Find("handfootPainting_Bg_Outline").GetComponent<SpriteRenderer>();
        _bgSprite.DOFade(0, 0.00001f).OnComplete(() =>
        {
            isInit = true;
        });
    }

    protected override void OnRaySynced()
    {
        Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in GameManager_Hits)
        {
#if UNITY_EDITOR
            Debug.Log($"{SceneManager.GetActiveScene().name}: ray's working");
#endif
            GetFromPool(hit.point);
            break;
        }
    }
    
    private void SetPool()
    {
        _printPool = new Queue<GameObject>();
        _prints = new GameObject[transform.childCount];
        var index = 0;
        foreach (Transform child in transform)
        {
            if (child != null) _prints[index++] = child.gameObject;
        }

        // Only enqueue each ParticleSystem instance once
        foreach (var obj in _prints)
            if (obj != null)
            {
                _printPool.Enqueue(obj);
                obj.gameObject.SetActive(false);
            }

        // Optionally, if you need more instances than available, clone them
        while (_printPool.Count < _poolSize * 10)
        {
            foreach (var obj in _prints)
                if (obj != null)
                {
                    var newPs = Instantiate(obj, transform);
                    newPs.transform.DORotate(new Vector3(0f, UnityEngine.Random.Range(-180f, 180f), 0f), 0.01f);
                    newPs.gameObject.SetActive(false);
                    _printPool.Enqueue(newPs);
                }
        }
        
    }

    private readonly float RETRUN_WAIT_TIME=100;
    
    private void GetFromPool(Vector3 spawnPosition)
    {
        var print = _printPool.Dequeue();
        print.transform.position = spawnPosition;
        print.gameObject.SetActive(true);

        DOVirtual.Float(0, 0, RETRUN_WAIT_TIME, _ => { }).OnComplete(() =>
        {
            print.gameObject.SetActive(false);
            _printPool.Enqueue(print); // Return the particle system to the pool
        });
    }
    
}
