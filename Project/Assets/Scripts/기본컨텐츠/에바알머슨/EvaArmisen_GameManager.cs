using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class EvaArmisen_GameManager : IGameManager
{
   // public string gameVersion; //Fish or Tree 
   // private SpriteRenderer _bgSprite;
    public static bool isInit { get; private set; }

    public Queue<GameObject> printPool { get; set; }
    private GameObject[] _prints;

    private ParticleSystem[] _ps;

    private readonly float _poolSize = 50;
    private float _elapsed;
    private readonly float _timeLimit = 15f;
    private float _remainTime;
    private bool _isRoundReadyToStart;
    private Animator _pictureAnimator;
    private readonly int IDLE_ANIM = Animator.StringToHash("Idle");
    private TextMeshProUGUI _tmp;

    //Glowing Outline MeshRenderer
    private Color _glowDefaultColor;

    public static event Action OnStampingFinished;
    public static event Action printInitEvent;
    public static event Action onRoundRestart;

    protected override void Init()
    {
        base.Init();
        DOTween.Init().SetCapacity(1500, 500);
        SetPool();
       // _bgSprite = GameObject.Find(gameVersion + "Mask").GetComponent<SpriteRenderer>();
        _tmp = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        _tmp.text = string.Empty;

        _pictureAnimator = GameObject.Find("EvaArmisenAnimation").GetComponent<Animator>();


        _ps = new ParticleSystem[2];
        _ps[0] = GameObject.Find("CFX_EvaRight").GetComponent<ParticleSystem>();
        _ps[1] = GameObject.Find("CFX_EvaLeft").GetComponent<ParticleSystem>();


    }

    protected override void BindEvent()
    {
        base.BindEvent();

        OnStampingFinished -= OnRoundFinished;
        OnStampingFinished += OnRoundFinished;

        onRoundRestart -= OnRoundRestart;
        onRoundRestart += OnRoundRestart;

        EvaArmisen_UIManager.onStartUI -= OnStartUI;
        EvaArmisen_UIManager.onStartUI += OnStartUI;
    }

    private float _elapsedToCount;
    private bool _isCountNarrationPlaying;

    private void OnStartUI()
    {
        _isRoundReadyToStart = true;
    }

    private void Update()
    {
        if (!_isRoundReadyToStart) return;
// #if UNITY_EDITOR
//         Debug.Log($"current glow Color{_outlineSpRenderer.material.color}");
// #endif
        _elapsed += Time.deltaTime;
        _remainTime = _timeLimit - _elapsed;


        if (_remainTime <= 6f && _remainTime >= 1)
        {
            if (!_isCountNarrationPlaying)
            {
                Managers.Sound.Play
                    (SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Count" + $"{(int)_remainTime}", 0.8f);
                _isCountNarrationPlaying = true;
                _elapsedToCount = 0;
            }

            if (_elapsedToCount > 1f) _isCountNarrationPlaying = false;
            _elapsedToCount += Time.deltaTime * 0.9f;
        }


        if (_elapsed > _timeLimit)
        {
            _isRoundReadyToStart = false;
            _elapsed = 0;
            OnStampingFinished.Invoke();
            _tmp.text = string.Empty;
        }
        else
        {
            _tmp.text = $"{(int)_remainTime}초";
        }
    }

    

    private void OnRoundRestart()
    {
        _elapsed = 0;
        _elapsedToCount = 0f;
        _isCountNarrationPlaying = false;
        _remainTime = _timeLimit;
       
    }

    private readonly string ON_READY_MESSAGE = "놀이를 다시 준비하고 있어요";
    private void OnRoundFinished()
    {
        DOVirtual.Float(0, 0, 1f, _ => { }).OnComplete(() =>
        {
            

            DOVirtual.Float(0, 0, 4f, _ => { }).OnComplete(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandPainting/OnRoundFinish", 0.8f);
                
                _pictureAnimator.SetBool(IDLE_ANIM,true);
                _ps[0].Play();
                _ps[1].Play();
                DOVirtual.Float(0, 0, 3f, _ => { }).OnComplete(() =>
                {
                    _ps[0].Stop();
                    _ps[1].Stop();
                    _pictureAnimator.SetBool(IDLE_ANIM,false); 
                });
           
                DOVirtual.Float(0, 0, 5, _ => { }).OnComplete(() =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/OnReady", 0.8f);
                    _tmp.text = ON_READY_MESSAGE;
                    DOVirtual.Float(0, 0, 3f, _ => { }).OnComplete(() =>
                    {
                        _tmp.text = string.Empty;
                    });
                    
                    DOVirtual.Float(0, 0, 5, _ => { }).OnComplete(() =>
                    {

                        onRoundRestart?.Invoke();
                    });
                });
               
              
            });
        });
    }

    protected override void OnRaySynced()
    {

        if (!_isRoundReadyToStart) return;
        if (!isStartButtonClicked) return;
    
        
        
        foreach (var hit in GameManager_Hits)
        {
            GetFromPool(hit.point);
            var randomChar = (char)Random.Range('A', 'F' + 1);
            Managers.Sound.Play(SoundManager.Sound.Effect, $"Audio/기본컨텐츠/HandFootFlip/Click_{randomChar}",
                0.3f);

            break;
        }
    }

    private void SetPool()
    {
        printPool = new Queue<GameObject>();
        _prints = new GameObject[transform.childCount];
        var index = 0;
        foreach (Transform child in transform)
            if (child != null)
                _prints[index++] = child.gameObject;

        // Only enqueue each ParticleSystem instance once
        foreach (var obj in _prints)
            if (obj != null)
            {
                printPool.Enqueue(obj);
                obj.gameObject.SetActive(false);
            }

        // Optionally, if you need more instances than available, clone them
        while (printPool.Count < _poolSize * 10)
            foreach (var obj in _prints)
                if (obj != null)
                {
                    var print = Instantiate(obj, transform);
                    print.transform.DORotateQuaternion(print.transform.rotation * Quaternion.Euler(0, Random.Range(-180, 180f), 0f), 0.01f);
                    print.gameObject.SetActive(false);
                    printPool.Enqueue(print);
                }
    }

    private readonly float RETRUN_WAIT_TIME = 100;

    private void GetFromPool(Vector3 spawnPosition)
    {
        if (printPool.Count <= 0) GrowPool();

        var print = printPool.Dequeue();
        print.transform.position = spawnPosition;
        print.gameObject.SetActive(true);

        // DOVirtual.Float(0, 0, RETRUN_WAIT_TIME, _ => { }).OnComplete(() =>
        // {
        //     print.gameObject.SetActive(false);
        //     printPool.Enqueue(print); // Return the particle system to the pool
        // });
    }

    protected void GrowPool()
    {
        foreach (var obj in _prints)
            if (obj != null)
            {
                var print = Instantiate(obj, transform);
                print.transform.DORotate(new Vector3(0f, Random.Range(-180f, 180f), 0f), 0.01f);
                print.gameObject.SetActive(false);
                printPool.Enqueue(print);
            }
    }
}