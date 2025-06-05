using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SuperMaxim.Messaging;
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
    public Construction_GameManager manager;

    private bool btnTwiceIssue = false;

    private bool audioTwiceIssue = false;

    public AudioSource audioSource;

    private void Start()
    {
        manager = FindObjectOfType<Construction_GameManager>();

        audioSource = GetComponent<AudioSource>();
        truckAni = GetComponent<Animator>();
        LiftUpClip = Resources.Load<AnimationClip>("Construction/Animation/LiftUpClip_Truck");
        LiftDownClip = Resources.Load<AnimationClip>("Construction/Animation/LiftDownClip_Truck");
        soil.SetActive(false);
    }

    public void StartSoilDumping()
    {
        float move = moveDistance + soilCountClass.plusMoveDistance;
        audioSource.clip = manager.HeavyMachinerySound;

        if (!btnTwiceIssue)
        {
            btnTwiceIssue = true;
            DOVirtual.DelayedCall(0.1f, () => btnTwiceIssue = false);

            if (isDumping == false && !audioTwiceIssue)
            {
                isDumping = true;
                audioTwiceIssue = true;
                Sequence seq = DOTween.Sequence();

                seq.AppendCallback(() =>
                {
                    audioSource.Play();
                    truckAni.SetBool("Move", true);
                    Vector3 targetPos = transform.position + transform.forward * (move);
                    transform.DOMove(targetPos, moveDuration).SetEase(Ease.Linear);
                });
                seq.AppendInterval(moveDuration);
                seq.AppendCallback(() => truckAni.SetBool("Move", false));

                seq.AppendInterval(1f);

                seq.AppendCallback(() => soil.SetActive(true));
                seq.AppendCallback(() => soilCountClass.SoilDecreaseStep(VehicleType.Truck));

                seq.AppendInterval(1f);

                seq.AppendCallback(() =>
                {
                    truckAni.SetBool("Move", true);
                    Vector3 targetPos = transform.position - transform.forward * (move);
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
                seq.AppendCallback(() => { audioSource.Stop(); truckAni.SetBool("LiftDown", false); });
                seq.AppendInterval(1);
                seq.AppendCallback(() =>
                {
                    isDumping = false; audioTwiceIssue = false;
                });
            }
        }
    }
}
