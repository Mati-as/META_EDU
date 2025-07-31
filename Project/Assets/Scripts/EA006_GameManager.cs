using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EA006_GameManager : Ex_BaseGameManager
{
    public enum MainSeq
    {
        Default,
        GrassColorChange,
        FindScarecrow,
        SparrowAppear,
        OnFinish
    }

    private enum Obj
    {
        WheatGroup_A,
        WheatGroup_B,
        WheatGroup_C,
        WheatGroup_D,
        WheatGroup_E,
            
        ScareCrowA,
        ScareCrowB,
        ScareCrowC,
        ScareCrowD,
        ScareCrowE,
        ScareCrowF,
        ScareCrowG,
        ScareCrowH,
        ScareCrowI,
        ScareCrowJ,
        
        SparrowA,
        SparrowB,
        SparrowC,
        SparrowD,
        SparrowE,
        Fx_OnSuccess
    }
    
    private enum AnimationName
    {
       ToA=1,
       ToB,
       ToC,
       ToD,
       ToE,
       OutA =11,
       OutB,
       OutC,
    }

    private enum AnimationAction
    {
       Fly,
       Eat,
    }

    public static event Action<int> SeqMessageEvent;
    public static event Action<int> SparrowCountEvent;
    private EA006_UIManager _uiManager;

    public int CurrentThemeMainSequence
    {
        get
        {
            return base.currentMainMainSequence;
        }
        set
        {
        
            base.currentMainMainSequence = value;
            
            Logger.Log($"Sequence Changed--------{(MainSeq)base.currentMainMainSequence} : {value}");
            SeqMessageEvent?.Invoke(base.currentMainMainSequence);
            SetWheatColliderStatus();

            //값반영 지연으로 value대신 _currentThemeSequence 사용하면 안됩니다 XXX
            switch (value)
            {
                case (int)MainSeq.GrassColorChange:
                    Logger.ContentTestLog($"풀 색상 변경 모드 시작 {(MainSeq)base.currentMainMainSequence} : {value}");
                    currentChangedCount = 0;
                    ChangeThemeSeqAnim((int)MainSeq.GrassColorChange);
                    ResetClickable();
                    SetWheatColliderStatus(true);
                    break;
                
                case (int)MainSeq.FindScarecrow:
                    TIME_LIMIT = 90;
                    _elapsedTime = 0;
                    Logger.ContentTestLog($"허수아비 모드 시작 {(MainSeq)base.currentMainMainSequence} : {value}");
                    SetWheatColliderStatus(false);
                    ChangeThemeSeqAnim((int)MainSeq.FindScarecrow);
                    ResetClickable();
                    OnScareCrowFindStart();
                    Managers.Sound.Play(SoundManager.Sound.Bgm,"Bgm/EA008");
                    break;

                case (int)MainSeq.SparrowAppear:
                    TIME_LIMIT = 60;
                    _elapsedTime = 0;
                    ResetClickable(false);//순서주의 
                    AppearSparrow(4);
                    ChangeThemeSeqAnim((int)MainSeq.SparrowAppear);
                    Logger.ContentTestLog($"참새 모드 시작 {(MainSeq)base.currentMainMainSequence} : {value}");
                    Managers.Sound.Play(SoundManager.Sound.Bgm,"Bgm/EA006",0.1f);

                    break;
                
                case (int)MainSeq.OnFinish:
                    ChangeThemeSeqAnim((int)MainSeq.OnFinish);
                    RestartScene(delay:11);
                    break;
                
                case (int)MainSeq.Default:
//                    Logger.LogError("시퀀스 에러--------------확인필요");
                    break;
            }
            
          
        }
    }
    protected Dictionary<int,bool> _isSeqActiveMap = new(); 

    private readonly Dictionary<int, Collider> colliderMap = new(); //group별임에 주의

    private ParticleSystem onSuccess;
    private readonly Dictionary<int, MeshRenderer[]> _mRendererMap = new(); //group별임에 주의
    private Material _defaultMat;
    private Material _changedMat; //클릭시 변경될 색상
    private readonly int COLOR_A_CHANGED = Shader.PropertyToID("_Color1");
    private readonly int COLOR_B_CHANGED = Shader.PropertyToID("_Color2");
   
  
    private Color _targetColorA;
    private Color _targetColorB;
    private int currentChangedCount = 0;
    private readonly int TargetColorChangedCount = 5;
    
    private int _currentScarecrowCount = 0;
    private readonly int TargetScarecrowCount = 10;
    
    private int _currentSparrowCount = 0;
    private readonly int SPARROW_CATCH_TARGET_COUNT = 30;
    private readonly int ANIM_NUM = Animator.StringToHash("animNum");
    private readonly int ANIM_ACTION = Animator.StringToHash("animAction");
    private void SetWheatColliderStatus(bool isOn = false)
    {
        for (int i = (int)Obj.WheatGroup_A; i <= (int)Obj.WheatGroup_E; i++) colliderMap[i].enabled = isOn;
    }

   
    protected override void Init()
    {
        base.Init();
        _uiManager = UIManagerObj.GetComponent<EA006_UIManager>();
        psResourcePath = "Runtime/EA006/FX_leaves";
        BindObject(typeof(Obj));

        _defaultMat = Resources.Load<Material>("Runtime/EA006/EA006_WheatA");
        _changedMat = Resources.Load<Material>("Runtime/EA006/EA006_WheatA_Changed");

        _targetColorA = _changedMat.GetColor(COLOR_A_CHANGED);
        _targetColorB = _changedMat.GetColor(COLOR_B_CHANGED);


        Debug.Assert(_changedMat != null, _defaultMat + " != null");
        for (int i = (int)Obj.WheatGroup_A; i <= (int)Obj.WheatGroup_E; i++)
        {
            _mRendererMap[i] = new MeshRenderer[123];
            _mRendererMap[i] = GetObject(i).GetComponentsInChildren<MeshRenderer>();

            colliderMap[i] = GetObject(i).GetComponent<Collider>();

            foreach (var meshRenderer in _mRendererMap[i])
            {
                var newMat = _defaultMat;
                meshRenderer.material = newMat;
            }
        }


        for (int i = (int)Obj.ScareCrowA; i <= (int)Obj.ScareCrowJ; i++)
        {
            GetObject(i).transform.localScale = Vector3.zero;
            GetObject(i).SetActive(false);
        }
        
        for (int i = (int)Obj.SparrowA; i <= (int)Obj.SparrowE; i++)
        {
            
            GetObject(i).SetActive(false);
                  
        }
        
        onSuccess = GetObject((int)Obj.Fx_OnSuccess).GetComponent<ParticleSystem>();
        
#if !UNITY_EDITOR
       TIME_LIMIT = 90; //허수아비 찾기 시간제한
#endif

    }
    
    
#if UNITY_EDITOR
    [SerializeField]
    private MainSeq seq;
#else
    private MainSeq seq = MainSeq.GrassColorChange;
#endif

    protected override void OnGameStartButtonClicked()
    {
        DOVirtual.DelayedCall(1f, () =>
        {

            CurrentThemeMainSequence =(int)seq;
        });
    }

    public override void OnRaySynced()
    {
        foreach (var hit in GameManager_Hits)
            switch (CurrentThemeMainSequence)
            {
                case (int)MainSeq.Default:
                    break;
                case (int)MainSeq.GrassColorChange:
                
                    OnRaySyncedOnRiceFieldColorChange(hit);
                 
                    break;
                case (int)MainSeq.FindScarecrow:
                   
                    
                    OnRaySyncedOnScareCrowFind(hit);
                
                    break;
                case (int)MainSeq.SparrowAppear:
                  
                    OnRaySyncedOnSparrow(hit);
                    break;
                default:
                    Logger.ContentTestLog("no raysynced---------");
                    break;
            }
    }

    #region A)풀색상 변화 파트 -----------------------------------------------------------
    
    private void OnRaySyncedOnRiceFieldColorChange(RaycastHit hit)
    {
        int id = hit.transform.GetInstanceID();
        if (!_tfIdToEnumMap.ContainsKey(id))
        {
            Logger.ContentTestLog("there's no wheat group");
            return;
        }
        
        var clickedWheatGroup = (Obj)_tfIdToEnumMap[id];
        
        if (!_isClickableMapByTfID[id]) return;
        _isClickableMapByTfID[id] = false;
        
        switch (clickedWheatGroup)
        {
            case Obj.WheatGroup_A:
                ApplyMaterialTween(clickedWheatGroup);
                break;
            case Obj.WheatGroup_B:
                ApplyMaterialTween(clickedWheatGroup);
                break;
            case Obj.WheatGroup_C:
                ApplyMaterialTween(clickedWheatGroup);
                break;
            case Obj.WheatGroup_D:
                ApplyMaterialTween(clickedWheatGroup);
                break;
            case Obj.WheatGroup_E:
                ApplyMaterialTween(clickedWheatGroup);
                break;
        }
        
        PlayRandomClickSound();
        PlayParticleEffect(hit.point);
        
        _isClickableMapByTfID[id] = false;
    }

    private void ApplyMaterialTween(Obj group)
    {
     
        foreach (var meshRenderer in _mRendererMap[(int)group])
        {
            var mat = meshRenderer.material;

            // 색상에 약간의 랜덤 편차 추가
            var randColorA = _targetColorA * Random.Range(0.95f, 1.05f);
            var randColorB = _targetColorB * Random.Range(0.90f, 1.05f);

            // 트위닝 적용
            mat.DOColor(randColorA, COLOR_A_CHANGED, 1f);
            mat.DOColor(randColorB, COLOR_B_CHANGED, 1f);
        }

        
        currentChangedCount++;

        if (currentChangedCount >= TargetColorChangedCount)
        {
            onSuccess.Play();
            Managers.Sound.Play(SoundManager.Sound.Effect, "SortedByScene/EA006/OnSuccess");
            
            DOVirtual.DelayedCall(1.5f, () =>
            {
                Logger.ContentTestLog("currentChangedCount : " + currentChangedCount + " / " + TargetColorChangedCount);

                if (currentChangedCount >= TargetColorChangedCount)
                {
                    currentChangedCount = 0;
                    OnAllGrassColorChanged();
                   
                }
            });
        }
          
        else
        {
            Logger.ContentTestLog("currentChangedCount : " + currentChangedCount + " / " + TargetColorChangedCount);
        }
    }



    private void PlayRandomClickSound()
    {
        Logger.ContentTestLog($"SortedByScene/EA006/Click_{(char)Random.Range('A','F'+1)}");
        Managers.Sound.Play(SoundManager.Sound.Effect,"SortedByScene/EA006/Click_"+(char)Random.Range('A','F'+1));
    }
    private void OnAllGrassColorChanged()
    {
        DOVirtual.DelayedCall(2f, () =>
        {
            CurrentThemeMainSequence = (int)MainSeq.FindScarecrow;
        });
       
    }

    #endregion


    #region 허수아비 찾기 파트----------------------------------------------------


    [SerializeField]
    [Range(0, 90)]
    private int TIME_LIMIT;
    private float _elapsedTime;
    [FormerlySerializedAs("_feverModeTime")]
    [SerializeField]
    [Range(0, 70)]
    private float FEVER_MODE_TIME; //허수아비가 더 많이 등장하는 시간.  (fevermodeTime시간 초 이상 부터 더 많은 허수아비가 나옵니다.)
    private void OnScareCrowFindStart()
    {
        DOVirtual.DelayedCall(1.5f, () =>
        {
            AppearScareCrow();
            Managers.Sound.Play(SoundManager.Sound.Effect, "SortedByScene/EA006/Narration/Hello");
        });
    }

    private void AppearScareCrow(int Count = 2)
    {

        int randomPoss = Random.Range(0, 100);
        if (randomPoss > 70)
        {
            Managers.Sound.Play(SoundManager.Sound.Effect, "SortedByScene/EA006/Narration/Hello");
        }
     
        
        int SelectedScarecrow = -1; //sentinel val
        for (int i = 0; i < Count; i++)
        {
            int randoScareCrow = Random.Range((int)Obj.ScareCrowA, (int)Obj.ScareCrowJ);

            while (SelectedScarecrow == randoScareCrow)
                randoScareCrow = Random.Range((int)Obj.ScareCrowA, (int)Obj.ScareCrowJ);
            SelectedScarecrow = randoScareCrow;
            
            GetObject(randoScareCrow).SetActive(true);
            GetObject(randoScareCrow).transform.DOScale(_defaultSizeMap[randoScareCrow], 1f);
            
            _isClickableMapByTfID[_enumToTfIdMap[randoScareCrow]] = true;
            _sequencePerEnumMap[randoScareCrow]?.Kill();
            _sequencePerEnumMap[randoScareCrow] = DOTween.Sequence();
            _sequencePerEnumMap[randoScareCrow].Append(GetObject(randoScareCrow).transform.DOLocalRotateQuaternion(
                _defaultRotationQuatMap[randoScareCrow] *Quaternion.Euler( Random.Range(-5,-10),0, 0), 0.6f));

            _sequencePerEnumMap[randoScareCrow].Append(GetObject(randoScareCrow).transform.DOLocalRotateQuaternion(
                _defaultRotationQuatMap[randoScareCrow] * Quaternion.Euler(Random.Range(-5, 10), 0, 0), 0.6f));
            _sequencePerEnumMap[randoScareCrow].SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void KillAllScarecrows()
    {
        for (int i = (int)Obj.ScareCrowA; i <= (int)Obj.ScareCrowJ; i++)
        {
            _sequencePerEnumMap[i]?.Kill();
            _sequencePerEnumMap[i].Append(GetObject(i).transform.DOScale(Vector3.zero,Random.Range(1,1.5f)));
        }
    }

    private void OnRaySyncedOnScareCrowFind(RaycastHit hit)
    {

        // if (_currentScarecrowCount >= TargetScarecrowCount)
        // {
        //     Logger.ContentTestLog("허수아비 모두 찾음,, return");
        //     return;
        // }
        var id = hit.transform.GetInstanceID();
        if (!_tfIdToEnumMap.ContainsKey(id))
        {
            Logger.ContentTestLog("there's no scarecrow");
            return;
        }
        
        if (!_tfidTotransformMap.ContainsKey(id))
        {
            Logger.ContentTestLog($"there's no key {hit.transform.gameObject.name}:{id}");
            return;
        }

        if (!_isClickableMapByTfID[id])
        {
            Logger.ContentTestLog("isAlready Clicked");
            return;
        }
        _isClickableMapByTfID[id] = false;
        
              
        PlayRandomClickSound();
        PlayParticleEffect(hit.point);
        
        _sequencePerEnumMap[_tfIdToEnumMap[id]]?.Kill();
        _sequencePerEnumMap[_tfIdToEnumMap[id]] = DOTween.Sequence();
        _sequencePerEnumMap[_tfIdToEnumMap[id]].Append(_tfidTotransformMap[id].transform.
            DOLocalRotateQuaternion(_defaultRotationQuatMap[_tfIdToEnumMap[id]] *Quaternion.Euler(0, Random.Range(10,180), 0), Random.Range(0.5f, 0.75f)));
        _sequencePerEnumMap[_tfIdToEnumMap[id]].Append(_tfidTotransformMap[id].transform.DOScale(Vector3.zero, Random.Range(0.5f, 0.75f)).OnComplete(
            () =>
            {
                if (_elapsedTime >= FEVER_MODE_TIME)
                {
                    //FEVER_MODE_TIME이상일때는 허수아비가 더 많이 나옵니다.
                    Logger.ContentTestLog("FeverMode----------------------------------------------");
                    AppearScareCrow(Random.Range(1, 5));
                }
                else
                {
                    AppearScareCrow(1);
                }
                
            }));
        
        _currentScarecrowCount++;
        

    }
    
    

    #endregion

    private void Update()
    {
        if (CurrentThemeMainSequence == (int)MainSeq.FindScarecrow ||
            CurrentThemeMainSequence == (int)MainSeq.SparrowAppear)
        {
            _elapsedTime += Time.deltaTime;
        
            if (_elapsedTime > TIME_LIMIT)
            {
                _elapsedTime = 0;

                if (CurrentThemeMainSequence == (int)MainSeq.FindScarecrow)
                {
                    _currentScarecrowCount = 0;
                    DOVirtual.DelayedCall(1.5f, () =>
                    {
                        for (int i = (int)Obj.ScareCrowA; i <= (int)Obj.ScareCrowJ; i++)
                            GetObject(i).transform.DOScale(Vector3.zero, 1f).OnComplete(() =>
                            {
                                _sequencePerEnumMap[i]?.Kill();
                                GetObject(i).SetActive(false);
                            });
                    });
            
                    _uiManager.PopInstructionUIFromScaleZero("허수아비 아저씨를 다 찾았어요!");
                    Managers.Sound.Play(SoundManager.Sound.Narration,"Audio/SortedByScene/EA006/Narration/FoundAllScarecrow");

                    DOVirtual.DelayedCall(4f, () =>
                    {
                        KillAllScarecrows();
                        CurrentThemeMainSequence = (int)MainSeq.SparrowAppear;
                    });

                }
                else if(CurrentThemeMainSequence ==(int)MainSeq.SparrowAppear)
                {
                    OnSparrowSectionFinished();
                }
             
            }
        }
        
      
        
        
    }


    int previousSparrowPos = -1;

    private Dictionary<int, bool> _isPositionAvailMap = new() {

        {1, true},
        {2, true},
        {3, true},
        {4, true},
        {5, true},
    };
    private Dictionary<int, int> _sparrowCurrentPosMap = new() {

      
    };
    #region 참새파트------------------------------------------------------------

    private bool _isFirstAppear =true;

    private void AppearSparrow(int count = 1)
    {
        List<int> availableSparrowsEnum = new();

        for (int i = (int)Obj.SparrowA; i <= (int)Obj.SparrowE; i++)
        {
            int tfId = _enumToTfIdMap[i];

            // 클릭 가능 상태(false)
            if (!_isClickableMapByTfID[tfId])
                availableSparrowsEnum.Add(i);
        }

        if (availableSparrowsEnum.Count == 0)
        {
            Logger.ContentTestLog("❌ 등장 가능한 참새가 없습니다.");
            return;
        }

        // if (availableSparrowsEnum.Count < 3)
        // {
        //     Logger.ContentTestLog("❌❌❌❌❌ 참새수가 적어 두마리 생성 .");
        //     count = 2;
        // }

        for (int i = 0; i < count; i++)
        {
            int randIdx = Random.Range(0, availableSparrowsEnum.Count);
            int sparrowEnumToActivate = availableSparrowsEnum[randIdx];
            availableSparrowsEnum.RemoveAt(randIdx);
            _sparrowCurrentPosMap.TryAdd(sparrowEnumToActivate, -1);

            int indexCache = i;
           

            int randomSparrowPos;
            do
            {
                randomSparrowPos = Random.Range((int)AnimationName.ToA, (int)AnimationName.ToE + 1);
            } while (randomSparrowPos == previousSparrowPos
                     || !_isPositionAvailMap[randomSparrowPos]
                     || _sparrowCurrentPosMap[sparrowEnumToActivate] == randomSparrowPos
                     ||_isClickableMapByTfID[_enumToTfIdMap[sparrowEnumToActivate]]
                     );

            
            _sparrowCurrentPosMap[sparrowEnumToActivate] = randomSparrowPos;
           // _isPositionAvailMap[sparrowEnumToActivate] = false; //해당 위치는 사용불가로 변경
            _isPositionAvailMap[randomSparrowPos] = false; // ✅ 위치 기준으로 차
            previousSparrowPos = randomSparrowPos;
            _isClickableMapByTfID[_enumToTfIdMap[sparrowEnumToActivate]] = true;

            _animatorMap[sparrowEnumToActivate].SetInteger(ANIM_NUM, -1); //animator 초기화 
            
            int capturedEnum = sparrowEnumToActivate;
            // Logger.ContentTestLog($"🐦 참새 등장: {(Obj)sparrowEnumToActivate} / pos: {randomSparrowPos}");
            DOVirtual.DelayedCall(1f, () =>
            {
                GetObject(capturedEnum).SetActive(true);
                _animatorMap[capturedEnum].SetInteger(ANIM_NUM, randomSparrowPos);
                _animatorMap[capturedEnum].SetInteger(ANIM_ACTION, (int)AnimationAction.Eat);

                _sequencePerEnumMap[capturedEnum]?.Kill();
                _sequencePerEnumMap[capturedEnum] = DOTween.Sequence();

              
             
                
                int randomPoss = Random.Range(0, 100);
                if (randomPoss > 60 && !_isFirstAppear)
                {
                 
                    _sequencePerEnumMap[capturedEnum].AppendInterval(Random.Range(1.5f,2f));
                    _sequencePerEnumMap[capturedEnum].AppendCallback(() =>
                    {
                       
                        
                        _animatorMap[capturedEnum].SetInteger(ANIM_NUM,
                            Random.Range((int)AnimationName.OutA, (int)AnimationName.OutC + 1));
                        _animatorMap[capturedEnum]
                            .SetInteger(ANIM_ACTION, Random.Range(ANIM_ACTION, (int)AnimationAction.Fly));
                        _isPositionAvailMap[_sparrowCurrentPosMap[capturedEnum]] = true; //해당 위치는 사용가능으로 변경
                        _sparrowCurrentPosMap[capturedEnum] = -1; //해당 참새의 위치를 초기화
                         //_isClickableMap[_enumToTfIdMap[capturedEnum]] = false;
                         
                         _isClickableMapByTfID[_enumToTfIdMap[sparrowEnumToActivate]] = false; //Available List에 넣기위해
                        Logger.ContentTestLog($"🐦 참새 생성가능:  {(Obj)capturedEnum}");
                    });

                    _sequencePerEnumMap[capturedEnum].AppendInterval(0.3f);
                    _sequencePerEnumMap[capturedEnum].AppendCallback(() =>
                    {
                        _animatorMap[capturedEnum].SetInteger(ANIM_NUM, 0);
                        _animatorMap[capturedEnum].SetInteger(ANIM_NUM, 0);
                       
                    });

                    _sequencePerEnumMap[capturedEnum].AppendInterval(1f);
                    _sequencePerEnumMap[capturedEnum].AppendCallback(() =>
                    {
                       
                        AppearSparrow(1);
                    });
                    _sequencePerEnumMap[capturedEnum].OnKill(() =>
                    {
                        // _animatorMap[capturedEnum].SetInteger(ANIM_NUM,
                        //     Random.Range((int)AnimationName.OutA, (int)AnimationName.OutC + 1));
                        // _animatorMap[capturedEnum]
                        //     .SetInteger(ANIM_ACTION, Random.Range(ANIM_ACTION, (int)AnimationAction.Fly));
                        
                    });
                }


     
                
                Logger.ContentTestLog($"🐦 참새 등장 위치 맵 업데이트: {(Obj)sparrowEnumToActivate} / pos: {randomSparrowPos}");
            });


            DOVirtual.DelayedCall(3f, () =>
            {
                _isFirstAppear = false;
            });
        }
    }

    
    
    private void OnRaySyncedOnSparrow(RaycastHit hit)
    {
        if (CurrentThemeMainSequence != (int)MainSeq.SparrowAppear) return;
        
        var id = hit.transform.GetInstanceID();
        
        if (!_tfIdToEnumMap.ContainsKey(id))
        {
            Logger.ContentTestLog($"invalid obj {hit.transform.gameObject.name}");
            return;
        }
        


        if (!_isClickableMapByTfID.ContainsKey(id))
        {
            Logger.ContentTestLog($"❗ isClickableMap에 ID 없음: {id}");
            return;
        }
        
       
        
        // 클릭한객체가 참새일때만 (Animator)
        if (_animatorMap.ContainsKey(_tfIdToEnumMap[id]))
        {
            Logger.ContentTestLog($"🐦 참새 클릭 Pos가능:  {(Obj)_tfIdToEnumMap[id]} / pos: {_sparrowCurrentPosMap[_tfIdToEnumMap[id]]}");
            _isPositionAvailMap[_sparrowCurrentPosMap[_tfIdToEnumMap[id]]] = true; //해당 위치는 사용가능으로 변경
            _sparrowCurrentPosMap[_tfIdToEnumMap[id]] = -1; //해당 참새의 위치를 초기화
          
            
            char ranChar= (char)Random.Range('A','D'+1); //C,D는 없으므로 50%확률로 소리재생
            Managers.Sound.Play(SoundManager.Sound.Effect,"SortedByScene/EA006/OnSparrow" +ranChar);
            
            
            PlayRandomClickSound();
            PlayParticleEffect(hit.point);
            
            if (!_isClickableMapByTfID[id])
            {
                Logger.ContentTestLog("isAlready Clicked");
                return; 
            }
     
            //Logger.Log($" 참새 클릭 false체크 아이디 확인-----{id} {(Obj)_tfIdToEnumMap[id]}:{_isClickableMap[id]}");
            
        
            DOVirtual.DelayedCall(0.1f, () =>
            {
             
                _animatorMap[_tfIdToEnumMap[id]].SetInteger(ANIM_NUM,Random.Range((int)AnimationName.OutA,(int)AnimationName.OutC+1));
                _animatorMap[_tfIdToEnumMap[id]]
                    .SetInteger(ANIM_ACTION, Random.Range((int)ANIM_ACTION, (int)AnimationAction.Fly));
                // GetObject(_tfIdToEnumMap[id]).SetActive(false);
               // GetObject(_tfIdToEnumMap[id]).SetActive(false);
            });
            DOVirtual.DelayedCall(0.4f, () =>
            {
                _animatorMap[_tfIdToEnumMap[id]].SetInteger(ANIM_NUM,0);
                DOVirtual.DelayedCall(1.2f, () =>
                {
                    AppearSparrow(1);
                });
            });
            _sequencePerEnumMap[_tfIdToEnumMap[id]]?.Kill();
            SparrowCountEvent?.Invoke(++_currentSparrowCount);
            
            if(_currentSparrowCount > SPARROW_CATCH_TARGET_COUNT)
            {
                OnSparrowSectionFinished();
            }
            _isClickableMapByTfID[id] = false;
        }
  

    }

    private bool _isSparrowSectionFinished;

    private void OnSparrowSectionFinished()
    {

        if (_isSparrowSectionFinished) return; 
        _isSparrowSectionFinished = true;
        //baseUIManager.ShutInstructionUI();
        baseUIManager.PopInstructionUIFromScaleZero("참새를 모두 잡았어요!");
        _currentSparrowCount = 0;
        CurrentThemeMainSequence = (int)MainSeq.OnFinish;
                
        DOVirtual.DelayedCall(3.5f, () =>
        {
            for (int i = (int)Obj.SparrowA; i <= (int)Obj.SparrowE; i++)
            {
                _animatorMap[i].SetInteger(ANIM_NUM,-1);
            }
        });
            
        AppearScareCrow(2);
        Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA006/Narration/Thanks");
    }
    

    #endregion
}