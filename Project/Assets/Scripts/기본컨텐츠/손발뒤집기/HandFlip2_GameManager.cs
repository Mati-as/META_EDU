using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Random = UnityEngine.Random;

public class HandFlip2_GameManager : IGameManager
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
    private Print[] _prints;
    private int PRINTS_COUNT;
    private Vector3 _rotateVector;
   
    
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
    public static event Action onRoundFinished;
    public static event Action onRoundFinishedForUI;
    public static event Action restart;
    public static event Action roundInit;
  
    public bool isATeamWin { get; private set; }
    public bool _isRoundFinished { get; private set; }
    private float _remainTime;
    private float _elapsed;
    private readonly float TIME_LIMIT = 30f;

    private int _colorACount;
    private int _colorBCount;


    public void OnStart()
    {
        
        onStart?.Invoke();
        
    }

    private float _elapsedToCount;
    private bool _isCountNarrationPlaying;

    private void Update()
    {
        if (_isRoundFinished) return;
        if (!isStartButtonClicked) return;
        if (!_UIManager.isStart) return;
        
      
        _remainTime = TIME_LIMIT - _elapsed;
        _elapsed += Time.deltaTime *0.9f;

   
    
        _tmp.text = _remainTime > 60? $"{(int)_remainTime / 60}분 {(int)_remainTime % 60}초" : $"{(int)_remainTime % 60}초";
        
     
        
        

        if (_remainTime <= 6f && _remainTime >= 1)
        {
           
            
            if (!_isCountNarrationPlaying)
            {
                Managers.Sound.Play
                    (SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Count"+$"{(int)_remainTime}",0.8f);
                _isCountNarrationPlaying = true;
                _elapsedToCount = 0;
            }
            
            if (_elapsedToCount > 1f) _isCountNarrationPlaying = false;
            _elapsedToCount += Time.deltaTime *0.9f;

          
        }
        
        if (_remainTime < 0)
        {
            onRoundFinished?.Invoke();
            _tmp.text = $"";
            _isRoundFinished = true;
           
        }

        if (!_UIManager.isStart) _remainTime = TIME_LIMIT;
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

    private TextMeshProUGUI _red;
    private TextMeshProUGUI _vs;
    private TextMeshProUGUI _blue;
    private void UpdateResultTMP(string red ="",string blue="",string vs ="vs")
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
        UpdateResultTMP(_colorACount.ToString(),_colorBCount.ToString());
        isATeamWin = _colorACount > _colorBCount;

        StartCoroutine(Initialize());
    }
    
    private WaitForSeconds _wait;
    private float _waitTIme= 4.5f;
    private IEnumerator Initialize()
    {

        yield return DOVirtual.Float(0, 0, 10f, _ => { }).WaitForCompletion();
        UpdateResultTMP(String.Empty,String.Empty,String.Empty);
        _tmp.text = "놀이를 다시 준비하고 있어요";
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/OnReady",0.8f);
        yield return DOVirtual.Float(0, 0, 3f, _ => { }).WaitForCompletion();
       
        _tmp.text = "";
      
        InitializeParams();
        SetColor(_currentRound);
        
        int count=0;

      
        
        foreach(var item in _meshRendererMap)
        {
            MeshRenderer meshRenderer = item.Value;
            meshRenderer.material.DOColor(CurrentColorPair[ count % (int)ColorSide.ColorCount],1f);
            _PrintMap[item.Key].currentColor = CurrentColorPair[ count % (int)ColorSide.ColorCount];
            count++;
        }
        
        foreach(var item in _childMeshRendererMap)
        {
            MeshRenderer meshRenderer = item.Value;
            meshRenderer.material.DOColor(CurrentColorPair[ count % (int)ColorSide.ColorCount],1f);
            _PrintMap[item.Key].currentColor = CurrentColorPair[ count % (int)ColorSide.ColorCount];
            count++;
        }
        
        FlipAll();
        
        yield return DOVirtual.Float(0, 0, 5f, _ => { }).WaitForCompletion();
        
       
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
        _colorACount = 0;
        _colorBCount = 0;
        _elapsedToCount = 0f;
        _isCountNarrationPlaying = false;
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

        _defaultPosition = Camera.main.transform.position;

        _PrintMap = new Dictionary<int, Print>();
        _colorPair = new Dictionary<string, Color>();
        _meshRendererMap = new Dictionary<int, MeshRenderer>();
        _childMeshRendererMap = new Dictionary<int, MeshRenderer>();
        _rotateVector = new Vector3(180, 0, 0);
        _tmp = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        _tmp.text = string.Empty;
        GetResultTMPs();
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
        CurrentColorPair[(int)ColorSide.ColorA] = colorOptions[2 * round % 6 ];
        CurrentColorPair[(int)ColorSide.ColorB] = colorOptions[2 * round % 6 + 1];

        ColorA = CurrentColorPair[(int)ColorSide.ColorA];
        ColorB = CurrentColorPair[(int)ColorSide.ColorB];
        
    }

    private Vector3 _defaultPosition; 

    protected override void OnRaySynced()
    {
        base.OnRaySynced();

        /* 클릭되면 안되는 경우 상세설명
         1. UI의 시작버튼 애니메이션이 끝나지 않은 경우
         2. 처음시작 시, Button 클릭 안 한 경우
         3. 라운드 끝난경우
         
         */
        if (!_UIManager.isStart) return;
        if (!isStartButtonClicked) return;
        if (_isRoundFinished) return;


       
        FlipAndChangeColor(GameManager_Ray);
        //  ChangeColor(GameManager_Ray);
    }
    
    private void ShakeCam()=> Camera.main.transform.DOShakePosition(0.22f, 0.055f, 5).OnComplete(() =>
    {
        Camera.main.transform.DOMove(_defaultPosition, 0.3f);
    });

    private void OnButtonClicked()
    {
        PrintsAppear();
    }

    private void PrintsAppear()
    {
        foreach (var print in _prints)
            print.printObj.transform
                .DOScale(print.defaultSize, 0.4f)
                .OnStart(() =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Effect, $"Audio/기본컨텐츠/HandFootFlip/Click_A",
                        0.25f);

                })
                .SetEase(Ease.InBounce)
                .SetDelay(Random.Range(1, 1.8f));

    }


    private void FlipAndChangeColor(Ray ray)
    {
        if (Physics.Raycast(ray, out hit))
        {
            
            if (hit.transform.gameObject.name.ToLower().Contains("black")) return;
            ShakeCam();

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