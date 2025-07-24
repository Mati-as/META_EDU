using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceAnimationController 
{
    private Animator _animator;

    private readonly int _seqNumHash = Animator.StringToHash("seqNum");
    private readonly int _finishHash = Animator.StringToHash("Finish");

    public SequenceAnimationController(Animator animator)
    {
        _animator = animator;
        if (_animator == null)
        {
            Logger.Log("Animator가 null입니다. SequenceAnimatorController 초기화 실패.");
        }
    }

    public void ChangeThemeSequence(int seqNum = 0)
    {
        _animator?.SetInteger(_seqNumHash, seqNum);
    }

    public void TriggerFinish()
    {
        _animator?.SetTrigger(_finishHash);
    }

    public void SetAnimator(Animator animator)
    {
        _animator = animator;
    }
}
