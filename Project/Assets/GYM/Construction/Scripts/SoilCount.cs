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

    private void Start()
    {
        manager = FindObjectOfType<Construction_GameManager>();

    }

    public void SoilDecreaseStep(VehicleType selected)
    {
        if (inputCount >= maxInputs)
            return;

        inputCount++;

        Vector3 step = (startScale - endScale) / maxInputs;
        Vector3 newScale = startScale - step * inputCount;

        switch (selected)
        {
            case VehicleType.Excavator:
                // 포크레인 관련 로직
                manager.ExcavatorStageSoil.transform.DOScale(newScale, 0.3f).SetEase(Ease.OutQuad);
                break;
            case VehicleType.Truck:
                // 트럭 관련 로직
                manager.TruckStageSoil.transform.DOScale(newScale, 0.3f).SetEase(Ease.OutQuad);
                break;
            case VehicleType.Rmc:
                // 레미콘 관련 로직
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
        manager.excavatorStageEnd = true;

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
            Messenger.Default.Publish(new NarrationMessage("트럭이 움직일 수 있게 터치해주세요", "11_트럭이_움직일_수_있게_터치해주세요"));
        });
    }

    void EndTruckStage()    //포크레인 스테이지 종료 메서드
    {
        manager.truckStageEnd = true;

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