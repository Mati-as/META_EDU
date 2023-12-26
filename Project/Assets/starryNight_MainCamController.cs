
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class starryNight_MainCamController : MonoBehaviour
{
    private Volume _volume;
    private ColorAdjustments colorAdjustments;
    private UnityEngine.Rendering.Universal.Vignette vignetteLayer;
    private Camera _camera;
    public float defaultExposure;
    public float startDelay;
    public static event Action paintMovingStart;
    
    private void Awake()
    {
        _camera = this.GetComponent<Camera>();
        _volume = GetComponent<Volume>();
        
        StarryNight_IntroCamController.onLightOut -= OnLightOut;
        StarryNight_IntroCamController.onLightOut += OnLightOut;
    }

    private void OnDestroy()
    {
        StarryNight_IntroCamController.onLightOut -= OnLightOut;
    }

    void Start()
    {

        if (_volume.profile.TryGet(out colorAdjustments) && _volume.profile.TryGet(out vignetteLayer))
        {
            // You can set initial values or log a message
            Debug.Log("Post Process Effects found");
        }
        else
        {
            Debug.LogError("Post Process Effects not found");
        }

        defaultExposure = colorAdjustments.postExposure.value;
       
        SetPostExposure(0);
        SetVignetteIntensity(1);
        _volume.enabled = false;
        
    }
    
    private void SetPostExposure(float exposureValue)
    {
        colorAdjustments.postExposure.value = exposureValue;
    }

    private void SetVignetteIntensity(float intensityValue)
    {
        vignetteLayer.intensity.value = intensityValue;
    }

    private void OnLightOut()
    {
        FadeIn();
    }

    private void FadeIn()
    {
        _volume.enabled = true;
        
        DOVirtual.Float(0, defaultExposure, 2, value =>
        {
            SetPostExposure(value);
        }).SetDelay(startDelay);
        
        DOVirtual.Float(1, 0, 2, value =>
        {
            SetVignetteIntensity(value);
        }).SetDelay(startDelay)
            .OnComplete(() =>
            {
                paintMovingStart?.Invoke();
            });
    }
}
