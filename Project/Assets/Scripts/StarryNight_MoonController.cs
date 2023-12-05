using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class StarryNight_MoonController : MonoBehaviour
{
    public Transform[] pathTargets;
    private Vector3 _defaultPosition;
    private float _defaultSize;
    public float scaleUpSize;
    
    public Material targetMaterial;
    public float colorChangeSpeed = 1.0f;
    private float _currentRotationX;



    
    public Color defaultColor;
    public Color[] targetColors;

    [Header("Chormatic Abbreation")] 
    public Volume volume;
    private ChromaticAberration _chromaticAberration;
    public float chromaticDuration;
    public float chromaticIntensity;
    public float chromaticDefault;
    
    void Start()
    {

        if (volume.profile.TryGet(out _chromaticAberration))
        {
            // 초기 설정
            _chromaticAberration.intensity.value = 0f;
        }
        
        
        DOTween.Init();
        transform.position = pathTargets[0].position;
        
        _defaultSize = transform.localScale.x;
        _defaultPosition = transform.position;
        
        
        if (targetMaterial == null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                targetMaterial = renderer.material;
            }
            else
            {
                Debug.LogError("Material or Renderer not found.");
                enabled = false;
            }
        }

        
        StartPathAnimation();
        DoRotate();
        //DoScaleUp(scaleUpSize);
        DoColorToTarget();
        DoVirtualChromaticIncrease();
    }


    [Space(10f)] [Header("rotation setting")]
    public float duration;
    private float currentRotation = 0f;
    public float rotationAmount;

    private void DoVirtualChromaticIncrease()
    {
        DOVirtual.Float(chromaticDefault, chromaticIntensity, chromaticDuration, value =>
        {
            _chromaticAberration.intensity.value = value;
        }).OnComplete(()=>DoVirtualChromaticDecrease());
    }

    private void DoVirtualChromaticDecrease()
    {
        DOVirtual.Float(chromaticIntensity, chromaticDefault, chromaticDuration, value =>
        {
            _chromaticAberration.intensity.value = value;
        }).OnComplete(()=>DoVirtualChromaticIncrease());;
    }

    private void DoRotate()
    {


            currentRotation = (currentRotation + rotationAmount) % 360; // Keep within 360 degrees
            Quaternion endRotation = Quaternion.Euler(0,0 , currentRotation);

            transform.DORotateQuaternion(endRotation, duration).SetEase(Ease.Linear).SetRelative().OnComplete(DoRotate);
    }
    private void DoRotateBack()
    {
        DOVirtual.Float(0,rotationAmount ,3f, rotationAmount =>
        {
            transform.rotation *= Quaternion.Euler( transform.rotation.x, transform.rotation.y,-rotationAmount);
        }).OnComplete(()=>
        {
            DoRotate();
        }); ;  
    }

  
    

    private float randomDuration;
    private void StartPathAnimation()
    {
        // 랜덤한 duration 값을 설정
        randomDuration = Random.Range(60,65);
        
        Vector3[] path = new Vector3[3];
        path[0] = pathTargets[0].position;
        path[1] = pathTargets[1].position;
        path[2] = pathTargets[2].position;
        // Path 설정 (여기에서는 원형 경로를 설정)
        transform.DOPath(path, randomDuration,PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                transform.position = _defaultPosition;
                DoColorToTarget();
                OnPathAnimationComplete();
            });
    }

 

    private void OnPathAnimationComplete()
    {
        // 애니메이션이 끝나면 다음 애니메이션 시작
        StartPathAnimation();
    }

    

    private static readonly int BaseColorPropertyID = Shader.PropertyToID("_Color");
    
    private void DoColorToTarget()
    {
        
#if UNITY_EDITOR
        Debug.Log("Color to Target..");
#endif
        int currentColorIndex= Random.Range(0, 3);;
        randomDuration = Random.Range(4, 7);
        
        DOVirtual
            .Color(defaultColor, targetColors[currentColorIndex], randomDuration, color =>
                { targetMaterial.SetColor(BaseColorPropertyID, color); })
            .OnComplete(()=>DoColorToDefault(currentColorIndex));
    }
    
    private void DoColorToDefault(int currentColorIndex)
    {
        DOVirtual
            .Color(targetColors[currentColorIndex], defaultColor, randomDuration, color =>
            {
                targetMaterial.SetColor(BaseColorPropertyID, color);
            })
        .OnComplete(()=>DoColorToTarget());
    }

    private void DoScaleUp(float targetSize)
    {
        transform.DOScale(targetSize, Random.Range(3,20))
            .OnComplete(() => DoScaleDown(_defaultSize));
    }

    private void DoScaleDown(float defaultSize)
    {
        transform.DOScale(defaultSize, Random.Range(3,20))
            .OnComplete(() => DoScaleUp(scaleUpSize));
    }
    // Update is called once per frame
 
}
