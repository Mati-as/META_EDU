using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;


public class StarryNight_MoonController : MonoBehaviour
{
    public Transform target;
    private Vector3 _defaultPosition;
    private float _defaultSize;
    public float scaleUpSize;
    
    public Material targetMaterial;
    public float colorChangeSpeed = 1.0f;
   

    
    public Color defaultColor;
    public Color[] targetColors;
    void Start()
    {
        _defaultPosition = transform.position;
        _defaultSize = transform.localScale.x;

      
      
        
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
        
        DoScaleUp();

    }

    private float randomDuration;
    private void StartPathAnimation()
    {
        // 랜덤한 duration 값을 설정
        randomDuration = Random.Range(20,22);
        
        Vector3[] path = new Vector3[2];
        path[0] = _defaultPosition;
        path[1] = target.position;
        // Path 설정 (여기에서는 원형 경로를 설정)
        transform.DOPath(path, randomDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
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

    private void DoScaleUp(float targetSize = 0.8f)
    {
        transform.DOScale(targetSize, Random.Range(3,20))
            .OnComplete(() => DoScaleDown(_defaultSize));
    }

    private void DoScaleDown(float defaultSize = 0.5f)
    {
        transform.DOScale(defaultSize, Random.Range(3,20))
            .OnComplete(() => DoScaleUp(scaleUpSize));
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
