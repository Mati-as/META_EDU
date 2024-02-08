using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class HandFlip2_GameManager : IGameManager
{
    private enum ColorSide
    {
        ColorA,
        ColorB,
        ColorCount
    }

    private Color _currentUnifiedColor = Color.red;
    private Color _previousUniColor = Color.black;
    private Print[] _prints;
    private int PRINTS_COUNT;
    private Vector3 _rotateVector;

    private bool _isPrintAppearFinished;
    private HandFlip2_UIManager _UIManager;


    //쌍이되는 컬러를  String으로 할당하여, 색상이름(string)에 따라 제어.
    private Dictionary<int, Print> _PrintMap;
    private Dictionary<string, Color> _colorPair;
    private Dictionary<int, MeshRenderer> _meshRendererMap;
    private Dictionary<int, MeshRenderer> _childMeshRendererMap;
    private Dictionary<int, Sequence> _moveSequence;
    private MeshRenderer[] _meshRenderers;

    private RaycastHit hit;
    private int COLOR_COUNT = 5;
    private TextMeshProUGUI _tmp;

    [SerializeField] private Color[] colorOptions; //색상조합목록

    public Color[] CurrentColorPair { get; private set; } //현재라운드의 색상 조합 (빨강-파랑, 오렌지-초록...)
    public Color ColorA {get; private set; }
    public Color ColorB {get; private set; }

    public static event Action onStart;
    private static event Action onRoundFinished;
    public static event Action onRoundFinishedForUI;
  
    public bool isATeamWin { get; private set; }
    public bool _isRoundFinished { get; private set; }
    private float _remainTime;
    private float _elapsed;
    private float _timeLimit=15;

    private int _colorACount;
    private int _colorBCount;


    public void OnStart()
    {
        onStart?.Invoke();
    }

    private void Update()
    {
        if (_isRoundFinished) return;
        if (!isStartButtonClicked) return;
        
        _elapsed += Time.deltaTime;
        _remainTime = _timeLimit - _elapsed;
        _tmp.text = $"{(int)_remainTime / 60}분 {(int)_remainTime % 60}초 남았어요!";
        if (_remainTime < 0)
        {
            onRoundFinished?.Invoke();
            _remainTime = _timeLimit;
            _tmp.text = $"그만!";
            _isRoundFinished = true;
        }
    }

    private void OnRoundFinished()
    {
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Whistle",0.5f);
        
        
        //그만! 이후 이긴팀 판정까지 걸리는 시간.
        DOVirtual.Float(0, 0, 5f, _ => { }).OnComplete(() =>
        {
            CountPrintsColor();
            CheckWinner();
            onRoundFinishedForUI?.Invoke();
        });

    }

    private void CountPrintsColor()
    {
        foreach (var print in _prints)
        {
            if (print.printObj.GetComponent<MeshRenderer>().material.color == ColorA)
            {
                _colorACount++;
            }
            else
            {
                _colorBCount++;
            }
        }

      
    }

    private void CheckWinner()
    {
        _tmp.text = _colorACount > _colorBCount ? $"홍팀 이겼다!" : "청팀 이겼다!";
        isATeamWin = _colorACount > _colorBCount;

    }

    private void InitializeParams()
    {
        _colorACount = 0;
        _colorBCount = 0;
    }




    protected override void Init()
    {
        UI_Scene_Button.onBtnShut -= OnButtonClicked;
        UI_Scene_Button.onBtnShut += OnButtonClicked;

        HandFlip2_UIManager.onStartUIFinished -= OnStart;
        HandFlip2_UIManager.onStartUIFinished += OnStart;

        HandFlip2_BlackPrintsController.onAllBlackPrintClicked -= FlipAll;
        HandFlip2_BlackPrintsController.onAllBlackPrintClicked += FlipAll;

        onRoundFinished -= OnRoundFinished;
        onRoundFinished += OnRoundFinished;

        base.Init();

        _PrintMap = new Dictionary<int, Print>();
        _colorPair = new Dictionary<string, Color>();
        _meshRendererMap = new Dictionary<int, MeshRenderer>();
        _childMeshRendererMap = new Dictionary<int, MeshRenderer>();
        _rotateVector = new Vector3(180, 0, 0);
        _tmp = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();

        _UIManager = GameObject.Find("HandFootFlip_UIManager").GetComponent<HandFlip2_UIManager>();


        Debug.Log($"(start)color option length: {colorOptions.Length}");
        var printsParent = GameObject.Find("Prints");

        if (printsParent == null)
        {
            Debug.LogError("Prints GameObject not found in the scene.");
            return;
        }

        CurrentColorPair = new Color[2];
        SetColor(0);

//반드시 게임 오브젝트의 갯수는 짝수..
#if UNITY_EDITOR
        Debug.Assert(PRINTS_COUNT % 2 == 0);
#endif
        PRINTS_COUNT = printsParent.transform.childCount;

        _prints = new Print[PRINTS_COUNT];


        for (var i = 0; i < PRINTS_COUNT; i++)
        {
            _prints[i] = new Print
            {
                printObj = printsParent.transform.GetChild(i).gameObject,
                defaultVector = printsParent.transform.GetChild(i).rotation.eulerAngles,
                currentColor = CurrentColorPair[i % (int)ColorSide.ColorCount],
                defaultSize = printsParent.transform.GetChild(i).gameObject.transform.localScale
            };


#if UNITY_EDITOR
            //Debug.Log($"colorname: {printsParent.transform.GetChild(i).gameObject.name.Substring(5)}");
#endif


            //Print 캐싱, Flip에서는 InstaceID를 기반으로 Prints를 참조 및 제어한다.
            var currentTransform = printsParent.transform.GetChild(i);
            //Transform Instance ID가 아닌 GameObject의 Instance ID를 참조할것에 주의합니다
            var currentInstanceID = currentTransform.gameObject.GetInstanceID();

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
    }


    private void SetColor(int round)
    {
        CurrentColorPair[(int)ColorSide.ColorA] = colorOptions[round % 3];
        CurrentColorPair[(int)ColorSide.ColorB] = colorOptions[round % 3 + 1];

        ColorA = CurrentColorPair[(int)ColorSide.ColorA];
        ColorB = CurrentColorPair[(int)ColorSide.ColorB];
    }

    protected override void OnRaySynced()
    {
        base.OnRaySynced();

        if (!_UIManager.isStart) return;
        if (!isStartButtonClicked) return;
        if (!_isPrintAppearFinished) return;

        FlipAndChangeColor(GameManager_Ray);
        //  ChangeColor(GameManager_Ray);
    }

    private void OnButtonClicked()
    {
        PrintsAppear();
    }

    private void PrintsAppear()
    {
        foreach (var print in _prints)
            print.printObj.transform
                .DOScale(print.defaultSize, 0.5f)
                .SetEase(Ease.InBounce)
                .SetDelay(Random.Range(2, 3.2f))
                .OnComplete(() =>
                {
                    DOVirtual.Float(0, 0, 2f, _ => { })
                        .OnComplete(() => { _isPrintAppearFinished = true; });
                });
    }


    private void FlipAndChangeColor(Ray ray)
    {
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.name.ToLower().Contains("black")) return;


            var currentInstanceID = hit.transform.gameObject.GetInstanceID();

            // Check if the object is already flipping or the sequence is active.
            if (_PrintMap.TryGetValue(currentInstanceID, out var printData) &&
                (printData.seq.IsActive() || printData.isCurrentlyFlipping))
            {
                Debug.Log("The seq is currently Active! Click later..");
                return;
            }

            // Ensure the sequence is initialized.
            printData.seq = DOTween.Sequence();

            printData.seq.Append(hit.transform
                .DOLocalRotate(_rotateVector + printData.printObj.transform.rotation.eulerAngles, 0.38f)
                .SetEase(Ease.InOutQuint)
                .OnStart(() =>
                {
                    printData.isCurrentlyFlipping = true;

                    var randomChar = (char)Random.Range('A', 'F' + 1);
                    Managers.Sound.Play(SoundManager.Sound.Effect, $"Audio/기본컨텐츠/HandFootFlip/Click_{randomChar}",
                        0.3f);

                    // Toggle 
                    var targetColor = printData.currentColor == ColorA ? ColorB : ColorA;
                    Debug.Log($"Changing to {(targetColor == ColorA ? "ColorA" : "ColorB")}");
                    printData.currentColor = targetColor;

                    // Apply the color
                    Action<Renderer, Color> doColorChange = (renderer, color) =>
                        renderer.material.DOColor(color, 0.2f).SetDelay(0.235f);
                    doColorChange(_meshRendererMap[currentInstanceID], targetColor);
                    doColorChange(_childMeshRendererMap[currentInstanceID], targetColor);
                })
                .OnComplete(() => printData.isCurrentlyFlipping = false));

            printData.seq.Play();
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("Flipping Failed");
#endif
        }
    }


    private void FlipAll()
    {
#if UNITY_EDITOR
        Debug.Log("검은색 모두 뒤집기");
#endif

        foreach (var print in _prints)
        {
            var currentInstanceID = print.printObj.GetInstanceID();

            // Check if the object is already flipping or the sequence is active.
            if (_PrintMap.TryGetValue(currentInstanceID, out var printData) &&
                (printData.seq.IsActive() || printData.isCurrentlyFlipping))
            {
                Debug.Log("The seq is currently Active! Click later..");
                return;
            }

            // Ensure the sequence is initialized.
            printData.seq = DOTween.Sequence();

            printData.seq.Append(print.printObj.transform
                .DOLocalRotate(_rotateVector + printData.printObj.transform.rotation.eulerAngles, 0.38f)
                .SetEase(Ease.InOutQuint)
                .OnStart(() =>
                {
                    printData.isCurrentlyFlipping = true;

                    var randomChar = (char)Random.Range('A', 'F' + 1);
                    Managers.Sound.Play(SoundManager.Sound.Effect, $"Audio/기본컨텐츠/HandFootFlip/Click_{randomChar}",
                        0.3f);

                    // Toggle 
                    var targetColor = printData.currentColor == ColorA ? ColorB : ColorA;
                   // Debug.Log($"Changing to {(targetColor == _colorA ? "ColorA" : "ColorB")}");
                    printData.currentColor = targetColor;

                    // Apply the color
                    Action<Renderer, Color> doColorChange = (renderer, color) =>
                        renderer.material.DOColor(color, 0.2f).SetDelay(0.235f);
                    doColorChange(_meshRendererMap[currentInstanceID], targetColor);
                    doColorChange(_childMeshRendererMap[currentInstanceID], targetColor);
                })
                .OnComplete(() => printData.isCurrentlyFlipping = false)
                .SetDelay(Random.Range(1.0f, 1.5f)));


            printData.seq.Play();
        }

        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/OnAllFlip",0.65f);
    }
}


public class Print
{
    public GameObject printObj;
    public bool side;
    public bool isCurrentlyFlipping;
    public Vector3 defaultVector;
    public Vector3 defaultSize;
    public Sequence seq;
    public Color currentColor;
}