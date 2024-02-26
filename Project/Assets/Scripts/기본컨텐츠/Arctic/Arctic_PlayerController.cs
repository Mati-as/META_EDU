using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class Arctic_PlayerController : MonoBehaviour
{

    private float _maxDetectableDistance;
  
    private float JUMP_HEIGHT=2.5f;
    private Animator _animator;
    private readonly int JUMP = Animator.StringToHash("Jump");
    private float _hitPointY;
    private Transform[] _currentGlaciers;
    private enum Path
    {
        Start,
        Mid,
        Arrival,
        Max
    }

    private enum Glacier
    {
        A,B,C,D,Max
    }

    private void Start()
    {
        
        BindEvent();

        _animator = GetComponent<Animator>();
        _currentGlaciers = new Transform[(int)Glacier.Max];
    }

    private RaycastHit hit;
    private void Update()
    {



        if (currentClickedGlacier != null)
        {
            if (!_isDoingPath && currentClickedGlacier.gameObject.name.Contains(gameObject.name))
            {
                transform.position = currentClickedGlacier.position + yOffset;
            }
        }
    
    }


    private void BindEvent()
    {
        Arctic_GameManager.On_GmRay_Synced -= OnRaySynced;
        Arctic_GameManager.On_GmRay_Synced += OnRaySynced;
    }

    private Transform currentClickedGlacier;
    Glacier _glacierEnum;

    protected void OnRaySynced()
    {
        if (_isDoingPath) return;
        
        if (Physics.Raycast(IGameManager.GameManager_Ray, out hit))
            if (hit.transform.gameObject.name.Contains(gameObject.name))
            {
                if (Enum.TryParse(gameObject.name, out _glacierEnum))
                    _currentGlaciers[(int)_glacierEnum] = hit.transform;

                currentClickedGlacier = hit.transform;
                JumpToClickedGlacier();
            }
    }


    private Vector3[] _currentProjectilePath;
    private bool _isDoingPath;
    public Vector3 yOffset;
    
    private void JumpToClickedGlacier()
    {
        _isDoingPath = true;

      
        
        var currentPlayerPos = transform.position;
        var positionToJump = currentClickedGlacier.position;
        
        _currentProjectilePath = new Vector3[(int)Path.Max];
        
        _currentProjectilePath[(int)Path.Start] = currentPlayerPos;
        
        _currentProjectilePath[(int)Path.Mid] = (currentPlayerPos + positionToJump) / 2 + Vector3.up * JUMP_HEIGHT;
        _currentProjectilePath[(int)Path.Arrival] 
            = new Vector3(positionToJump.x ,positionToJump.y + yOffset.y,positionToJump.z + yOffset.z);

        
      
        
        transform.DOPath(_currentProjectilePath, 1.25f).
            SetEase(Ease.InOutSine)
            .OnStart(()=>{  PlayJumpAnimation();})
            .OnComplete(() =>
            {
                transform.DOMove(currentClickedGlacier.position + yOffset, 0.1f).OnComplete(() =>
                {
                    _isDoingPath = false;
                });
            })
            .SetDelay(0.1f);
    }

    private void PlayJumpAnimation()
    {
        _animator.SetTrigger(JUMP);
    }
}
