using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class EA008_BubbleShower_GameManager : Ex_BaseGameManager
{
    private enum ColorSide
    {
        ColorA,
        ColorB,
        ColorCount
    }

    private int _currentRound;
    private Color _currentUnifiedColor = Color.red;
    private Color _previousUniColor = Color.black;
    private BubbleGermObj[] _prints;
    private int PRINTS_COUNT;
    private Vector3 _rotateVector;
    
    private readonly Stack<ParticleSystem> _particlPool = new();


    private EA008_BubbleShower_UIManager _UIManager;


    //쌍이되는 컬러를  String으로 할당하여, 색상이름(string)에 따라 제어.
    private Dictionary<int, BubbleGermObj> _PrintMap;

    private Dictionary<int, Quaternion> _originalEulerMap;
    private Dictionary<string, Color> _colorPair;
    private Dictionary<int, MeshRenderer> _meshRendererMap;
    private Dictionary<int, MeshRenderer> _childMeshRendererMap;
    private Dictionary<int, Sequence> _moveSequence;
    private MeshRenderer[] _meshRenderers;

    private RaycastHit hit;
    private int COLOR_COUNT = 5;
    private TextMeshProUGUI _tmp;

    [SerializeField] private Color[] colorOptions; //색상조합목록

    public Color[] CurrentColorPair
    {
        get;
        private set;
    } //현재라운드의 색상 조합 (빨강-파랑, 오렌지-초록...)

    public Color ColorA
    {
        get;
        private set;
    }

    public Color ColorB
    {
        get;
        private set;
    }

    public static event Action onStart;
    public static event Action onRoundFinished;
    public static event Action onRoundFinishedForUI;
    public static event Action restart;
    public static event Action roundInit;

    public bool isATeamWin
    {
        get;
        private set;
    }

    public bool _isRoundFinished
    {
        get;
        private set;
    }

    private float _remainTime;
    private float _elapsed;
    [SerializeField] private float TIME_LIMIT = 30f;

    private int _germCount;
    private int _bubbleCount;


public void OnStart()
{
    onStart?.Invoke();

    foreach (var bubbleGermObj in _prints)
    {
        // ✅ 기존 Shake 애니메이션 완전히 중지 후 제거
        bubbleGermObj.shakeSeq?.Kill();
        bubbleGermObj.shakeSeq = DOTween.Sequence();

        // ✅ 새로운 Shake 애니메이션을 처음부터 재설정
        bubbleGermObj.shakeSeq.Append(bubbleGermObj.printObj.transform
                .DOShakePosition(3f, 0.2f, 2, 120)
                .SetLoops(-1, LoopType.Yoyo))
            .SetAutoKill(false);
            //.Pause();

        // ✅ 기존 ShakeRotation 중지 후 재설정
        bubbleGermObj.shakeRotationSeq?.Kill();
        bubbleGermObj.shakeRotationSeq = DOTween.Sequence();

        // ✅ 현재 회전 값 저장 (중복 실행 방지)
        Quaternion initialRotation = bubbleGermObj.printObj.transform.localRotation;

        // ✅ 새로운 Rotate Shake 애니메이션 적용 (현재 회전 값 기준으로 랜덤 회전)
        bubbleGermObj.shakeRotationSeq.Append(bubbleGermObj.printObj.transform
                .DORotateQuaternion(
                    initialRotation * Quaternion.Euler(0, Random.Range(-60, 60), 0), 3f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.Linear))
            .SetAutoKill(false);
        // .Pause();
    }
    
    DOVirtual.DelayedCall(0.6f, () =>
    {
        _isClickable = true;
    });
}


    private float _elapsedToCount;
    private bool _isCountNarrationPlaying;

    private void Update()
    {
        if (_isRoundFinished) return;
        if (!isStartButtonClicked) return;
        if (!_UIManager.isStart) return;


        _remainTime = TIME_LIMIT - _elapsed;
        _elapsed += Time.deltaTime * 0.9f;


        _tmp.text = _remainTime > 60
            ? $"{(int)_remainTime / 60}분 {(int)_remainTime % 60}초"
            : $"{(int)_remainTime % 60}초";


        if (_remainTime <= 6f && _remainTime >= 1)
        {
            if (!_isCountNarrationPlaying)
            {
                Managers.Sound.Play
                    (SoundManager.Sound.Effect, "Audio/BasicContents/HandFlip2/Count" + $"{(int)_remainTime}", 0.8f);
                _isCountNarrationPlaying = true;
                _elapsedToCount = 0;
            }

            if (_elapsedToCount > 1f) _isCountNarrationPlaying = false;
            _elapsedToCount += Time.deltaTime * 0.9f;
        }

        if (_remainTime < 0)
        {
            onRoundFinished?.Invoke();
            _tmp.text = "";
            _isRoundFinished = true;
            _isClickable = false;
        }

        if (!_UIManager.isStart) _remainTime = TIME_LIMIT;
    }

    private void OnRoundFinished()
    {
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFlip2/Whistle", 0.5f);


        //그만! 이후 이긴팀 판정까지 걸리는 시간.
        DOVirtual.Float(0, 0, 5f, _ =>
        {
        }).OnComplete(() =>
        {
            CountPrintsColor();
            CheckWinner();
            onRoundFinishedForUI?.Invoke();
        });
    }

    private void CountPrintsColor()
    {
        foreach (var print in _prints)
            // if (print.printObj.GetComponent<MeshRenderer>().material.color == ColorA)
            if (print.isGermSide)
                _germCount++;
            else
                _bubbleCount++;
    }


    private TextMeshProUGUI _red;
    private TextMeshProUGUI _vs;
    private TextMeshProUGUI _blue;

    private void UpdateResultTMP(string red = "", string blue = "", string vs = "vs")
    {
        _red.text = red;
        _blue.text = blue;
        _vs.text = vs;
    }

    private void GetResultTMPs()
    {
        _red = GameObject.Find("Red").GetComponent<TextMeshProUGUI>();
        _red.text = string.Empty;
        _blue = GameObject.Find("Blue").GetComponent<TextMeshProUGUI>();
        _blue.text = string.Empty;
        _vs = GameObject.Find("Vs").GetComponent<TextMeshProUGUI>();
        _vs.text = string.Empty;
    }

    private void CheckWinner()
    {
        UpdateResultTMP(_germCount.ToString(), _bubbleCount.ToString());
        isATeamWin = _germCount > _bubbleCount;

        StartCoroutine(Initialize());
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UI_InScene_StartBtn.onGameStartBtnShut -= OnGameStartButtonClicked;
        EA008_BubbleShower_UIManager.onStartUIFinished -= OnStart;
        EA008_BubbleShower_FlipAllGermController.onAllBlackPrintClicked -= FlipAll;
        onRoundFinished -= OnRoundFinished;
    }

    private WaitForSeconds _wait;
    private float _waitTIme = 4.5f;

    private IEnumerator Initialize()
    {
        yield return DOVirtual.Float(0, 0, 10f, _ =>
        {
        }).WaitForCompletion();
        UpdateResultTMP(string.Empty, string.Empty, string.Empty);
        _tmp.text = "놀이를 다시 준비하고 있어요";
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFlip2/OnReady", 0.8f);

        InitFlip();
        yield return DOVirtual.Float(0, 0, 3f, _ =>
        {
        }).WaitForCompletion();

        _tmp.text = "";

        InitializeParams();
        SetColor(_currentRound);

        int count = 0;


        yield return DOVirtual.Float(0, 0, 5f, _ =>
        {
        }).WaitForCompletion();


        _isRoundFinished = false;
        restart?.Invoke();
    }


    private void InitializeParams()
    {
#if UNITY_EDITOR
        Debug.Log("GM ReInit..");
#endif
        roundInit?.Invoke();

        _elapsed = 0;
        _remainTime = TIME_LIMIT;
        _currentRound++;
        _germCount = 0;
        _bubbleCount = 0;
        _elapsedToCount = 0f;
        _isCountNarrationPlaying = false;

        foreach (var bubbleGermObj in _prints)
        {
            // ✅ 모든 DOTween 애니메이션 중지 후 초기화
            bubbleGermObj.flipSeq?.Kill();
            bubbleGermObj.flipSeq = DOTween.Sequence();

            bubbleGermObj.shakeSeq?.Kill();
            bubbleGermObj.shakeSeq = DOTween.Sequence();

            bubbleGermObj.shakeRotationSeq?.Kill();
            bubbleGermObj.shakeRotationSeq = DOTween.Sequence();
        }
    }




    protected override void Init()
    {
        UI_InScene_StartBtn.onGameStartBtnShut -= OnGameStartButtonClicked;
        UI_InScene_StartBtn.onGameStartBtnShut += OnGameStartButtonClicked;

        EA008_BubbleShower_UIManager.onStartUIFinished -= OnStart;
        EA008_BubbleShower_UIManager.onStartUIFinished += OnStart;

        EA008_BubbleShower_FlipAllGermController.onAllBlackPrintClicked -= FlipAll;
        EA008_BubbleShower_FlipAllGermController.onAllBlackPrintClicked += FlipAll;

        onRoundFinished -= OnRoundFinished;
        onRoundFinished += OnRoundFinished;

        base.Init();

        _defaultPosition = Camera.main.transform.position;

        _originalEulerMap = new Dictionary<int, Quaternion>();
        _PrintMap = new Dictionary<int, BubbleGermObj>();
        _colorPair = new Dictionary<string, Color>();
        _meshRendererMap = new Dictionary<int, MeshRenderer>();
        _childMeshRendererMap = new Dictionary<int, MeshRenderer>();
        _rotateVector = new Vector3(180, 0, 0);
        _tmp = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        _tmp.text = string.Empty;
        GetResultTMPs();
        _UIManager = GameObject.Find("HandFootFlip_UIManager").GetComponent<EA008_BubbleShower_UIManager>();


        Debug.Log($"(start)color option length: {colorOptions.Length}");
        var printsParent = GameObject.Find("Prints");

        if (printsParent == null)
        {
            Debug.LogError("Prints GameObject not found in the scene.");
            return;
        }

        CurrentColorPair = new Color[2];
        SetColor(0);


        PRINTS_COUNT = printsParent.transform.childCount;

        //반드시 게임 오브젝트의 갯수는 홀수
        Debug.Assert(PRINTS_COUNT % 2 == 1);


        _prints = new BubbleGermObj[PRINTS_COUNT];


        for (int i = 0; i < PRINTS_COUNT; i++)
        {
            _prints[i] = new BubbleGermObj {
                printObj = printsParent.transform.GetChild(i).gameObject,
                defaultVector = printsParent.transform.GetChild(i).rotation.eulerAngles,
                currentColor = CurrentColorPair[i % (int)ColorSide.ColorCount],
                defaultSize = printsParent.transform.GetChild(i).gameObject.transform.localScale,
               
                flipSeq = DOTween.Sequence()
            };
            _originalEulerMap.TryAdd(_prints[i].printObj.GetInstanceID(),
                _prints[i].printObj.gameObject.transform.localRotation);


            //Print 캐싱, Flip에서는 InstaceID를 기반으로 Prints를 참조 및 제어한다.
            var currentTransform = printsParent.transform.GetChild(i);
            //Transform Instance ID가 아닌 GameObject의 Instance ID를 참조할것에 주의합니다
            int currentInstanceID = currentTransform.gameObject.GetInstanceID();

            _PrintMap.TryAdd(currentInstanceID, _prints[i]);

            //MeshRenderer 캐싱, Instace ID로 MeshRenderer제어
            var meshRenderer = currentTransform.GetComponent<MeshRenderer>();
            _meshRendererMap.TryAdd(currentInstanceID, meshRenderer);


            meshRenderer.material.color = CurrentColorPair[i % (int)ColorSide.ColorCount];


            meshRenderer = currentTransform.GetChild(0).GetComponentInChildren<MeshRenderer>();

            _childMeshRendererMap.TryAdd(currentInstanceID, meshRenderer);

            meshRenderer.material.color = CurrentColorPair[i % (int)ColorSide.ColorCount];

            printsParent.transform.GetChild(i).gameObject.transform.localScale = Vector3.zero;
        }

        InitFlip();
        Logger.Log("GM Initialized..-----------------------------");
    }


    private void SetColor(int round)
    {
        CurrentColorPair[(int)ColorSide.ColorA] = colorOptions[2 * round % 6];
        CurrentColorPair[(int)ColorSide.ColorB] = colorOptions[2 * round % 6 + 1];

        ColorA = CurrentColorPair[(int)ColorSide.ColorA];
        ColorB = CurrentColorPair[(int)ColorSide.ColorB];
    }

    private Vector3 _defaultPosition;

    private bool _isClickable;
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        /* 클릭되면 안되는 경우 상세설명
         1. UI의 시작버튼 애니메이션이 끝나지 않은 경우
         2. 처음시작 시, Button 클릭 안 한 경우
         3. 라운드 끝난경우

         */
        if (!_UIManager.isStart) return;
        if (_isRoundFinished) return;
        if (!_isClickable) return;


        FlipAndChangeColor(GameManager_Ray);
        //  ChangeColor(GameManager_Ray);
    }

    private void ShakeCam()
    {
        Camera.main.transform.DOShakePosition(0.3f, 0.035f, 2).OnComplete(() =>
        {
            Camera.main.transform.DOMove(_defaultPosition, 0.3f);
        });
    }

 

    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();
      
        initialMessage= "세균팀과 거품팀으로 나눠, 놀이 해볼까요?";
         baseUIManager.PopInstructionUIFromScaleZero(initialMessage);
        Managers.Sound.Play(SoundManager.Sound.Narration, "OnGameStartNarration/" + SceneManager.GetActiveScene().name + "_intronarration");
        
        DOVirtual.DelayedCall(4f, () =>
        {
            PrintsAppear();
        });
    }

    private void PrintsAppear()
    {
        foreach (var print in _prints)
            print.printObj.transform
                .DOScale(print.defaultSize, 0.4f)
                .OnStart(() =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFootFlip/Click_A",
                        0.25f);
                })
                .SetEase(Ease.InBounce)
                .SetDelay(Random.Range(1, 1.8f));
    }


    private void FlipAndChangeColor(Ray ray)
    {
        if (Physics.Raycast(ray, out hit))
        {
            var currentPS = GetFromPool();
            currentPS.transform.position = hit.point;
            currentPS.Play();
            StartCoroutine(ReturnToPoolAfterDelay(currentPS));


            if (hit.transform.gameObject.name.ToLower().Contains("black")) return;
            ShakeCam();

            int currentInstanceID = hit.transform.gameObject.GetInstanceID();

            // Check if the object is already flipping or the sequence is active.
            if (_PrintMap.TryGetValue(currentInstanceID, out var bubbleOrGermData) &&
                (bubbleOrGermData.flipSeq.IsActive() || bubbleOrGermData.isCurrentlyFlipping))
                // Debug.Log("The seq is currently Active! Click later..");
                return;

            

            
            // 기존 실행 중인 Sequence Kill (중복 실행 방지)
            bubbleOrGermData.flipSeq?.Kill();
            bubbleOrGermData.shakeRotationSeq?.Kill();
            bubbleOrGermData.shakeSeq?.Kill();
            bubbleOrGermData.scaleSeq?.Kill();
// 새로운 Sequence 생성
            bubbleOrGermData.shakeSeq = DOTween.Sequence();
            bubbleOrGermData.scaleSeq = DOTween.Sequence();
            bubbleOrGermData.flipSeq = DOTween.Sequence();
            bubbleOrGermData.shakeRotationSeq = DOTween.Sequence();

// 목표 회전값 계산 (Local 기준)
            Quaternion targetRotation =
                Quaternion.Euler(_rotateVector) * bubbleOrGermData.printObj.transform.localRotation;

// 회전 애니메이션 중지 (간섭 방지)
            bubbleOrGermData.shakeRotationSeq.Pause();

// Flip 애니메이션 실행
            bubbleOrGermData.flipSeq.Append(bubbleOrGermData.printObj.transform
                .DOLocalRotateQuaternion(targetRotation, 0.38f)
                .SetEase(Ease.InOutQuint)
                .OnStart(() =>
                {
                    bubbleOrGermData.isCurrentlyFlipping = true;

                    char randomChar = (char)Random.Range('A', 'F' + 1);

                    if (!bubbleOrGermData.isGermSide)
                        Managers.Sound.Play(SoundManager.Sound.Effect, "EA008/WaterBubbleEffectSound", 0.8f);
                    else
                        Managers.Sound.Play(SoundManager.Sound.Effect,
                            $"Audio/BasicContents/HandFootFlip/Click_{randomChar}", 0.3f);

                    bubbleOrGermData.isGermSide = !bubbleOrGermData.isGermSide;
                })
                .OnComplete(() =>
                {
                    bubbleOrGermData.isCurrentlyFlipping = false;

                    // Flip이 완료된 후 현재 회전 값을 가져옴 (Shake 반영을 위해)
                    Quaternion finalRotation = bubbleOrGermData.printObj.transform.localRotation;

                    // Shake 애니메이션 (Local 기준으로 흔들기)
                    bubbleOrGermData.shakeSeq.Append(bubbleOrGermData.printObj.transform
                        .DOLocalMove(bubbleOrGermData.printObj.transform.localPosition + new Vector3(0.2f, 0, 0),
                            0.1f) // 오른쪽 이동
                        .SetLoops(2, LoopType.Yoyo) // 흔들림 효과
                        .SetEase(Ease.InOutQuint));

                    // Rotate 애니메이션 (Shake 이후 실행, Flip 완료된 상태를 기반으로 적용)
                    bubbleOrGermData.shakeRotationSeq.Append(bubbleOrGermData.printObj.transform
                        .DORotateQuaternion(
                            finalRotation * Quaternion.Euler(0, Random.Range(-60, 60), 0), 3f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.Linear));

                    // 애니메이션 실행
                    bubbleOrGermData.shakeSeq.Play();
                    bubbleOrGermData.shakeRotationSeq.Play();
                }));

// Scale 애니메이션 실행
            bubbleOrGermData.scaleSeq.Join(hit.transform
                    .DOScale(bubbleOrGermData.defaultSize * 1.4f, 0.38f)
                    .SetEase(Ease.InOutQuint))
                .Append(hit.transform
                    .DOScale(bubbleOrGermData.defaultSize, 0.28f)
                    .SetEase(Ease.InOutQuint));
        }
    }


    private void InitFlip()
    {
        foreach (var print in _prints)
        {
            print.flipSeq?.Kill();
            print.shakeRotationSeq?.Kill();
            print.shakeSeq?.Kill();
            print.scaleSeq?.Kill();
        }
        
        int count = 0;

        foreach (var print in _prints)
        {
            int currentInstanceID = print.printObj.GetInstanceID();

            // Check if the object is already flipping or the sequence is active.
            if (_PrintMap.TryGetValue(currentInstanceID, out var printData) &&
                (printData.flipSeq.IsActive() || printData.isCurrentlyFlipping))
                // Debug.Log("The seq is currently Active! Click later..");
                return;

            bool isGermSide = false;
            var eulerToFlip = Quaternion.Euler(Vector3.zero);

            if (count % 2 == 0)
            {
                isGermSide = false;
                eulerToFlip = _originalEulerMap[currentInstanceID] * Quaternion.Euler(_rotateVector);
                printData.flipSeq = DOTween.Sequence();
            }
            else
            {
                isGermSide = true;
                eulerToFlip = _originalEulerMap[currentInstanceID];
                printData.flipSeq = DOTween.Sequence();
            }

            Logger.Log($"Default Original Euler : {_originalEulerMap[currentInstanceID]}");

            printData.flipSeq.Append(print.printObj.transform
                .DOLocalRotateQuaternion(eulerToFlip, 0.38f)
                .SetEase(Ease.InOutQuint)
                .OnStart(() =>
                {
                    printData.isCurrentlyFlipping = true;
                    printData.isGermSide = isGermSide;
                    char randomChar = (char)Random.Range('A', 'F' + 1);
                    Managers.Sound.Play(SoundManager.Sound.Effect,
                        $"Audio/BasicContents/HandFootFlip/Click_{randomChar}",
                        0.3f);
                })
                .OnComplete(() => printData.isCurrentlyFlipping = false)
                .SetDelay(Random.Range(1.0f, 1.5f)));


            printData.flipSeq.Play();
            
        
            count++;
        }

        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFlip2/OnAllFlip", 0.65f);
    }

    private void FlipAll()
    {
        foreach (var print in _prints)
        {
            int currentInstanceID = print.printObj.GetInstanceID();

            if (_PrintMap.TryGetValue(currentInstanceID, out var printData) &&
                (printData.flipSeq.IsActive() || printData.isCurrentlyFlipping))
                return;

            printData.flipSeq.Kill();
            printData.flipSeq = DOTween.Sequence();

            printData.flipSeq.Append(print.printObj.transform
                .DOLocalRotateQuaternion(Quaternion.Euler(_rotateVector) * printData.printObj.transform.localRotation, 0.38f)
                .SetEase(Ease.InOutQuint)
                .OnStart(() =>
                {
                    printData.isCurrentlyFlipping = true;
                    printData.isGermSide = !printData.isGermSide;
                    char randomChar = (char)Random.Range('A', 'F' + 1);
                    Managers.Sound.Play(SoundManager.Sound.Effect,
                        $"Audio/BasicContents/HandFootFlip/Click_{randomChar}", 0.3f);
                }));

            // ✅ Flip 이후에 Shake를 실행하도록 변경
            printData.flipSeq.OnComplete(() =>
            {
                Quaternion finalRotation = printData.printObj.transform.localRotation;

                printData.shakeSeq.Rewind();
                printData.shakeSeq.Play();

                printData.shakeRotationSeq.Kill();
                printData.shakeRotationSeq = DOTween.Sequence();

                printData.shakeRotationSeq.Append(printData.printObj.transform
                        .DORotateQuaternion(
                            finalRotation * Quaternion.Euler(0, Random.Range(-60, 60), 0), 3f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.Linear))
                    .SetAutoKill(false)
                    .Play();
                
                
                printData.isCurrentlyFlipping = false;
            });

            printData.flipSeq.Play();
        }
    }

    private void SetPool()
    {
        var particlePrefab = Resources.Load<GameObject>("Runtime/EA008/ClickEffect");

        for (int i = 0; i < 100; i++)
        {
            var ps = Instantiate(particlePrefab, Vector3.zero, Quaternion.identity).GetComponent<ParticleSystem>();
            _particlPool.Push(ps);
        }
    }

    private ParticleSystem GetFromPool()
    {
        if (_particlPool.Count > 0)
        {
            var ps = _particlPool.Pop();
            ;
            ps.gameObject.SetActive(true);
            return ps;
        }

        SetPool();
        var newPs = _particlPool.Pop();
        ;
        newPs.gameObject.SetActive(true);
        return newPs;
    }

    private WaitForSeconds _poolReturnWait;

    protected IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps)
    {
        if (_poolReturnWait == null) _poolReturnWait = new WaitForSeconds(ps.main.startLifetime.constantMax);

        yield return _poolReturnWait;


        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        _particlPool.Push(ps); // Return the particle system to the pool
    }
}


public class BubbleGermObj
{
    public GameObject printObj;
    public bool isGermSide;
    public bool isCurrentlyFlipping;
    // public Quaternion defaultRotation;
    // public Vector3 defaultPosition;
    public Vector3 defaultVector;
    public Vector3 defaultSize;
    public Sequence flipSeq;
    public Sequence shakeRotationSeq;
    public Sequence shakeSeq;
    public Sequence scaleSeq;
    public Color currentColor;
}