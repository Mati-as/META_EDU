using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class Hopscotch_GameManager : MonoBehaviour
{
  
    [Header("Reference")] 
    [SerializeField]
    private Hopscotch_EffectController effectController;
    [Space(20f)]
    
    public Transform stepGroup;
    private Transform[] _steps;
    private int _stepCount;

    private ParticleSystem _inducingParticle;
    private ParticleSystem _successParticle;
    private readonly string PATH ="게임별분류/기본컨텐츠/Hopscotch/";

    public float successParticleDuration;
    private bool _isSuccesssParticlePlaying;
    public float offset;
    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        
        PlayInducingParticle(0);
    }

    private void OnClick()
    {
        // 발판 밟은 직후에는 다음 발판을 누를 수 없게합니다.
        if (_isSuccesssParticlePlaying) return;

        if (CheckOnStep())
        {
            PlaySuccessParticle(_currentStep);
        }
        
    }

    private void BindEvent()
    {
        Hopscotch_EffectController.Hopscotch_OnClick -= OnClick;
        Hopscotch_EffectController.Hopscotch_OnClick += OnClick;
    }

    private void OnDestroy()
    {
        Hopscotch_EffectController.Hopscotch_OnClick -= OnClick;
    }

    private void Init()
    {
        BindEvent();
        LoadParticles();
        GetSteps();
    }

    private void LoadParticles()
    {
        var psPrefab1 = Resources.Load<GameObject>(PATH + "CFX_inducingParticle");
        var psPrefab2 = Resources.Load<GameObject>(PATH + "CFX_successParticle");
        
        if(psPrefab1 != null)
        {
            _inducingParticle = Instantiate(psPrefab1, offset*Vector3.down , transform.rotation,transform).GetComponent<ParticleSystem>();;
        }
        else
        { 
#if UNITY_EDITOR
         
            Debug.LogError($"널에러 발생: {PATH}" + "CFX_inducingParticle");
#endif
        }
        
        if(psPrefab2 != null)
        {
            _successParticle = Instantiate(psPrefab2, offset*Vector3.down , transform.rotation,transform).GetComponent<ParticleSystem>();;
            _successParticle.Stop();
        }
    }

    private void GetSteps()
    {
        _stepCount = stepGroup.childCount;
        _steps = new Transform[_stepCount];
        
        for (int i = 0; i < _stepCount; ++i)
        {
            _steps[i] = stepGroup.GetChild(i);
        
        }
    }

    private int _currentStep = 0;
    private bool _isGameFinished;

    private void PlayInducingParticle(int currentPosition)
    {
        if (currentPosition > _stepCount) return;
        if (_isGameFinished) return;
        
            _inducingParticle.Stop();
            _inducingParticle.gameObject.transform.position = AddOffset(_steps[currentPosition].position);
            _inducingParticle.gameObject.SetActive(true);
            _inducingParticle.Play();
        
       
    }

    private Vector3 AddOffset(Vector3 position)
    {
        return position + Vector3.down * offset;
    }

  
    private void PlaySuccessParticle( int currentPosition)
    {
        if (currentPosition > _stepCount) return;
        if (_isGameFinished) return;
        
            _successParticle.Stop();
            _successParticle.gameObject.transform.position = AddOffset(_steps[currentPosition].position);
            _successParticle.gameObject.SetActive(true);
            _successParticle.Play();

            _isSuccesssParticlePlaying = true;
            DOVirtual
                .Float(0, 0, successParticleDuration, val => val++)
                .OnComplete(() =>
                {
                  
                    _successParticle.Stop();
                    _successParticle.gameObject.SetActive(false);
                    _isSuccesssParticlePlaying = false;
                   
                    _currentStep++;
                    PlayInducingParticle(_currentStep);
                });
        
        
        
    }
    private bool CheckOnStep()
    {

        foreach (var hit in effectController.hits)
        {
            if (hit.transform.gameObject.name == "Step_" + _currentStep.ToString())
            {
                
#if UNITY_EDITOR
                Debug.Log("Step_" + _currentStep.ToString());
#endif
                return true;
            }
        }
        return false;
    }
}
