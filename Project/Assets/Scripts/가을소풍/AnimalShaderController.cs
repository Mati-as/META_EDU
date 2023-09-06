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
    private readonly int _emissionColor = Shader.PropertyToID("_emissionColor");
    private readonly int _fresnelPower = Shader.PropertyToID("_FresnelPower");
    
    private Material _mat;
    void Awake()
    {
        _meshRenderer = GetComponent<SkinnedMeshRenderer>();
        _mat = _meshRenderer.material; // material instance를 가져옵니다.
        _mat.EnableKeyword("_EMISSION");        // emission을 활성화합니다.
        _mat.SetColor(_emissionColor, outlineColor);
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
                _meshRenderer.enabled = true;
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
        _elapsedTime += Time.deltaTime;
        fresnelElapsedTime += Time.deltaTime;
        
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
        _mat.SetFloat(_fresnelPower, fresnelPower);
    }
}
