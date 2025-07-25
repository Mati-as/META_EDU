using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarAnimationController : Ex_MonoBehaviour
{
    public enum AnimClip
    {
        Idle,
        SitAndPoint,
        Nervous,
        LookOver,
        HideFace,
        Wave,
        Crawl,
        
    }
    
    public enum LegAnimClip
    {
        Idle,
        Walk,
    }



    private Dictionary<int, Animator> _controllerMap = new();
    private Dictionary<int, int> tfIdToIndexMap= new();
    private int CHARACTER_ANIM_NUM = Animator.StringToHash("AnimNum");
    private static readonly int ToDefault = Animator.StringToHash("ToDefault");
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int LegAnimNum = Animator.StringToHash("LegAnimNum");

    protected override void Init()
    {
        base.Init();
        
        for(int i = 0 ;i < transform.childCount; i++)
        {
            _controllerMap.Add(i,transform.GetChild(i).GetComponent<Animator>());
            tfIdToIndexMap.Add(transform.GetChild(i).GetInstanceID(),i);
            SetDefault(i);
        }

    
    }

    public void SetLegAnim(int animator,int animName)
    {
        _controllerMap[animator].SetInteger(CHARACTER_ANIM_NUM,(int)animName);
    }
    public void SetWalking(int index,bool isWalking =false)
    {
        _controllerMap[index].SetBool(IsWalking, isWalking);
    }
    public void PlayAnimation(int animator,AnimClip animName)
    {
        
        _controllerMap[animator].SetTrigger(ToDefault);
        _controllerMap[animator].SetInteger(CHARACTER_ANIM_NUM,(int)animName);
    }
    
    public void PlayAnimationByTransID(int transID,AnimClip animName)
    {
        _controllerMap[tfIdToIndexMap[transID]].SetTrigger(ToDefault);
        _controllerMap[tfIdToIndexMap[transID]].SetInteger(CHARACTER_ANIM_NUM,(int)animName);
    }

    public void PlayAnimationForAll()
    {
        
    }

    public bool IsTransIDValid(int id)
    {
        if (tfIdToIndexMap.ContainsKey(id) && _controllerMap.ContainsKey(tfIdToIndexMap[id])) return true;
        return false;
    }

    public void PauseAnimator(int animator)
    {
        _controllerMap[animator].speed = 0f;
    }
    
    public void PauseAnimatorByID(int transID)
    {
        _controllerMap[tfIdToIndexMap[transID]].speed = 0f;
    }

    public void ResumeAnimator(int animator)
    {
        _controllerMap[animator].speed = 1f;
    }

    public void ResumeAnimatorByID(int transID)
    {
        _controllerMap[tfIdToIndexMap[transID]].speed = 1f;
    }
    public void SetDefault(int animator)
    {
        _controllerMap[animator].SetInteger(CHARACTER_ANIM_NUM,(int)AnimClip.Idle);
    }
}
