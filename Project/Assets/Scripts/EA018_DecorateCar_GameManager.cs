using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Random = UnityEngine.Random;

public class EA018_DecorateCar_GameManager : Ex_BaseGameManager
{
    private enum Obj
    {
        Sprites_Ambulance,
        Sprites_PoliceCar,
        Sprites_FireTruck,

        SpriteBg,

        Seat_A,
        Seat_B,
        Seat_C,
        Seat_D,
        Seat_E,
        Seat_F,
        Seat_G,
        
        CarName_Police,
        CarName_FireTruck,
        CarName_Ambulance,
        Car_PoliceCar,
        Car_FireTruck,
        Car_Ambulance,
        
        TMP_Ambulance,
        TMP_FireTruck,
        TMP_Police,
        
    }

    private Dictionary<int, Vector3> _defaultPosMap = new();
    private Vector3 strenth = new Vector3(8f, 8f, 8f);
    private bool _isClickableOnFinish = false;
    
    private void OnRaySyncedOnFinish()
    {
        if (!_isClickableOnFinish) return; 
        
        foreach (var hit in GameManager_Hits)
        {
            int ID = hit.transform.GetInstanceID();
            
            if(_tfIdToEnumMap.TryGetValue(ID, out var obj))
            {
                if (obj == (int)Obj.CarName_Police)
                {
                    _isClickableMap.TryAdd((int)Obj.CarName_Police, true);
                    if (!_isClickableMap[(int)Obj.CarName_Police]) return;
                    _isClickableMap[(int)Obj.CarName_Police] = false;
                    DOVirtual.DelayedCall(0.7f, () =>
                    {
                        _isClickableMap[(int)Obj.CarName_Police] = true;
                    });
                    
                    
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA018/Name_PoliceCar");
                    GetObject((int)Obj.Car_PoliceCar).transform.DOShakeRotation(1.5f, strenth, 10, 90)
                        .OnComplete(() =>
                        {
                            GetObject((int)Obj.Car_PoliceCar).transform
                                .DORotateQuaternion(_defaultRotationQuatMap[(int)Obj.Car_PoliceCar], 0.5f);
                            GetObject((int)Obj.Car_PoliceCar).transform.DOMove(
                                _defaultPosMap[(int)Obj.Car_PoliceCar], 0.5f);
                        });
                    
                    GetObject((int)Obj.Car_PoliceCar).transform.DOShakeScale(1, new Vector3(0.1f, 0.1f, 0.1f), 10, 90);
                   
                    GetObject((int)Obj.CarName_Police).transform.DOShakeScale(1, new Vector3(0.05f, 0.05f, 0.05f), 10, 90)
                        .OnComplete(() =>
                        {
                            GetObject((int)Obj.CarName_Police).transform
                                .DORotateQuaternion(_defaultRotationQuatMap[(int)Obj.CarName_Police], 0.5f);
                            GetObject((int)Obj.CarName_Police).transform.DOMove(
                                _defaultPosMap[(int)Obj.CarName_Police], 0.5f);
                            
                            GetObject((int)Obj.TMP_Police).transform
                                .DORotateQuaternion(_defaultRotationQuatMap[(int)Obj.TMP_Police], 0.5f);
                            GetObject((int)Obj.TMP_Police).transform.DOMove(
                                _defaultPosMap[(int)Obj.TMP_Police], 0.5f);
                        });;

                }
                if (obj == (int)Obj.CarName_FireTruck)
                {
                    _isClickableMap.TryAdd((int)Obj.CarName_FireTruck, true);
                    if (!_isClickableMap[(int)Obj.CarName_FireTruck]) return;
                    _isClickableMap[(int)Obj.CarName_FireTruck] = false;
                    DOVirtual.DelayedCall(0.7f, () =>
                    {
                        _isClickableMap[(int)Obj.CarName_FireTruck] = true;
                    });
                    
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA018/Name_FireTruck");
                    GetObject((int)Obj.Car_FireTruck).transform.DOShakeRotation(1.5f, strenth, 10, 90)
                        .OnComplete(() =>
                        {
                            GetObject((int)Obj.Car_FireTruck).transform
                                .DORotateQuaternion(_defaultRotationQuatMap[(int)Obj.Car_FireTruck], 0.5f);
                            GetObject((int)Obj.Car_FireTruck).transform.DOMove(_defaultPosMap[(int)Obj.Car_FireTruck], 0.5f);
                        });
                    
                    GetObject((int)Obj.Car_FireTruck).transform.DOShakeScale(1, new Vector3(0.1f, 0.1f, 0.1f), 10, 90);
                    GetObject((int)Obj.CarName_FireTruck).transform.DOShakeScale(1, new Vector3(0.05f, 0.05f, 0.05f), 10, 90)
                        .OnComplete(() =>
                        {
                            GetObject((int)Obj.CarName_FireTruck).transform
                                .DORotateQuaternion(_defaultRotationQuatMap[(int)Obj.CarName_FireTruck], 0.5f);
                            GetObject((int)Obj.CarName_FireTruck).transform.DOMove(
                                _defaultPosMap[(int)Obj.CarName_FireTruck], 0.5f);
                            
                            GetObject((int)Obj.TMP_FireTruck).transform
                                .DORotateQuaternion(_defaultRotationQuatMap[(int)Obj.TMP_FireTruck], 0.5f);
                            GetObject((int)Obj.TMP_FireTruck).transform.DOMove(
                                _defaultPosMap[(int)Obj.TMP_FireTruck], 0.5f);
                        });;
                }
                if (obj == (int)Obj.CarName_Ambulance)
                {
                    _isClickableMap.TryAdd((int)Obj.CarName_Ambulance, true);
                    if (!_isClickableMap[(int)Obj.CarName_Ambulance]) return;
                    _isClickableMap[(int)Obj.CarName_Ambulance] = false;
                    DOVirtual.DelayedCall(0.7f, () =>
                    {
                        _isClickableMap[(int)Obj.CarName_Ambulance] = true;
                    });
                    
                    
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA018/Name_Ambulance");
                    GetObject((int)Obj.Car_Ambulance).transform.DOShakeRotation(1.5f, strenth, 10, 90)
                        .OnComplete(() =>
                        {
                            GetObject((int)Obj.Car_Ambulance).transform
                                .DORotateQuaternion(_defaultRotationQuatMap[(int)Obj.Car_Ambulance], 0.5f);
                            GetObject((int)Obj.Car_Ambulance).transform.DOMove(
                                _defaultPosMap[(int)Obj.Car_Ambulance], 0.5f);
                        });
                    
                    GetObject((int)Obj.Car_Ambulance).transform.DOShakeScale(1, new Vector3(0.1f, 0.1f, 0.1f), 10, 90);
                    GetObject((int)Obj.CarName_Ambulance).transform.DOShakeScale(1, new Vector3(0.05f, 0.05f, 0.05f), 10, 90)
                        .OnComplete(() =>
                        {
                            GetObject((int)Obj.CarName_Ambulance).transform.rotation =
                                _defaultRotationQuatMap[(int)Obj.CarName_Ambulance];

                            GetObject((int)Obj.CarName_Ambulance).transform.position =_defaultPosMap[(int)Obj.CarName_Ambulance];
                            
                            GetObject((int)Obj.TMP_Ambulance).transform
                                .DORotateQuaternion(_defaultRotationQuatMap[(int)Obj.TMP_Ambulance], 0.5f);
                            GetObject((int)Obj.TMP_Ambulance).transform.DOMove(
                                _defaultPosMap[(int)Obj.TMP_Ambulance], 0.5f);
                        });
                }
                
            }
        }
    }

