using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using 가을소풍;

public class AnimalShaderController : MonoBehaviour
{
    
    //▼ Scriptable Objects 참조 부분, 스태틱 및 초기화 설정
    [Header("Reference (Scriptable Object)")] 
    [Space(15f)]
    [SerializeField] 
    private AnimalShaderScriptableObject _animalShaderScriptableObject;
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
        
    
    //▼ 기타 
    private bool isTouchedDown;
    public bool IsTouchedDown
    {
        get { return isTouchedDown;}
        set { isTouchedDown = value; }
    }
    
 

    
    void Awake()
    {
        IsTouchedDown = false;
        
        _meshRenderer = GetComponent<SkinnedMeshRenderer>();
      
        _mat = _meshRenderer.material; // material instance를 가져옵니다.
        _mat.EnableKeyword("_EMISSION");        // emission을 활성화합니다.
        _mat.SetColor(EMISSION_COLOR, _animalData.outlineColor);
        _meshRenderer.enabled = false;
    }


    private readonly string TAG_ARRIVAL= "arrival";
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TAG_ARRIVAL))
        {
            isTouchedDown = true;
            Debug.Log("Touched Down!");
        }
    }

    void Update()
    {
        if (GameManager.isRoundReady)
        {
            _elapsedForInPlayGlowOn = 0f;
            isTouchedDown = false;

        }

        if (GameManager.isRoundStarted)
        {
            _elapsedForInPlayGlowOn += Time.deltaTime;
            
            if (_elapsedForInPlayGlowOn > _animalShaderScriptableObject.waitTimeForTurningOnGlow)
            {
      
                fresnelElapsedTime += Time.deltaTime;
                
                _meshRenderer.enabled = true;
                BrightenOutlineWithLerp();
                SetIntensity(_animalShaderScriptableObject.colorIntensityRange);
                ChangeAnimalOutlineColor();
            }
        }

        if (GameManager.isRoundReady)
        {
         
        }
        
        if (GameManager.isCorrected)
        {
            _elapsedForInPlayGlowOn += Time.deltaTime;
            ChangeAnimalOutlineColor();
            SetIntensity(_animalShaderScriptableObject.colorIntensityRange);
        }
      
        if (GameManager.isRoundFinished)
        {
            _meshRenderer.enabled = false;
        }
        
        if (GameManager.isGameFinished)
        {
            _meshRenderer.enabled = false;
        }

    }
    
    // ▼ 메소드 목록 ----------------------------
    
    private void ChangeAnimalOutlineColor()
    {
            //fresnel power 4~8 -> 
            var currentFresnel = Mathf.Clamp(6 + 2 * Mathf.Sin(fresnelElapsedTime * _animalShaderScriptableObject.fresnelSpeed - 1),
                _animalShaderScriptableObject.minFresnelPower, _animalShaderScriptableObject.maxFresnelPower);
            SetFresnelPower(currentFresnel);
    }
    
    private void SetFresnelPower(float fresnelPower)
    {
        _mat.SetFloat(FRESNEL_COLOR, fresnelPower);
    }

    
    private void SetIntensity(float range)
    {
        float t = (Mathf.Sin(fresnelElapsedTime * _animalShaderScriptableObject.fresnelSpeed) + 1) * 0.5f; // t는 0과 1 사이의 값
        Color currrentColor = Color.Lerp(_animalData.outlineColor/range,_animalData.outlineColor * range , t);
        _mat.SetColor(EMISSION_COLOR, currrentColor);
    }
    
    

    private void BrightenOutlineWithLerp()
    {
        _colorLerp += Time.deltaTime * _animalShaderScriptableObject.outlineTurningOnSpeed;
        Color color =Color.Lerp(Color.black, _animalData.outlineColor, _colorLerp);
        
        _mat.SetColor(EMISSION_COLOR,color);
    }
}
