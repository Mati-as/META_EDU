using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Playground_PropRotater : MonoBehaviour
{
    // Start is called before the first frame update
    private float rotateDuration = 3f;
    void Start()
    {
        TurnOnPropellers();
    }

    private void TurnOnPropellers()
    {
        
          transform.gameObject.SetActive(true);
         
                    // Do not use DoQuaternion 
                    float rotationAmountDegrees
                        = Random.Range(0, 2) % 2 == 1 ? 150  : -150;
                    Vector3 targetEulerRotation
                        = transform.eulerAngles + new Vector3(0, rotationAmountDegrees, 0);

                    transform
                        .DORotate(targetEulerRotation, rotateDuration, RotateMode.FastBeyond360)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1,LoopType.Yoyo);



    }
}
