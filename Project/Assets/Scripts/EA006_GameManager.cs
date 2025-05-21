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
    public enum SequenceName
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

    public int currentThemeSequence
    {
        get
        {
            return base._currentMainSequence;
        }
        set
        {
        
            base._currentMainSequence = value;
            
            Logger.Log($"Sequence Changed--------{(SequenceName)base._currentMainSequence} : {value}");
            SeqMessageEvent?.Invoke(base._currentMainSequence);
            SetWheatColliderStatus();

            //값반영 지연으로 value대신 _currentThemeSequence 사용하면 안됩니다 XXX
            switch (value)
            {
                case (int)SequenceName.GrassColorChange:
                    Logger.ContentTestLog($"풀 색상 변경 모드 시작 {(SequenceName)base._currentMainSequence} : {value}");
                    currentChangedCount = 0;
                    ChangeThemeSeqAnim((int)SequenceName.GrassColorChange);
                    ResetClickable();
                    SetWheatColliderStatus(true);
                    break;
                
                case (int)SequenceName.FindScarecrow:
                    _elapsedTime = 0;
                    Logger.ContentTestLog($"허수아비 모드 시작 {(SequenceName)base._currentMainSequence} : {value}");
                    SetWheatColliderStatus(false);
                    ChangeThemeSeqAnim((int)SequenceName.FindScarecrow);
                    ResetClickable();
                    OnScareCrowFindStart();
                    Managers.Sound.Play(SoundManager.Sound.Bgm,"Bgm/EA008");
                    break;

                case (int)SequenceName.SparrowAppear:
                    ResetClickable(false);//순서주의 
                    AppearSparrow(2);
                    ChangeThemeSeqAnim((int)SequenceName.SparrowAppear);
                    Logger.ContentTestLog($"참새 모드 시작 {(SequenceName)base._currentMainSequence} : {value}");
                    Managers.Sound.Play(SoundManager.Sound.Bgm,"Bgm/EA006");

                    break;
                
                case (int)SequenceName.OnFinish:
                    ChangeThemeSeqAnim((int)SequenceName.OnFinish);
                    break;
                
                case (int)SequenceName.Default:
                    Logger.LogError("시퀀스 에러--------------확인필요");
                    break;
            }
            
          
        }
    }
    protected Dictionary<int,bool> _isSeqActiveMap = new(); 

    private readonly Dictionary<int, Collider> colliderMap = new(); //group별임에 주의


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
    private readonly int TargetSparrowCount = 10;
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
    }

    protected override void OnGameStartStartButtonClicked()
    {
        DOVirtual.DelayedCall(1f, () =>
        {

            currentThemeSequence = (int)SequenceName.GrassColorChange;
        });
    }

    public override void OnRaySynced()
    {
        foreach (var hit in GameManager_Hits)
            switch (currentThemeSequence)
            {
                case (int)SequenceName.Default:
                    break;
                case (int)SequenceName.GrassColorChange:
                
                    OnRaySyncedOnGrassColorChange(hit);
                 
                    break;
                case (int)SequenceName.FindScarecrow:
                   
                    
                    OnRaySyncedOnScareCrowFind(hit);
                
                    break;
                case (int)SequenceName.SparrowAppear:
                  
                    OnRaySyncedOnSparrow(hit);
                    break;
                default:
                    Logger.ContentTestLog("no raysynced---------");
                    break;
            }
    }

    #region A)풀색상 변화 파트 -----------------------------------------------------------
    
    private void OnRaySyncedOnGrassColorChange(RaycastHit hit)
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
            var randColorB = _targetColorB * Random.Range(0.95f, 1.05f);

            // 트위닝 적용
            mat.DOColor(randColorA, COLOR_A_CHANGED, 1f);
            mat.DOColor(randColorB, COLOR_B_CHANGED, 1f);
        }

        
        currentChangedCount++;

        if (currentChangedCount >= TargetColorChangedCount)
        {
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
        currentThemeSequence = (int)SequenceName.FindScarecrow;
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
        });
    }

    private void AppearScareCrow(int Count = 2)
    {
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
        if (currentThemeSequence != (int)SequenceName.FindScarecrow) return; 
        
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
            
            currentThemeSequence = (int)SequenceName.SparrowAppear;
        }
    }


    int previousSparrowPos = -1;
    #region 참새파트------------------------------------------------------------

    private void AppearSparrow(int count = 1)
    {
        List<int> availableSparrows = new();
    
        for (int i = (int)Obj.SparrowA; i <= (int)Obj.SparrowE; i++)
        {
            int tfId = _enumToTfIdMap[i];

            // 클릭 가능 상태(false)
            if (!_isClickableMap[tfId])
            {
                Logger.Log($"{(Obj)i} 참새 Active 가능 상태 : {_isClickableMap[tfId]}");
                availableSparrows.Add(i);
            }
            else
            {
                Logger.Log($"{(Obj)i} 참새 Active 불가상태 : {_isClickableMap[tfId]}");
            }
            
        }

        if (availableSparrows.Count == 0)
        {
            Logger.ContentTestLog("❌ 등장 가능한 참새가 없습니다.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            int randIdx = Random.Range(0, availableSparrows.Count);
            int sparrowToActivate = availableSparrows[randIdx];
            availableSparrows.RemoveAt(randIdx);
            

           

            int randomSparrowPos;
            do
            {
                randomSparrowPos = Random.Range((int)AnimationName.ToA, (int)AnimationName.ToE + 1);
            } while (randomSparrowPos == previousSparrowPos);

            previousSparrowPos = randomSparrowPos;

            Logger.ContentTestLog($"🐦 참새 등장: {(Obj)sparrowToActivate} / pos: {randomSparrowPos}");
            DOVirtual.DelayedCall(1f, () =>
            {
                GetObject(sparrowToActivate).SetActive(true);
                _isClickableMap[_enumToTfIdMap[sparrowToActivate]] = true;
                _animatorMap[sparrowToActivate].SetInteger(ANIM_NUM, randomSparrowPos);
                _animatorMap[sparrowToActivate].SetInteger(ANIM_ACTION, (int)AnimationAction.Eat);
            });
         
        
        }
    }

    
    
    private void OnRaySyncedOnSparrow(RaycastHit hit)
    {
        if (currentThemeSequence != (int)SequenceName.SparrowAppear) return;
        
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
        
        
        
        // 클릭한객체가 참새일때만 
        if (_animatorMap.ContainsKey(_tfIdToEnumMap[id]))
        {
        
            PlayRandomClickSound();
            PlayParticleEffect(hit.point);
            
            if (!_isClickableMap[id])
            {
                Logger.ContentTestLog("isAlready Clicked");
                return; 
            }
            _isClickableMap[id] = false;
            Logger.Log($" 참새 클릭 false체크 아이디 확인-----{id} {(Obj)_tfIdToEnumMap[id]}:{_isClickableMap[id]}");
            
            _animatorMap[_tfIdToEnumMap[id]].SetInteger(ANIM_NUM,Random.Range((int)AnimationName.OutA,(int)AnimationName.OutC+1));
            _animatorMap[_tfIdToEnumMap[id]]
                .SetInteger(ANIM_ACTION, Random.Range((int)ANIM_ACTION, (int)AnimationAction.Fly));
            DOVirtual.DelayedCall(0.5f, () =>
            {
                _animatorMap[_tfIdToEnumMap[id]].SetInteger(ANIM_NUM,0);
               // GetObject(_tfIdToEnumMap[id]).SetActive(false);
            });
            DOVirtual.DelayedCall(1.0f, () =>
            {
                AppearSparrow(Random.Range(1,3));
            });
            
            SparrowCountEvent?.Invoke(++_currentSparrowCount);
            
            if(_currentSparrowCount > TargetSparrowCount)
            {
                _currentSparrowCount = 0;
                currentThemeSequence = (int)SequenceName.OnFinish;
                
                DOVirtual.DelayedCall(2.5f, () =>
                {
                    for (int i = (int)Obj.SparrowA; i <= (int)Obj.SparrowE; i++)
                    {
                        _animatorMap[i].SetInteger(ANIM_NUM,-1);
                    }
                });
            
                AppearScareCrow(2);
            }

        }
     

    }

    #endregion
}