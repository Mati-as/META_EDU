using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;

public class EA012_UIManager : Base_UIManager
{   
    private EA012_GameManager _gm;

    private void Awake()
    {
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
    private float carInfoNarrationDuration = 5f;
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
                GetText((int)TMPs.TMP_Instruction).text = "먼저 친구들 각자 표시된 자리에 앉아주세요!";
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnSeatSelection");
                break;

            case "OnSeatSelectFinished":
                GetText((int)TMPs.TMP_Instruction).text = "다 앉았구나! 이제 자동차를 만나러 가볼까?";
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnSeatSelectFinished");
                break;

            
            
            case nameof(EA012_GameManager.MainSeq.SeqB_Ambulance):
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnTireRolling");
                GetText((int)TMPs.TMP_Instruction).text = "굴러다니는 바퀴를 잡아 멈춰볼까요?";
                break;
            case nameof(EA012_GameManager.MainSeq.SeqC_PoliceCar):
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnTireRolling");
                GetText((int)TMPs.TMP_Instruction).text = "굴러다니는 바퀴를 잡아 멈춰볼까요?";
                break;
            case nameof(EA012_GameManager.MainSeq.SeqD_FireTruck):
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnTireRolling");
                GetText((int)TMPs.TMP_Instruction).text = "굴러다니는 바퀴를 잡아 멈춰볼까요?";
                break;
            case nameof(EA012_GameManager.MainSeq.SeqE_Taxi):
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnTireRolling");
                GetText((int)TMPs.TMP_Instruction).text = "굴러다니는 바퀴를 잡아 멈춰볼까요?";
                break;
            case nameof(EA012_GameManager.MainSeq.SeqF_Bus):
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnTireRolling");
                GetText((int)TMPs.TMP_Instruction).text = "굴러다니는 바퀴를 잡아 멈춰볼까요?";
                break;
            case nameof(EA012_GameManager.MainSeq.CarMoveHelpFinished):
                DOVirtual.DelayedCall(4f, () =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/NextCar");
                    GetText((int)TMPs.TMP_Instruction).text = "이제 다음 자동차를 만나러 가볼까요?";
                });
                break;

            case "Arrival":
                GetText((int)TMPs.TMP_Instruction).text = $"우와! {payload.CurrentCarName}가 도착했어요! 고마워요!";
                break;
            case "AllTiresRemoved":
                GetText((int)TMPs.TMP_Instruction).text = "성공!";
                break;


            case nameof(EA012_GameManager.MainSeq.SeqB_Ambulance_Move):
                
                _gm.SetClickableWithDelayOfNar(carInfoNarrationDuration +delayOffset+ waitTimeBeforeTouch);

                GetText((int)TMPs.TMP_Instruction).text = "흰색 자동차는 아픈 사람을 도와주고\n삐뽀삐뽀 소리가나요~";
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/CarInfo_Ambulance");
                DOVirtual.DelayedCall(carInfoNarrationDuration, () =>
                {
                    
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_Ambulance");
                    GetText((int)TMPs.TMP_Instruction).text = "구급차";
                    DOVirtual.DelayedCall(waitTimeBeforeTouch, () =>
                    {
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Ambulance_TouchInduce");
                        GetText((int)TMPs.TMP_Instruction).text = "앰뷸런스를 움직일 수 있게 도와주세요!\n길을 터치해주세요!";
                    });

                });
                break;
            case nameof(EA012_GameManager.MainSeq.SeqC_PoliceCar_Move):
               
                _gm.SetClickableWithDelayOfNar(carInfoNarrationDuration +delayOffset+ waitTimeBeforeTouch);

                GetText((int)TMPs.TMP_Instruction).text = "파란 자동차는 나쁜 사람들을 잡아주고 애앵애앵 소리가 나요";
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/CarInfo_PoliceCar");
               
                DOVirtual.DelayedCall(carInfoNarrationDuration, () =>
                {
                    GetText((int)TMPs.TMP_Instruction).text = "경찰차";
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_PoliceCar");
                    DOVirtual.DelayedCall(waitTimeBeforeTouch, () =>
                    {
                        GetText((int)TMPs.TMP_Instruction).text = "경찰차를 움직일 수 있게 도와주세요!\n길을 터치해주세요!";
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/PoliceCar_TouchInduce");
                    });
                });
                break;
            
            
            case nameof(EA012_GameManager.MainSeq.SeqD_FireTruck_Move):
               
                    _gm.SetClickableWithDelayOfNar(carInfoNarrationDuration +delayOffset+ waitTimeBeforeTouch);

                GetText((int)TMPs.TMP_Instruction).text = "빨간색 자동차는 불을 끄러 가고 이용이용 소리가 나요";
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/CarInfo_FireTruck");
                
                DOVirtual.DelayedCall(carInfoNarrationDuration, () =>
                {
                    GetText((int)TMPs.TMP_Instruction).text = "소방차";
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_PoliceCar");
                    DOVirtual.DelayedCall(waitTimeBeforeTouch, () =>
                    {
                        GetText((int)TMPs.TMP_Instruction).text = "소방차를 움직일 수 있게 도와주세요!\n길을 터치해주세요!";
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/PoliceCar_TouchInduce");
                    });
                });
                break;
            
            
            case nameof(EA012_GameManager.MainSeq.SeqE_Taxi_Move):
               
                    _gm.SetClickableWithDelayOfNar(carInfoNarrationDuration +delayOffset+ waitTimeBeforeTouch);
                GetText((int)TMPs.TMP_Instruction).text = "노랑색 자동차는 ";
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/CarInfo_FireTruck");
                
                DOVirtual.DelayedCall(carInfoNarrationDuration, () =>
                {
                    GetText((int)TMPs.TMP_Instruction).text = "택시";
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_PoliceCar");
                    DOVirtual.DelayedCall(waitTimeBeforeTouch, () =>
                    {
                        GetText((int)TMPs.TMP_Instruction).text = "택시를 움직일 수 있게 도와주세요!\n길을 터치해주세요!";
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/PoliceCar_TouchInduce");
                    });
                });
                break;
            case nameof(EA012_GameManager.MainSeq.SeqF_Bus_Move):
                
                    _gm.SetClickableWithDelayOfNar(carInfoNarrationDuration +delayOffset+ waitTimeBeforeTouch);
                GetText((int)TMPs.TMP_Instruction).text = "길다란 자동차는 ";
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/CarInfo_FireTruck");
                
                DOVirtual.DelayedCall(carInfoNarrationDuration, () =>
                {
                    GetText((int)TMPs.TMP_Instruction).text = "버스";
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_PoliceCar");
                    DOVirtual.DelayedCall(3, () =>
                    {
                        GetText((int)TMPs.TMP_Instruction).text = "버스를 움직일 수 있게 도와주세요!\n길을 터치해주세요!";
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/PoliceCar_TouchInduce");
                    });
                });
                break;


            case nameof(EA012_GameManager.MainSeq.Finished):
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/OnFinishReview");
                GetText((int)TMPs.TMP_Instruction).text = "오늘 알아본 탈 것을 다시 볼까요?";
                break;
           
        }

   
        //초기화때 gm Null일 가능성있으므로 주의 
 
        // 대사 재생 후 클릭 가능하게 설정
    }

 

}
