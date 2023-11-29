using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class PaintingContentRefractionController : MonoBehaviour
{
    public Material material;
    public float refractionSpeed;
    public float amplitude;
    public float minRefraction;
    public float rotationDurationForEachLoop;
    private float _rotationAmount;
    private void Update()
    {
        // Sin 함수를 사용하여 "_refractionPower" 값을 왔다갔다 변경합니다.
        float refractionValue = minRefraction + Mathf.Sin(Time.time * refractionSpeed) * amplitude + amplitude;

#if UNITY_EDITOR
        Debug.Log($"refractionValue: {refractionValue}");
#endif
        // 머티리얼의 "_refractionPower" 프로퍼티를 변경합니다.
        material.SetFloat("_refractionPower", refractionValue);
        
        
        
      
     
    }

    private void Start()
    {
        DOTween.To(() => transform.eulerAngles, x => transform.eulerAngles = x,
                new Vector3(360f, transform.eulerAngles.y, transform.eulerAngles.z), rotationDurationForEachLoop)
            .SetEase(Ease.Linear)// 일정한 속도로 회전
            .SetLoops(-1, LoopType.Incremental); 


        //     .SetLoops(-1, LoopType.Incremental); // 무한히 증가
        // transform.DORotateQuaternion(new Quaternion(360f,0f, 0f), rotationDurationForEachLoop, RotateMode.FastBeyond360)
        //     .SetEase(Ease.Linear) 
        //     .SetLoops(-1, LoopType.Restart);
    }

    public Transform uppperTarget;
    public Transform lowerTarget; 

    // private void MoveDown()
    // {
    //     transform.DOMove(lowerTarget.position, Random.Range(0,0.5f)).OnComplete(MoveUp);
    // }
    //
    // private void MoveUp()
    // {
    //     transform.DOMove(uppperTarget.position, Random.Range(0,0.5f)).OnComplete(MoveDown);
    // }
}
