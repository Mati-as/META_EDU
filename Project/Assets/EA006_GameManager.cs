using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EA006_GameManager : Ex_BaseGameManager
{

    private enum SequenceName
    {
        Default,
        GrassColorChange,
        FindScareCrow,
        SparrowAppear,
        OnFinish
    }

    private enum Obj
    {
        WheatGroup_A,
        WheatGroup_B,
        WheatGroup_C,
        WheatGroup_D,
     //  WheatGroup_E
    }

    
    private Dictionary<int,MeshRenderer[]> _mRendererMap = new(); //group별임에 주의
    private Material _defaultMat;
    private Material _changedMat; //클릭시 변경될 색상


    protected override void Init()
    {
        base.Init();
        BindObject(typeof(Obj));

        _defaultMat = Resources.Load<Material>("Runtime/EA006/EA006_WheatA");
        _changedMat = Resources.Load<Material>("Runtime/EA006/EA006_WheatA_Changed");

        Debug.Assert(_changedMat != null, _defaultMat + " != null");
        for (int i = (int)Obj.WheatGroup_A; i <= (int)Obj.WheatGroup_D; i++)
        {
            _mRendererMap[i] = new MeshRenderer[123];
            _mRendererMap[i] = GetObject(i).GetComponentsInChildren<MeshRenderer>();

            foreach (var meshRenderer in _mRendererMap[i])
            {
                var newMat = _defaultMat;
                meshRenderer.material = newMat;
            }
        }

  
    }

    protected override void OnGameStartStartButtonClicked()
    {
     
        ChangeSeqAnim((int)SequenceName.GrassColorChange);
    }

    public override void OnRaySynced()
    {
        foreach (RaycastHit hit in GameManager_Hits)
        {
            switch (currentSequence)
            {
                case (int)SequenceName.Default:
                    break;
                case (int)SequenceName.GrassColorChange:
                    OnRaySyncedOnGrassColorChange(hit);
                    break;
                case (int)SequenceName.FindScareCrow:
                    OnRaySyncedOnScareCrowFind(hit);
                    break;
                case (int)SequenceName.SparrowAppear:
                    OnRaySyncedOnSparrow(hit);
                    break;
                default:
                    Logger.Log("no raysynced---------");
                    break;
            }
        }
    }

    private void OnRaySyncedOnGrassColorChange(RaycastHit hit)
    {
        int id = hit.transform.GetInstanceID();
        if(!_IdToEnumMap.ContainsKey(id))
        {
            Logger.Log("there's no wheat group");
            return;
        }
        if (_IdToEnumMap[id] == (int)Obj.WheatGroup_A)
            foreach (var meshRenderer in _mRendererMap[(int)Obj.WheatGroup_A])
                meshRenderer.material = _changedMat;
        if (_IdToEnumMap[id] == (int)Obj.WheatGroup_B)
            foreach (var meshRenderer in _mRendererMap[(int)Obj.WheatGroup_B])
                meshRenderer.material = _changedMat;
        if (_IdToEnumMap[id] == (int)Obj.WheatGroup_C)
            foreach (var meshRenderer in _mRendererMap[(int)Obj.WheatGroup_C])
                meshRenderer.material = _changedMat;
        if (_IdToEnumMap[id] == (int)Obj.WheatGroup_D)
            foreach (var meshRenderer in _mRendererMap[(int)Obj.WheatGroup_D])
                meshRenderer.material = _changedMat;
        // if (_IdToEnumMap[id] == (int)Obj.WheatGroup_E)
        //     foreach (var meshRenderer in _mRendererMap[(int)Obj.WheatGroup_E])
        //         meshRenderer.material = _changedMat;
    }
    private void OnRaySyncedOnScareCrowFind(RaycastHit hit)
    {
        
    }
    
    private void OnRaySyncedOnSparrow(RaycastHit hit)
    {
        
    }
    
}
