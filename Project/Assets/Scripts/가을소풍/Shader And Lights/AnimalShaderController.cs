using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


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
    private SkinnedMeshRenderer _meshRenderer;
    private readonly int EMISSION_COLOR = Shader.PropertyToID("_emissionColor");
    private readonly int FRESNEL_COLOR = Shader.PropertyToID("_FresnelPower");
    private Material _mat;
    
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
        _meshRenderer.enabled = false;
    }
    
    void Update()
    {
        if (GameManager.isRoundStarted)
        {
            _elapsedForInPlayGlowOn += Time.deltaTime;
            
            if (_elapsedForInPlayGlowOn > _shaderAndCommon.waitTimeForTurningOnGlow)
            {
                fresnelElapsedTime += Time.deltaTime;
                _meshRenderer.enabled = true;
                isGlowOn = true;
                BrightenOutlineWithLerp();
                SetIntensity(_shaderAndCommon.colorIntensityRange);
                ChangeFresnelOfAnimalOutlineColor();
            }
        }
        
        if (GameManager.isCorrected && gameObject.name != GameManager.answer)
        {
           
                _elapsedForInPlayGlowOn += Time.deltaTime;
                BrightenOutlineWithLerp();
                ChangeFresnelOfAnimalOutlineColor();
            
            //SetIntensity(_shaderAndCommon.colorIntensityRange);
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
    }
   
    private void OnRoundStarted()
    {
    }

    private void OnCorrect()
    {
        Debug.Log("쉐이더 코루틴 동작 중...");
        StopCoroutineWithNullCheck(_coroutines);
        _coroutines[0] = StartCoroutine(TurnOffOutLineWithLerpCoroutine());
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
        _meshRenderer = GetComponent<SkinnedMeshRenderer>();
        _mat = _meshRenderer.material; // material instance를 가져옵니다.
        _mat.EnableKeyword("_EMISSION");        // emission을 활성화합니다.
        _mat.SetColor(EMISSION_COLOR, _animalData.outlineColor);
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
        _mat.SetFloat(FRESNEL_COLOR, fresnelPower);
    }
    
    private void SetIntensity(float range)
    {
        float t = (Mathf.Sin(fresnelElapsedTime * _shaderAndCommon.fresnelSpeed) + 1) * 0.5f; // t는 0과 1 사이의 값
        Color currrentColor = Color.Lerp(_animalData.outlineColor/range,_animalData.outlineColor * range , t);
        _mat.SetColor(EMISSION_COLOR, currrentColor);
    }
    
    private void BrightenOutlineWithLerp()
    {
        _colorLerp += Time.deltaTime * _shaderAndCommon.outlineTurningOnSpeed;
        Color color =Color.Lerp(Color.black, _animalData.outlineColor, _colorLerp);
        
        _mat.SetColor(EMISSION_COLOR,color);
    }
    
    IEnumerator TurnOffOutLineWithLerpCoroutine()
    {
        _colorLerp = 0f;
        
        while (GameManager.isCorrected || GameManager.isRoundFinished)
        {
            TurnOffOutLineWithLerp();
            yield return null;
        }
    }
    
    private void TurnOffOutLineWithLerp()
    {
        _colorLerp += Time.deltaTime * _shaderAndCommon.outlineTurningOffSpeed;
        Color color =Color.Lerp(_animalData.outlineColor, Color.black , _colorLerp);
        
        _mat.SetColor(EMISSION_COLOR,color);
    }
    
    private void SubscribeGameManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onGameStartEvent += OnGameStart;
        
        GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onRoundReadyEvent += OnRoundReady;

        GameManager.onCorrectedEvent -= OnCorrect;
        GameManager.onCorrectedEvent += OnCorrect;

        GameManager.onRoundFinishedEvent -= OnRoundFinished;
        GameManager.onRoundFinishedEvent += OnRoundFinished;

        GameManager.onRoundStartedEvent -= OnRoundStarted;
        GameManager.onRoundStartedEvent += OnRoundStarted;
        
        GameManager.onGameFinishedEvent -= OnGameFinished;
        GameManager.onGameFinishedEvent += OnGameFinished;
    }
    
    private void UnsubscribeGamaManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onCorrectedEvent -= OnCorrect;
        GameManager.onRoundFinishedEvent -= OnRoundFinished;
        GameManager.onRoundStartedEvent -= OnRoundStarted;
        GameManager.onGameFinishedEvent -= OnGameFinished;
    }

    private void TurnOffOutlineMesh()
    {
        _meshRenderer.enabled = false;
        isGlowOn = false;
    }

    
}
