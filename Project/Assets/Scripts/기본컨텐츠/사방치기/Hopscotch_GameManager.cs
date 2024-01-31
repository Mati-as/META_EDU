using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = System.Random;

public class Hopscotch_GameManager : IGameManager
{
    
    [FormerlySerializedAs("videoGameManager")]
    [FormerlySerializedAs("effectController")]
    [Header("Reference")] 

    [Space(20f)]
    
    public Transform stepGroup;

    private RectTransform[] _numberTextRects;
    private Transform[] _steps;
    private int _stepCount;

    private ParticleSystem _inducingParticle;
    private ParticleSystem _successParticle;
    private readonly string PATH ="게임별분류/기본컨텐츠/Hopscotch/";

    public float successParticleDuration;
    private bool _isSuccesssParticlePlaying;
    public float offset;
    
    private float _stepSizeChangeRate =1.11f;
    private Dictionary<Transform, Quaternion> _defaultQuaternionMap;
    private Dictionary<Transform, Vector3> _defaultSizeMap;
    private Dictionary<RectTransform, Vector3> _uiDefaultSizeMap;
    private Dictionary<Transform, Rigidbody> _rigidbodies;
    
    
   
    

    private CanvasGroup _cg;

    private void Start()
    {
        GameObject inPlayTexts = GameObject.Find("InPlayTexts");

        _cg = inPlayTexts.GetComponent<CanvasGroup>();
        _cg.DOFade(0, 0.01f);
        if (inPlayTexts != null)
        {
            _numberTextRects = inPlayTexts.GetComponentsInChildren<RectTransform>()
                .Where(rt => rt.gameObject != inPlayTexts)
                .ToArray();
        }
        else
        {
            Debug.LogError("GameObject named 'InPlayTexts' not found in the scene.");
        }

        foreach (var rect in _numberTextRects)
        {
            _uiDefaultSizeMap.Add(rect,rect.localScale);
        }
        
        

        DoIntroMove();
    }
    
#if UNITY_EDITOR
public bool isManuallyInvoked;
private void Update()
{
    if (isManuallyInvoked)
    {
        onStageClear?.Invoke();
        isManuallyInvoked = false;
    }


}
#endif


    private void BindEvent()
    {
        onStageClear -= OnStageClear;
        onStageClear += OnStageClear;
    }

    private void OnDestroy()
    {
        onStageClear -= OnStageClear;
    }

    protected override void Init()
    {
        base.Init();
        
        BindEvent();
        _defaultQuaternionMap = new Dictionary<Transform, Quaternion>();
        _defaultSizeMap = new Dictionary<Transform, Vector3>();
        _uiDefaultSizeMap = new Dictionary<RectTransform, Vector3>();
        _rigidbodies = new Dictionary<Transform, Rigidbody>();
        LoadParticles();
        GetSteps();
    }

#if UNITY_EDITOR
    private bool isChecked;
#endif

    protected override void OnRaySynced()
    {
        base.OnRaySynced();
#if UNITY_EDITOR
        if (!isChecked)
        {
            Debug.Log($"{SceneManager.GetActiveScene().name} : OnRaySynced");
            isChecked = true;
        }

#endif
        // 발판 밟은 직후에는 다음 발판을 누를 수 없게합니다.
        if (_isSuccesssParticlePlaying) return;
        
        // 게임시작전, 게임초기화 시 클릭 X
        if (!_isClickable) return;

        if (CheckOnStep())
        {
            PlaySuccessParticle(_currentStep);
        }
    }

