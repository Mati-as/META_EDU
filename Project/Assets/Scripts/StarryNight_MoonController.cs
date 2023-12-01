using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.PlayerLoop;
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
    public float rotationAmount;


    
    public Color defaultColor;
    public Color[] targetColors;
    void Start()
    {

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
        DoRoatate();
        DoScaleUp(scaleUpSize);
        DoColorToTarget();
    }
    
  

    private void DoRoatate()
    {
        DOVirtual.Float(0,rotationAmount ,3f, rotationAmount =>
        {
            transform.rotation *= Quaternion.Euler(rotationAmount,rotationAmount,rotationAmount);
        }).OnComplete(()=>DoRoatateBack());  
    }
    private void DoRoatateBack()
    {
        DOVirtual.Float(0,rotationAmount ,3f, rotationAmount =>
        {
            transform.rotation *= Quaternion.Euler(-rotationAmount,-rotationAmount,-rotationAmount);
        }).OnComplete(()=>
        {
            DoRoatate();
        }); ;  
    }

  
    

    private float randomDuration;
    private void StartPathAnimation()
    {
        // 랜덤한 duration 값을 설정
        randomDuration = Random.Range(40,42);
        
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

    

    private static readonly int BaseColorPropertyID = Shader.PropertyToID("_BaseColor");
    
    private void DoColorToTarget()
    {
        int currentColorIndex;
        currentColorIndex = Random.Range(0, 3);
        
        DOVirtual
            .Color(defaultColor, targetColors[currentColorIndex], randomDuration/2, color =>
                { targetMaterial.SetColor(BaseColorPropertyID, color); })
            .OnComplete(()=>DoColorToDefault(currentColorIndex));
    }
    
    private void DoColorToDefault(int currentColorIndex)
    {
        DOVirtual
            .Color(targetColors[currentColorIndex], defaultColor, randomDuration/2, color =>
            {
                targetMaterial.SetColor(BaseColorPropertyID, color);
            });
        //.OnComplete(()=>DoColorToTarget());
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
