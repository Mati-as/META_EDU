using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
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
        SparrowA,
        SparrowB,
        SparrowC,
        SparrowD,
        SparrowE,
        
    }
    
    private enum AnimationName
    {
       ToA,
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

    public int currentSequence
    {
        get
        {
            return _currentSequence;
        }
        set
        {
            _currentSequence = value;
            Logger.Log($"Sequence Changed--------{(SequenceName)_currentSequence}");
            SeqMessageEvent?.Invoke(_currentSequence);
            SetWheatColliderStatus();

            switch (_currentSequence)
            {
                case (int)SequenceName.Default:
                    break;
                case (int)SequenceName.GrassColorChange:
                    currentChangedCount = 0;
                    ChangeSeqAnim((int)SequenceName.GrassColorChange);
                    ResetClickable();
                    SetWheatColliderStatus(true);
                    break;
                
                case (int)SequenceName.FindScarecrow:
                    SetWheatColliderStatus(false);
                    ChangeSeqAnim((int)SequenceName.FindScarecrow);
                    ResetClickable();
                    OnScareCrowFindStart();
                    break;
                
                case (int)SequenceName.SparrowAppear:
                    SetWheatColliderStatus(false);
                    ChangeSeqAnim((int)SequenceName.SparrowAppear);
                    ResetClickable();
                    AppearSparrow(2);
                    Logger.Log("참새모드 시작 중-----------------");
                    break;
                case (int)SequenceName.OnFinish:
                    break;
            }
        }
    }


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
    
    private readonly int ANIM_NUM = Animator.StringToHash("animNum");
    private readonly int ANIM_ACTION = Animator.StringToHash("animAction");
    private void SetWheatColliderStatus(bool isOn = false)
    {
        for (int i = (int)Obj.WheatGroup_A; i <= (int)Obj.WheatGroup_E; i++) colliderMap[i].enabled = isOn;
    }

    protected override void Init()
    {
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


        for (int i = (int)Obj.ScareCrowA; i <= (int)Obj.ScareCrowF; i++)
        {
            GetObject(i).transform.localScale = Vector3.zero;
            GetObject(i).SetActive(false);
        }
    }

    protected override void OnGameStartStartButtonClicked()
    {
        currentSequence =(int)SequenceName.GrassColorChange;
    }

    public override void OnRaySynced()
    {
        foreach (var hit in GameManager_Hits)
            switch (currentSequence)
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

        // if (!_isClickableMap[id])
        // {
        //     Logger.ContentTestLog("Already Clicked");
        //     return;
        // }
     
        var clickedWheatGroup = (Obj)_tfIdToEnumMap[id];

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
            DOVirtual.DelayedCall(1.5f, () =>
            {
                Logger.ContentTestLog("currentChangedCount : " + currentChangedCount + " / " + TargetColorChangedCount);
                currentChangedCount = 0;
                OnAllGrassColorChanged();
            });
        else
        {
            Logger.ContentTestLog("currentChangedCount : " + currentChangedCount + " / " + TargetColorChangedCount);
        }
    }



    private void OnAllGrassColorChanged()
    {
        currentSequence = (int)SequenceName.FindScarecrow;
    }

    #endregion


    #region 허수아비 찾기 파트----------------------------------------------------

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
            int randoScareCrow = Random.Range((int)Obj.ScareCrowA, (int)Obj.ScareCrowF);

            while (SelectedScarecrow == randoScareCrow)
                randoScareCrow = Random.Range((int)Obj.ScareCrowA, (int)Obj.ScareCrowF);
            SelectedScarecrow = randoScareCrow;
            
            GetObject(randoScareCrow).SetActive(true);
            GetObject(randoScareCrow).transform.DOScale(_defaultSizeMap[randoScareCrow], 1f);
            
            
            _sequenceMap[randoScareCrow]?.Kill();
            _sequenceMap[randoScareCrow] = DOTween.Sequence();
            _sequenceMap[randoScareCrow].Append(GetObject(randoScareCrow).transform.DOLocalRotateQuaternion(
                _defaultRotationQuatMap[randoScareCrow] *Quaternion.Euler( Random.Range(-10,-20),0, 0), 0.6f));

            _sequenceMap[randoScareCrow].Append(GetObject(randoScareCrow).transform.DOLocalRotateQuaternion(
                _defaultRotationQuatMap[randoScareCrow] * Quaternion.Euler(Random.Range(10, 20), 0, 0), 0.6f));
            _sequenceMap[randoScareCrow].SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void OnScareCrowClicked()
    {
        
    }

    private void OnRaySyncedOnScareCrowFind(RaycastHit hit)
    {
        if (currentSequence != (int)SequenceName.FindScarecrow)
        {
            Logger.ContentTestLog("it's not find scarecrow sequence");
            return;
        }

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

        if ( !_isClickableMap[id] ) Logger.ContentTestLog("isAlready Clicked");
        _isClickableMap[id] = false;
        
        
        _sequenceMap[_tfIdToEnumMap[id]]?.Kill();
        _sequenceMap[_tfIdToEnumMap[id]] = DOTween.Sequence();
        _sequenceMap[_tfIdToEnumMap[id]].Append(_tfidTotransformMap[id].transform.
            DOLocalRotateQuaternion(_defaultRotationQuatMap[_tfIdToEnumMap[id]] *Quaternion.Euler(0, Random.Range(10,180), 0), Random.Range(0.5f, 0.75f)));
        _sequenceMap[_tfIdToEnumMap[id]].Append(_tfidTotransformMap[id].transform.DOScale(Vector3.zero, Random.Range(0.5f, 0.75f)).OnComplete(
            () =>
            {
                AppearScareCrow(Random.Range(1, 3));
            }));
        
        _currentScarecrowCount++;
        

        if(_currentScarecrowCount >= TargetScarecrowCount)
            DOVirtual.DelayedCall(1.5f, () =>
            {
                _currentScarecrowCount = 0;

                for (int i = (int)Obj.ScareCrowA; i <= (int)Obj.ScareCrowF; i++)
                {
                    GetObject(i).transform.DOScale(Vector3.zero, 1f).OnComplete(() =>
                    {
                        GetObject(i).SetActive(false);
                    });
                  
                }

                currentSequence = (int)SequenceName.SparrowAppear;
            });
    }
    
    

    #endregion
    
    


    #region 참새파트------------------------------------------------------------

    private void AppearSparrow(int Count = 2)
    {
        if (currentSequence != (int)SequenceName.SparrowAppear) return;
        

        int previousSparrow = -1; //sentinel val
        int previousSparrowPos = -1;
        
        for (int i = 0; i < Count; i++)
        {
            int randoScareCrow = Random.Range((int)Obj.SparrowA, (int)Obj.SparrowE);

            _isClickableMap[enumToTfIdMap[randoScareCrow]] = true;
            
            
            while (previousSparrow == randoScareCrow) randoScareCrow = Random.Range((int)Obj.SparrowA, (int)Obj.SparrowE);
            previousSparrow = randoScareCrow;
            
            GetObject(randoScareCrow).SetActive(true);
            GetObject(randoScareCrow).transform.DOScale(_defaultSizeMap[randoScareCrow], 1f);
            
           
            
            int randomSparrowPos = Random.Range((int)AnimationName.ToA, (int)AnimationName.ToE + 1);
            
            while(previousSparrowPos == randomSparrowPos) randomSparrowPos = Random.Range((int)AnimationName.ToA, (int)AnimationName.ToE + 1);
            previousSparrowPos = randomSparrowPos;
            
            _animatorMap[randoScareCrow].SetInteger(ANIM_NUM,randomSparrowPos);
            _animatorMap[randoScareCrow]
                .SetInteger(ANIM_ACTION, Random.Range((int)ANIM_ACTION, (int)AnimationAction.Eat));
        }
    }

    
    
    private void OnRaySyncedOnSparrow(RaycastHit hit)
    {
        if (currentSequence != (int)SequenceName.SparrowAppear) return;
        
        var id = hit.transform.GetInstanceID();
        
        if (!_tfIdToEnumMap.ContainsKey(id))
        {
            Logger.ContentTestLog($"invalid obj {hit.transform.gameObject.name}");
            return;
        }
        
        if ( !_isClickableMap[id] ) Logger.ContentTestLog("isAlready Clicked");
        _isClickableMap[id] = false;

        DOVirtual.DelayedCall(2f, () =>
        {
            _isClickableMap[id] = true;
        });
        
        if (!_tfIdToEnumMap.ContainsKey(id))
        {
            Logger.ContentTestLog("there's no scarecrow");
            return;
        }
      
        // 클릭한객체가 참새일때만 
        if (_animatorMap.ContainsKey(_tfIdToEnumMap[id]))
        {
            _animatorMap[_tfIdToEnumMap[id]].SetInteger(ANIM_NUM,Random.Range((int)AnimationName.OutA,(int)AnimationName.OutC+1));
            _animatorMap[_tfIdToEnumMap[id]]
                .SetInteger(ANIM_ACTION, Random.Range((int)ANIM_ACTION, (int)AnimationAction.Fly));
            DOVirtual.DelayedCall(0.5f, () =>
            {
                AppearSparrow(Random.Range(0,2));
            });
            
            SparrowCountEvent?.Invoke(++_currentSparrowCount);
        }
     

    }

    #endregion
}