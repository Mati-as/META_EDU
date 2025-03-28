using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MusicInstruments_ParrotUIController : MonoBehaviour
{
    private Slider _parrotSlider;
    private Animator _parrotAnimator;

    private readonly int RUN_ANIM = Animator.StringToHash("Run");
    private readonly int IDLE_ANIM = Animator.StringToHash("Idle");
    private readonly int FLY_ANIM = Animator.StringToHash("Fly");
    private readonly int SPIN_ANIM = Animator.StringToHash("Spin");


    private Sequence _autoSeq;
    private Quaternion _defaultQuat;
    private ParticleSystem _ps;
    private bool _isParticlePlaying;

    private void Start()
    {
        _parrotSlider = GameObject.Find("ParrotSlider").GetComponent<Slider>();
        _ps = GameObject.Find("CFX_MusicInstruments").GetComponent<ParticleSystem>();

        _parrotAnimator = GetComponent<Animator>();
        _defaultQuat = transform.rotation;
        PlayAutomatic(transform, 80f, _defaultQuat);

        _parrotSlider.onValueChanged.AddListener(HandleSliderValueChanged);
    }

    private void OnDestroy()
    {
        _parrotSlider.onValueChanged.RemoveListener(HandleSliderValueChanged);
    }

    private bool _isSoundPlaying;
    private float _previousSliderValue;

    private void HandleSliderValueChanged(float value)
    {
        var currentVal = _parrotSlider.value;

      
        if (currentVal < _previousSliderValue)
        {
            _previousSliderValue = currentVal;
            return;
        }

      
        if (currentVal < 0.24f)
        {
            SetAnimatorState(true);
            _isSoundPlaying = false;
            _previousSliderValue = currentVal;
            return;
        }

      
        if (!_isSoundPlaying &&
            ((currentVal > 0.30f && currentVal < 0.32f) || (currentVal > 0.66f && currentVal < 0.67f)))
        {
            _isSoundPlaying = true;
            var soundPath = currentVal < 0.32f ? "Audio/BD004/OnParrotSliderFirst" : "Audio/BD004/OnParrotSliderSecond";
            Managers.Sound.Play(SoundManager.Sound.Narration, soundPath, 1f);
        }

      
        else if (currentVal > 0.30f && currentVal < 0.66f)
        {
            SetAnimatorState(run: true);

            _isParticlePlaying = false;
            _isMaxAudioPlaying = false;

            if (!_isSoundPlaying)
            {
                _isSoundPlaying = true;
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/MusicInstruments/Run", 0.3f);
            }
        }

       
        else if (currentVal > 0.66f && currentVal < 0.90f)
        {
            if (currentVal < 0.77f)
            {
                _isParticlePlaying = false;
                _isSoundPlaying = false;
            }
            SetAnimatorState(fly: true);
        }

       
 
     
        if (currentVal > 0.97f)
        {
            SetAnimatorState(spin: true);

            if (!_isParticlePlaying)
            {
                _isParticlePlaying = true;
                _ps.Play();
                
                if (!_isSoundPlaying && !_isMaxAudioPlaying)
                {
                    _isMaxAudioPlaying = true;
                    _isSoundPlaying = true;

                  
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/MusicInstruments/Spin", 0.2f);
                    
                    var randomChar = (char)Random.Range('A', 'C' + 1);
                    Managers.Sound.Play(SoundManager.Sound.Narration, $"Audio/BD004/OnParrotSliderMax"+ randomChar,
                        1f);
                }
            }
        }

        
        _previousSliderValue = currentVal;
    }


    private void SetAnimatorState(bool idle = false, bool run = false, bool fly = false, bool spin = false)
    {
        _parrotAnimator.SetBool(IDLE_ANIM, idle);
        _parrotAnimator.SetBool(RUN_ANIM, run);
        _parrotAnimator.SetBool(FLY_ANIM, fly);
        _parrotAnimator.SetBool(SPIN_ANIM, spin);
    }


    private bool _isMaxAudioPlaying; // 15초마다 한번씩 실행 될 수 있도록 함.
    private float _elapsed;

    private void Update()
    {
        if (_isMaxAudioPlaying)
        {
            _elapsed += Time.deltaTime;
            Logger.Log($"waiting for max replay....{_elapsed}");
            
            if (_elapsed > 15)
            {
                _isSoundPlaying = false;
                _isMaxAudioPlaying = false;
                _isParticlePlaying = false;
                
                _elapsed = 0;
            }
        }
    }


    private void PlayAutomatic(Transform thisTransform, float rotateAmount, Quaternion defaultRotation)
    {
#if UNITY_EDITOR
        Debug.Log("cymbal");
#endif


        // 시퀀스 중복실행방지용
        if (_autoSeq != null && _autoSeq.IsActive() && _autoSeq.IsPlaying()) return;


        _autoSeq = DOTween.Sequence();

        _autoSeq
            .Append(thisTransform
                .DORotateQuaternion(defaultRotation * Quaternion.Euler(0, Random.Range(-rotateAmount, rotateAmount), 0),
                    1f)
                .SetEase(Ease.InOutSine))
            .AppendInterval(Random.Range(0.5f, 0.8f))
            .Append(thisTransform.DORotateQuaternion(defaultRotation * Quaternion.Euler(0, 0, 0), 1)
                .SetEase(Ease.OutQuint))
            .AppendInterval(Random.Range(0.5f, 1.1f))
            .SetLoops(-1, LoopType.Yoyo);
    }
}