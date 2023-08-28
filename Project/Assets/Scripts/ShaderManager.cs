using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ShaderManager : MonoBehaviour
{
    [Header("Background Color Settings")] [Space(10f)]
    public Material skyMaterial;

    public Color defaultSkyColor;
    public Color defaultHorizonColor;

    public Color inPlayBgColor;
    public float colorChangingSpeed;

    public float TimingToBeChanged;
    private float _elapsedTimeForBg;
    private float _progress;

    //셰이더 파라미터 컨트롤
    private readonly int _skyColor = Shader.PropertyToID("_SkyColor");
    private readonly int _horizontalColor = Shader.PropertyToID("_HorizonColor");

    [Space(20)] [Header("Animal Color Default Settings")] [Space(10f)]
    public float waitTime;

    [FormerlySerializedAs("_animalMaterial")]
    private int defaultFresnel = 5000;
    public Material animalMaterial;

    public Color startColor;
    public Color inPlayColor;

    [Space(10)] public float fresnelSpeed;
    private float fresnelElapsedTime;

    [FormerlySerializedAs("minFresnelP")] public float minFresnelPower;
    [FormerlySerializedAs("maxFresnelP")] public float maxFresnelPower;

    private readonly int _emissionColor = Shader.PropertyToID("_emissionColor");
    private readonly int _fresnelPower = Shader.PropertyToID("_FresnelPower");

    [Space(20f)] [Header("Color Settings")] [Space(10f)]
    public float minBrightness;

    public float maxBrightness;

    [FormerlySerializedAs("brightnessIncreasingSpeed")]
    public float animalColorChangeSpeed;

    private float _brightness;
    private float _elapsedTime;
    private float _lerpProgress; // 추가된 변수

    private void Awake()
    {
        SetAnimalOutlineColor(inPlayColor);
        SetFresnelPower(defaultFresnel);
        SetSkyColor(defaultSkyColor);
        SetHorizonColor(defaultHorizonColor);
    }


    private void Update()
    {
        _elapsedTimeForBg += Time.deltaTime;
        fresnelElapsedTime += Time.deltaTime;

        if (_elapsedTimeForBg > TimingToBeChanged)
        {
            _progress += Time.deltaTime * colorChangingSpeed;

            var currentColor = Color.Lerp(defaultSkyColor, inPlayBgColor, _progress);
            SetSkyColor(currentColor);

            var currentHorizonColor = Color.Lerp(defaultHorizonColor, inPlayBgColor, _progress);
            SetHorizonColor(currentHorizonColor);
        }

        if (GameManager.IsGameStarted)
        {
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime > waitTime)
            {
                _lerpProgress += Time.deltaTime * animalColorChangeSpeed; // 진행도 업데이트
                _brightness = Mathf.Lerp(minBrightness, maxBrightness, _lerpProgress);

                var currentAnimalOutlineColor = Color.Lerp(startColor, inPlayColor, _lerpProgress);
                SetAnimalOutlineColor(currentAnimalOutlineColor);
                
                
                //fresnel power 4~8 -> 
                var currentFresnel = Mathf.Clamp(6 + 2 * Mathf.Sin(fresnelElapsedTime * fresnelSpeed - 1),
                    minFresnelPower, maxFresnelPower);
                SetFresnelPower(currentFresnel);
                
                
            }

           
        }
    }

    private void OnApplicationQuit()
    {
        SetAnimalOutlineColor(startColor);
        SetSkyColor(defaultSkyColor);
        SetHorizonColor(defaultHorizonColor);
    }


    private void SetSkyColor(Color color)
    {
        skyMaterial.SetColor(_skyColor, color);
    }

    private void SetHorizonColor(Color color)
    {
        skyMaterial.SetColor(_horizontalColor, color);
    }

    private void SetAnimalOutlineColor(Color color)
    {
        animalMaterial.SetColor(_emissionColor, color);
    }

    private void SetFresnelPower(float fresnelPower)
    {
        animalMaterial.SetFloat(_fresnelPower, fresnelPower);
    }
}