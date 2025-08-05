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

    public enum ExpressionAnimClip
    {
        Default,
        Happy,
        Surprised,
        Sad_Cry,
        Angry,
        Happy_Heart,
        Kiss,
    }


    private Dictionary<int, Animator> _controllerMap = new();
    private Dictionary<int, int> tfIdToIndexMap= new();
    private int CHARACTER_ANIM_NUM = Animator.StringToHash("AnimNum");
    private static readonly int TO_DEFAULT = Animator.StringToHash("ToDefault");
    private static readonly int IS_WALKING = Animator.StringToHash("IsWalking");
    private static readonly int LEG_ANIM = Animator.StringToHash("LegAnimNum");
    private static readonly int EXPRESSION_ANIM = Animator.StringToHash("ExpressionAnimNum");
    private static readonly int TO_DEFAULT_EXPRESSION = Animator.StringToHash("ToDefault_Expression");

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

    public void SetLegAnim(int animatorIdx,int animName)
    {
        _controllerMap[animatorIdx].SetInteger(LEG_ANIM,(int)animName);
    }
    
    public void SetExpression(int animatorIdx,int animName)
    {
        InitExpression(animatorIdx);
        _controllerMap[animatorIdx].SetInteger(EXPRESSION_ANIM,(int)animName);
    }
    public void InitExpression(int animatorIdx)
    {
        _controllerMap[animatorIdx].SetTrigger(TO_DEFAULT_EXPRESSION);
        _controllerMap[animatorIdx].SetInteger(EXPRESSION_ANIM,-1);
    }
    public void SetWalking(int animatorIdx,bool isWalking =false)
    {
        _controllerMap[animatorIdx].SetBool(IS_WALKING, isWalking);
    }
    public void PlayAnimation(int animatorIdx,AnimClip animName)
    {
        
        _controllerMap[animatorIdx].SetTrigger(TO_DEFAULT);
        _controllerMap[animatorIdx].SetInteger(CHARACTER_ANIM_NUM,(int)animName);
    }
    
    public void PlayAnimationByTransID(int transID,AnimClip animName)
    {
        _controllerMap[tfIdToIndexMap[transID]].SetTrigger(TO_DEFAULT);
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
