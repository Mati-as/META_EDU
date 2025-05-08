using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Truck : MonoBehaviour
{
    [SerializeField] private Animator truckAni;

    [SerializeField] private float moveDistance;
    [SerializeField] private float moveDuration;

    [SerializeField] private GameObject soil;

    private AnimationClip LiftUpClip;
    private AnimationClip LiftDownClip;

    private bool isDumping = false;

    public SoilCount soilCountClass;

    private void Start()
    {
        Transform parentTransform = transform.parent;
        foreach (Transform sibling in parentTransform)
        {
            if (sibling != transform) // 자기 자신은 제외
            {
                soilCountClass = sibling.GetComponent<SoilCount>();
                if (soilCountClass != null)
                {
                    break;
                }
            }
        }

        truckAni = GetComponent<Animator>();
        LiftUpClip = Resources.Load<AnimationClip>("Construction/Animation/LiftUpClip_Truck");
        LiftDownClip = Resources.Load<AnimationClip>("Construction/Animation/LiftDownClip_Truck");
        soil.SetActive(false);
    }

    public void StartSoilDumping()
    {
        if (isDumping == false && soilCountClass.soilCount > 0)
        {
            isDumping = true;

            Sequence seq = DOTween.Sequence();

            seq.AppendCallback(() =>
            {
                truckAni.SetBool("Move", true);
                Vector3 targetPos = transform.position + transform.forward * moveDistance;
                transform.DOMove(targetPos, moveDuration).SetEase(Ease.Linear);
            });
            seq.AppendInterval(moveDuration);
            seq.AppendCallback(() => truckAni.SetBool("Move", false));

            seq.AppendInterval(1f);
            //트럭에 흙 쌓는 사운드나 이펙트 나레이션
            seq.AppendCallback(() => soil.SetActive(true));
            seq.AppendCallback(() => soilCountClass.soilCount--);
            seq.AppendCallback(() => soilCountClass.leftSoilCount());
            seq.AppendInterval(1f);

            seq.AppendCallback(() =>
            {
                truckAni.SetBool("Move", true);
                Vector3 targetPos = transform.position - transform.forward * moveDistance;
                transform.DOMove(targetPos, moveDuration).SetEase(Ease.Linear);
            });
            seq.AppendInterval(moveDuration);
            seq.AppendCallback(() => truckAni.SetBool("Move", false));

            seq.AppendInterval(0.5f);

            seq.AppendCallback(() => truckAni.SetBool("LiftUp", true));
            seq.AppendInterval(LiftUpClip.length - 1);
            seq.AppendCallback(() => soil.SetActive(false));
            seq.AppendInterval(1);
            seq.AppendCallback(() => truckAni.SetBool("LiftUp", false));

            seq.AppendInterval(0.5f);

            seq.AppendCallback(() => truckAni.SetBool("LiftDown", true));
            seq.AppendInterval(LiftDownClip.length - 1);
            seq.AppendCallback(() => truckAni.SetBool("LiftDown", false));
            seq.AppendInterval(1);
            seq.AppendCallback(() => isDumping = false);
        }
    }

}