    private void LoadParticles()
    {
        var inducingPs = Resources.Load<GameObject>(PATH + "CFX_inducingParticle");
        var successPs = Resources.Load<GameObject>(PATH + "CFX_successParticle");
        
        if(inducingPs != null)
        {
            _inducingParticle = Instantiate(inducingPs ,transform).GetComponent<ParticleSystem>();;
            _inducingParticle.Stop();
        }
        else
        { 
#if UNITY_EDITOR
         
            Debug.LogError($"널에러 발생: {PATH}" + "CFX_inducingParticle");
#endif
        }
        
        if(successPs != null)
        {
            _successParticle = Instantiate(successPs,transform).GetComponent<ParticleSystem>();;
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

        targetPos = new Vector3[_stepCount];
        for (var i = 0; i < _stepCount; i++) targetPos[i] = _steps[i].transform.position;
        
        foreach (var step in _steps)
        {
            _defaultSizeMap.Add(step, step.localScale);
            _rigidbodies.Add(step,step.GetComponent<Rigidbody>());
          
            _defaultQuaternionMap.Add(step,step.rotation);
            step.position += defaultOffset;
        }
        

    }

    public float numberDoScaleSize;
    private float _defalutSize;


    private void OnScaleSequenceKilled(RectTransform number)
    {
      //  if (_scaleBackSequence.IsActive()) _scaleBackSequence.Kill();

        _scaleBackSequence = DOTween.Sequence();

#if UNITY_EDITOR
  
#endif
        _scaleBackSequence.Append(number.DOScale(_uiDefaultSizeMap[number], 0.8f).SetEase(Ease.Linear));
        _scaleBackSequence.Play();
    }

    private void OnScaleSequenceKilled(Transform number)
    {
        //if (_scaleBackSequence.IsActive()) _scaleBackSequence.Kill();

        _scaleBackSequence = DOTween.Sequence();

#if UNITY_EDITOR
#endif
        _scaleBackSequence
            .Append(number.DOScale(_defaultSizeMap[number], 0.8f).SetEase(Ease.Linear));
        _scaleBackSequence.Play();
    }

    private void DoScaleUp(RectTransform number)
    {
        _currentScaleSequence = DOTween.Sequence();

        _currentScaleSequence
            .Append(number
                .DOScale(numberDoScaleSize, 0.45f)
                .OnKill(() => { OnScaleSequenceKilled(number); })
                .SetEase(Ease.Linear))
           
            .Append(number
                .DOScale(_uiDefaultSizeMap[number], 0.45f)
                .OnKill(() => { OnScaleSequenceKilled(number); })
                .SetEase(Ease.Linear))
            
            .AppendInterval(0.5f)
            
            
            .SetLoops(-1, LoopType.Yoyo); // 무한 반복 설정

        _currentScaleSequence.Play();
    }

    private void DoScaleUp(Transform step)
    {
        _stepCurrentScaleSequence = DOTween.Sequence();

        _stepCurrentScaleSequence
            .Append(step
                .DOScale(_defaultSizeMap[step]*_stepSizeChangeRate, 0.45f)
                .OnKill(() => { OnScaleSequenceKilled(step); })
                .SetEase(Ease.Linear))
           
            .Append(step
                .DOScale(_defaultSizeMap[step], 0.45f)
                .OnKill(() => { OnScaleSequenceKilled(step); })
                .SetEase(Ease.Linear))
            
            .AppendInterval(0.5f)
            
            
            .SetLoops(-1, LoopType.Yoyo); // 무한 반복 설정

        _stepCurrentScaleSequence.Play();
    }


    private int _currentStep = 0;
    private bool _isGameFinished;
    private bool _isClickable;

    private void PlayInducingParticle(int currentPosition)
    {
        if (currentPosition >= _steps.Length) return;
        
            _inducingParticle.Stop();
            _inducingParticle.gameObject.transform.position = AddOffset(_steps[currentPosition].position);
            _inducingParticle.gameObject.SetActive(true);
            _inducingParticle.Play();
        
    }

    private Sequence _currentScaleSequence;
    private Sequence _stepCurrentScaleSequence;
    private Sequence _scaleBackSequence;
    public static event Action onStageClear;
    private Vector3 AddOffset(Vector3 position)
    {
        return position + Vector3.forward * offset;
    }

    public float nextStepInducingParticleDelay;

    private void PlaySuccessParticle(int currentPosition)
    {
        if (_isSuccesssParticlePlaying) return;
        
        _isSuccesssParticlePlaying = true;
        _currentScaleSequence.Kill();
        _stepCurrentScaleSequence.Kill();

#if UNITY_EDITOR
      
#endif

        _inducingParticle.Stop();
        _successParticle.Stop();
        _successParticle.gameObject.transform.position = AddOffset(_steps[currentPosition].position);
        _successParticle.gameObject.SetActive(true);
        _successParticle.Play();

        int randomEffectSoundIndex = UnityEngine.Random.Range(1, 7);
        Managers.Sound.Play(SoundManager.Sound.Effect, path: "Audio/Hopscotch/chime_tinkle_wood_bell_positive_0"+$"{randomEffectSoundIndex}",volume:0.25f);

        DOVirtual
            .Float(0, 0, successParticleDuration, val => val++)
            .OnComplete(() =>
            {
                _successParticle.Stop();
                _successParticle.gameObject.SetActive(false);

                DOVirtual
                    .Float(0, 0, nextStepInducingParticleDelay, val => val++)
                    .OnComplete(() =>
                    {
                        _isSuccesssParticlePlaying = false;
                        _currentStep++;


                        if (_currentStep >= _stepCount)
                        {

                            onStageClear?.Invoke();
                        }
                        else
                        {
                            if (_currentStep == 2)
                                DoScaleUp(_steps[1]);
                            else
                                DoScaleUp(_steps[_currentStep]);

                            DoScaleUp(_numberTextRects[_currentStep]);
                            PlayInducingParticle(_currentStep);
                        }
                    });
            });
    }
    
    public Vector3[] targetPos;
    public float stackInterval;
    public Vector3 defaultOffset;

    private void DoIntroMove()
    {
        for (var i = 0; i < _stepCount; ++i)
        {
            var i1 = i;
            _steps[i].transform
                .DOMove(targetPos[i], 1f + stackInterval * i)
                .OnComplete(() =>
                {
                    if (i1 >= _stepCount - 1)
                    {
                        DOVirtual
                            .Float(0, 0, 2, val => val++)
                            .OnComplete(() =>
                            {
                                _cg.DOFade(1, 1);
#if UNITY_EDITOR
                             
#endif
                             
                                if (i1 >= _stepCount - 1)
                                {
                                    DoScaleUp(_numberTextRects[0]);
                                    DoScaleUp(_steps[0]);
                                }
                                
                                //DoScaleUp(_numberTextRects[1]);
                                if (i1 >= _stepCount - 1) PlayInducingParticle(0);
                                _isClickable = true;
                            });
                    }
                      
                })
                .SetDelay(2f);
        }
    }

    public float waitTimeToRestartGame;
    public ParticleSystem _stageClearBubble;
    public static event Action onStageParticlePlay; 

    public float randomForceMax;
    private void OnStageClear()
    {
        _currentStep = 0;
        _cg.DOFade(0, 0.5f);
      
        
        DOVirtual.Float(0, 0, waitTimeToRestartGame, val => val++)
            .OnComplete(() =>
            {
                PlayInducingParticle(_currentStep);
            });

        foreach (var step in _steps)
        {
            _rigidbodies[step].constraints = RigidbodyConstraints.None;
            _rigidbodies[step].AddForce(Vector3.down * UnityEngine.Random.Range(5,randomForceMax),ForceMode.Impulse);
        }
        
        
        DOVirtual.Float(0, 1, 3.3f, _ => { })
            .OnComplete(() =>
            {
                _stageClearBubble.Play();
                onStageParticlePlay?.Invoke();
            });
        
        DOVirtual.Float(0, 1, 4f, _ => { }).OnComplete(() =>
        {
            foreach (var step in _steps)
            {
                _rigidbodies[step].constraints = RigidbodyConstraints.FreezeAll;
            }
            foreach (var step in _steps)
            {
                step.position += defaultOffset;
              
                step.rotation = _defaultQuaternionMap[step];
            }
            
            
        });
        
        
        

        DOVirtual.Float(0, 1, 6f, _ => { })
   
            .OnComplete(() =>
            {
                
                DoIntroMove();
            });
    
        
        

#if UNITY_EDITOR
Debug.Log("step Collapsing!!" );
#endif
    }
    private bool CheckOnStep()
    {

       
        foreach (var hit in GameManager_Hits)
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
