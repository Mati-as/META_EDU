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

    private void Start()
    {
        manager = FindObjectOfType<Construction_GameManager>();

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
        if (!manager.truckStageEnd)
        {

            if (isDumping == false && soilCountClass.soilCount > 0)
            {
                isDumping = true;
                soilCountClass.soilCount--;

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
                //seq.AppendCallback(() => soilCountClass.leftSoilCount());
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
            if (soilCountClass.soilCount <= 0)     //포크레인 스테이지 종료 타이밍
            {
                //트럭 스테이지로 이동
                //성공 효과음, 이펙트
                manager.truckStageEnd = true;
                //나레이션 재생
                Sequence seq = DOTween.Sequence();

                seq.AppendCallback(() =>
                {
                    Messenger.Default.Publish(new NarrationMessage("우리 친구들이 트럭을 도와줘서 흙을 많이 옮겼어요!", "13_우리_친구들이_트럭을_도와줘서_흙을_많이_옮겼어요_"));
                    Managers.Sound.Play(SoundManager.Sound.Effect, manager.victoryAuidoClip);
                    manager.Btn_Truck.SetActive(false);
                });
                //seq.AppendInterval(5f);
                //seq.AppendCallback(() =>
                //{
                //    Messenger.Default.Publish(new NarrationMessage("다른 일을 하는 자동차를 만나러 가요", "10_다른_일을_하는_자동차를_만나러_가요_"));
                //    manager.rmcVirtualCamera.Priority = 22;
                //    manager.truckVirtualCamera.Priority = 10;
                //});
                //seq.AppendInterval(5f);
                //seq.AppendCallback(() =>
                //{
                //    manager.Btn_Rmc.SetActive(true);

                //});
            }
        }
    }

}
