using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ScratchPainting_GameManager : IGameManager
{
    public static bool isInit { get; private set; }

    public Queue<GameObject> printPool { get; set; }
    private GameObject[] _prints;

    private readonly float _poolSize = 50;
    //private SpriteRenderer _bgSprite;
    private float _elapsed;
    
   [Range(0,30)]
    public  float timeLimit = 30f;
    private float _remainTime;
    private bool _isRoundReady;
    private TextMeshProUGUI _tmp;

    //Glowing Outline MeshRenderer
    private SpriteRenderer _outlineSpRenderer;
    private Color _glowDefaultColor;
    private Sequence _glowSeq;

    private Vector3 _defaultPrintScale;

    public static event Action OnStampingFinished;
    public static event Action printInitEvent;
    public static event Action onRoundRestart;

    protected override void Init()
    {
        base.Init();
        DOTween.Init().SetCapacity(1500, 500);
        SetPool();

        _tmp = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        _tmp.text = string.Empty;
        
        DEFAULT_SENSITIVITY = 0.11f;
    }

    protected override void BindEvent()
    {
        base.BindEvent();

        OnStampingFinished -= OnRoundFinished;
        OnStampingFinished += OnRoundFinished;

        onRoundRestart -= OnRoundRestart;
        onRoundRestart += OnRoundRestart;

        ScratchPainting_UIManager.onStartUI -= OnStartUI;
        ScratchPainting_UIManager.onStartUI += OnStartUI;
    }

    private void OnDestroy()
    {
        OnStampingFinished -= OnRoundFinished;
        onRoundRestart -= OnRoundRestart;
        ScratchPainting_UIManager.onStartUI -= OnStartUI;
    }

    private float _elapsedToCount;
    private bool _isCountNarrationPlaying;

    private void OnStartUI()
    {
        _isRoundReady = true;
    }

    private void Update()
    {
        if (!_isRoundReady) return;
        _elapsed += Time.deltaTime;
        _remainTime = timeLimit - _elapsed;


        if (_remainTime <= 6f && _remainTime >= 1)
        {
            if (!_isCountNarrationPlaying)
            {
                Managers.soundManager.Play
                    (SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Count" + $"{(int)_remainTime}", 0.8f);
                _isCountNarrationPlaying = true;
                _elapsedToCount = 0;
            }

            if (_elapsedToCount > 1f) _isCountNarrationPlaying = false;
            _elapsedToCount += Time.deltaTime * 0.9f;
        }


        if (_elapsed > timeLimit)
        {
            _isRoundReady = false;
            OnStampingFinished.Invoke();
            _tmp.text = string.Empty;
        }
        else
        {
            _tmp.text = $"{(int)_remainTime}초";
        }
    }

    

    private void BlinkOutline()
    {
#if UNITY_EDITOR
        Debug.Log("Outline Glowing~~!@##~!@#@!~$@~#");
#endif

        _glowSeq.Append(_outlineSpRenderer.material.DOColor(_glowDefaultColor * 1.25f, 2f));
        _glowSeq.AppendInterval(0.3f);
        _glowSeq.Append(_outlineSpRenderer.material.DOColor(_glowDefaultColor * 0.5f, 0.7f));
        _glowSeq.AppendInterval(0.99f);
        _glowSeq.SetLoops(-1, LoopType.Yoyo);
    }

    private void OnRoundRestart()
    {
        _elapsedToCount = 0f;
        _isCountNarrationPlaying = false;

        _remainTime = timeLimit;
        _elapsed = 0;
    }

    private void OnRoundFinished()
    {
        DOVirtual.Float(0, 0, 1f, _ => { }).OnComplete(() =>
        {
            _glowSeq.Kill();
            _glowSeq = DOTween.Sequence();
            //_glowSeq.Append(_outlineSpRenderer.material.DOColor(_glowDefaultColor, 0.3f));


            DOVirtual.Float(0, 0, 2.5f, _ => { }).OnComplete(() =>
            {
                FadeInBg();
                //"그만" UI 팝업? 

                Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandPainting/OnRoundFinish", 0.8f);
                DOVirtual.Float(0, 0, 3, _ => { }).OnComplete(() =>
                {
                  // Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/OnReady", 0.8f);
                  //  _tmp.text = "놀이를 다시 준비하고 있어요";
                });

                DOVirtual.Float(0, 0, 120f, _ => { }).OnComplete(() => { printInitEvent?.Invoke(); });

                DEFAULT_SENSITIVITY = 0.05f;
                DOVirtual.Float(0, 0, 120, _ => { }).OnComplete(() => { onRoundRestart?.Invoke(); });
            });
        });
    }

    private void FadeInBg() => DOVirtual.Float(0, 0, 0.2f, _ => { }).OnComplete(() =>
    {
#if UNITY_EDITOR
        Debug.Log("DoFade In");
#endif
       // _bgSprite.DOFade(1, 1f);
    });


    public override void OnRaySynced()
    {
        if (!_isRoundReady) return;
        if (!isStartButtonClicked) return;


        Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in GameManager_Hits)
            if (hit.transform.gameObject.name.Contains("CrayonBg"))
            {
                GetFromPool(hit.point);
                var randomChar = (char)Random.Range('A', 'F' + 1);
                Managers.soundManager.Play(SoundManager.Sound.Effect, $"Audio/기본컨텐츠/HandFootFlip/Click_{randomChar}",
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

                    _defaultPrintScale = print.transform.localScale;


                    print.transform.DORotate(new Vector3(0f, Random.Range(-180f, 180f), 0f), 0.01f);
                    print.gameObject.SetActive(false);
                    printPool.Enqueue(print);
                }
    }

    private readonly float RETRUN_WAIT_TIME = 250;

    private void GetFromPool(Vector3 spawnPosition)
    {
        if (printPool.Count <= 0) GrowPool();

        var print = printPool.Dequeue();
        print.transform.position = spawnPosition;
        print.gameObject.SetActive(true);

        print.transform.localScale = Vector3.zero;
        print.transform.DOScale(_defaultPrintScale, 0.05f).SetEase(Ease.InOutElastic);

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
                print.transform.DORotate(new Vector3(0f, Random.Range(-180f, 180f), 0f), 0.01f);
                print.gameObject.SetActive(false);
                printPool.Enqueue(print);
            }
    }
}