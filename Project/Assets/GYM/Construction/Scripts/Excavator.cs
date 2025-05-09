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
    private AnimationClip Dumpclip;

    private bool isDigging = false;

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

        excavatorAni = GetComponent<Animator>();
        Digclip = Resources.Load<AnimationClip>("Construction/Animation/digClip_Excavator");
        Dumpclip = Resources.Load<AnimationClip>("Construction/Animation/dumpClip_Excavator");
        soil.SetActive(false);
    }

    public void StartExcavation()
    {
        if (!manager.excavatorStageEnd)
        {

            if (isDigging == false)
                //&& soilCountClass.soilCount > 0)
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
                seq.AppendInterval(1);
                seq.AppendCallback(() => excavatorAni.SetBool("Dig", false));
                seq.AppendCallback(() => soilCountClass.SoilDecreaseStep());

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
            if (soilCountClass.inputCount >= soilCountClass.maxInputs)     //포크레인 스테이지 종료 타이밍
            {
                //성공 효과음, 이펙트
                manager.excavatorStageEnd = true;
                //나레이션 재생
                Sequence seq = DOTween.Sequence();

                seq.AppendCallback(() =>
                {
                    Messenger.Default.Publish(new NarrationMessage("우리 친구들이 포크레인을 도와줘서 흙을 많이 팠어요!", "9_우리_친구들이_포크레인을_도와줘서_흙을_많이_팠어요_"));
                    Managers.Sound.Play(SoundManager.Sound.Effect, manager.victoryAuidoClip);
                    manager.Btn_Excavator.SetActive(false);
                });
                seq.AppendInterval(5f);
                seq.AppendCallback(() =>
                {
                    Messenger.Default.Publish(new NarrationMessage("다른 일을 하는 자동차를 만나러 가요", "10_다른_일을_하는_자동차를_만나러_가요_"));
                    manager.truckVirtualCamera.Priority = 21;
                    manager.excavatorVirtualCamera.Priority = 10;
                });
                seq.AppendInterval(5f);
                seq.AppendCallback(() =>
                {
                    manager.Btn_Truck.SetActive(true);

                });

            }
        }
    }

}
