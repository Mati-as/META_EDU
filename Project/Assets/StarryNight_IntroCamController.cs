using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



public class StarryNight_IntroCamController : MonoBehaviour
{
    public Transform[] pathTransforms;
    private Vector3[] _path;
    public Transform lookAtTarget;
    public static event Action onGameIsReady;
    public static event Action onLightOut;//when the screen is completely dark.
    private Volume _volume;
    private ColorAdjustments colorAdjustments;
    private UnityEngine.Rendering.Universal.Vignette vignetteLayer;
    private Camera _camera;
    

    private void Awake()
    {
        _camera = this.GetComponent<Camera>();
        _volume = GetComponent<Volume>();


        onLightOut -= OnLightOut;
        onLightOut += OnLightOut;
    }

    private void OnDestroy()
    {
        onLightOut -= OnLightOut;
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
        
        
        //move to start position
        transform.position = pathTransforms[0].position;
        
        _path = new Vector3[pathTransforms.Length];
        
        for (int i = 0; i < pathTransforms.Length; i++)
        {
            _path[i] = pathTransforms[i].position;
        }
        
        

        transform.DOPath(_path, 10, PathType.CatmullRom, resolution: 5)
            .SetEase(Ease.OutQuint)
            .SetLookAt(lookAtTarget,stableZRotation:true)
            .OnPlay(() =>
            {
                // Get the current Euler angles
                // Vector3 currentEulerAngles = transform.rotation.eulerAngles;
                // Vector3 newEulerAngles = new Vector3(currentEulerAngles.x, -180, currentEulerAngles.z);
                // transform.rotation = Quaternion.Euler(newEulerAngles);
            })
            .OnComplete(() =>
            {
                float _defaultExposure =colorAdjustments.postExposure.value;
                
                DOVirtual.Float(_defaultExposure, -2, 2.5f, value =>
                {
                    SetPostExposure(value);
                }).OnComplete(() =>
                {
                    
                });
                
                DOVirtual.Float(0, 1, 2.3f, value =>
                {
                    SetVignetteIntensity(value);
                }).OnComplete(() =>
                {
                    onLightOut?.Invoke();
                    onGameIsReady?.Invoke();
                });
              
             
            });
        

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

  
