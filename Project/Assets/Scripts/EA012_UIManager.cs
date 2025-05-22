using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;

public class EA012_UIManager : Base_UIManager
{   
    private EA012_GameManager _gm;

    protected override void Awake()
    {
        base.Awake();
        // 메시지 구독
        Messenger.Default.Subscribe<EA012Payload>(OnNarrationReceived);
        if (_gm == null)
        {
            _gm = GameObject.FindWithTag("GameManager").GetComponent<EA012_GameManager>();
        }
        
        Debug.Assert(_gm != null, "GameManager not found");
    }

    private void OnDestroy()
    {
        // 구독 해제
        Messenger.Default.Unsubscribe<EA012Payload>(OnNarrationReceived);
    }

    private float waitTimeBeforeTouch = 2f;
    private float carInfoNarrationDuration = 5.5f;
    private float delayOffset = 5f;
    private void OnNarrationReceived(EA012Payload payload)
    {
        Debug.Log($"[Narration Received] {payload.Narration}");


        if(_gm!=null)
        {
            if(Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip!=null) 
                _gm.SetClickableWithDelayOfNar(Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length + 2f) ; 
        }

    
        switch (payload.Narration)
        {
            
            case nameof(EA012_GameManager.MainSeq.Default):
                break;

            case nameof(EA012_GameManager.MainSeq.SeatSelection):
                PopFromZeroInstructionUI("먼저 친구들,\n각자 표시된 자리에 앉아주세요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnSeatSelection");
                break;

            case "OnSeatSelectFinished":
                PopFromZeroInstructionUI("다 앉았구나!\n이제 자동차를 만나러 가볼까?");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnSeatSelectFinished");
                break;

            
            
            case nameof(EA012_GameManager.MainSeq.SeqB_Ambulance):
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnTireRolling");
                PopFromZeroInstructionUI("굴러다니는 바퀴를 잡아 멈춰볼까요?");
                break;
            case nameof(EA012_GameManager.MainSeq.SeqC_PoliceCar):
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnTireRolling");
                PopFromZeroInstructionUI("굴러다니는 바퀴를 잡아 멈춰볼까요?");
                break;
            case nameof(EA012_GameManager.MainSeq.SeqD_FireTruck):
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnTireRolling");
                PopFromZeroInstructionUI("굴러다니는 바퀴를 잡아 멈춰볼까요?");
                break;
            case nameof(EA012_GameManager.MainSeq.SeqE_Taxi):
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnTireRolling");
                PopFromZeroInstructionUI("굴러다니는 바퀴를 잡아 멈춰볼까요?");
                break;
            case nameof(EA012_GameManager.MainSeq.SeqF_Bus):
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnTireRolling");
                PopFromZeroInstructionUI("굴러다니는 바퀴를 잡아 멈춰볼까요?");
                break;
            case nameof(EA012_GameManager.MainSeq.CarMoveHelpFinished):
                DOVirtual.DelayedCall(4f, () =>
                {
                    if (!_gm.isLastCar)
                    {
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/NextCar");
                        PopFromZeroInstructionUI("이제 다음 자동차를 만나러 가볼까요?");
                    }
                    
                });
                break;

            case "Arrival":
                TMP_Instruction.text = $"우와! {payload.CurrentCarName}가 도착했어요! 고마워요!";
                break;
            case "AllTiresRemoved":
                PopFromZeroInstructionUI("성공!");
                break;


            case nameof(EA012_GameManager.MainSeq.SeqB_Ambulance_Move):
                
                _gm.SetClickableWithDelayOfNar(carInfoNarrationDuration +delayOffset+ waitTimeBeforeTouch);

                PopFromZeroInstructionUI("흰색 자동차는 아픈 사람을 도와주고\n삐뽀삐뽀 소리가나요~");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/CarInfo_Ambulance");
                DOVirtual.DelayedCall(carInfoNarrationDuration, () =>
                {
                    
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_Ambulance");
                    PopFromZeroInstructionUI("구급차!");
                    DOVirtual.DelayedCall(waitTimeBeforeTouch, () =>
                    {
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Ambulance_TouchInduce");
                        PopFromZeroInstructionUI("구급차를 움직일 수 있게 도와주세요!\n길을 터치해주세요!");
                    });

                });
                break;
            case nameof(EA012_GameManager.MainSeq.SeqC_PoliceCar_Move):
               
                _gm.SetClickableWithDelayOfNar(carInfoNarrationDuration +delayOffset+ waitTimeBeforeTouch);

                PopFromZeroInstructionUI("파란 자동차는 나쁜 사람들을 잡아주고\n 애앵애앵 소리가 나요");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/CarInfo_PoliceCar");
               
                DOVirtual.DelayedCall(carInfoNarrationDuration, () =>
                {
                    PopFromZeroInstructionUI("경찰차!");
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_PoliceCar");
                    DOVirtual.DelayedCall(waitTimeBeforeTouch, () =>
                    {
                        PopFromZeroInstructionUI("경찰차를 움직일 수 있게 도와주세요!\n길을 터치해주세요!");
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/PoliceCar_TouchInduce");
                    });
                });
                break;
            
            
            case nameof(EA012_GameManager.MainSeq.SeqD_FireTruck_Move):
               
                    _gm.SetClickableWithDelayOfNar(carInfoNarrationDuration +delayOffset+ waitTimeBeforeTouch);

                PopFromZeroInstructionUI("빨간색 자동차는 불을 끄러 가고\n이용이용 소리가 나요");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/CarInfo_FireTruck");
                
                DOVirtual.DelayedCall(carInfoNarrationDuration, () =>
                {
                    PopFromZeroInstructionUI("소방차!");
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_FireTruck");
                    DOVirtual.DelayedCall(waitTimeBeforeTouch, () =>
                    {
                        PopFromZeroInstructionUI("소방차를 움직일 수 있게 도와주세요!\n길을 터치해주세요!");
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/FireTruck_TouchInduce");
                    });
                });
                break;
            
            
            case nameof(EA012_GameManager.MainSeq.SeqE_Taxi_Move):
               
                    _gm.SetClickableWithDelayOfNar(carInfoNarrationDuration +delayOffset+ waitTimeBeforeTouch);
                    PopFromZeroInstructionUI("노랑색 자동차는");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/CarInfo_FireTruck");
                
                DOVirtual.DelayedCall(carInfoNarrationDuration, () =>
                {
                    PopFromZeroInstructionUI("택시!");
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_Taxi");
                    DOVirtual.DelayedCall(waitTimeBeforeTouch, () =>
                    {
                        PopFromZeroInstructionUI("택시를 움직일 수 있게 도와주세요!\n길을 터치해주세요!");
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Taxi_TouchInduce");
                    });
                });
                break;
            case nameof(EA012_GameManager.MainSeq.SeqF_Bus_Move):
                
                    _gm.SetClickableWithDelayOfNar(carInfoNarrationDuration +delayOffset+ waitTimeBeforeTouch);
                PopFromZeroInstructionUI("길다란 자동차는");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/CarInfo_Bus");
                
                DOVirtual.DelayedCall(carInfoNarrationDuration, () =>
                {
                    PopFromZeroInstructionUI("버스!");
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_Bus");
                    DOVirtual.DelayedCall(3, () =>
                    {
                        PopFromZeroInstructionUI("버스를 움직일 수 있게 도와주세요!\n길을 터치해주세요!");
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Bus_TouchInduce");
                    });
                });
                break;


            case nameof(EA012_GameManager.MainSeq.Finished):
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnFinishReview");
                PopFromZeroInstructionUI("오늘 알아본 탈 것을 다시 볼까요?");
                break;
           
        }

   
        //초기화때 gm Null일 가능성있으므로 주의 
 
        // 대사 재생 후 클릭 가능하게 설정
    }

 

}
