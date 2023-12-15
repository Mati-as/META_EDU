using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class BackgroundShaderController : MonoBehaviour
{
    public Material skyMaterial;
    
    private readonly int _skyColor = Shader.PropertyToID("_SkyColor");
    private readonly int _horizontalColor = Shader.PropertyToID("_HorizonColor");
    private readonly int _emissionColor = Shader.PropertyToID("_emissionColor");
    public Color[] colors;


    private int _currentColorIndex;
   private void Start()
   {
       //디폴트값을 할당하는 로직.. PreviousColor에 값 할당 로직포함
       SetSkyColor(colors[0],colors[0]);
       
       SubscribeGameManagerEvents();
       _currentColorIndex++;
   }

   private void OnDestroy()
   {
       UnsubscribeGamaManagerEvents();
   }
   private void OnApplicationQuit()
   {
       skyMaterial.SetColor(_skyColor, colors[0]);
   }
   

   private void OnGameStart()
   {
       
   }

   private void OnRoundReady()
   {
#if UNITY_EDITOR
       Debug.Log("The Color of sky is changing...");
#endif
       if (_currentColorIndex <= colors.Length)
       {
           SetSkyColor(_previousColor,colors[_currentColorIndex]);
       }
       _currentColorIndex++;
       
   }

   private void OnCorrect()
   {
       
   }

   private void OnRoundFinished()
   {
       
   }

   private void OnRoundStarted()
   {
       
   }

   private void OnGameFinished()
   {      
       SetSkyColor(_previousColor,colors[colors.Length - 1],3f);
   }

   
   private Color _previousColor;
   
   private void SetSkyColor(Color currentColor,Color nextColor,float changingDuration = 10f)
   {
       DOVirtual
           .Color(currentColor, nextColor, changingDuration, newColor =>
       {
           skyMaterial.SetColor(_skyColor, newColor);
       }).OnComplete(()=>
       {
           _previousColor = nextColor;
       });


   }
   
   private void SetHorizonColor(Color color)
   {
       skyMaterial.SetColor(_skyColor, color);
   }


    
   private void SubscribeGameManagerEvents()
   {
       AnimalTrip_GameManager.onGameStartEvent -= OnGameStart;
       AnimalTrip_GameManager.onGameStartEvent += OnGameStart;
        
       AnimalTrip_GameManager.onRoundReadyEvent -= OnRoundReady;
       AnimalTrip_GameManager.onRoundReadyEvent += OnRoundReady;

       AnimalTrip_GameManager.onCorrectedEvent -= OnCorrect;
       AnimalTrip_GameManager.onCorrectedEvent += OnCorrect;

       AnimalTrip_GameManager.onRoundFinishedEvent -= OnRoundFinished;
       AnimalTrip_GameManager.onRoundFinishedEvent += OnRoundFinished;

       AnimalTrip_GameManager.onRoundStartedEvent -= OnRoundStarted;
       AnimalTrip_GameManager.onRoundStartedEvent += OnRoundStarted;
        
       AnimalTrip_GameManager.onGameFinishedEvent -= OnGameFinished;
       AnimalTrip_GameManager.onGameFinishedEvent += OnGameFinished;
   }
    
   private void UnsubscribeGamaManagerEvents()
   {
       AnimalTrip_GameManager.onGameStartEvent -= OnGameStart;
       AnimalTrip_GameManager.onRoundReadyEvent -= OnRoundReady;
       AnimalTrip_GameManager.onCorrectedEvent -= OnCorrect;
       AnimalTrip_GameManager.onRoundFinishedEvent -= OnRoundFinished;
       AnimalTrip_GameManager.onRoundStartedEvent -= OnRoundStarted;
       AnimalTrip_GameManager.onGameFinishedEvent -= OnGameFinished;
   }





    // Archive: 이전 밤 시스템에서 활용한 쉐이더 컨트롤 로직들 입니다 (11/20/23)
    // [Header("Skybox Color Settings")] [Space(10f)]
    // public Material skyMaterial;
    //
    // public Color defaultSkyColor;
    // public Color defaultHorizonColor;
    //
    // public Color inPlayBgColor;
    // public float colorChangingSpeed;
    //
    // public float TimingToBeChanged;
    // private float _elapsedTimeForBg;
    // private float _progress;
    // private float _progressWhenFinished;
    //
    // //셰이더 파라미터 컨트롤
    // private readonly int _skyColor = Shader.PropertyToID("_SkyColor");
    // private readonly int _horizontalColor = Shader.PropertyToID("_HorizonColor");
    // private readonly int _emissionColor = Shader.PropertyToID("_emissionColor");
    //
    // private float _brightness;
    // private float _elapsedTime;
    // private float _lerpProgress; // 추가된 변수
    //
    // private void Awake()
    // {
    //   
    //     SetSkyColor(defaultSkyColor);
    //     SetHorizonColor(defaultHorizonColor);
    // }
    //
    //
    // private void Update()
    // {
    //     if (GameManager.isGameStarted)
    //     {
    //         ChangedSkyColor();
    //     }
    //
    //     if (GameManager.isGameFinished)
    //     {
    //         GetSkyColorBack();
    //     }
    // }
    //
    //
    //
    // private void ChangedSkyColor()
    // {
    //     _elapsedTimeForBg += Time.deltaTime;
    //    
    //
    //     if (_elapsedTimeForBg > TimingToBeChanged)
    //     {
    //         _progress += Time.deltaTime * colorChangingSpeed;
    //
    //         var currentColor = Color.Lerp(defaultSkyColor, inPlayBgColor, _progress);
    //         SetSkyColor(currentColor);
    //
    //         var currentHorizonColor = Color.Lerp(defaultHorizonColor, inPlayBgColor, _progress);
    //         SetHorizonColor(currentHorizonColor);
    //     }
    // }
    //
    // private void GetSkyColorBack()
    // {
    //     _progressWhenFinished += Time.deltaTime * colorChangingSpeed;
    //
    //     var currentColor = Color.Lerp( inPlayBgColor,defaultSkyColor, _progressWhenFinished);
    //     SetSkyColor(currentColor);
    //
    //     var currentHorizonColor = Color.Lerp( inPlayBgColor,defaultHorizonColor, _progressWhenFinished);
    //     SetHorizonColor(currentHorizonColor);
    // }
    //
    //
    // private void OnApplicationQuit()
    // {
    //     SetSkyColor(defaultSkyColor);
    //     SetHorizonColor(defaultHorizonColor);
    // }
    //
    //
    // private void SetSkyColor(Color color)
    // {
    //     skyMaterial.SetColor(_skyColor, color);
    // }
    //
    // private void SetHorizonColor(Color color)
    // {
    //     skyMaterial.SetColor(_horizontalColor, color);
    // }


}