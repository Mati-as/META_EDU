using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;

public class Excavator : MonoBehaviour
{
    [SerializeField] private Animator excavatorAni;

    [SerializeField] private float moveDistance;
    [SerializeField] private float moveDuration;

    [SerializeField] private GameObject soil;

    private AnimationClip Digclip;

    private bool isDigging = false;

    public SoilCount soilCountClass;

    public Construction_GameManager manager;

    private bool btnTwiceIssue = false;
    private void Start()
    {
        manager = FindObjectOfType<Construction_GameManager>();

        excavatorAni = GetComponent<Animator>();
        Digclip = Resources.Load<AnimationClip>("Construction/Animation/digClip_Excavator");
        soil.SetActive(false);
    }

    public void StartExcavation()
    {
        if (!btnTwiceIssue)
        {
            btnTwiceIssue = true;
            DOVirtual.DelayedCall(0.1f, () => btnTwiceIssue = false);

            if (isDigging == false)
            {
                isDigging = true;

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
                seq.AppendInterval(0.5f);
                seq.AppendCallback(() => excavatorAni.SetBool("Dig", false));
                seq.AppendCallback(() => soilCountClass.SoilDecreaseStep(VehicleType.Excavator));

                seq.AppendInterval(1f);

                seq.AppendCallback(() =>
                {
                    excavatorAni.SetBool("Move", true);
                    Vector3 targetPos = transform.position - transform.forward * moveDistance;
                    transform.DOMove(targetPos, moveDuration).SetEase(Ease.Linear);
                });
                seq.AppendInterval(moveDuration);
                seq.AppendCallback(() => excavatorAni.SetBool("Move", false));

                seq.AppendInterval(1.5f);
                seq.AppendCallback(() => soil.SetActive(false));
                seq.AppendCallback(() => isDigging = false);
            }

        }
    }
}