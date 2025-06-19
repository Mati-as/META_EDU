using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class EA031_GameManager : Ex_BaseGameManager
{
    private enum MainSeq
    {
        Default_LateInit,
        OnIntro,
        OnFireAndAlarm,
        CloseMuthAndNose,
        TakeExit,
        OnEscape,
        OnFinish
    }

    private enum Objs
    {
        ToxicGas,
        SirenAlert,
        IntroAvatarController,
    }

    private AvatarController _introAvatarController;
    private ParticleSystem _sirenPs;
    private ParticleSystem _smokePs;
   public int CurrentMainMainSeq
    {
        get => CurrentMainMainSequence;
        set
        {
            CurrentMainMainSequence = value;

            //  Messenger.Default.Publish(new EA012Payload(_currentMainSequence.ToString()));
            Logger.ContentTestLog($"Current Sequence: {((MainSeq)CurrentMainMainSeq).ToString()}");
            // commin Init Part.
          

            
            ChangeThemeSeqAnim(value);
            switch (value)
            {
                
                case (int)MainSeq.Default_LateInit:
                  
                    break;
                
                case (int)MainSeq.OnIntro:
                    break;

                case (int)MainSeq.OnFireAndAlarm:
                    GetObject((int)Objs.ToxicGas).SetActive(true);
                    GetObject((int)Objs.SirenAlert).SetActive(true);
                    _uiManager.PopFromZeroInstructionUI("검정 연기가 보여~무슨 냄새지?");
                    DOVirtual.DelayedCall(3.5f, () =>
                    {
                        float playTime = 0;

                        _uiManager.PopFromZeroInstructionUI("친구들~ 불이 났어요! 모두 대피 해야해요!");
                        DOVirtual.Float(0f, 0f, 10f, _ =>
                        {
                        }).OnUpdate(() =>
                        {
                            playTime += Time.deltaTime;
                            if (playTime > 0.6f)
                            {
                                _sirenPs.Play();
                                playTime = 0f;
                            }

                        });
                    });


                    for (int i = 0; i < 4; i++)
                    {
                        _introAvatarController.PlayAnimation(i, AvatarController.AnimClip.LookOver);
                        DOVirtual.DelayedCall(UnityEngine.Random.Range(0.1f, 0.5f), () =>
                        {
                            _introAvatarController.PlayAnimation(3, AvatarController.AnimClip.LookOver);
                        });

                    }


          
                    break;
                
                
                case (int)MainSeq.CloseMuthAndNose:
                    break;
                
                case (int)MainSeq.TakeExit:
                   
                    break;
                
                case (int)MainSeq.OnEscape:
                   
                    break;

                case (int)MainSeq.OnFinish:
                 
                    break;
            }
        }
    }
   protected sealed override void Init()
   {
       psResourcePath = "Runtime/EA010/FX_leaves";
       
        BindObject(typeof(Objs));
       base.Init();
       _uiManager = UIManagerObj.GetComponent<Base_UIManager>();
       _smokePs = GetObject((int)Objs.ToxicGas).GetComponent<ParticleSystem>();
         _sirenPs = GetObject((int)Objs.SirenAlert).GetComponent<ParticleSystem>();
    

       Logger.Log("Init--------------------------------");
   }

   private Base_UIManager _uiManager;
   

   protected override void OnGameStartStartButtonClicked()
   {
      
       base.OnGameStartStartButtonClicked();
         CurrentMainMainSeq = (int)MainSeq.OnIntro;
         _smokePs.Clear();
         _smokePs.Stop();
         _smokePs.Play();

         DOVirtual.DelayedCall(3f, () =>
         {
             CurrentMainMainSeq = (int)MainSeq.OnFireAndAlarm;
         });
         _introAvatarController = GetObject((int)Objs.IntroAvatarController).GetComponent<AvatarController>();
   }
}
