using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;

public enum VehicleType
{
    Excavator,
    Truck,
    Rmc
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

    private void Start()
    {
        manager = FindObjectOfType<Construction_GameManager>();

    }

    public void SoilDecreaseStep(VehicleType selected)
    {
        if (inputCount > maxInputs)
            return;

        inputCount++;

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
                manager.ExcavatorStageSoil.transform.DOScale(manager.ExcavatorStageSoil.transform.localScale * 1.1f, 0.1f)
                .SetEase(Ease.OutQuad)
                .SetLoops(4, LoopType.Yoyo) // 커졌다 작아졌다 반복
                .OnComplete(() =>
                {
                    manager.ExcavatorStageSoil.transform.DOScale(newScale, 0.15f).SetEase(Ease.OutBack); 
                });
                break;
            case VehicleType.Truck:
                plusMoveDistance += 3f;
                manager.TruckStageSoil.transform.DOScale(newScale, 0.3f).SetEase(Ease.OutQuad);
                manager.TruckStageSoil.transform.DOScale(manager.TruckStageSoil.transform.localScale * 1.1f, 0.1f)
                .SetEase(Ease.OutQuad)
                .SetLoops(4, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    manager.TruckStageSoil.transform.DOScale(newScale, 0.15f).SetEase(Ease.OutBack);
                });
                break;
            case VehicleType.Rmc:   //레미콘 미정
                manager.RmcStageSoil.transform.DOScale(newScale, 0.3f).SetEase(Ease.OutQuad);
                break;
        }
        if (inputCount == maxInputs)    //스테이지 종료 호출 타이밍
        {
            switch (selected)
            {
                case VehicleType.Excavator:
                    EndExcavatorStage();
                    break;
                case VehicleType.Truck:
                    EndTruckStage();
                    break;
                case VehicleType.Rmc:
                    break;

            }
        }
    }

    void EndExcavatorStage()    //포크레인 스테이지 종료 메서드
    {
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
            manager.introVirtualCamera.Priority = 21;
            manager.excavatorVirtualCamera.Priority = 10;
            inputCount = 0;
        });
        seq.AppendInterval(4f);
        seq.AppendCallback(() =>
        {
            manager.introVirtualCamera.Priority = 10;
            manager.TruckShowCamera.Priority = 20;

            Messenger.Default.Publish(new NarrationMessage("트럭은 많은 흙을 옮겨 줄 수 있어요", "3_트럭은_많은_흙을_옮겨_줄_수_있어요"));
            //instrcutionUI가 비활성화됨 문제의 원인을 파악하지못햇음
            manager.truckAni.SetBool("LiftUp", true);

            manager.Btns_TruckIntro.SetActive(true);
            manager.Btns_TruckIntro.transform.DOScale(1f, 0.4f)
                .From(0.01f)
                .SetEase(Ease.Flash) // 팡! 튀어나오는 느낌
                .OnComplete(() =>
                {
                    manager.Btns_TruckIntro.transform.DOShakeScale(0.2f, 0.2f, 10, 90f);
                });
            
        });
        seq.AppendInterval(1f);
        seq.AppendCallback(() =>
        {
            manager.truckAni.SetBool("LiftUp", false);
            manager.truckAni.SetBool("LiftDown", true);
            
        });
        
    }

    void EndTruckStage()    //트럭 스테이지 종료 메서드
    {
        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("우리 친구들이 트럭을 도와줘서 흙을 많이 옮겼어요!", "13_우리_친구들이_트럭을_도와줘서_흙을_많이_옮겼어요_"));
            Managers.Sound.Play(SoundManager.Sound.Effect, manager.victoryAuidoClip);
            manager.Btn_Truck.SetActive(false);
        });
        seq.AppendInterval(5f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("다른 일을 하는 자동차를 만나러 가요", "14_다른_일을_하는_자동차를_만나러_가요"));
            manager.introVirtualCamera.Priority = 21;
            manager.truckVirtualCamera.Priority = 10;
            inputCount = 0;
        });
        seq.AppendInterval(4f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("레미콘이 움직일 수 있게 터치해주세요", "15_레미콘이_움직일_수_있게_터치해주세요"));
        });
    }

}     