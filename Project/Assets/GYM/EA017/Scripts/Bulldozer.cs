using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;

public class Bulldozer : MonoBehaviour
{
    [SerializeField] private Construction_GameManager manager;
    [SerializeField] private SoilCount soilCountClass;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator bulldozerAnimator;

    [SerializeField] private float moveDistance;
    [SerializeField] private float moveDuration;

    private bool btnTwiceIssue = false;         //버튼 중첩 방지용 
    private bool audioTwiceIssue = false;       
    private bool isWork = false;

    [SerializeField] private AnimationClip workClip;

    [SerializeField] private float workClipLength;

    private void Start()
    {
        manager = FindAnyObjectByType<Construction_GameManager>();
        soilCountClass = FindAnyObjectByType<SoilCount>();

        audioSource = GetComponent<AudioSource>();
        bulldozerAnimator = GetComponent<Animator>();
        workClip = Resources.Load<AnimationClip>("Construction/Animation/workClip_Bulldozer");

        workClipLength = workClip.length;

    }

    public void StartBulldozerWork()
    {
        manager.ClickSound();

        float move = moveDistance + soilCountClass.plusMoveDistance;

        if (!btnTwiceIssue)
        {
            btnTwiceIssue = true;
            DOVirtual.DelayedCall(0.1f, () => btnTwiceIssue = false);
            audioSource.clip = manager.HeavyMachinerySound;

            if (!isWork && !audioTwiceIssue)
            {
                isWork = true;
                audioTwiceIssue = true;

                Sequence seq = DOTween.Sequence();

                seq.AppendCallback(() =>
                {
                    audioSource.Play();
                    bulldozerAnimator.SetBool("Move", true);
                    Vector3 targetPos = transform.position + transform.forward * move;
                    transform.DOMove(targetPos, moveDuration).SetEase(Ease.OutQuad);
                });
                seq.AppendInterval(moveDuration);
                seq.AppendCallback(() => bulldozerAnimator.SetBool("Move", false));

                seq.AppendCallback(() => bulldozerAnimator.SetBool("Work", true));
                seq.AppendInterval(workClipLength - 1);
                seq.AppendInterval(0.5f);
                seq.AppendCallback(() => bulldozerAnimator.SetBool("Work", false));
                seq.AppendCallback(() => soilCountClass.SoilDecreaseStep(VehicleType.Bulldozer));

                seq.AppendInterval(0.5f);

                seq.AppendCallback(() =>
                {
                    bulldozerAnimator.SetBool("Move", true);
                    Vector3 targetPos = transform.position - transform.forward * move;
                    transform.DOMove(targetPos, moveDuration).SetEase(Ease.OutQuad);
                });
                seq.AppendInterval(moveDuration);
                seq.AppendCallback(() => { bulldozerAnimator.SetBool("Move", false); audioSource.Stop(); });

                seq.AppendInterval(1f);
                seq.AppendCallback(() => { isWork = false; audioTwiceIssue = false; });

            }

        }
    }
}