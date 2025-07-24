using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;
using Random = UnityEngine.Random;

public class EA012_GameManager : Base_GameManager
{

    public enum GameObj
    {
        Seats, // 자리 전체 참조용
        Cars, // Vehicle 전체 참조용
        Wheels, // Wheel 전체 참조용
        Seat_A,
        Seat_B,
        Seat_C,
        Seat_D,
        Seat_E,
        Seat_F,
        Seat_G,
        
        Car_Ambulance,
        Car_PoliceCar,
        Car_FireTruck,
        Car_Bus,
        Car_Taxi,
        
        Building_Ambulance,
        Building_PoliceCar,
        Building_FireTruck,
        Building_Taxi,
        Building_Bus,
        PosGroup,
        PosA,
        PosB,
        PosC,
        PosD,
        PosE,
        PosF,
        PosG,
        PosH,
        PosI,
        PosJ,
        PosK,
        PosL,
        PosM,
        OutA,
        OutB,
        OutC,
        OutPosGroup,
        HelpMoveBtns,
        BtnA,
        BtnB,
        BtnC,
        BtnD,
        BtnE,
        BtnF,
        BtnG,
      
    }

    public enum MainSeq
    {
        Default=-1,
        SeatSelection,
        SeqB_Ambulance,
        SeqB_Ambulance_Move,
        SeqC_PoliceCar,
        SeqC_PoliceCar_Move,
        SeqD_FireTruck,
        SeqD_FireTruck_Move,
        SeqE_Taxi,
        SeqE_Taxi_Move,
        SeqF_Bus,
        SeqF_Bus_Move,

        CarMoveHelpFinished = 100,
        Finished = -123
    }

    public enum CarAnimSeq
    {
        Ambulance_Appear=1,
        Ambulance_Leave,
        PoliceCar_Appear,
        PoliceCar_Leave,
        FireTruck_Appear,
        FireTruck_Leave,
        Taxi_Appear,
        Taxi_Leave,
        Bus_Appear,
        Bus_Leave,
        Finished =-123
    }

    public enum TireNum
    {
        Ambulance,
        PoliceCar,
        FireTruck,
        Taxi,
        Bus,   
    }

    private bool _isClickableByAnimationOrNarration = true;//나레이션 재생 중 혹은 여러 기타요인으로 클릭을 막을경우 총괄
    public bool isClickableByAnimationOrNarration
    {
        get
        {
            return _isClickableByAnimationOrNarration;
        }
        set
        {
            _isClickableByAnimationOrNarration = value;
        }
    }//나레이션 재생 중 혹은 여러 기타요인으로 클릭을 막을경우 총괄
    private Animator _carAnimator;
    private int MainAnimFinished = -123;
    private int CAR_ANIM_ON = Animator.StringToHash("CarAnimOn");
    private int CAR_ANIM_OFF = Animator.StringToHash("CarAnimOff");
    [SerializeField]
    [Range(0,20)]
    private float TirePathDurationMin = 4f;
    [SerializeField]
    [Range(0,20)]
    private float TirePathDurationMax = 8f;
    
    private const int SEAT_COUNT = 7;
    private const int CAR_COUNT = 5;
    [SerializeField]
    [Range(0,20)]
    private int WHEEL_COUNT_TO_REMOVE = 20;
    private int currentRemovedTireCount;
    private int None_CAR_ANIM = 100;
    private int CAR_ANIM_DEFAULT = 0;

    private int _currentSelectedSeatCount;
    private int _currentCarSeqCount;
    private Dictionary<int, Collider> _helpMoveBtnColliderMap = new();
    
  //  private Dictionary<int,Material> _materialMap = new();
    private Dictionary<int,bool> isSeatClickedMap = new();
    private Dictionary<int, Dictionary<int,Transform>> tireGroupMap = new();
    private Dictionary<int, Dictionary<int,Sequence>> tireSeqMap = new();

    

    private void ActivateBuilding(int buldingindex)
    {
        DOVirtual.DelayedCall(1f,() =>
        {
            GetObject((int)GameObj.Building_Ambulance).SetActive(false);
            GetObject((int)GameObj.Building_PoliceCar).SetActive(false);
            GetObject((int)GameObj.Building_FireTruck).SetActive(false);
            GetObject((int)GameObj.Building_Bus).SetActive(false);
            GetObject((int)GameObj.Building_Taxi).SetActive(false);
            
            GetObject(buldingindex).SetActive(true);
            GetObject(buldingindex).transform.DOScale(0, 0.01f);
            GetObject(buldingindex).transform.DOScale(_defaultSizeMap[buldingindex], 1f);
        });

        
    }