    private void OnFinishInit()
    {
        
           
        GetObject((int)Obj.CarName_Police).SetActive(true);
        GetObject((int)Obj.CarName_FireTruck).SetActive(true);
        GetObject((int)Obj.CarName_Ambulance).SetActive(true);
         _defaultPosMap.Add((int)Obj.CarName_Police,GetObject((int)Obj.CarName_Police).transform.position);
         _defaultPosMap.Add((int)Obj.CarName_FireTruck, GetObject((int)Obj.CarName_FireTruck).transform.position);
         _defaultPosMap.Add((int)Obj.CarName_Ambulance, GetObject((int)Obj.CarName_Ambulance).transform.position);
   
        GetObject((int)Obj.CarName_Police).GetComponent<SpriteRenderer>().DOFade(0,0.001f);
        GetObject((int)Obj.CarName_FireTruck).GetComponent<SpriteRenderer>().DOFade(0,0.001f);
        GetObject((int)Obj.CarName_Ambulance).GetComponent<SpriteRenderer>().DOFade(0,0.001f);
         
        GetObject((int)Obj.CarName_Police).GetComponent<SpriteRenderer>().DOFade(1,1f);
        GetObject((int)Obj.CarName_FireTruck).GetComponent<SpriteRenderer>().DOFade(1,1f);
        GetObject((int)Obj.CarName_Ambulance).GetComponent<SpriteRenderer>().DOFade(1,1f);
         
        GetObject((int)Obj.CarName_Police).GetComponent<Collider>().enabled =true;
        GetObject((int)Obj.CarName_FireTruck).GetComponent<Collider>().enabled =true;
        GetObject((int)Obj.CarName_Ambulance).GetComponent<Collider>().enabled =true;
        
        GetObject((int)Obj.Car_PoliceCar).GetComponent<Collider>().enabled =true;
        GetObject((int)Obj.Car_FireTruck).GetComponent<Collider>().enabled =true;
        GetObject((int)Obj.Car_Ambulance).GetComponent<Collider>().enabled =true;

        DOVirtual.DelayedCall(6, () =>
        {
            _isClickableOnFinish = true;
            mainAnimator.enabled = false;
        });
    }
    

