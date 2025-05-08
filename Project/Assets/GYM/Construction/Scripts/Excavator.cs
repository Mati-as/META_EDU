using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Excavator : MonoBehaviour
{
    [SerializeField] private Animator excavatorAni;

    [SerializeField] private float moveDistance;
    [SerializeField] private float moveDuration;

    [SerializeField] private GameObject soil;

    private AnimationClip Digclip;
    private AnimationClip Dumpclip;

    private bool isDigging = false;

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

        excavatorAni = GetComponent<Animator>();
        Digclip = Resources.Load<AnimationClip>("Construction/Animation/digClip_Excavator");
        Dumpclip = Resources.Load<AnimationClip>("Construction/Animation/dumpClip_Excavator");
        soil.SetActive(false);
    }

    public void StartExcavation()
    {
        if (isDigging == false && soilCountClass.soilCount > 0)
        {
            isDigging = true;
            //if (soilCountClass.soilCount > 0)
            //{
                soilCountClass.soilCount--;
            //}
            //else
            //{
            //    soilCountClass.soilCount = 0;
            //}

            Sequence seq = DOTween.Sequence();

            seq.AppendCallback(() =>
            {
                excavatorAni.SetBool("Move", true);
                Vector3 targetPos = transform.position + transform.forward * moveDistance;
                transform.DOMove(targetPos, moveDuration).SetEase(Ease.Linear);
            });
            seq.AppendInterval(moveDuration);
            seq.AppendCallback(() => excavatorAni.SetBool("Move", false));

            seq.AppendCallback(() => excavatorAni.SetBool("Dig", true));
            seq.AppendInterval(Digclip.length - 1);
            seq.AppendCallback(() => soil.SetActive(true));
            seq.AppendInterval(1);
            seq.AppendCallback(() => excavatorAni.SetBool("Dig", false));
            seq.AppendCallback(() => soilCountClass.leftSoilCount());

            seq.AppendInterval(0.5f);

            seq.AppendCallback(() =>
            {
                excavatorAni.SetBool("Move", true);
                Vector3 targetPos = transform.position - transform.forward * moveDistance;
                transform.DOMove(targetPos, moveDuration).SetEase(Ease.Linear);
            });
            seq.AppendInterval(moveDuration);
            seq.AppendCallback(() => excavatorAni.SetBool("Move", false));

            seq.AppendInterval(0.5f);

            seq.AppendCallback(() => excavatorAni.SetBool("Dump", true));
            seq.AppendInterval(1f);
            seq.AppendCallback(() => soil.SetActive(false));
            seq.AppendInterval(Dumpclip.length - 1);
            seq.AppendCallback(() => excavatorAni.SetBool("Dump", false));
            seq.AppendCallback(() => isDigging = false);
        }
    }

}