    #region Main Seq (카메라 조작)---------------------------

    

   
    public  MainSeq currentMainSeq
    {
        get => _currentMainSeq;
        set
        {
           
            _currentMainSeq = value;
            
            Messenger.Default.Publish(new EA012Payload(_currentMainSeq.ToString()));
            
            Logger.ContentTestLog($"Current Sequence: {currentMainSeq.ToString()}");



            // commin Init Part.
          
            
            switch (value)
            {
                case MainSeq.Default:
                    break;
                case MainSeq.Finished:
                    ChangeThemeSeqAnim((int)value);

                    //Default로 인해 파라미터 조절 실패하는경우 주의. 현재 딜레이로 해결중 
                    DOVirtual.DelayedCall(1.5f, () =>
                    {
                        _carAnimator.enabled = true;
                        _carAnimator.SetInteger(CAR_NUM ,(int)CarAnimSeq.Finished);
                        
                        DOVirtual.DelayedCall(12f, () =>
                        {
                            isClickableOnFinish = true;
                            _carAnimator.enabled = false;
                        });

                    });
                    
                    RestartScene(delay:20);

                  
        
                    break;
                case MainSeq.SeatSelection:
                    AnimateAllSeats();
                    ChangeThemeSeqAnim((int)value);
                    break;
                case MainSeq.SeqB_Ambulance:
                    StartRollingTire((int)TireNum.Ambulance);
                    ActivateBuilding((int)GameObj.Building_Ambulance);
                    ChangeThemeSeqAnim((int)value);
                    break;
                case MainSeq.SeqC_PoliceCar:
                    StartRollingTire((int)TireNum.PoliceCar);
                    ActivateBuilding((int)GameObj.Building_PoliceCar);
                    ChangeThemeSeqAnim((int)value);
                    break;
                case MainSeq.SeqD_FireTruck:
                    StartRollingTire((int)TireNum.FireTruck);
                    ActivateBuilding((int)GameObj.Building_FireTruck);
                    ChangeThemeSeqAnim((int)value);
                    break;
                case MainSeq.SeqE_Taxi:
                    StartRollingTire((int)TireNum.Taxi);
                    ActivateBuilding((int)GameObj.Building_Taxi);
                    ChangeThemeSeqAnim((int)value);
                    break;
                case MainSeq.SeqF_Bus:
                    StartRollingTire((int)TireNum.Bus);
                    ActivateBuilding((int)GameObj.Building_Bus);
                    ChangeThemeSeqAnim((int)value);
                    break;

                case MainSeq.CarMoveHelpFinished:
                    ChangeThemeSeqAnim((int)value);
                    break;
                
                
                case MainSeq.SeqB_Ambulance_Move:
                
                    _isCarMoveFinished = false;
                    _carAnimator.enabled = false;
                    currentCarAnimSeq = CarAnimSeq.Ambulance_Appear;
                    // SetHelpCarMoveBtnStatus();
                    //  AnimateAllCarMoveHelpBtns();
                    break;
                case MainSeq.SeqC_PoliceCar_Move:
                    
                    _isCarMoveFinished = false;
                    _carAnimator.enabled = false;
                    currentCarAnimSeq = CarAnimSeq.PoliceCar_Appear;
                    //                  SetHelpCarMoveBtnStatus();
//                    AnimateAllCarMoveHelpBtns();
                    break;
                case MainSeq.SeqD_FireTruck_Move:
                 
                    _isCarMoveFinished = false;
                     _carAnimator.enabled = false;
                    currentCarAnimSeq = CarAnimSeq.FireTruck_Appear;

                    break;
                case MainSeq.SeqE_Taxi_Move:
                   
                    _isCarMoveFinished = false;
                     _carAnimator.enabled = false;
                    currentCarAnimSeq = CarAnimSeq.Taxi_Appear;
                    //                  SetHelpCarMoveBtnStatus();
//                    AnimateAllCarMoveHelpBtns();
                    break;
                case MainSeq.SeqF_Bus_Move:
                   
                    _isCarMoveFinished = false;
                     _carAnimator.enabled = false;
                    currentCarAnimSeq = CarAnimSeq.Bus_Appear;
                    //                  SetHelpCarMoveBtnStatus();
             //      AnimateAllCarMoveHelpBtns();
                    break;
            }
        }
    }
    
    #endregion


    private Tween clickableTween;
    public void SetClickableWithDelayOfNar(float delayAmount)
    {

        isClickableByAnimationOrNarration = false;
        clickableTween?.Kill();
        
        clickableTween = DOVirtual.DelayedCall(delayAmount, () =>
        {
            Logger.ContentTestLog("[HelpBtn] 클릭 가능 상태입니다.");
            isClickableByAnimationOrNarration = true;
        });
        clickableTween.OnKill(() =>
        {
            Logger.ContentTestLog("click delay Killed..false again");
        });
         
        Logger.ContentTestLog($" {Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.name} : 클릭불가 상태 지속시간 : {delayAmount}");
    }

