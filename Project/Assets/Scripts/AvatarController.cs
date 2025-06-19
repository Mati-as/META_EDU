using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarController : Ex_MonoBehaviour
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

    private Dictionary<int, Animator> _controllerMap = new();
    private int CHARACTER_ANIM_NUM = Animator.StringToHash("AnimNum");
    private static readonly int ToDefault = Animator.StringToHash("ToDefault");

    protected override void Init()
    {
        base.Init();
        
        for(int i = 0 ;i < transform.childCount; i++)
        {
            _controllerMap.Add(i,transform.GetChild(i).GetComponent<Animator>());
            SetDefault(i);
        }

    
    }

    public void PlayAnimation(int animator,AnimClip animName)
    {
        _controllerMap[animator].SetTrigger(ToDefault);
        _controllerMap[animator].SetInteger(CHARACTER_ANIM_NUM,(int)animName);
    }

    public void PlayAnimationForAll()
    {
        
    }

    public void SetDefault(int animator)
    {
        _controllerMap[animator].SetInteger(CHARACTER_ANIM_NUM,(int)AnimClip.Idle);
    }
}
