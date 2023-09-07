using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimalShaderController : MonoBehaviour
{
    
    public bool isTouchedDown;
    
    [Space(20)] [Header("Animal Color Default and Fresnel Settings")]
    public Color outlineColor;
    
    
    [Space(20)] [Header("Fresnel Control")]
    public float fresnelSpeed;
    private float fresnelElapsedTime;
    public float minFresnelPower;
    public float maxFresnelPower;
    private float _elapsedTime;
    
    public float waitTime;
    private float _elapsed;
    private SkinnedMeshRenderer _meshRenderer;

    [Space(20)] [Header("Game Interaction Control")]

    public float waitTimeForTurningOnGlow;
    private float _elapsedForInPlayGlowOn;
    private readonly int EMISSION_COLOR = Shader.PropertyToID("_emissionColor");
    private readonly int FRESNEL_COLOR = Shader.PropertyToID("_FresnelPower");
    
    private Material _mat;
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
            
            if (_elapsedForInPlayGlowOn > waitTimeForTurningOnGlow)
            {
                _elapsedTime += Time.deltaTime;
                fresnelElapsedTime += Time.deltaTime;
                
               
                _meshRenderer.enabled = true;
                BrightenOutlineWithLerp();
                SetIntensity(ColorIntensityRange);
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
            SetIntensity(ColorIntensityRange);
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
       
        
        if (_elapsedTime > waitTime)
        {
            //fresnel power 4~8 -> 
            var currentFresnel = Mathf.Clamp(6 + 2 * Mathf.Sin(fresnelElapsedTime * fresnelSpeed - 1),
                minFresnelPower, maxFresnelPower);
            SetFresnelPower(currentFresnel);
           
        }
        
    }
    
    private void SetFresnelPower(float fresnelPower)
    {
        _mat.SetFloat(FRESNEL_COLOR, fresnelPower);
    }


    private const float RGB_MAXIMUM = 255f;
    [Range(0,RGB_MAXIMUM)]
    public float ColorIntensityRange;

    

    
  
    private void SetIntensity(float range)
    {
        float t = (Mathf.Sin(fresnelElapsedTime * fresnelSpeed) + 1) * 0.5f; // t는 0과 1 사이의 값
        Color currrentColor = Color.Lerp(outlineColor/ColorIntensityRange, outlineColor *ColorIntensityRange , t);
        _mat.SetColor(EMISSION_COLOR, currrentColor);
    }
    
    public float outlineOnSpeed;
    private float _colorLerp;
    private void BrightenOutlineWithLerp()
    {
        _colorLerp += Time.deltaTime * outlineOnSpeed;
        Color color =Color.Lerp(Color.black, outlineColor, _colorLerp);
        
        _mat.SetColor(EMISSION_COLOR,color);
    }
}