    private void OnTireRemovalFinished()
    {
        _isCarMoveFinished = false;
        SetAllHelpCarMoveBtnStatus();
        AnimateAllCarMoveHelpBtns();
   
        
    }
    private MainSeq _currentMainSeq = MainSeq.Default;

    protected override void Init()
    {
        SHADOW_MAX_DISTANCE = 60;
        PsResourcePath = "Runtime/SortedByScene/EA012/Fx_Click";
        DOTween.SetTweensCapacity(500,1300);
        base.Init();
        BindObject(typeof(GameObj));
        Messenger.Default.Publish(new EA012Payload(nameof(MainSeq.Default)));
        
        for (int i = (int)GameObj.Seat_A; i < (int)GameObj.Seat_A + SEAT_COUNT; i++)
        {
            isSeatClickedMap.Add(i, false);
        }

        _carAnimator = GetObject((int)GameObj.Cars).GetComponent<Animator>();
        SetTireGroupDictionary();
        for (int i = (int)GameObj.BtnA; i <= (int)GameObj.BtnG; i++)
        {
            Logger.ContentTestLog($"AnimateAllBtns :Animating seat {(GameObj)i}");
            GetObject(i).transform.localScale = Vector3.zero;
            _helpMoveBtnColliderMap.Add(i,GetObject(i).GetComponentInChildren<Collider>());
        }
    }

#if UNITY_EDITOR
    [SerializeField]
    private MainSeq startSeq;
#else
     private MainSeq startSeq = MainSeq.SeatSelection;
#endif
    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();
        currentMainSeq = startSeq;
    }


    public override void OnRaySynced()
    {
        if (currentMainSeq == MainSeq.SeatSelection)
        {
            OnRaySyncedOnSeatSelection();
        }
        else if (currentMainSeq == MainSeq.SeqB_Ambulance)
        {
            OnRaySyncedOnTireSelection((int)TireNum.Ambulance);
        }
        else if (currentMainSeq == MainSeq.SeqC_PoliceCar)
        {
            OnRaySyncedOnTireSelection((int)TireNum.PoliceCar);
        }
        else if (currentMainSeq == MainSeq.SeqD_FireTruck)
        {
            OnRaySyncedOnTireSelection((int)TireNum.FireTruck);
        }
        else if (currentMainSeq == MainSeq.SeqE_Taxi)
        {
            OnRaySyncedOnTireSelection((int)TireNum.Taxi);
        }
        else if (currentMainSeq == MainSeq.SeqF_Bus)
        {
            OnRaySyncedOnTireSelection((int)TireNum.Bus);
        }
        
        
        
        else if (currentMainSeq == MainSeq.SeqB_Ambulance_Move)
        {
            OnRaySyncOnHelpCarMovePart(GameObj.Car_Ambulance);
        }
        else if (currentMainSeq == MainSeq.SeqC_PoliceCar_Move)
        {
            OnRaySyncOnHelpCarMovePart(GameObj.Car_PoliceCar);
        }
        else if (currentMainSeq == MainSeq.SeqD_FireTruck_Move)
        {
            OnRaySyncOnHelpCarMovePart(GameObj.Car_FireTruck);
        }
        else if (currentMainSeq == MainSeq.SeqE_Taxi_Move)
        {
            OnRaySyncOnHelpCarMovePart(GameObj.Car_Taxi);
        }
        else if (currentMainSeq == MainSeq.SeqF_Bus_Move)
        {
            OnRaySyncOnHelpCarMovePart(GameObj.Car_Bus);
        }
            
        else if (currentMainSeq == MainSeq.Finished)
        {
            OnRaySyncOnFinish();
            //_carAnimator.enabled = false;
           
        }
    }

    private bool isClickableOnFinish = false;
    private Base_UIManager _uiManager;
    private void OnRaySyncOnFinish()
    {

        if (!isClickableOnFinish) return;

        if (_uiManager == null)
            _uiManager = UIManagerObj.GetComponent<Base_UIManager>();
        foreach (var hit in GameManager_Hits)
        {
            int id = hit.transform.GetInstanceID();


            switch (_tfIdToEnumMap[id])
            {
                case (int)GameObj.Car_Ambulance:
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_Ambulance");
                    GetObject((int)GameObj.Car_Ambulance).transform.localScale =_defaultSizeMap[(int)GameObj.Car_Ambulance];
                    hit.transform.DOShakeScale(1, 0.4f);
                    _uiManager.PopInstructionUIFromScaleZero("구급차");
                    break;
                case (int)GameObj.Car_PoliceCar:
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_PoliceCar");
                        GetObject((int)GameObj.Car_PoliceCar).transform.localScale =_defaultSizeMap[(int)GameObj.Car_PoliceCar];
                    hit.transform.DOShakeScale(1, 0.4f);
                    _uiManager.PopInstructionUIFromScaleZero("경찰차");
                    break;
                case (int)GameObj.Car_FireTruck:
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_FireTruck");
                        GetObject((int)GameObj.Car_FireTruck).transform.localScale =_defaultSizeMap[(int)GameObj.Car_FireTruck];
                    _uiManager.PopInstructionUIFromScaleZero("소방차");
                    hit.transform.DOShakeScale(1, 0.4f);
                    break;
                case (int)GameObj.Car_Bus:
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_Bus");
                        GetObject((int)GameObj.Car_Bus).transform.localScale =_defaultSizeMap[(int)GameObj.Car_Bus];
                    _uiManager.PopInstructionUIFromScaleZero("버스");
                    hit.transform.DOShakeScale(1, 0.4f);
                    break;
                case (int)GameObj.Car_Taxi:
                    _uiManager.PopInstructionUIFromScaleZero("택시");
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA012/Narration/Name_Taxi");
                        GetObject((int)GameObj.Car_Taxi).transform.localScale =_defaultSizeMap[(int)GameObj.Car_Taxi];
                    hit.transform.DOShakeScale(1, 0.4f);
                    break;
            }
         
        }
    }
    #region Seat Selection Part
    
    int TIRE_GROUPCOUNT = 5;
    private static readonly int CAR_NUM = Animator.StringToHash("CarNum");

    private void SetTireGroupDictionary()
    {
        char tireGroupName = 'A'; //ABCD순으로 증가하여 폴더 prefab 순회
        
        for (int i = 0; i < TIRE_GROUPCOUNT; i++)
        {
            var newDict = new Dictionary<int, Transform>();
            var newSeqMap = new Dictionary<int, Sequence>();
            
            tireGroupMap.Add(i, newDict);
            tireSeqMap.Add(i, newSeqMap);
            var prefab = Resources.Load<GameObject>("Runtime/SortedByScene/EA012/Wheel_" + tireGroupName);

          
            for (int tire = 0; tire < WHEEL_COUNT_TO_REMOVE; tire++)
            {
                var tirePrefab = Instantiate(prefab).transform;
                
                _isClickedMapByTfID.Add(tirePrefab.transform.GetInstanceID(), false);
                tireGroupMap[i].Add(tire, tirePrefab);
                tireSeqMap[i].Add(tirePrefab.transform.GetInstanceID(),DOTween.Sequence());
                tirePrefab.gameObject.SetActive(false);
            }

            tireGroupName++;
        }
    }
    
    private void StartRollingTire(int currentTireGroup)
    {
        if (!tireSeqMap.ContainsKey(currentTireGroup))
            tireSeqMap[currentTireGroup] = new Dictionary<int, Sequence>();
        DOVirtual.DelayedCall(2f, () =>
        {
            _isTireRemovalFinished = false;
        });
      
        Managers.Sound.Play(SoundManager.Sound.Effect,"EA012/TireRoll");
        for (int i = 0; i < WHEEL_COUNT_TO_REMOVE; i++)
        {
            var tire = tireGroupMap[currentTireGroup][i];
            var tireID = tire.GetInstanceID();
            
            // 기존 시퀀스가 있다면 정리
            if (tireSeqMap[currentTireGroup].ContainsKey(tireID))
            {
                tireSeqMap[currentTireGroup][tireID]?.Kill();
                tireSeqMap[currentTireGroup].Remove(tireID);
            }

            Sequence seq = DOTween.Sequence();
            tireSeqMap[currentTireGroup][tireID] = seq;
            
            // 위치 초기화
            int startIndex = Random.Range((int)GameObj.OutA, (int)GameObj.OutC + 1);
            Transform startTransform = GetObject(startIndex).transform;
            tire.position = startTransform.position;
            tire.rotation = startTransform.rotation;

            int destinationIndex = Random.Range((int)GameObj.PosA, (int)GameObj.PosM + 1);
            Transform destinationTransform = GetObject(destinationIndex).transform;

            List<Vector3> loopPoints = new() { destinationTransform.position };
            if (destinationIndex > (int)GameObj.PosA)
                loopPoints.Add(GetObject(destinationIndex - 1).transform.position);
            if (destinationIndex < (int)GameObj.PosM)
                loopPoints.Add(GetObject(destinationIndex + 1).transform.position);
            loopPoints = loopPoints.OrderBy(_ => Random.value).ToList();

            // 시퀀스 생성

            seq.Append(tire.DOMove(destinationTransform.position, Random.Range(1.2f,1.9f)).SetEase(Ease.InOutSine));
            seq.Append(tire.DOPath(loopPoints.ToArray(), Random.Range(TirePathDurationMin,TirePathDurationMax), PathType.CatmullRom)
                .SetDelay(Random.Range(0.2f, 0.5f))
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.05f)
                .SetLoops(100, LoopType.Yoyo));

            tire.DOLocalRotate(new Vector3(0, 0, 360f), Random.Range(1.2f,2f), RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(100, LoopType.Incremental);

            tire.transform.gameObject.SetActive(true);
            tire.transform.localScale = 2 * Vector3.one;
            // 시퀀스 저장

        }
    }

    #endregion

    private int _seatClickedCount =1;
    private Dictionary<int,MeshRenderer> _seatMeshRendererMap = new();
    [SerializeField]
    Color _defaultColor;
    
    [FormerlySerializedAs("_selectedFilter")] [SerializeField] 
    private Color _selectedColor;
    
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
                
                MeshRenderer renderer = hit.transform.GetComponent<MeshRenderer>();
                _seatMeshRendererMap.TryAdd(_tfIdToEnumMap[hitTransformID], renderer);
                _seatMeshRendererMap[_tfIdToEnumMap[hitTransformID]].material.DOColor(_selectedColor, 0.35f);
                
                Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Seat_" + _seatClickedCount);
                _seatClickedCount++;

                
                _sequencePerEnumMap[_tfIdToEnumMap[hitTransformID]]?.Kill();

                foreach (int key in isSeatClickedMap.Keys)
                    if (!isSeatClickedMap[key])
                        isAllSeatClicked = false;

                if (isAllSeatClicked)
                {
                    Logger.ContentTestLog("모든 자리가 선택되었습니다--------");

                    DeactivateSeats();
                    
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Common/OnAllSeatSelected");
                    Messenger.Default.Publish(new EA012Payload("OnSeatSelectFinished"));

                    DOVirtual.DelayedCall(4, () =>
                    {
                        currentMainSeq = MainSeq.SeqB_Ambulance;
                    });
                    break;
                }


                PlayParticleEffect(hit.point);
            }
        }
    }
    
    
    private bool _isTireRemovalFinished;
    #region 타이어 및 제거 파트

    private void OnRaySyncedOnTireSelection(int currentTireGroup)
    {
        //Logger.ContentTestLog($"{(TireNum)currentTireGroup} : 현재 타이어 그룹 ");
        foreach (var hit in GameManager_Hits)
        {
            if (!hit.transform.gameObject.name.Contains("Wheel")) continue;
            else
            {
                PlayParticleEffect(hit.point);
                Logger.ContentTestLog($"Tire clicked: {hit.transform.name}");

                var clickedTire = hit.transform;
                int clickedTransformID = clickedTire.GetInstanceID();

                // 🔐 Prevent duplicate clicks
                if (!_isClickedMapByTfID.TryGetValue(clickedTransformID, out bool wasClicked) || wasClicked)
                {
                    Logger.ContentTestLog($"이미 클릭된 타이어이거나 ID가 존재하지 않음: clicked? {wasClicked} :{clickedTransformID}");
                    continue;
                }

                _isClickedMapByTfID[clickedTransformID] = true;
                currentRemovedTireCount++;

                // 🔥 Kill and remove existing sequence
                if (tireSeqMap[currentTireGroup].TryGetValue(clickedTransformID, out Sequence existingSeq))
                {
                    existingSeq.Kill();
                    tireSeqMap[currentTireGroup].Remove(clickedTransformID);
                }
                else
                {
                    Logger.ContentTestLog($"tire관련 key 없음: 그룹 {currentTireGroup}, 키 {clickedTransformID}");
                }

                // 🎵 Play sound
                char randomChar = (char)Random.Range('A', 'D' + 1);
                Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Click" + randomChar);

                // 🌀 Animate disappearance
                clickedTire.DOScale(Vector3.zero, 0.35f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        clickedTire.gameObject.SetActive(false);
                    });

              
            }

            
        }

        if (!_isTireRemovalFinished && currentRemovedTireCount >= WHEEL_COUNT_TO_REMOVE )
        {
          
            currentRemovedTireCount = 0;
           // CurrentSeq = Seq.WheelSelectFinished;
           //group*2는단순 수적관계임 주의 ---------------------------------
            Logger.ContentTestLog($"모든 타이어 제거됨--------------------Caranim:{(currentTireGroup*2)+1}{(CarAnimSeq)(currentTireGroup*2)+1}------");
            Messenger.Default.Publish(new EA012Payload("AllTiresRemoved"));
            _carAnimator.SetInteger(CAR_NUM ,currentTireGroup*2 + 1);
          
            
            
           
            Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/OnCarAppear");
          
            DOVirtual.DelayedCall(0.2f,()=>
            {
                _isTireRemovalFinished = true;
                Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/CarArrival");
            });
            
            //타이어가 제거된 후에 차량이 나타나는 시퀀스. 너무 짧으면 차량 애니메이션이 안보임
            DOVirtual.DelayedCall(6.5f, () =>
            {
                currentMainSeq++;
                Logger.ContentTestLog($"current seq -> {currentMainSeq} -------------------");
                OnTireRemovalFinished();
            });
           
        }
        else
        {
            Logger.ContentTestLog($"남은타이어 개수 {currentRemovedTireCount}/ {WHEEL_COUNT_TO_REMOVE}");
        }
    }

    #endregion
    
    private void AnimateAllSeats()
    {
        for (int i = (int)GameObj.Seat_A; i <= (int)GameObj.Seat_G; i++)
        {
            Logger.ContentTestLog($"AnimateAllSeats :Animating seat {(GameObj)i}");
            AnimateSeatLoop((GameObj)i);
        }
    }

    private void SetAllHelpCarMoveBtnStatus(bool isActive=true)
    {
        
        // helpbtnmove 클릭시 초기화 로직 한번에 진행 ----------------------------------------------
        if (isActive)
        {
            foreach (var key in _helpMoveBtnColliderMap.Keys.ToArray())
            {
                _helpMoveBtnColliderMap[key].enabled =false;
            }
            
            for (int i = (int)GameObj.BtnA; i <= (int)GameObj.BtnG; i++)
            {

                if (i == (int)GameObj.BtnA)
                {
                    _helpMoveBtnColliderMap[(int)GameObj.BtnA].enabled =true;
                   // AnimateSeatLoopSelectively(GameObj.BtnA);
                }
                
         //       Logger.ContentTestLog($"AnimateAllBtns :Animating seat {(GameObj)i}");
        //        GetObject(i).transform.DOScale(_defaultSizeMap[i], 0.25f);
            }
        }
        else
        {
            for (int i = (int)GameObj.BtnA; i <= (int)GameObj.BtnG; i++)
            {
                
//                Logger.ContentTestLog($"AnimateAllBtns :Animating seat {(GameObj)i}");
                var i1 = i;
                GetObject(i).transform.DOScale(0, 0.25f).OnComplete(() =>
                {
                    GetObject(i1).SetActive(false);
                });
            }     
         
        }
           
        
    }
    
    private void KillHelpBtns()
    {
        for (int i = (int)GameObj.BtnA; i <= (int)GameObj.BtnG; i++)
        {
            Logger.ContentTestLog($"AnimateAllBtns :Animating seat {(GameObj)i}");
            
            _sequencePerEnumMap[(int)i]?.Kill();
            _sequencePerEnumMap[(int)i] = DOTween.Sequence();
        }
        SetAllHelpCarMoveBtnStatus(false);
    }

    private Sequence _helpMoveBtnIntroSeq;

    private void AnimateAllCarMoveHelpBtns()
    {
        //차가 버튼으로움직일땐 비활성화 -----------------
        _helpMoveBtnIntroSeq?.Kill();
        _helpMoveBtnIntroSeq = DOTween.Sequence();


        //GetObject((int)GameObj.Car_Ambulance).
        for (int i = (int)GameObj.BtnA; i <= (int)GameObj.BtnG; i++)
        {
            GetObject(i).SetActive(true);

            _helpMoveBtnIntroSeq.AppendCallback(() =>
            {
                //GetObject(i).transform.localScale = Vector3.zero;
            });
        }

        _helpMoveBtnIntroSeq.AppendCallback(() =>
        {
            _helpMoveBtnIntroSeq
                .Append(GetObject((int)GameObj.BtnA).transform.DOScale(_defaultSizeMap[(int)GameObj.BtnA], 0.25f));
            AnimateSeatLoopSelectively(GameObj.BtnA);
        });
    }

    private void AnimateSeatLoopSelectively(GameObj seat)
    {
        if (!_sequencePerEnumMap.ContainsKey((int)seat)) return; 
        
        
        var SeatTransform = GetObject((int)seat).transform;
        _sequencePerEnumMap[(int)seat]?.Kill();
        _sequencePerEnumMap[(int)seat] = DOTween.Sequence();
        _sequencePerEnumMap[(int)seat]
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat]*1.1f, 0.25f))
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat]*0.9f, 0.35f))
            .SetLoops(-1,LoopType.Yoyo)
            .OnKill(()=>
            {
                SeatTransform.DOScale(_defaultSizeMap[(int)seat], 1);
            });

        _sequencePerEnumMap[(int)seat].Play();
    }
    private void AnimateSeatLoop(GameObj seat)
    {
        var SeatTransform = GetObject((int)seat).transform;
        
        _sequencePerEnumMap[(int)seat]?.Kill();
        _sequencePerEnumMap[(int)seat] = DOTween.Sequence();
        _sequencePerEnumMap[(int)seat]
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat]*1.1f, 0.25f))
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat]*0.9f, 0.35f))
            .SetLoops(-1,LoopType.Yoyo)
            .OnKill(()=>
            {
                SeatTransform.DOScale(_defaultSizeMap[(int)seat], 1);
            });
        
        _sequencePerEnumMap[(int)seat].Play();
    }

    private void DeactivateSeats()
    {
        for (int i = (int)GameObj.Seat_A; i <= (int)GameObj.Seat_G; i++)
        {
            _sequencePerEnumMap[i]?.Kill();
        
        
        }


        TweenCallback _scaleCallback = () =>
        {
            for (int i = (int)GameObj.Seat_A; i <= (int)GameObj.Seat_G; i++)
            {
                var SeatTransform = GetObject(i).transform;
                _sequencePerEnumMap[i] = DOTween.Sequence();
                _sequencePerEnumMap[i].Append(SeatTransform.DOScale(Vector3.zero, 0.75f));
            }
        };

        DOVirtual.DelayedCall(1f, _scaleCallback);

    }

    private CarAnimSeq currentCarAnimSeq;
    public bool isLastCar
    {
        get;
        private set;
    }
   // private MainSeq currentMainSeq;
    private bool _isCarMoveFinished;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    #region 자동차 옮겨주기 파트
    
    private void OnRaySyncOnHelpCarMovePart(GameObj currentCar)
    {
        if (!_isClickableByAnimationOrNarration)
        {
            Logger.ContentTestLog("[HelpBtn] 클릭 불가 상태입니다. ----- 대기 필요 ");
            return;
        }

        foreach (var hit in GameManager_Hits)
        {
            PlayParticleEffect(hit.point);
        }
        foreach (var hit in GameManager_Hits)
            if (hit.transform.name.Contains("BtnA")) OnHelpBtnClicked(GameObj.BtnA     ,currentCar);
            else if (hit.transform.name.Contains("BtnB")) OnHelpBtnClicked(GameObj.BtnB,currentCar);
            else if (hit.transform.name.Contains("BtnC")) OnHelpBtnClicked(GameObj.BtnC,currentCar);
            else if (hit.transform.name.Contains("BtnD")) OnHelpBtnClicked(GameObj.BtnD,currentCar);
            else if (hit.transform.name.Contains("BtnE")) OnHelpBtnClicked(GameObj.BtnE,currentCar);
            else if (hit.transform.name.Contains("BtnF")) OnHelpBtnClicked(GameObj.BtnF,currentCar);
            else if (!_isCarMoveFinished&&hit.transform.name.Contains("BtnG"))
            {
                
                if(_isCarMoveFinished) return;
                {
                    //중복실행방지
                }
                _isCarMoveFinished = true;
                OnHelpBtnClicked(GameObj.BtnG, currentCar);
                
                Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/OnCarAppear");
               
                DOVirtual.DelayedCall(1.5f,()=>
                {
                    OnHelpMoveFinisehd();


                    currentMainSeq = MainSeq.CarMoveHelpFinished;
                    currentCarAnimSeq++;
                    // SetInteger first? 
                    _carAnimator.enabled = true;
                    _carAnimator.SetInteger(CAR_NUM,(int)currentCarAnimSeq);
                    Logger.ContentTestLog($"{(int)currentCarAnimSeq} : {currentCarAnimSeq} : 현재 차량 애니메이션 시퀀스----------------");
                 
                    
                    switch (currentCarAnimSeq)
                    {
                        case CarAnimSeq.Ambulance_Leave :
                             Messenger.Default.Publish(new EA012Payload("Arrival","구급차"));
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Narration/Ambulance_Arrival");
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Siren_Ambulance");
                            
                            break;
                        
                        case CarAnimSeq.PoliceCar_Leave:
                             Messenger.Default.Publish(new EA012Payload("Arrival","경찰차"));
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Narration/PoliceCar_Arrival");
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Siren_PoliceCar");
                             
                            break;
                        
                        case CarAnimSeq.FireTruck_Leave:
                             Messenger.Default.Publish(new EA012Payload("Arrival","소방차"));
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Narration/FireTruck_Arrival");
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Siren_FireTruck");
                             break;
                      
                        case CarAnimSeq.Taxi_Leave:
                             Messenger.Default.Publish(new EA012Payload("Arrival","택시"));
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Narration/Taxi_Arrival");
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Taxi_Honk");
                             break;
                        
                        case CarAnimSeq.Bus_Leave:
                             Messenger.Default.Publish(new EA012Payload("Arrival","버스"));
                             isLastCar = true;
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Narration/Bus_Arrival");
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Bus_Honk");
                             break;
                    }

                });
               

                // 차량 옮겨주는 애니메이션 종료 후 다음 시퀀스로 이동 및 초기화 진행 ----------------------------------------------
                DOVirtual.DelayedCall(12f, () =>
                {
                      
                   
                        
                    switch (currentCarAnimSeq)
                    {
                        case CarAnimSeq.Ambulance_Leave :
                            currentMainSeq = MainSeq.SeqC_PoliceCar;
                            break;
                        
                        case CarAnimSeq.PoliceCar_Leave:
                            _isCarMoveFinished = false;
                            currentMainSeq = MainSeq.SeqD_FireTruck;
                            break;
                        
                        case CarAnimSeq.FireTruck_Leave:
                            currentMainSeq = MainSeq.SeqE_Taxi;
                            break;
                      
                        case CarAnimSeq.Taxi_Leave:
                            _isCarMoveFinished = false;
                            currentMainSeq = MainSeq.SeqF_Bus;
                            break;
                        
                        case CarAnimSeq.Bus_Leave:
                            _isCarMoveFinished = false;
                            
                            currentMainSeq = MainSeq.Finished;
                            break;
                    }
              
                    _carAnimator.SetInteger(CAR_NUM, CAR_ANIM_DEFAULT);
                    Logger.ContentTestLog($"car anim seq {currentCarAnimSeq} 다음 로직으로 이동 ----------currentMainSeq : {currentMainSeq}");
         
                });
            }
    }

