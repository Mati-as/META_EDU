using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using DG.Tweening;


public class AnimalShaderController : MonoBehaviour
{
    
    //▼ Scriptable Objects 참조 부분, 스태틱 및 초기화 설정

    [Header("Reference (Scriptable Object)")] 
    [Space(15f)]
    [SerializeField] 
    private ShaderAndCommon _shaderAndCommon;
    [SerializeField] 
    private AnimalData _animalData;
    
    //▼ Time.deltaTime이용 인스턴스.
    private float fresnelElapsedTime;
    private float _elapsed;
    private float _colorLerp;
    private float _elapsedForInPlayGlowOn;
    
    //▼ 쉐이더 컨트롤 및 머테리얼 할당.
    private SkinnedMeshRenderer _glowMeshRenderer;
    private SkinnedMeshRenderer _bodyMeshRenderer;
    private readonly int EMISSION_COLOR = Shader.PropertyToID("_emissionColor");
    private readonly int FRESNEL_COLOR = Shader.PropertyToID("_FresnelPower");
    private readonly int BODY_COLOR = Shader.PropertyToID("_Color");
    private Material _glowMat;
    private Material _bodyMat;
    
    public static bool isGlowOn { get; private set; }
    /*
     아래 코루틴 변수들은 IEnumerator 컨테이너 역할만 담당합니다.
     어떤 함수가 사용되는지는 StartCoroutine에서확인 및 디버깅 해야합니다.
     */
    private Coroutine _coroutineA;
    private Coroutine _coroutineB;
    private Coroutine _coroutineC;
    private Coroutine _coroutineD;
    private Coroutine[] _coroutines;
    
