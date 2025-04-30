using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class WheelSpinner : MonoBehaviour
{


    void Start()
    {

    }
    void Anim_Active(GameObject obj)
    {
        //이전에 만들었던 것처럼 팝업 다 끝나면 더 이상 클릭 되지 않도록 필요, 과일 참고 필요


        //Sequence seq = DOTween.Sequence();
        //seq.Append(obj.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic).OnStart(() => obj.SetActive(true))).SetDelay(2f);
        obj.transform.DOScale(1, 1f).SetEase(Ease.OutElastic).OnComplete(() => obj.SetActive(true));
    }

    void Anim_Inactive(GameObject obj)
    {
        obj.transform.DOScale(0, 0.5f).SetEase(Ease.OutElastic).OnComplete(() => obj.SetActive(false));
    }
}
