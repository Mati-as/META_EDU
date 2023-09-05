using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowController : MonoBehaviour
{
    
    [Space(20)] [Header("Animal Color Default and Fresnel Settings")]
    public Color outlineColor;
    [Space(10)]
    public float fresnelSpeed;
    private float fresnelElapsedTime;
    public float minFresnelPower;
    public float maxFresnelPower;
    private float _elapsedTime;
    
    public float waitTime;
    private float _elapsed;
    private SkinnedMeshRenderer _meshRenderer;
    
    
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


    void Update()
    {
        if (GameManager.isGameStarted)
        {
            _elapsed += Time.deltaTime;
            if (_elapsed > waitTime)
            {
                _meshRenderer.enabled = true;
                ChangeAnimalOutlineColor();
            }
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
