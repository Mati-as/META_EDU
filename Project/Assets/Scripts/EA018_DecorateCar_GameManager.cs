using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;

public class EA018_DecorateCar_GameManager : Ex_BaseGameManager
{


    private enum Obj
    {
        Sprites_Ambulance,
        Sprites_PoliceCar,
        Sprites_FireTruck,
        
        SpriteBg,
        
    }
    

    private Dictionary<int, Animator[]> _subPartsAnimMap = new();
    private Dictionary<int,Animator> tfIDToAnimatorMap = new();
    // 첫번쨰 Sprite이미지는 WholePicture로 사용 중
    // 1.whole 2~8 sub parts sprite Images
    private Dictionary<int, SpriteRenderer[]> _spritesMap = new();

    private enum MainSeq
    {
        SeatSelection,
        
        Ambulance_Intro,
        Ambulance_Outro,
        
        PoliceCar_Intro,
        PoliceCar_Outro,
        
        FireTruck_Intro,
        FireTruck_Outro,
        
        OnFinish
    }
    public enum SubPartsAnim
    {
        Default,
        Seperated,
        Completing,
    }
    
       public  int CurrentMainSeq
    {
        get => _currentMainSequence;
        set
        {
           
            _currentMainSequence = value;
            
            Messenger.Default.Publish(new EA012Payload(_currentMainSequence.ToString()));
            
            Logger.ContentTestLog($"Current Sequence: {CurrentMainSeq.ToString()}");



            // commin Init Part.
          
            
            switch (value)
            {
                case (int)MainSeq.SeatSelection:
                    ChangeThemeSeqAnim((int)value);
                    break;
                
                case (int)MainSeq.Ambulance_Intro:
                    ChangeThemeSeqAnim((int)value);
                    break;
                case (int)MainSeq.Ambulance_Outro:
                    ChangeThemeSeqAnim((int)value);
                    break;
                
                case (int)MainSeq.PoliceCar_Intro:
                    break;
                case (int)MainSeq.PoliceCar_Outro:
                    break;
                
                case (int)MainSeq.FireTruck_Intro:
                    break;
                case (int)MainSeq.FireTruck_Outro:
                    break;
                
                case (int)MainSeq.OnFinish:
                    break;
                
            }
        }
    }
       

    private const int COUNT_TO_COMPLETE = 10;

    protected override void Init()
    {
        DOTween.SetTweensCapacity(1000,1000);
        BindObject(typeof(Obj));
        
        Animator[] ambulAnims = GetObject((int)Obj.Sprites_Ambulance).GetComponentsInChildren<Animator>(false);
        
        _subPartsAnimMap.Add((int)Obj.Sprites_Ambulance,  ambulAnims);
        
        Animator[] policeAnims = GetObject((int)Obj.Sprites_PoliceCar).GetComponentsInChildren<Animator>(false);
        _subPartsAnimMap.Add((int)Obj.Sprites_PoliceCar, policeAnims);  
        
        Animator[] fireAnims = GetObject((int)Obj.Sprites_FireTruck).GetComponentsInChildren<Animator>(false);
        _subPartsAnimMap.Add((int)Obj.Sprites_FireTruck, fireAnims);

        
        
        SpriteRenderer[] Amb_Sprites =
            GetObject((int)Obj.Sprites_Ambulance).GetComponentsInChildren<SpriteRenderer>(false);
        _spritesMap.Add((int)Obj.Sprites_Ambulance,Amb_Sprites);
        
        SpriteRenderer[] policeSprites =
            GetObject((int)Obj.Sprites_PoliceCar).GetComponentsInChildren<SpriteRenderer>(false);
        _spritesMap.Add((int)Obj.Sprites_PoliceCar,  policeSprites);  
        
        SpriteRenderer[] Sprites_FireTruck =
            GetObject((int)Obj.Sprites_FireTruck).GetComponentsInChildren<SpriteRenderer>(false);
        _spritesMap.Add((int)Obj.Sprites_FireTruck, Sprites_FireTruck);
        
        
        
        //For RaySync Control. 
        foreach (var key in _subPartsAnimMap.Keys.ToArray())
        {
            foreach (var animator in _subPartsAnimMap[key])
            {
                tfIDToAnimatorMap.Add(animator.transform.GetInstanceID(), animator);
            }
        }

        SetSubPartsAnimatorStatus(false);
        SetSpriteStatus(false);
        
        foreach (var sprite in _spritesMap[0])
        {
            Logger.ContentTestLog($" Animator added: {sprite.name}");
        }

        
        base.Init();
    }
    
