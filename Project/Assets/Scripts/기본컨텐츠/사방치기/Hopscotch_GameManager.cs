using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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
    private readonly string PATH = "게임별분류/기본컨텐츠/Hopscotch/";

    public float successParticleDuration;
    private bool _isSuccesssParticlePlaying;
    public float offset;

    private readonly float _stepSizeChangeRate = 1.11f;
    private Dictionary<Transform, Quaternion> _defaultQuaternionMap;
    private Dictionary<Transform, Vector3> _defaultSizeMap;
    private Dictionary<RectTransform, Vector3> _uiDefaultSizeMap;
    private Dictionary<Transform, Rigidbody> _rigidbodies;

    private readonly float _stageResetDelay = 4f;


    public float waitTimeToRestartGame;
    public ParticleSystem _stageClearBubble;
    public static event Action onStageParticlePlay;

    //stage 성공시 표출 UI
    private RectTransform _stageClearUI;
    private Vector3 _stageClearUIDefaultScale;

    public float randomForceMax;

    private CanvasGroup _numCvGrup;
    private Sequence _currentScaleSequence;
    private Sequence _stepCurrentScaleSequence;
    private Sequence _scaleBackSequence;
    public static event Action onStageClear;
    public float nextStepInducingParticleDelay;

    private Vector3 _camDefaultPosition;

    private void Start()
    {
        var inPlayTexts = GameObject.Find("InPlayTexts");

        _numCvGrup = inPlayTexts.GetComponent<CanvasGroup>();
        _numCvGrup.DOFade(0, 0.01f);
        if (inPlayTexts != null)
            _numberTextRects = inPlayTexts.GetComponentsInChildren<RectTransform>()
                .Where(rt => rt.gameObject != inPlayTexts)
                .ToArray();
        else
            Debug.LogError("GameObject named 'InPlayTexts' not found in the scene.");

        foreach (var rect in _numberTextRects) _uiDefaultSizeMap.Add(rect, rect.localScale);

   
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

        UI_Scene_Button.onBtnShut -= DoIntroMove;
        UI_Scene_Button.onBtnShut += DoIntroMove;
    }

    private void OnDestroy()
    {
        UI_Scene_Button.onBtnShut -= DoIntroMove;
        onStageClear -= OnStageClear;
    }

    protected override void Init()
    {
        _defaultQuaternionMap = new Dictionary<Transform, Quaternion>();
        _defaultSizeMap = new Dictionary<Transform, Vector3>();
        _uiDefaultSizeMap = new Dictionary<RectTransform, Vector3>();
        _rigidbodies = new Dictionary<Transform, Rigidbody>();

        
      
        if (Camera.main != null) _camDefaultPosition = Camera.main.transform.position;


        base.Init();
        
        BindEvent();
        InitializeStageClearUI();
        LoadParticles();
        GetSteps();
    }


    private void InitializeStageClearUI()
    {
        
        _stageClearUI = GameObject.Find("StageClearUI").GetComponent<RectTransform>();
        _stageClearUIDefaultScale = _stageClearUI.localScale;
        _stageClearUI.localScale = Vector3.zero;
        _stageClearUI.transform.gameObject.SetActive(false);
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

        if (CheckOnStep()) OnCorrectStep(_currentStep);
    }

    private void LoadParticles()
    {
        var inducingPs = Resources.Load<GameObject>(PATH + "CFX_inducingParticle");
        var successPs = Resources.Load<GameObject>(PATH + "CFX_successParticle");

        if (inducingPs != null)
        {
            _inducingParticle = Instantiate(inducingPs, transform).GetComponent<ParticleSystem>();
            ;
            _inducingParticle.Stop();
        }
        else
        {
#if UNITY_EDITOR

            Debug.LogError($"널에러 발생: {PATH}" + "CFX_inducingParticle");
#endif
        }

        if (successPs != null)
        {
            _successParticle = Instantiate(successPs, transform).GetComponent<ParticleSystem>();
            _successParticle.Stop();
        }
    }


    private void GetSteps()
    {
        _stepCount = stepGroup.childCount;
        _steps = new Transform[_stepCount];

        for (var i = 0; i < _stepCount; ++i) _steps[i] = stepGroup.GetChild(i);

        targetPos = new Vector3[_stepCount];
        for (var i = 0; i < _stepCount; i++) targetPos[i] = _steps[i].transform.position;

        foreach (var step in _steps)
        {
            _defaultSizeMap.Add(step, step.localScale);
            _rigidbodies.Add(step, step.GetComponent<Rigidbody>());

            _defaultQuaternionMap.Add(step, step.rotation);
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
        // step.DORotate(step.transform.rotation.eulerAngles + new Vector3(-20,0,0), 0.2f)
        //     .OnComplete(() =>
        //     {
        //         step.DORotate(step.transform.rotation.eulerAngles + new Vector3(40,0,0), 0.2f);
        //     });


        _scaleBackSequence.Append(number.DOScale(_uiDefaultSizeMap[number], 0.8f).SetEase(Ease.Linear));
        _scaleBackSequence.Play();
    }

    private void OnScaleSequenceKilled(Transform step)
    {
        //if (_scaleBackSequence.IsActive()) _scaleBackSequence.Kill();

        _scaleBackSequence = DOTween.Sequence();

#if UNITY_EDITOR
#endif

        step.DORotateQuaternion(_defaultQuaternionMap[step] * Quaternion.Euler(-30, 0, 0), 0.33f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                step.DORotateQuaternion(_defaultQuaternionMap[step] * Quaternion.Euler(30, 0, 0), 0.33f)
                    .OnComplete(() =>
                    {
                        _scaleBackSequence.Append(
                            step.DOScale(_defaultSizeMap[step], 0.2f)
                                .OnStart(() =>
                                {
                                    step.DORotateQuaternion(_defaultQuaternionMap[step], 0.5f);
#if UNITY_EDITOR
                                    Debug.Log("제자리 돌아오기 및 회전 트윈");
#endif
                                })
                                .SetEase(Ease.Linear));
                    });
            });


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
                .DOScale(_defaultSizeMap[step] * _stepSizeChangeRate, 0.45f)
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


    private int _currentStep;
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


    private Vector3 AddOffset(Vector3 position)
    {
        return position + Vector3.forward * offset;
    }



    private void OnCorrectStep(int currentPosIndex)
    {
        ShakeCam();
        PlaySuccessParticle(currentPosIndex);
        
    }
    private void ShakeCam()=> Camera.main.DOShakePosition(1.2f, 0.5f, 7).OnComplete(()=>
        {
            Camera.main.transform.DOMove(_camDefaultPosition, 0.5f);
        });

    private void PlaySuccessParticle(int currentPosition)
    {
        if (_isSuccesssParticlePlaying) return;
        

        //중복 실행 방지
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

        var randomEffectSoundIndex = Random.Range(1, 7);
        Managers.Sound.Play(SoundManager.Sound.Effect,
            "Audio/Hopscotch/chime_tinkle_wood_bell_positive_0" + $"{randomEffectSoundIndex}", 0.25f);

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
                        DOVirtual
                            .Float(0, 0, 2, val => val++)
                            .OnComplete(() =>
                            {
                                _numCvGrup.DOFade(1, 1);
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
                })
                .SetDelay(2f);
        }
    }

    private void OnStageClear()
    {
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/Hopscotch/Effect_onStageClear", 0.3f);
     
        _currentStep = 0;
        _numCvGrup.DOFade(0, 0.4f);

        _stageClearUI.transform.gameObject.SetActive(true);
        _stageClearUI.localScale = Vector3.zero;
        _stageClearUI
            .DOScale(_stageClearUIDefaultScale, 2f)
            .OnComplete(() =>
                {
                    DOVirtual.Float(0, 0, 3.5f, val => val++)
                        .OnComplete(() =>
                        {
                            _stageClearUI
                                .DOScale(Vector3.zero, 1.2f)
                                .OnComplete(() =>
                                {
                                    _stageClearUI.transform.gameObject.SetActive(false);
                                });
                        });
                }
            ).SetDelay(2f);// 성공 시 - 성공 애니메이션 표출까지 걸리는 시간에 대한 Delay값  

        

        DOVirtual.Float(0, 0, waitTimeToRestartGame, val => val++)
            .OnComplete(() => { PlayInducingParticle(_currentStep); });

        foreach (var step in _steps)
        {
            _rigidbodies[step].constraints = RigidbodyConstraints.None;
            _rigidbodies[step].AddForce(Vector3.down * Random.Range(5, randomForceMax), ForceMode.Impulse);
        }


        DOVirtual.Float(0, 1, 3.3f, _ => { })
            .OnComplete(() =>
            {
                _stageClearBubble.Play();
                onStageParticlePlay?.Invoke();
            });

        DOVirtual.Float(0, 1, 4f, _ => { }).OnComplete(() =>
        {
            foreach (var step in _steps) _rigidbodies[step].constraints = RigidbodyConstraints.FreezeAll;
            foreach (var step in _steps)
            {
                step.position += defaultOffset;

                step.rotation = _defaultQuaternionMap[step];
            }
        });


        DOVirtual.Float(0, 1, _stageResetDelay, _ => { })
            .OnComplete(() => { DoIntroMove(); });


#if UNITY_EDITOR
        Debug.Log("step Collapsing!!");
#endif
    }

    private bool CheckOnStep()
    {
        foreach (var hit in GameManager_Hits)
            if (hit.transform.gameObject.name == "Step_" + _currentStep)
            {
#if UNITY_EDITOR
                Debug.Log("Step_" + _currentStep);
#endif
                return true;
            }

        return false;
    }
}