private void OnHelpMoveFinisehd()
{
    KillHelpBtns();
    
 //   currentMainSeq++;
}


private void OnHelpBtnClicked(GameObj clickedBtn, GameObj currentCar)
{
    // BtnG일 경우 종료
    if (clickedBtn > GameObj.BtnG)
        // Logger.ContentTestLog("[HelpBtn] BtnG 도달 → CarAnim Seq 2 설정됨");
        return;


 
    //C인경우 재생안되도록 설계됨. NextHelpMOveBtnC 파일 넣지 않은상태가 정상동작
    char randomCharAB = (char)Random.Range('A', 'F' + 1);
    Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Narration/NextHelpMoveBtn" + randomCharAB);

    char randomChar = (char)Random.Range('A', 'D' + 1);
    Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Click" + randomChar);

    Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/CarMove");

    _sequencePerEnumMap[(int)clickedBtn]?.Kill();
    _sequencePerEnumMap[(int)clickedBtn] = DOTween.Sequence();

    DOVirtual.DelayedCall(1f, () =>
    {
        GetObject((int)clickedBtn).transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InOutElastic);
    });
    AnimateSeatLoopSelectively(clickedBtn + 1);


    //     _carAnimator.SetTrigger(CAR_ANIM_OFF);
    if (_helpMoveBtnColliderMap.ContainsKey((int)clickedBtn)) _helpMoveBtnColliderMap[(int)clickedBtn].enabled = false;
    if (_helpMoveBtnColliderMap.ContainsKey((int)clickedBtn + 1))
        _helpMoveBtnColliderMap[(int)clickedBtn + 1].enabled = true;

    int currentIdx = (int)clickedBtn - 1;
    int nextIdx = currentIdx + 1;


    var car = GetObject((int)currentCar).transform;
    var start = clickedBtn == GameObj.BtnA
        ? GetObject((int)currentCar).transform.position
        : GetObject(currentIdx).transform.position;
    var end = clickedBtn == GameObj.BtnA
        ? GetObject((int)GameObj.BtnA).transform.position
        : GetObject(nextIdx).transform.position;
    var mid = Vector3.Lerp(start, end, 0.5f) + Vector3.up * 0.5f;

    Vector3[] path = { start, mid, end };

    car.DOPath(path, 1.5f, PathType.CatmullRom)
        .SetEase(Ease.InOutSine)
        .SetLookAt(0.1f, Vector3.forward)
        .OnComplete(() =>
        {
            Logger.ContentTestLog($"[HelpBtn] 차량 이동 완료: {clickedBtn} → {(GameObj)nextIdx}");
        });
}

#endregion
}




public interface IPayload{};
public class EA012Payload : IPayload
{
    public string Narration { get; }
    public string CurrentCarName { get; }

    public EA012Payload(string narration,string carname ="car")
    {
        Narration = narration;
        CurrentCarName = carname;
    }
}
