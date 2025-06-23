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
    private Construction_GameManager _manager;
    public int soilCount;

    public int inputCount = 0;
    public int maxInputs = 21;

    private readonly Vector3 _startScale = new Vector3(8.37f, 18f, 7.8f);
    private readonly Vector3 _endScale = new Vector3(0f, 0f, 0f);

    public float plusMoveDistance = 0;

    [SerializeField] private int decreaseIndex = 0;
    [SerializeField] private List<AudioClip> decreasingSound = new List<AudioClip>(3);

    [SerializeField] private bool twiceIssue = false;

    private void Start()
    {
        _manager = FindObjectOfType<Construction_GameManager>();
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

        if (inputCount >= maxInputs && !twiceIssue)    //스테이지 종료 호출 타이밍
        {
            twiceIssue = true;
            switch (selected)
            {
                case VehicleType.Excavator:
                    EndExcavatorStage();
                    decreaseIndex = 0;
                    break;
                case VehicleType.Truck:
                    EndTruckStage();
                    decreaseIndex = 0;
                    break;
                case VehicleType.Bulldozer:
                    EndBulldozerStage();
                    decreaseIndex = 0;
                    break;
            }
        }
        
        int stepSize = 7; // 7번 모이면 1단계 스케일 감소
        if (inputCount % stepSize != 0) // 7로나눠서 딱떨어지지않으면 줄어들지 않게끔
            return;

        int totalSteps = maxInputs / stepSize;
        int stepIndex = inputCount / stepSize;
        var step = (_startScale - _endScale) / totalSteps;
        var newScale = _startScale - step * stepIndex;
        var bulldozerScale = _endScale + step * stepIndex;

        switch (selected)
        {
            case VehicleType.Excavator:
                _manager.excavatorStageSoil.transform.DOScale(newScale, 0.3f).SetEase(Ease.OutQuad);
                Managers.Sound.Play(SoundManager.Sound.Effect, decreasingSound[decreaseIndex]);
                _manager.excavatorStageSoil.transform.DOScale(_manager.excavatorStageSoil.transform.localScale * 1.1f, 0.1f)
                .SetEase(Ease.OutQuad)
                .SetLoops(4, LoopType.Yoyo) // 커졌다 작아졌다 반복
                .OnComplete(() =>
                {
                    _manager.excavatorStageSoil.transform.DOScale(newScale, 0.15f).SetEase(Ease.OutBack);
                    decreaseIndex++;
                });
                break;
            case VehicleType.Truck:
                plusMoveDistance += 3f;
                _manager.truckStageSoil.transform.DOScale(newScale, 0.3f).SetEase(Ease.OutQuad);
                Managers.Sound.Play(SoundManager.Sound.Effect, decreasingSound[decreaseIndex]);
                _manager.truckStageSoil.transform.DOScale(_manager.truckStageSoil.transform.localScale * 1.1f, 0.1f)
                .SetEase(Ease.OutQuad)
                .SetLoops(4, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    _manager.truckStageSoil.transform.DOScale(newScale, 0.15f).SetEase(Ease.OutBack);
                    decreaseIndex++;
                });
                break;
            case VehicleType.Bulldozer:
                plusMoveDistance += 3f;
                _manager.bulldozerStageSoil.transform.DOScale(bulldozerScale, 0.3f).SetEase(Ease.OutQuad);
                Managers.Sound.Play(SoundManager.Sound.Effect, decreasingSound[decreaseIndex]);
                DOVirtual.DelayedCall(0.3f, () => _manager.bulldozerStageSoil.transform
                    .DOScale(_manager.bulldozerStageSoil.transform.localScale * 1.1f, 0.1f)
                    .SetEase(Ease.OutQuad)
                    .SetLoops(4, LoopType.Yoyo)
                    .OnComplete(() =>
                    {
                        _manager.bulldozerStageSoil.transform.DOScale(bulldozerScale, 0.15f).SetEase(Ease.OutBack);
                        decreaseIndex++;
                    }));
                
                break;
        }
        
    }

    private void EndExcavatorStage()    //포크레인 스테이지 종료 메서드
    {
        var seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("우리 친구들이 포크레인을 도와줘서\n흙을 많이 팠어요!", "9_우리_친구들이_포크레인을_도와줘서_흙을_많이_팠어요_"));
            Managers.Sound.Play(SoundManager.Sound.Effect, _manager.victoryAudioClip);
            _manager.btnExcavatorStage.SetActive(false);
        });
        seq.AppendInterval(7f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("다른 일을 하는 자동차를 만나러 가요", "10_다른_일을_하는_자동차를_만나러_가요_"));
            _manager.introVirtualCamera.Priority = 21;
            _manager.excavatorVirtualCamera.Priority = 10;
            inputCount = 0;
        });
        seq.AppendInterval(5f);
        seq.AppendCallback(() =>
        {
            _manager.introVirtualCamera.Priority = 10;
            _manager.truckShowCamera.Priority = 20;

            Messenger.Default.Publish(new NarrationMessage("트럭은 많은 흙을 옮겨 줄 수 있어요", "3_트럭은_많은_흙을_옮겨_줄_수_있어요"));
            _manager.truckAni.SetBool("LiftUp", true);
            Managers.Sound.Play(SoundManager.Sound.Effect, _manager.heavyMachinerySound);
            DOVirtual.DelayedCall(3.75f, () => { Managers.Sound.Stop(SoundManager.Sound.Effect); _manager.twiceAudioIssue = true; });


        });
        seq.AppendInterval(1f);
        seq.AppendCallback(() =>
        {
            _manager.truckAni.SetBool("LiftUp", false);
            _manager.truckAni.SetBool("LiftDown", true);
            twiceIssue = false;
        });
        seq.AppendInterval(4.3f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("트럭이 멈췄어요!", "27_트럭이_멈췄어요_"));

        });
        seq.AppendInterval(4f);
        seq.AppendCallback(() =>
        {

            _manager.btnTruckIntro.SetActive(true);
            _manager.btnTruckIntro.transform.DOScale(1f, 0.4f)
                .From(0.01f)
                .SetEase(Ease.Flash) // 팡! 튀어나오는 느낌
                .OnComplete(() =>
                {
                    _manager.btnTruckIntro.transform.DOShakeScale(0.2f, 0.2f, 10, 90f);
                });
            Messenger.Default.Publish(new NarrationMessage("트럭이 움직일 수 있게 동작 버튼을 터치해주세요!", "5_트럭이_움직일_수_있게_동작_버튼을_터치해주세요"));

        });
        seq.AppendInterval(4f);
        seq.AppendCallback(() =>
        {
            _manager.canNextBtnClick = true;
        });
    }

    private void EndTruckStage()    //트럭 스테이지 종료 메서드
    {
        var seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("우리 친구들이 트럭을 도와줘서\n흙을 많이 옮겼어요!", "13_우리_친구들이_트럭을_도와줘서_흙을_많이_옮겼어요_"));
            Managers.Sound.Play(SoundManager.Sound.Effect, _manager.victoryAudioClip);
            _manager.btnTruckStage.SetActive(false);
        });
        seq.AppendInterval(6f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("다른 일을 하는 자동차를 만나러 가요", "10_다른_일을_하는_자동차를_만나러_가요_"));
            _manager.introVirtualCamera.Priority = 21;
            _manager.excavatorVirtualCamera.Priority = 10;
            inputCount = 0;
        });
        seq.AppendInterval(5f);
        seq.AppendCallback(() =>
        {
            _manager.introVirtualCamera.Priority = 10;
            _manager.bulldozerShowCamera.Priority = 20;

            Messenger.Default.Publish(new NarrationMessage("불도저는 많은 흙을 옮겨 줄 수 있어요", "19_불도저는_많은_흙을_옮겨_줄_수_있어요_"));
            _manager.bulldozerAni.SetBool("Work", true);

            Managers.Sound.Play(SoundManager.Sound.Effect, _manager.heavyMachinerySound);
            DOVirtual.DelayedCall(3.2f, () => { Managers.Sound.Stop(SoundManager.Sound.Effect); _manager.twiceAudioIssue = true; });


        });
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() =>
        {
            _manager.bulldozerAni.SetBool("Work", false);
            plusMoveDistance = 0f;
            twiceIssue = false;
        });
        seq.AppendInterval(5.2f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("불도저가 멈췄어요!", "28_불도저가_멈췄어요_"));

        });
        seq.AppendInterval(4f);
        seq.AppendCallback(() =>
        {
            _manager.btnBulldozerIntro.SetActive(true);
            _manager.btnBulldozerIntro.transform.DOScale(1f, 0.4f)
                .From(0.01f)
                .SetEase(Ease.Flash) // 팡! 튀어나오는 느낌
                .OnComplete(() =>
                {
                    _manager.btnBulldozerIntro.transform.DOShakeScale(0.2f, 0.2f, 10, 90f);
                });
            Messenger.Default.Publish(new NarrationMessage("불도저가 움직일 수 있게 동작 버튼을 터치해주세요!", "6_불도저가_움직일_수_있게_동작_버튼을_터치해주세요"));

        });
        seq.AppendInterval(4f);
        seq.AppendCallback(() =>
        {
            _manager.canNextBtnClick = true;
        });



    }

    private void EndBulldozerStage()    //불도저 스테이지 종료 메서드 (게임 종료)
    {
        var seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("우리 친구들이 불도저를 도와줘서\n튼튼한 건물이 지어지고 있어요!", "0_우리_친구들이_불도저를_도와줘서_튼튼한_건물이_지어지고_있어요_"));
            Managers.Sound.Play(SoundManager.Sound.Effect, _manager.victoryAudioClip);
            _manager.btnBulldozerStage.SetActive(false);
        });
        seq.AppendInterval(7.3f);
        seq.AppendCallback(() =>
        {
            _manager.endVirtualCamera.Priority = 21;
            _manager.bulldozerVirtualCamera.Priority = 10;
            Managers.Sound.Play(SoundManager.Sound.Effect, "Construction/Audio/Clapping", 0.5f);
            Messenger.Default.Publish(new NarrationMessage("와! 우리가 힘을 합쳐\n튼튼한 건물이 완성되었어요!", "1_와__우리가_힘을_합쳐_튼튼한_건물이_완성되었어요_"));
            inputCount = 0;
            plusMoveDistance = 0f;
            twiceIssue = false;
            _manager.endBuilding.SetActive(true);
        });
    }

}     