    private void SetSubPartsAnimatorStatus(bool isActive)
    {
        foreach (var key in _subPartsAnimMap.Keys.ToArray())
        {
            foreach (var animator in _subPartsAnimMap[key])
            {
                animator.enabled = isActive;
            }
        }
    }

    private void SetSpriteStatus(bool isActive)
    {
        foreach (var key in _spritesMap.Keys.ToArray())
        {
            foreach (SpriteRenderer sprite in _spritesMap[key])
            {
                sprite.enabled = isActive;
            }
        }
    }
    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        
        DOVirtual.DelayedCall(1.5f, () =>
        {
            initialMessage = "먼저 친구들,\n각자 표시된 자리에 앉아주세요!";
            _uiManagerCommonBehaviorController.ShowInitialMessage(initialMessage);
            
            CurrentMainSeq = (int)MainSeq.SeatSelection; 
            
            //테스트용으로 3초뒤에 시퀀스 변경
            DOVirtual.DelayedCall(3f, () =>
            {
                CurrentMainSeq = (int)1;
                ChangeThemeSeqAnim(1);
                OnPoliceCarIntro();
            });
            
            
            Logger.ContentTestLog("Mainseq Changed SeatSelection -------------------");
        });

    }

    #region  Subparts Movingpart------------------

    


    private void OnintroForSprites()
    {
        foreach (var key in _subPartsAnimMap.Keys.ToArray())
        {
            foreach (var animator in _subPartsAnimMap[key])
            {
                animator.enabled = false; 
            }
        }
    }

    private bool _isPartsClickable = false;

    private void MoveTowardToCompletion()
    {
        if (!_isPartsClickable) return;
        
        _isPartsClickable = false;
        DOVirtual.DelayedCall(0.15f, () =>
        {
            _isPartsClickable = true;
        });
        
        
       

        foreach (var hit in GameManager_Hits)
        {
            int ID = hit.transform.GetInstanceID();
            
            if (tfIDToAnimatorMap.ContainsKey(ID))
            {
                Animator animator = tfIDToAnimatorMap[ID];

                // OnCompletion 클립 가져오기
                AnimationClip clip = animator.runtimeAnimatorController.animationClips
                    .FirstOrDefault(c => c.name == "Complete");

                if (clip != null)
                {
                    float totalDuration = clip.length;
                    float partialDuration = totalDuration * 0.1f; // 10분의 1

                    // Animator 활성화
                    animator.enabled = true;
                    animator.Play("Complete", 0, 0f); // 처음부터 재생

                    // 0.5초만 재생하고 비활성화
                    DOVirtual.DelayedCall(0.5f, () =>
                    {
                        animator.enabled = false;
                    });
                    
                    Logger.ContentTestLog("Sub Parts Move Foward -------------------");
                }
                else
                {
                    Logger.LogWarning("Clip is null");
                }
            }
        }


    }
    
    #endregion
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        MoveTowardToCompletion();
    }
    
    #region  AmbulancePart

    private void OnPoliceCarIntro()
    {
        SetSubPartsAnimatorStatus(false);
        SetSpriteStatus(true);

        foreach (var sprite in _spritesMap[0])
        {
            sprite.DOFade(0, 0.01f).OnComplete(() =>
            {
              sprite.DOFade(1, 1f);
            });
        }
         // Physics.RaycastAll(GameManager_Ray);
        Logger.ContentTestLog("OnPoliceCarIntro -------------------");
        _isPartsClickable = true;
    }
    private void OnAmbulancePartIntro()
    {
        SetSubPartsAnimatorStatus(true);
        SetSpriteStatus(true);

        foreach (var sprite in _spritesMap[0])
        {
            sprite.enabled = true;
        }
    }

    #endregion
}
