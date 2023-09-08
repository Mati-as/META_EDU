using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using 가을소풍;

public class AnimalShaderController : MonoBehaviour
{
    
    private bool _isTouchedDown;
    
    [Space(20)] [Header("Animal Color Default and Fresnel Settings")]
    public Color outlineColor;
    
    
    [Space(20)] [Header("Fresnel Control")]
    private float fresnelElapsedTime;
    private float _elapsed;
    private SkinnedMeshRenderer _meshRenderer;

    [Space(20)] [Header("Game Interaction Control")]
    private float _elapsedForInPlayGlowOn;
    private readonly int EMISSION_COLOR = Shader.PropertyToID("_emissionColor");
    private readonly int FRESNEL_COLOR = Shader.PropertyToID("_FresnelPower");
    
    private Material _mat;

    [SerializeField] 
    private AnimalShaderScriptableObject _animalShaderScriptableObject;
    
    
    void Awake()
    {
        _meshRenderer = GetComponent<SkinnedMeshRenderer>();
        _mat = _meshRenderer.material; // material instance를 가져옵니다.
        _mat.EnableKeyword("_EMISSION");        // emission을 활성화합니다.
        _mat.SetColor(EMISSION_COLOR, outlineColor);
        _meshRenderer.enabled = false;
    }


    private readonly string TAG_ARRIVAL= "arrival";
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TAG_ARRIVAL))
        {
            _isTouchedDown = true;
            Debug.Log("Touched Down!");
        }
    }

    void Update()
    {
        
        if (GameManager.isRoundReady)
        {
            _elapsedForInPlayGlowOn = 0f;
            _isTouchedDown = false;

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
        Color currrentColor = Color.Lerp(outlineColor/range,outlineColor * range , t);
        _mat.SetColor(EMISSION_COLOR, currrentColor);
    }
    
    
    private float _colorLerp;
    private void BrightenOutlineWithLerp()
    {
        _colorLerp += Time.deltaTime * _animalShaderScriptableObject.outlineTurningOnSpeed;
        Color color =Color.Lerp(Color.black, outlineColor, _colorLerp);
        
        _mat.SetColor(EMISSION_COLOR,color);
    }
}
