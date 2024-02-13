using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;
using Sequence = DG.Tweening.Sequence;

public class HandFootPainting_GameManager : IGameManager
{
    public static bool isInit { get; private set; }
    
    public Queue<GameObject> printPool { get; set; }
    private GameObject[] _prints;

    private float _poolSize = 50;
    private SpriteRenderer _bgSprite;
    private float _elapsed;
    private float _timeLimit= 15f;
    private float _remainTime;
    private bool _isRoundReady;
    private TextMeshProUGUI _tmp;
    
    //Glowing Outline MeshRenderer
    private SpriteRenderer _outlineSpRenderer;
    private Color _glowDefaultColor;
    private Sequence _glowSeq;
    
    public static event Action onRoundFinished;
    public static event Action printInitEvent;
    public static event Action onRoundRestart;
    protected override void Init()
    {
        base.Init();
        DOTween.Init().SetCapacity(500,200);
        SetPool();
        _bgSprite = GameObject.Find("handfootPainting_Bg_Outline").GetComponent<SpriteRenderer>();
        _tmp = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        _outlineSpRenderer =GameObject.Find("handfootPainting_Bg_Outline").GetComponent<SpriteRenderer>();
        _glowDefaultColor = _outlineSpRenderer.material.color;
        
        _bgSprite.DOFade(0, 0.00001f).OnComplete(() =>
        {
            isInit = true;
        });
    }

    protected override void BindEvent()
    {
        base.BindEvent();
        
        onRoundFinished -= OnRoundFinished;
        onRoundFinished += OnRoundFinished;

        onRoundRestart -= OnRoundRestart;
        onRoundRestart += OnRoundRestart;
    }

    private void Update()
    {
        if (!_isRoundReady) return;
        
        _elapsed += Time.deltaTime;
        _remainTime = _timeLimit - _elapsed;
        
        
        if (_elapsed > _timeLimit)
        {
            _isRoundReady = false;
            onRoundFinished.Invoke();
         
        }

        else
        {
            _tmp.text = $"남은 시간 : {(int)_remainTime}초";
        }
    }

    protected override void OnStartButtonClicked()
    {
        base.OnStartButtonClicked();
      
        _isRoundReady = true;
        
        _glowSeq = DOTween.Sequence();
        BlinkOutline();

    }

    private void BlinkOutline()
    {
        
        _glowSeq.Append(_outlineSpRenderer.material.DOColor(_glowDefaultColor * 1.5f, 0.4f))
            .Append(_outlineSpRenderer.material.DOColor(_glowDefaultColor*0.5f, 0.4f))
            .SetLoops(-1, LoopType.Restart);

        _glowSeq.Play();
    }

    private void OnRoundRestart()
    {
        _remainTime = _timeLimit;
        _elapsed = 0;
        _bgSprite.DOFade(0, 2f)
            .OnStart(() => { _tmp.text = "놀이 시작!"; }).OnComplete(() => { _isRoundReady = true; });
    }

    private void OnRoundFinished()
    {
        
        _glowSeq.Kill();
        _glowSeq = DOTween.Sequence();
        _glowSeq.Append(_outlineSpRenderer.material.DOColor(_glowDefaultColor, 0.3f));
        
        //"그만" UI 팝업? 
        FadeInBg();
        DOVirtual.Float(0, 0, 3, _ => { }).OnComplete(() =>
        {
            _tmp.text = $"놀이를 다시 준비하고 있어요";
        });
        
        DOVirtual.Float(0, 0, 4.5f, _ => { }).OnComplete(() =>
        {
            printInitEvent?.Invoke();
        });
        
        DOVirtual.Float(0, 0, 7, _ => { }).OnComplete(() =>
        {
            onRoundRestart?.Invoke();
        });
      
    }

    private void FadeInBg()=> DOVirtual.Float(0, 0, 1.5f, _ => { }).OnComplete(() =>
    {
    #if UNITY_EDITOR
        Debug.Log("DoFade In");
    #endif
        _bgSprite.DOFade(1, 1f);
    });


    protected override void OnRaySynced()
    {
        if (!_isRoundReady) return;
        
        
        Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in GameManager_Hits)
        {

            GetFromPool(hit.point);
            
        
            break;
        }
    }
    
    private void SetPool()
    {
        printPool = new Queue<GameObject>();
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
                printPool.Enqueue(obj);
                obj.gameObject.SetActive(false);
            }

        // Optionally, if you need more instances than available, clone them
        while (printPool.Count < _poolSize * 10)
        {
            foreach (var obj in _prints)
                if (obj != null)
                {
                    var print = Instantiate(obj, transform);
                    print.transform.DORotate(new Vector3(0f, UnityEngine.Random.Range(-180f, 180f), 0f), 0.01f);
                    print.gameObject.SetActive(false);
                    printPool.Enqueue(print);
                }
        }
        
    }

    private readonly float RETRUN_WAIT_TIME=100;
    
    private void GetFromPool(Vector3 spawnPosition)
    {
        if (printPool.Count <= 0) GrowPool();
        
        var print = printPool.Dequeue();
        print.transform.position = spawnPosition;
        print.gameObject.SetActive(true);

        DOVirtual.Float(0, 0, RETRUN_WAIT_TIME, _ => { }).OnComplete(() =>
        {
            print.gameObject.SetActive(false);
            printPool.Enqueue(print); // Return the particle system to the pool
        });
    }
    protected void GrowPool()
    {
      
        foreach (var obj in _prints)
            if (obj != null)
            {
                var print = Instantiate(obj, transform);
                print.transform.DORotate(new Vector3(0f, UnityEngine.Random.Range(-180f, 180f), 0f), 0.01f);
                print.gameObject.SetActive(false);
                printPool.Enqueue(print);
            }
    }
}
