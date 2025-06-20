using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;
using MyGame.Messages;

public enum VehicleType
{
    Excavator,
    Truck,
    Bulldozer
}

public class SoilCount : MonoBehaviour
{
    Construction_GameManager manager;
    public int soilCount;

    public int inputCount = 0;
    public int maxInputs = 21;

    private Vector3 startScale = new Vector3(8.37f, 18f, 7.8f);
    private Vector3 endScale = new Vector3(-0.1f, -0.1f, -0.1f);

    public float plusMoveDistance = 0;

    [SerializeField] private int decreaseindex = 0;
    [SerializeField] private List<AudioClip> decreasingSound = new List<AudioClip>(3);

    [SerializeField] private bool twiceissue = false;

    private void Start()
    {
        manager = FindObjectOfType<Construction_GameManager>();
        decreasingSound.Add(Resources.Load<AudioClip>("Construction/Audio/Game Decrease 1"));
        decreasingSound.Add(Resources.Load<AudioClip>("Construction/Audio/Game Decrease 2"));
        decreasingSound.Add(Resources.Load<AudioClip>("Construction/Audio/Game Decrease 3"));

    }

    public void SoilDecreaseStep(VehicleType selected)
    {
        inputCount++;

        if (inputCount == 11)
        {
            Managers.Sound.Play(SoundManager.Sound.Narration, "Construction/Audio/audio_4_흙이_줄어들고_있어요__더_많이_터치해주세요");
        }

        if (inputCount >= maxInputs && !twiceissue)    //스테이지 종료 호출 타이밍
        {
            twiceissue = true;
            switch (selected)
            {
                case VehicleType.Excavator:
                    EndExcavatorStage();
                    decreaseindex = 0;
                    break;
                case VehicleType.Truck:
                    EndTruckStage();
                    decreaseindex = 0;
                    break;
                case VehicleType.Bulldozer:
                    EndBulldozerStage();
                    decreaseindex = 0;
                    break;
            }
        }
        
        int stepSize = 7; // 7번 모이면 1단계 스케일 감소
        if (inputCount % stepSize != 0) // 7로나눠서 딱떨어지지않으면 줄어들지 않게끔
            return;

        int totalSteps = maxInputs / stepSize;
        int stepIndex = inputCount / stepSize;
        Vector3 step = (startScale - endScale) / totalSteps;
        Vector3 newScale = startScale - step * stepIndex;

        switch (selected)
        {
            case VehicleType.Excavator:
                manager.ExcavatorStageSoil.transform.DOScale(newScale, 0.3f).SetEase(Ease.OutQuad);
                Managers.Sound.Play(SoundManager.Sound.Effect, decreasingSound[decreaseindex]);
                manager.ExcavatorStageSoil.transform.DOScale(manager.ExcavatorStageSoil.transform.localScale * 1.1f, 0.1f)
                .SetEase(Ease.OutQuad)
                .SetLoops(4, LoopType.Yoyo) // 커졌다 작아졌다 반복
                .OnComplete(() =>
                {
                    manager.ExcavatorStageSoil.transform.DOScale(newScale, 0.15f).SetEase(Ease.OutBack);
                    decreaseindex++;
                });
                break;
            case VehicleType.Truck:
                plusMoveDistance += 3f;
                manager.TruckStageSoil.transform.DOScale(newScale, 0.3f).SetEase(Ease.OutQuad);
                Managers.Sound.Play(SoundManager.Sound.Effect, decreasingSound[decreaseindex]);
                manager.TruckStageSoil.transform.DOScale(manager.TruckStageSoil.transform.localScale * 1.1f, 0.1f)
                .SetEase(Ease.OutQuad)
                .SetLoops(4, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    manager.TruckStageSoil.transform.DOScale(newScale, 0.15f).SetEase(Ease.OutBack);
                    decreaseindex++;
                });
                break;
            case VehicleType.Bulldozer:
                plusMoveDistance += 3f;
                manager.BulldozerStageSoil.transform.DOScale(newScale, 0.3f).SetEase(Ease.OutQuad);
                Managers.Sound.Play(SoundManager.Sound.Effect, decreasingSound[decreaseindex]);
                manager.BulldozerStageSoil.transform.DOScale(manager.BulldozerStageSoil.transform.localScale * 1.1f, 0.1f)
                .SetEase(Ease.OutQuad)
                .SetLoops(4, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    manager.BulldozerStageSoil.transform.DOScale(newScale, 0.15f).SetEase(Ease.OutBack);
                    decreaseindex++;
                });
                break;
        }
        
    }

    void EndExcavatorStage()    //포크레인 스테이지 종료 메서드
    {
        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("우리 친구들이 포크레인을 도와줘서\n흙을 많이 팠어요!", "9_우리_친구들이_포크레인을_도와줘서_흙을_많이_팠어요_"));
            Managers.Sound.Play(SoundManager.Sound.Effect, manager.victoryAuidoClip);
            manager.Btn_ExcavatorStage.SetActive(false);
        });
        seq.AppendInterval(7f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("다른 일을 하는 자동차를 만나러 가요", "10_다른_일을_하는_자동차를_만나러_가요_"));
            manager.introVirtualCamera.Priority = 21;
            manager.excavatorVirtualCamera.Priority = 10;
            inputCount = 0;
        });
        seq.AppendInterval(5f);
        seq.AppendCallback(() =>
        {
            manager.introVirtualCamera.Priority = 10;
            manager.truckShowCamera.Priority = 20;

            Messenger.Default.Publish(new NarrationMessage("트럭은 많은 흙을 옮겨 줄 수 있어요", "3_트럭은_많은_흙을_옮겨_줄_수_있어요"));
            manager.truckAni.SetBool("LiftUp", true);
            Managers.Sound.Play(SoundManager.Sound.Effect, manager.HeavyMachinerySound);
            DOVirtual.DelayedCall(3.75f, () => { Managers.Sound.Stop(SoundManager.Sound.Effect); manager.twiceAudioIssue = true; });


        });
        seq.AppendInterval(1f);
        seq.AppendCallback(() =>
        {
            manager.truckAni.SetBool("LiftUp", false);
            manager.truckAni.SetBool("LiftDown", true);
            twiceissue = false;
        });
        seq.AppendInterval(4.3f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("트럭이 멈췄어요!", "27_트럭이_멈췄어요_"));

        });
        seq.AppendInterval(4f);
        seq.AppendCallback(() =>
        {

            manager.Btns_TruckIntro.SetActive(true);
            manager.Btns_TruckIntro.transform.DOScale(1f, 0.4f)
                .From(0.01f)
                .SetEase(Ease.Flash) // 팡! 튀어나오는 느낌
                .OnComplete(() =>
                {
                    manager.Btns_TruckIntro.transform.DOShakeScale(0.2f, 0.2f, 10, 90f);
                });
            Managers.Sound.Play(SoundManager.Sound.Narration, "Construction/Audio/audio_5_트럭이_움직일_수_있게_동작_버튼을_터치해주세요");

        });
        seq.AppendInterval(4f);
        seq.AppendCallback(() =>
        {
            manager.canNextBtnClick = true;
        });
    }

    void EndTruckStage()    //트럭 스테이지 종료 메서드
    {
        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("우리 친구들이 트럭을 도와줘서\n흙을 많이 옮겼어요!", "13_우리_친구들이_트럭을_도와줘서_흙을_많이_옮겼어요_"));
            Managers.Sound.Play(SoundManager.Sound.Effect, manager.victoryAuidoClip);
            manager.Btn_TruckStage.SetActive(false);
        });
        seq.AppendInterval(6f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("다른 일을 하는 자동차를 만나러 가요", "10_다른_일을_하는_자동차를_만나러_가요_"));
            manager.introVirtualCamera.Priority = 21;
            manager.excavatorVirtualCamera.Priority = 10;
            inputCount = 0;
        });
        seq.AppendInterval(5f);
        seq.AppendCallback(() =>
        {
            manager.introVirtualCamera.Priority = 10;
            manager.bulldozerShowCamera.Priority = 20;

            Messenger.Default.Publish(new NarrationMessage("불도저는 많은 흙을 옮겨 줄 수 있어요", "19_불도저는_많은_흙을_옮겨_줄_수_있어요_"));
            manager.bulldozerAni.SetBool("Work", true);

            Managers.Sound.Play(SoundManager.Sound.Effect, manager.HeavyMachinerySound);
            DOVirtual.DelayedCall(3.2f, () => { Managers.Sound.Stop(SoundManager.Sound.Effect); manager.twiceAudioIssue = true; });


        });
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() =>
        {
            manager.bulldozerAni.SetBool("Work", false);
            plusMoveDistance = 0f;
            twiceissue = false;
        });
        seq.AppendInterval(5.2f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("불도저가 멈췄어요!", "28_불도저가_멈췄어요_"));

        });
        seq.AppendInterval(4f);
        seq.AppendCallback(() =>
        {
            manager.Btns_BulldozerIntro.SetActive(true);
            manager.Btns_BulldozerIntro.transform.DOScale(1f, 0.4f)
                .From(0.01f)
                .SetEase(Ease.Flash) // 팡! 튀어나오는 느낌
                .OnComplete(() =>
                {
                    manager.Btns_BulldozerIntro.transform.DOShakeScale(0.2f, 0.2f, 10, 90f);
                });

            Managers.Sound.Play(SoundManager.Sound.Narration, "Construction/Audio/audio_6_불도저가_움직일_수_있게_동작_버튼을_터치해주세요");

        });
        seq.AppendInterval(4f);
        seq.AppendCallback(() =>
        {
            manager.canNextBtnClick = true;
        });



    }

    void EndBulldozerStage()    //불도저 스테이지 종료 메서드 (게임 종료)
    {
        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("우리 친구들이 불도저를 도와줘서\n튼튼한 건물이 지어지고 있어요!", "0_우리_친구들이_불도저를_도와줘서_튼튼한_건물이_지어지고_있어요_"));
            Managers.Sound.Play(SoundManager.Sound.Effect, manager.victoryAuidoClip);
            manager.Btn_BulldozerStage.SetActive(false);
        });
        seq.AppendInterval(7.3f);
        seq.AppendCallback(() =>
        {
            manager.endVirtualCamera.Priority = 21;
            manager.bulldozerVirtualCamera.Priority = 10;
            Managers.Sound.Play(SoundManager.Sound.Effect, "Construction/Audio/Clapping", 0.5f);
            Messenger.Default.Publish(new NarrationMessage("와! 우리가 힘을 합쳐\n튼튼한 건물이 완성되었어요!", "1_와__우리가_힘을_합쳐_튼튼한_건물이_완성되었어요_"));
            inputCount = 0;
            plusMoveDistance = 0f;
            twiceissue = false;
            manager.endBuilding.SetActive(true);
        });
    }

}     