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

    public int CurrentThemeMainSequence
    {
        get
        {
            return base.CurrentMainMainSequence;
        }
        set
        {
        
            base.CurrentMainMainSequence = value;
            
            Logger.Log($"Sequence Changed--------{(MainSeq)base.CurrentMainMainSequence} : {value}");
            SeqMessageEvent?.Invoke(base.CurrentMainMainSequence);
            SetWheatColliderStatus();

            //값반영 지연으로 value대신 _currentThemeSequence 사용하면 안됩니다 XXX
            switch (value)
            {
                case (int)MainSeq.GrassColorChange:
                    Logger.ContentTestLog($"풀 색상 변경 모드 시작 {(MainSeq)base.CurrentMainMainSequence} : {value}");
                    currentChangedCount = 0;
                    ChangeThemeSeqAnim((int)MainSeq.GrassColorChange);
                    ResetClickable();
                    SetWheatColliderStatus(true);
                    break;
                
                case (int)MainSeq.FindScarecrow:
                    _elapsedTime = 0;
                    Logger.ContentTestLog($"허수아비 모드 시작 {(MainSeq)base.CurrentMainMainSequence} : {value}");
                    SetWheatColliderStatus(false);
                    ChangeThemeSeqAnim((int)MainSeq.FindScarecrow);
                    ResetClickable();
                    OnScareCrowFindStart();
                    Managers.Sound.Play(SoundManager.Sound.Bgm,"Bgm/EA008");
                    break;

                case (int)MainSeq.SparrowAppear:
                    ResetClickable(false);//순서주의 
                    AppearSparrow(4);
                    ChangeThemeSeqAnim((int)MainSeq.SparrowAppear);
                    Logger.ContentTestLog($"참새 모드 시작 {(MainSeq)base.CurrentMainMainSequence} : {value}");
                    Managers.Sound.Play(SoundManager.Sound.Bgm,"Bgm/EA006",0.1f);

                    break;
                
                case (int)MainSeq.OnFinish:
                    ChangeThemeSeqAnim((int)MainSeq.OnFinish);
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
        psResourcePath = "Runtime/EA006/FX_leaves";
        base.Init();
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
    }
    
    
#if UNITY_EDITOR
    [SerializeField]
    private MainSeq seq;
#else
    private MainSeq seq = MainSeq.GrassColorChange;
#endif

    protected override void OnGameStartStartButtonClicked()
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
        
        if (!_isClickableMap[id]) return;
        _isClickableMap[id] = false;
        
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
        
        _isClickableMap[id] = false;
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
            
            _isClickableMap[_enumToTfIdMap[randoScareCrow]] = true;
            _sequenceMap[randoScareCrow]?.Kill();
            _sequenceMap[randoScareCrow] = DOTween.Sequence();
            _sequenceMap[randoScareCrow].Append(GetObject(randoScareCrow).transform.DOLocalRotateQuaternion(
                _defaultRotationQuatMap[randoScareCrow] *Quaternion.Euler( Random.Range(-5,-10),0, 0), 0.6f));

            _sequenceMap[randoScareCrow].Append(GetObject(randoScareCrow).transform.DOLocalRotateQuaternion(
                _defaultRotationQuatMap[randoScareCrow] * Quaternion.Euler(Random.Range(-5, 10), 0, 0), 0.6f));
            _sequenceMap[randoScareCrow].SetLoops(-1, LoopType.Yoyo);
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

        if (!_isClickableMap[id])
        {
            Logger.ContentTestLog("isAlready Clicked");
            return;
        }
        _isClickableMap[id] = false;
        
              
        PlayRandomClickSound();
        PlayParticleEffect(hit.point);
        
        _sequenceMap[_tfIdToEnumMap[id]]?.Kill();
        _sequenceMap[_tfIdToEnumMap[id]] = DOTween.Sequence();
        _sequenceMap[_tfIdToEnumMap[id]].Append(_tfidTotransformMap[id].transform.
            DOLocalRotateQuaternion(_defaultRotationQuatMap[_tfIdToEnumMap[id]] *Quaternion.Euler(0, Random.Range(10,180), 0), Random.Range(0.5f, 0.75f)));
        _sequenceMap[_tfIdToEnumMap[id]].Append(_tfidTotransformMap[id].transform.DOScale(Vector3.zero, Random.Range(0.5f, 0.75f)).OnComplete(
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
        if (CurrentThemeMainSequence != (int)MainSeq.FindScarecrow) return; 
        
        _elapsedTime += Time.deltaTime;
        
        if (_elapsedTime > TIME_LIMIT)
        {
            _elapsedTime = 0;

            _currentScarecrowCount = 0;
            DOVirtual.DelayedCall(1.5f, () =>
            {
                for (int i = (int)Obj.ScareCrowA; i <= (int)Obj.ScareCrowJ; i++)
                    GetObject(i).transform.DOScale(Vector3.zero, 1f).OnComplete(() =>
                    {
                        _sequenceMap[i]?.Kill();
                        GetObject(i).SetActive(false);
                    });
            });
            
            CurrentThemeMainSequence = (int)MainSeq.SparrowAppear;
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

    private void AppearSparrow(int count = 1)
    {
        List<int> availableSparrowsEnum = new();
    
        for (int i = (int)Obj.SparrowA; i <= (int)Obj.SparrowE; i++)
        {
            int tfId = _enumToTfIdMap[i];

            // 클릭 가능 상태(false)
            if (!_isClickableMap[tfId])
            {
            //Logger.Log($"{(Obj)i} 참새 Active 가능 상태 : {_isClickableMap[tfId]}");
                availableSparrowsEnum.Add(i);
            }
            else
            {
             //   Logger.Log($"{(Obj)i} 참새 Active 불가상태 : {_isClickableMap[tfId]}");
            }
            
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

           

            int randomSparrowPos;
            do
            {
                randomSparrowPos = Random.Range((int)AnimationName.ToA, (int)AnimationName.ToE + 1);
            } while (randomSparrowPos == previousSparrowPos
                     || !_isPositionAvailMap[randomSparrowPos]
                     || _sparrowCurrentPosMap[sparrowEnumToActivate] == randomSparrowPos
                     ||_isClickableMap[_enumToTfIdMap[sparrowEnumToActivate]]
                     );

            
            _sparrowCurrentPosMap[sparrowEnumToActivate] = randomSparrowPos;
           // _isPositionAvailMap[sparrowEnumToActivate] = false; //해당 위치는 사용불가로 변경
            _isPositionAvailMap[randomSparrowPos] = false; // ✅ 위치 기준으로 차
            previousSparrowPos = randomSparrowPos;
            _isClickableMap[_enumToTfIdMap[sparrowEnumToActivate]] = true;
            
            _animatorMap[sparrowEnumToActivate].SetInteger(ANIM_NUM, -1); //animator 초기화 
           // Logger.ContentTestLog($"🐦 참새 등장: {(Obj)sparrowEnumToActivate} / pos: {randomSparrowPos}");
            DOVirtual.DelayedCall(1f, () =>
            {
                GetObject(sparrowEnumToActivate).SetActive(true);
                _animatorMap[sparrowEnumToActivate].SetInteger(ANIM_NUM, randomSparrowPos);
                _animatorMap[sparrowEnumToActivate].SetInteger(ANIM_ACTION, (int)AnimationAction.Eat);
              
                Logger.ContentTestLog($"🐦 참새 등장 위치 맵 업데이트: {(Obj)sparrowEnumToActivate} / pos: {randomSparrowPos}");
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
        


        if (!_isClickableMap.ContainsKey(id))
        {
            Logger.ContentTestLog($"❗ isClickableMap에 ID 없음: {id}");
            return;
        }
        
        
        
        // 클릭한객체가 참새일때만 (Animator)
        if (_animatorMap.ContainsKey(_tfIdToEnumMap[id]))
        {
            _isPositionAvailMap[_sparrowCurrentPosMap[_tfIdToEnumMap[id]]] = true; //해당 위치는 사용가능으로 변경
            Logger.ContentTestLog($"🐦 참새 클릭 Pos가능:  {(Obj)_tfIdToEnumMap[id]} / pos: {_sparrowCurrentPosMap[_tfIdToEnumMap[id]]}");
            _sparrowCurrentPosMap[_tfIdToEnumMap[id]] = -1; //해당 참새의 위치를 초기화
            
            
            char ranChar= (char)Random.Range('A','D'+1); //C,D는 없으므로 50%확률로 소리재생
            Managers.Sound.Play(SoundManager.Sound.Effect,"SortedByScene/EA006/OnSparrow" +ranChar);
            
            
            PlayRandomClickSound();
            PlayParticleEffect(hit.point);
            
            if (!_isClickableMap[id])
            {
                Logger.ContentTestLog("isAlready Clicked");
                return; 
            }
            _isClickableMap[id] = false;
            //Logger.Log($" 참새 클릭 false체크 아이디 확인-----{id} {(Obj)_tfIdToEnumMap[id]}:{_isClickableMap[id]}");
            
        
            DOVirtual.DelayedCall(0.1f, () =>
            {
             
                _animatorMap[_tfIdToEnumMap[id]].SetInteger(ANIM_NUM,Random.Range((int)AnimationName.OutA,(int)AnimationName.OutC+1));
                _animatorMap[_tfIdToEnumMap[id]]
                    .SetInteger(ANIM_ACTION, Random.Range((int)ANIM_ACTION, (int)AnimationAction.Fly));
               // GetObject(_tfIdToEnumMap[id]).SetActive(false);
            });
            DOVirtual.DelayedCall(0.4f, () =>
            {
                _animatorMap[_tfIdToEnumMap[id]].SetInteger(ANIM_NUM,0);
                DOVirtual.DelayedCall(0.7f, () =>
                {
                    AppearSparrow(1);
                });
            });
            
            SparrowCountEvent?.Invoke(++_currentSparrowCount);
            
            if(_currentSparrowCount > SPARROW_CATCH_TARGET_COUNT)
            {
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

        }
     

    }

    #endregion
}