    private const int SEAT_COUNT = 7;

    private readonly Dictionary<int, Animator[]> _subPartsAnimMap = new();

    private readonly Dictionary<int, Animator> tfIDToAnimatorMap = new();

    // 첫번쨰 Sprite이미지는 WholePicture로 사용 중
    // 1.whole 2~8 sub parts sprite Images
    private readonly Dictionary<int, SpriteRenderer[]> _spritesMap = new();
    private Dictionary<int,SpriteRenderer> _tfIdToSpriteMap = new();

    private SpriteRenderer _spriteBgRenderer;

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

    private void ActivateSprites(int car)
    {
        GetObject((int)Obj.Sprites_Ambulance).SetActive(false);
        GetObject((int)Obj.Sprites_PoliceCar).SetActive(false);
        GetObject((int)Obj.Sprites_FireTruck).SetActive(false);
        
        GetObject(car).SetActive(true);
    }
    public enum SubPartsAnim
    {
        Default,
        Seperated,
        Completing
    }

    public int CurrentMainMainSeq
    {
        get
        {
            return CurrentMainMainSequence;
        }
        set
        {
            CurrentMainMainSequence = value;

            //  Messenger.Default.Publish(new EA012Payload(_currentMainSequence.ToString()));
            Logger.ContentTestLog($"Current Sequence: {CurrentMainMainSeq.ToString()}");


            // commin Init Part.
            ChangeThemeSeqAnim(value);

            switch (value)
            {
                case (int)MainSeq.SeatSelection:
                    AnimateAllSeats();
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA018/Narration/OnSeatSelection");
                    ChangeThemeSeqAnim(value);
                    break;

                
                case (int)MainSeq.Ambulance_Intro:
                    OnCarIntro((int)Obj.Sprites_Ambulance);
                    
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA018/Narration/Intro_Ambulance");
                    Managers.Sound.Play(SoundManager.Sound.Effect,"EA018/Siren_Ambulance");
                    
                    _uiManager.PopFromZeroInstructionUI("각 부품을 터치해서 구급차를 완성시켜요!");
                   
                    DOVirtual.DelayedCall(Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length + 1,
                        () =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Narration,"EA018/Narration/PutTogether_Ambulance");   
                        });
;                    break;
                case (int)MainSeq.Ambulance_Outro:
                    _uiManager.PopFromZeroInstructionUI("구급차를 완성했어!");
                    
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA018/Narration/Outro_Ambulance");
                    Managers.Sound.Play(SoundManager.Sound.Effect,"EA018/Siren_Ambulance");
          
                    
                    OnOutro((int)Obj.Sprites_Ambulance);
                    break;
                
                
                case (int)MainSeq.PoliceCar_Intro:
                    Logger.ContentTestLog("OnPoliceCarIntro -------------------");
                    OnCarIntro((int)Obj.Sprites_PoliceCar);
                    
                    
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA018/Narration/Intro_PoliceCar");
                    Managers.Sound.Play(SoundManager.Sound.Effect,"EA018/Siren_PoliceCar");
                    DOVirtual.DelayedCall(Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length + 1,
                        () =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Narration,"EA018/Narration/PutTogether_PoliceCar");   
                        });
                    _uiManager.PopFromZeroInstructionUI("이번엔, 각 부품을 터치해서 경찰차를 완성시켜요!");
                    break;
                case (int)MainSeq.PoliceCar_Outro:
                    
                    _uiManager.PopFromZeroInstructionUI("경찰차를 완성! 출동!");
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA018/Narration/Outro_PoliceCar");
                    Managers.Sound.Play(SoundManager.Sound.Effect,"EA018/Siren_PoliceCar");
                    OnOutro((int)Obj.Sprites_PoliceCar);
                    break;

                
                case (int)MainSeq.FireTruck_Intro:
                    _uiManager.PopFromZeroInstructionUI("마지막으로, 각 부품을 터치해서 소방차를 완성시켜요!");
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA018/Narration/Intro_FireTruck");
                    Managers.Sound.Play(SoundManager.Sound.Effect,"EA018/Siren_FireTruck");
                    OnCarIntro((int)Obj.Sprites_FireTruck);
                    
                    DOVirtual.DelayedCall(Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length + 1,
                        () =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Narration,"EA018/Narration/PutTogether_FireTruck");   
                        });
                    
                    break;
                case (int)MainSeq.FireTruck_Outro:
                    _uiManager.PopFromZeroInstructionUI("소방차를 완성! 출동!");
                    
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA018/Narration/Outro_FireTruck");
                    Managers.Sound.Play(SoundManager.Sound.Effect,"EA018/Siren_FireTruck");
                    OnOutro((int)Obj.Sprites_FireTruck);
                    break;

                case (int)MainSeq.OnFinish:
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA018/Narration/OnFinish");
                    _uiManager.PopFromZeroInstructionUI("친구들 덕분에 도와줄 수 있었어요!\n고마워요!");
     
                    OnFinishInit();
                    break;
            }
        }
    }

    private void AnimateAllSeats()
    {
        for (int i = (int)Obj.Seat_A; i <= (int)Obj.Seat_G; i++)
        {
            Logger.ContentTestLog($"AnimateAllSeats :Animating seat {(Obj)i}");
            AnimateSeatLoop((Obj)i);
        }
    }

    private void AnimateSeatLoop(Obj seat)
    {
        var SeatTransform = GetObject((int)seat).transform;

        _sequenceMap[(int)seat]?.Kill();
        _sequenceMap[(int)seat] = DOTween.Sequence();
        _sequenceMap[(int)seat]
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat] * 1.1f, 0.25f))
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat] * 0.9f, 0.35f))
            .SetLoops(-1, LoopType.Yoyo)
            .OnKill(() =>
            {
                SeatTransform.DOScale(_defaultSizeMap[(int)seat], 1);
            });

        _sequenceMap[(int)seat].Play();
    }


    private void OnOutro(int round)
    {
        
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA018/Car_OnOutroA");
        var currentSprite = _spritesMap[round];
        _spritesMap[round][0].DOFade(0f, 2f);
        
        _spriteBgRenderer.DOFade(0, 1f).OnComplete(() =>
        {
        });
        
        _spriteBgRenderer.DOFade(0f, 1.5f)
            .OnComplete(() =>
            {
                foreach (var sprite in _spritesMap[(int)round])
                {
                    sprite.DOFade(0, 2f);
                }
            });
        
        DOVirtual.DelayedCall(12, () =>
        {

            if (round == (int)MainSeq.FireTruck_Outro)
            {
                CurrentMainMainSeq = (int)MainSeq.OnFinish;
            }

            else
            {
                CurrentMainMainSeq++;
            }
           
            isClickable = true;

        });

    }

    protected override void Init()
    {
        DOTween.SetTweensCapacity(1000, 1000);
        BindObject(typeof(Obj));

        var ambulAnims = GetObject((int)Obj.Sprites_Ambulance).GetComponentsInChildren<Animator>(false);

        _subPartsAnimMap.Add((int)Obj.Sprites_Ambulance, ambulAnims);

        var policeAnims = GetObject((int)Obj.Sprites_PoliceCar).GetComponentsInChildren<Animator>(false);
        _subPartsAnimMap.Add((int)Obj.Sprites_PoliceCar, policeAnims);

        var fireAnims = GetObject((int)Obj.Sprites_FireTruck).GetComponentsInChildren<Animator>(false);
        _subPartsAnimMap.Add((int)Obj.Sprites_FireTruck, fireAnims);


        var Amb_Sprites =
            GetObject((int)Obj.Sprites_Ambulance).GetComponentsInChildren<SpriteRenderer>(false);
        _spritesMap.Add((int)Obj.Sprites_Ambulance, Amb_Sprites);

        var policeSprites =
            GetObject((int)Obj.Sprites_PoliceCar).GetComponentsInChildren<SpriteRenderer>(false);
        _spritesMap.Add((int)Obj.Sprites_PoliceCar, policeSprites);

        var Sprites_FireTruck =
            GetObject((int)Obj.Sprites_FireTruck).GetComponentsInChildren<SpriteRenderer>(false);
        _spritesMap.Add((int)Obj.Sprites_FireTruck, Sprites_FireTruck);


        //For RaySync Control. 
        foreach (int key in _subPartsAnimMap.Keys.ToArray())
        foreach (var animator in _subPartsAnimMap[key])
            tfIDToAnimatorMap.Add(animator.transform.GetInstanceID(), animator);

        SetAllAnimators(false);
        SetAllSprites(false);

        foreach(var sprite in _spritesMap)
        {
            foreach (var spriteRenderer in sprite.Value)
            {
                _tfIdToSpriteMap.Add(spriteRenderer.transform.GetInstanceID(), spriteRenderer);
            }
        }
      


        psResourcePath = "Runtime/EA018/Fx_Click";

        _spriteBgRenderer = GetObject((int)Obj.SpriteBg).GetComponent<SpriteRenderer>();
        _spriteBgRenderer.DOFade(0, 0.0001f);
        base.Init();

        _uiManager = UIManagerObj.GetComponent<EA018_UIManager>();

        for (int i = (int)Obj.Seat_A; i < (int)Obj.Seat_A + SEAT_COUNT; i++) isSeatClickedMap.Add(i, false);
        
        GetObject((int)Obj.Car_PoliceCar).GetComponent<Collider>().enabled =false;
        GetObject((int)Obj.Car_FireTruck).GetComponent<Collider>().enabled =false;
        GetObject((int)Obj.Car_Ambulance).GetComponent<Collider>().enabled =false;
        
        GetObject((int)Obj.CarName_Police).GetComponent<SpriteRenderer>().DOFade(0,0.001f);
        GetObject((int)Obj.CarName_FireTruck).GetComponent<SpriteRenderer>().DOFade(0,0.001f);
        GetObject((int)Obj.CarName_Ambulance).GetComponent<SpriteRenderer>().DOFade(0,0.001f);
        
        GetObject((int)Obj.CarName_Police).SetActive(false);   
        GetObject((int)Obj.CarName_FireTruck).SetActive(false);
        GetObject((int)Obj.CarName_Ambulance).SetActive(false);
    }

    private void SetAnimatorStatus(int index, bool isActive = false)
    {
        foreach (var animator in _subPartsAnimMap[index])
            animator.enabled = isActive;
    }

    private void SetSpriteStatus(int index, bool isActive = false)
    {
        Logger.ContentTestLog($"{(Obj)index} is turned on?: {isActive}");

        foreach (var sprite in _spritesMap[index])
        {
            sprite.enabled = isActive;
            Logger.ContentTestLog($"{sprite.name} is turned on?: {isActive}");
        }
    }

    private void SetAllAnimators(bool isActive)
    {
        foreach (int key in _subPartsAnimMap.Keys.ToArray())
        foreach (var animator in _subPartsAnimMap[key])
            animator.enabled = isActive;
    }

    private void SetAllSprites(bool isActive)
    {
        foreach (int key in _spritesMap.Keys.ToArray())
        foreach (var sprite in _spritesMap[key])
            sprite.enabled = isActive;
    }

    
    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();

        DOVirtual.DelayedCall(1.5f, () =>
        {
            initialMessage = "먼저 친구들,\n각자 표시된 자리에 앉아주세요!";
            _uiManagerCommonBehaviorController.ShowInitialMessage(initialMessage);

            CurrentMainMainSeq = (int)MainSeq.SeatSelection;


            Logger.ContentTestLog("Mainseq Changed SeatSelection -------------------");
        });
    }

    private readonly Dictionary<int, bool> isSeatClickedMap = new();
    private readonly Dictionary<int, MeshRenderer> _seatMeshRendererMap = new();
    private int _seatClickedCount = 1;

    [SerializeField] private Color _selectedColor;

    private void OnRaySyncedOnSeatSelection()
    {
        bool isAllSeatClicked = true;
        foreach (var hit in GameManager_Hits)
        {
            int hitTransformID = hit.transform.GetInstanceID();
            if (hit.transform.gameObject.name.Contains("Seat"))
            {
                if (isSeatClickedMap[_tfIdToEnumMap[hitTransformID]]) return;
                isSeatClickedMap[_tfIdToEnumMap[hitTransformID]] = true;

                var renderer = hit.transform.GetComponent<MeshRenderer>();
                _seatMeshRendererMap.TryAdd(_tfIdToEnumMap[hitTransformID], renderer);
                _seatMeshRendererMap[_tfIdToEnumMap[hitTransformID]].material.DOColor(_selectedColor, 0.35f);

                Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Seat_" + _seatClickedCount);
                _seatClickedCount++;


                _sequenceMap[_tfIdToEnumMap[hitTransformID]]?.Kill();

                foreach (int key in isSeatClickedMap.Keys)
                    if (!isSeatClickedMap[key])
                        isAllSeatClicked = false;

                if (isAllSeatClicked)
                {
                    Logger.ContentTestLog("모든 자리가 선택되었습니다--------");

                    // Messenger.Default.Publish(new EA012Payload("OnSeatSelectFinished"));
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA018/Narration/OnSeatSelectFinished");
                    _uiManager.PopFromZeroInstructionUI("다 앉았구나! 이제 자동차들을 보러가자!");
                    DOVirtual.DelayedCall(4, () =>
                    {
                        CurrentMainMainSeq = (int)MainSeq.Ambulance_Intro;
                    });
                    break;
                }


                PlayParticleEffect(hit.point);
            }
        }
    }

    #region Subparts Movingpart------------------

    private void OnintroForSprites()
    {
        foreach (int key in _subPartsAnimMap.Keys.ToArray())
        foreach (var animator in _subPartsAnimMap[key])
            animator.enabled = false;
    }

    private bool _isPartsClickable;

    private int currentArrivedSubPartCount;
    private const int MAX_SUBPART_COUNT = 7;

    private const int COUNT_TO_COMPLETE = 10;
    private readonly Dictionary<int, int> _partProgress = new(); // 각 파트의 클릭 진행 상태
    private readonly Dictionary<int, bool> isArrivedMap = new();
    private bool isClickable = true;
    private Color defaultColor = Color.white;

    private bool _isRoundFinished =true;

    private void OnRoundFinished()
    {
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA018/OnComplete");
        isClickable = false;
        _isRoundFinished = true; 
        CurrentMainMainSeq++;
        currentArrivedSubPartCount = 0;
    }

    private void MoveTowardToCompletion()
    {
        if (!isClickable) return;


        foreach (var hit in GameManager_Hits)
        {
            int ID = hit.transform.GetInstanceID();


            _isClickableMap.TryAdd(ID, true);
            if (_isClickableMap[ID] == false) continue;
            _isClickableMap[ID] = false;
            
            DOVirtual.DelayedCall(0.12f, () =>
            {
                if (!isArrivedMap.ContainsKey(ID) || !isArrivedMap[ID])
                    _isClickableMap[ID] = true;
            });

            // 현재 진행도 저장 및 증가
            isArrivedMap.TryAdd(ID, false);
            if (isArrivedMap[ID]) return;

            if (tfIDToAnimatorMap.TryGetValue(ID, out var animator))
            {
               
                var tf = animator.transform;
                // OnCompletion 클립 가져오기
                _defaultSizeMap.TryAdd(ID, tf.localScale);
                _defaultRotationQuatMap.TryAdd(ID, tf.localRotation);


                var clip = animator.runtimeAnimatorController.animationClips
                    .FirstOrDefault(c => c.name.Contains("Complete"));

                if (clip != null)
                {
                    _partProgress.TryAdd(ID, 0);
                    _partProgress[ID] = Mathf.Min(_partProgress[ID] + 1, COUNT_TO_COMPLETE);
                    

                    float progressNormalized = _partProgress[ID] / (float)COUNT_TO_COMPLETE;

                    //도착시 로직 
                    if (!isArrivedMap[ID] && _partProgress[ID] >= COUNT_TO_COMPLETE)
                    {
                        isArrivedMap[ID] = true;
                        
                        char randoChar = (char)Random.Range('A', 'B' + 1);
                        Managers.Sound.Play(SoundManager.Sound.Effect, "EA018/OnPartArrive_" + randoChar);
                  
                        
                        currentArrivedSubPartCount++;

                        _sequenceMap[ID]?.Kill();
                        _sequenceMap[ID] = DOTween.Sequence();
           
                        _sequenceMap[ID].Append(tf.transform.DOScale(_defaultSizeMap[ID] * 1.2f, 0.25f)
                            .SetEase(Ease.OutBounce));
                        _sequenceMap[ID].Join(tf.transform.DOLocalRotate(Vector3.zero, 0.35f));
                        _sequenceMap[ID].Append(tf.transform.DOScale(_defaultSizeMap[ID] * 0.8f, 0.2f)
                            .SetEase(Ease.OutBounce));
                        _sequenceMap[ID].Append(tf.transform.DOScale(_defaultSizeMap[ID] * 1.2f, 0.25f)
                            .SetEase(Ease.OutBounce));
                        _sequenceMap[ID].Append(tf.transform.DOScale(_defaultSizeMap[ID], 0.5f));
                       
                        _sequenceMap[ID].AppendCallback(() =>
                        {
                            _isClickableMap[ID] = false;
                            animator.enabled = true;
                            _tfIdToSpriteMap[ID].DOFade(0.8f, 0.75f);
                        });

                  
                       
                        Logger.ContentTestLog($"sub part Arrived {currentArrivedSubPartCount}");

                        if (currentArrivedSubPartCount >= MAX_SUBPART_COUNT)
                        {
                            Logger.ContentTestLog("All sub parts Arrived -------------------");
                            OnRoundFinished();
                        }
                
                    }
                    else 
                    {
                        PlayParticleEffect(hit.point);

                        char randoChar = (char)Random.Range('A', 'E' + 1);
                        Managers.Sound.Play(SoundManager.Sound.Effect, "EA018/Click_" + randoChar);
                        // Animator 재생
                        animator.enabled = true;
                        animator.Play("Complete", 0, progressNormalized);

                        // 재생 잠깐 유지 후 정지
                        DOVirtual.DelayedCall(0.1f, () =>
                        {
                            animator.enabled = false;


                            
                            _sequenceMap.TryAdd(ID, DOTween.Sequence());
                            _sequenceMap[ID]?.Kill();
                            _sequenceMap[ID] = DOTween.Sequence();


                            int colorSeqID = ID + 1234;
                            _sequenceMap.TryAdd(colorSeqID, DOTween.Sequence());
                            _sequenceMap[colorSeqID]?.Kill();
                            _sequenceMap[colorSeqID] = DOTween.Sequence();
                            
                            _sequenceMap[colorSeqID]
                                .Append(_tfIdToSpriteMap[ID].DOColor(_tfIdToSpriteMap[ID].color * 1.5f, 0.28f));
                            _sequenceMap[colorSeqID]
                                .Append(_tfIdToSpriteMap[ID].DOColor(_tfIdToSpriteMap[ID].color * 0.9f, 0.28f));
                            _sequenceMap[colorSeqID].SetLoops(1);
                            _sequenceMap[colorSeqID].Append(_tfIdToSpriteMap[ID].DOColor(defaultColor, 0.1f));
                            _sequenceMap[colorSeqID].OnKill(() =>
                            {
                             _tfIdToSpriteMap[ID].DOColor(defaultColor, 0.001f);
                            });
                         
                            int effectType = Random.Range(0, 3); // 0~2 랜덤

                            var originalRotation = tfIDToAnimatorMap[ID].transform.localRotation;
                            switch (effectType)
                            {
                                case 0: // Shake
                                    _sequenceMap[ID]
                                        .Append(tf.DOShakePosition(0.5f, new Vector3(Random.Range(0.1f,0.5f), Random.Range(0.1f,0.5f), Random.Range(0.1f,0.5f))).OnKill(() =>
                                        {
                                            tf.localRotation = _defaultRotationQuatMap[ID];
                                        }));
                                    break;
                                case 1: // Scale Up & Down
                                    _sequenceMap[ID].Append(tf.DOScale(_defaultSizeMap[ID] * 1.2f, 0.30f)
                                            .SetLoops(2, LoopType.Yoyo))
                                        .OnKill(() =>
                                        {
                                            tf.localRotation = _defaultRotationQuatMap[ID];
                                        });
                                    break;
                                case 2: // Rotate
                                    float angle = Random.Range(-20f, 20f);

                                    _sequenceMap[ID].Append(tf
                                        .DORotateQuaternion(_defaultRotationQuatMap[ID]* new Quaternion(0, 0, Random.Range(-15,15), 0), 0.25f)
                                        .SetLoops(2, LoopType.Yoyo)
                                        .SetEase(Ease.InOutSine));
                                    break;
                            }
                        });
                    }
                }
                else
                    Logger.LogWarning("Clip is null");
            }
        }


    }

    #if UNITY_EDITOR
    [SerializeField]
    [Range(0,50)]
    private float timeLimit = 50;
    #else
        private float timeLimit = 60;
    #endif
    private float _elapsed = 0f;
    private void Update()
    {
        if (_isRoundFinished)
        {
            return;
        }
        
        _elapsed+= Time.deltaTime;
        
        if( _elapsed > timeLimit)
        {

            _elapsed = 0f;
            Logger.ContentTestLog("Time Limit Exceeded -------------------");
            
            
            DOVirtual.DelayedCall(1f, () =>
            {
                OnRoundFinished();
            });
            foreach (var key in tfIDToAnimatorMap.Keys.ToArray())
            {
                tfIDToAnimatorMap[key].enabled = true;
                tfIDToAnimatorMap[key].Play("Complete", 0);
            }
        }
 
    }

    private EA018_UIManager _uiManager;

    #endregion

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (CurrentMainMainSeq == (int)MainSeq.SeatSelection)
            OnRaySyncedOnSeatSelection();
        else if (CurrentMainMainSeq == (int)MainSeq.OnFinish)
        {
            OnRaySyncedOnFinish();
        }
        else MoveTowardToCompletion();
    }

    #region AmbulancePart

    private void OnCarIntro(int introCar)
    {
        ActivateSprites(introCar);

        SetAnimatorStatus(introCar);
        SetSpriteStatus(introCar, true);
        foreach (var sprite in _spritesMap[introCar]) sprite.DOFade(0, 0.001f);

        DOVirtual.DelayedCall(0.5f, () =>
        {

            int count = 0;
            _spriteBgRenderer.DOFade(1f, 1.7f)
                .OnComplete(() =>
                {
                    foreach (var sprite in _spritesMap[introCar])
                    {
                        if (count != 0) sprite.DOFade(1, 1f);
                        count++;
                    }
                });
        
            _spritesMap[introCar][0].DOFade(0.10f, 2f);
        });


        DOVirtual.DelayedCall(1f, () =>
        {
            _elapsed = 0; 
            _isRoundFinished = false;
        });

        Logger.ContentTestLog("OnPoliceCarIntro -------------------");
        //   _isPartsClickable = true;
    }

    private void OnAmbulancePartIntro()
    {
        SetAnimatorStatus((int)Obj.Sprites_Ambulance, true);
        SetSpriteStatus((int)Obj.Sprites_Ambulance, true);
    }

    #endregion
}