using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StarryNight_IntroCamController : MonoBehaviour
{
    public Transform[] pathTransforms;
    private Vector3[] _path;
    public Transform lookAtTarget;
    
    public static event Action onGameIsReady;
    public static event Action onLightOut; //when the screen is completely dark.

    
    private Volume _volume;
    private ColorAdjustments colorAdjustments;
    private Vignette vignetteLayer;
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _volume = GetComponent<Volume>();

        onLightOut -= OnLightOut;
        onLightOut += OnLightOut;

        UI_Scene_Button.onBtnShut -= StartAnimation;
        UI_Scene_Button.onBtnShut += StartAnimation;
        
        transform.DOLookAt(lookAtTarget.position, 0.01f);
    }

    private void OnDestroy()
    {
        UI_Scene_Button.onBtnShut -= StartAnimation;
        onLightOut -= OnLightOut;
    }

    private void StartAnimation()
    {
        _path = new Vector3[pathTransforms.Length];

        for (var i = 0; i < pathTransforms.Length; i++) _path[i] = pathTransforms[i].position;


        transform.DOLookAt(lookAtTarget.position, 0.01f);

        transform.DOPath(_path, 6, PathType.CatmullRom, resolution: 5)
            .SetEase(Ease.OutQuint)
            .SetLookAt(lookAtTarget, true)
            .OnComplete(() =>
            {
                var _defaultExposure = colorAdjustments.postExposure.value;

                DOVirtual.Float(_defaultExposure, -2, 2.5f, value => { SetPostExposure(value); }).OnComplete(() => { });

                DOVirtual.Float(0, 1, 2.3f, value =>
                {
                    var newPath = new Vector3[2];
                    newPath[0] = transform.position;
                    newPath[1] = transform.position - Vector3.forward * 20;

                    transform.DOPath(newPath, 1.0f);

                    SetVignetteIntensity(value);
                }).OnComplete(() =>
                {
                    onLightOut?.Invoke();
                    onGameIsReady?.Invoke();
                });
            }).SetDelay(1.1f);
    }

    private void Start()
    {
        if (_volume.profile.TryGet(out colorAdjustments) && _volume.profile.TryGet(out vignetteLayer))
            // You can set initial values or log a message
            Debug.Log("Post Process Effects found");
        else
            Debug.LogError("Post Process Effects not found");

        //move to start position
        transform.position = pathTransforms[0].position;
    }

    public void SetPostExposure(float exposureValue)
    {
        colorAdjustments.postExposure.value = exposureValue;
    }

    public void SetVignetteIntensity(float intensityValue)
    {
        vignetteLayer.intensity.value = intensityValue;
    }


    public void OnLightOut()
    {
        _camera.enabled = false;
    }
}