    // 코루틴 WaitForSeconds 캐싱 자료사전
    private Dictionary<float, WaitForSeconds> waitForSecondsCache = new Dictionary<float, WaitForSeconds>();

    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds))
        {
            waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        }
        return waitForSecondsCache[seconds];
    }
    
    // ▼ Unity Loop  -----------------------------------------------
    
    void Awake()
    {
        SetCoroutine();
        SubscribeGameManagerEvents();
        GetAndInitializeMat();
    }

    private float _elapsedForBlink;
    public float blinkInterval = 10; 
    
    void Update()
    {
        if (AnimalTrip_GameManager.isRoundStarted)
        {
            _elapsedForInPlayGlowOn += Time.deltaTime;
            
            if (_elapsedForInPlayGlowOn > _shaderAndCommon.waitTimeForTurningOnGlow)
            {
                fresnelElapsedTime += Time.deltaTime;
                //_glowMeshRenderer.enabled = true;
                isGlowOn = true;
                BrightenOutlineWithLerp();
               //    SetIntensity(_shaderAndCommon.colorIntensityRange);
               // ChangeFresnelOfAnimalOutlineColor();
            }

            _elapsedForBlink += Time.deltaTime;

            if (_elapsedForBlink > blinkInterval)
            {
                DOVirtual.Float(0, 1, Random.Range(1,2.5f), val => val++)
                    .OnComplete(() =>
                    {
#if UNITY_EDITOR
                     
#endif

                        BlinkBodyColor();
                        blinkInterval = Random.Range(8, 12);
                        _elapsedForBlink = 0; 
                    });
                
                _elapsedForBlink = 0; 
            }

        

        }
        else
        {
            _blinkTween.Kill();
        }
        

        if (AnimalTrip_GameManager.isGameFinished)
        {
            TurnOffOutlineMesh();
        }
        
        
    }
    
    void OnDestroy()
    {
        UnsubscribeGamaManagerEvents();
    }

    
    // 1. 상태 기준 분류 --------------------------------------------
    
    private void OnGameStart()
    {



    }

    private void OnRoundReady()
    {
        _elapsedForInPlayGlowOn = 0f;
        StopCoroutineWithNullCheck(_coroutines);
        

        
        
    }

    private Tween _blinkTween;
    private void DarkenBodyColor(float duration  = 1.25f)
    {

        
          DOVirtual
            .Color(_animalData.defaultColor, _animalData.darkenedColor, duration, color =>
            {

             
                _bodyMat.SetColor(BODY_COLOR, color);
                _bodyMeshRenderer.material = _bodyMat;
            });
    }

    private void BlinkBodyColor(float duration =0.3f ,float interval=0.3f)
    {
    
       _blinkTween = DOVirtual
            .Color(_animalData.darkenedColor, _animalData.defaultColor, duration, color =>
            {
                _bodyMat.SetColor(BODY_COLOR, color);
                _bodyMeshRenderer.material = _bodyMat;
            })
            .OnComplete(() =>
            {
                _blinkTween = DOVirtual
                    .Color(_animalData.defaultColor, _animalData.darkenedColor, duration, color =>
                    {
                        _bodyMat.SetColor(BODY_COLOR, color);
                        _bodyMeshRenderer.material = _bodyMat;
                    })
                    .SetDelay(interval);
            });
        
    }
    
    

    private void BrightenBodyColor(float duration =1.4f)
    {
#if UNITY_EDITOR
        Debug.Log("쉐이더 코루틴 동작 중...");
#endif

        _coroutines[0] = StartCoroutine(TurnOffOutLineWithLerpCoroutine());
        
        DOVirtual
            .Color(_animalData.darkenedColor, _animalData.defaultColor, duration, color =>
            {
                _bodyMat.SetColor(BODY_COLOR, color);
                _bodyMeshRenderer.material = _bodyMat;
            });

    }
   
    private void OnRoundStarted()
    {
        DarkenBodyColor();
    }

    private Color _currentColor;

    private void OnCorrect()
    {
        BrightenBodyColor();
       

    }
    
    private void OnRoundFinished()
    {
        TurnOffOutlineMesh();
    }
    
    private void OnGameFinished()
    {
        TurnOffOutlineMesh();
    }

    // 2. IEnumerator 및 기타 함수 -------------------------------------------------------------------
    
    private void SetCoroutine()
    {
        _coroutines = new Coroutine[4];
        _coroutines[0] = _coroutineA;
        _coroutines[1] = _coroutineB;
        _coroutines[2] = _coroutineC;
        _coroutines[3] = _coroutineD;
        
    }

    private void StopCoroutineWithNullCheck(Coroutine[] coroutines)
    {
        Debug.Log("코루틴 종료 (AnimalShaderController)");
        foreach (Coroutine cR in coroutines)
        {
            if (cR  != null)
            {
                StopCoroutine(cR);
            }
        }
    }
    
    
    
    private void GetAndInitializeMat()
    {
//         _bodyMeshRenderer = Util.FindComponentInSiblings<SkinnedMeshRenderer>(transform);
//         _bodyMat = _bodyMeshRenderer.sharedMaterial;
//         _bodyMat.EnableKeyword("_Color");
//         _bodyMat.SetColor(COLOR, _animalData.defaultColor);
// #if UNITY_EDITOR
//         Debug.Log($" bodyMat name is.......{_bodyMat}");
// #endif
//
//        
        _glowMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        _glowMeshRenderer.enabled = false;

// Instead of creating a new material, use a MaterialPropertyBlock
        MaterialPropertyBlock block = new MaterialPropertyBlock();

// Load the material from the Resources folder as before
        var tempMat = Resources.Load<Material>("게임별분류/가을소풍/" + _animalData.englishName);
        _bodyMat = tempMat;
        _glowMat = tempMat;
     
    
// Get the SkinnedMeshRenderer
        _bodyMeshRenderer = Utils.FindComponentInSiblings<SkinnedMeshRenderer>(transform);
        
      
            // _glowMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        if (_bodyMeshRenderer != null && _bodyMat != null)
        {
            Debug.Log("dark material assigned");
       
        }


        
    }
    private void ChangeFresnelOfAnimalOutlineColor()
    {
            //fresnel power 4~8 -> 
            var currentFresnel = Mathf.Clamp(6 + 2 * Mathf.Sin(fresnelElapsedTime * _shaderAndCommon.fresnelSpeed - 1),
                _shaderAndCommon.minFresnelPower, _shaderAndCommon.maxFresnelPower);
            SetFresnelPower(currentFresnel);
    }
    
    private void SetFresnelPower(float fresnelPower)
    {
        _glowMat.SetFloat(FRESNEL_COLOR, fresnelPower);
    }
    
    private void SetIntensity(float range)
    {
        float t = (Mathf.Sin(fresnelElapsedTime * _shaderAndCommon.fresnelSpeed) + 1) * 0.5f; // t는 0과 1 사이의 값
        Color currrentColor = Color.Lerp(_animalData.outlineColor/range,_animalData.outlineColor * range , t);
        _glowMat.SetColor(EMISSION_COLOR, currrentColor);
    }
    
    private void BrightenOutlineWithLerp()
    {
        _colorLerp += Time.deltaTime * _shaderAndCommon.outlineTurningOnSpeed;
        Color color =Color.Lerp(Color.black, _animalData.outlineColor, _colorLerp);
        
        _glowMat.SetColor(EMISSION_COLOR,color);
    }

    Color randomColorWhenOnCorrect;

    private Color SetRandomColorWhenCorrectForNonAnswerAnimals()
    {
        //BrightenOutlineWithLerp();
        int randomColorIndex = Random.Range(1, 4);
        switch (randomColorIndex)
        {
            case 1:
                return randomColorWhenOnCorrect =_shaderAndCommon.RANODOM_COLOR_A;
                break;
                        
            case 2:
                return  randomColorWhenOnCorrect = _shaderAndCommon.RANODOM_COLOR_B;
                break;
                    
            case 3:
                return randomColorWhenOnCorrect = _shaderAndCommon.RANODOM_COLOR_C;
                break;
            
            default: 
                return randomColorWhenOnCorrect = _shaderAndCommon.RANODOM_COLOR_C;
                break;
        }
        
       
    }
    IEnumerator TurnOffOutLineWithLerpCoroutine()
    {
        _colorLerp = 0f;
        randomColorWhenOnCorrect = SetRandomColorWhenCorrectForNonAnswerAnimals();
        while (true)
        {
            _colorLerp += Time.deltaTime * _shaderAndCommon.outlineTurningOffSpeed;

            if (AnimalTrip_GameManager.isCorrected && _animalData.englishName == AnimalTrip_GameManager.answer)
            {
                TurnOffOutLineWithLerp(Color.black);
                if (_colorLerp > 0.6f)
                {
                    Debug.Log("정답 맞춰서 메쉬 끄기");
                    TurnOffOutlineMesh();
                }
            }
            
            // 정답아닌 나머지 동물 컬러 결정.
            if (AnimalTrip_GameManager.isCorrected &&  _animalData.englishName != AnimalTrip_GameManager.answer)
            {
              
                TurnOffOutLineWithLerp(_shaderAndCommon.RANODOM_COLOR_A);
                 ChangeFresnelOfAnimalOutlineColor();
            }

            if (AnimalTrip_GameManager.isRoundFinished)
            {
                StopCoroutineWithNullCheck(_coroutines);
            }
            
            yield return null;
        }
    }
    
    private void TurnOffOutLineWithLerp(Color targerColor)
    {
      
        Color color =Color.Lerp(_animalData.outlineColor, targerColor , _colorLerp);
        
        _glowMat.SetColor(EMISSION_COLOR,color);
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

    private void TurnOffOutlineMesh()
    {
        _glowMeshRenderer.enabled = false;
        isGlowOn = false;
    }

    
}
