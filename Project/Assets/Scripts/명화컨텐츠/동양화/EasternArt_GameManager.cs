using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class EasternArt_GameManager : IGameManager
{
    
    public static readonly int RIGHT_IDLE = Animator.StringToHash("Right");
    public static readonly int RIGHT_GROWLING = Animator.StringToHash("RightGrowling");
    public static readonly int LEFT_IDLE = Animator.StringToHash("Left");
    public static readonly int LEFT_GROWLING = Animator.StringToHash("LeftGrowling");
    
    
    [Header("gameObjs")] public Transform camera;
    public SpriteRenderer originalSpriteRenderer;


    [Space(15f)] [Header("LookAt")] public Transform lookAtA;
    public Transform lookAtB;


    public Transform[] cameraPath;
    public Transform arrivalB;

    private Vector3[] _pathVector;
    private Vector3[] _newVector;

    [Header("Skinned Picture")] 
    public GameObject skinnedPicture;

    private Transform[] _skinnedPictureChildren;

    public Animator mainTigerAnimator;
    private float _defaultAnimatorSpeed;
    private Sequence mainTigerSequence;
    private bool _isMainTigerAnimPlaying;

    
    public float animationInterval = 10f;
    public float growlingDuration = 2.0f;

    private AudioSource _tigerGrowlingAudioSource;

    private AudioClip _tigerGrowlA;
    private AudioClip _tigerGrowlB;
    private AudioClip _tigerGrowlC;

    private AudioClip[] _tigerGrowlClips;


    [SerializeField] private GameObject originalPicture;
    [SerializeField] private SpriteRenderer newBackground;

    [FormerlySerializedAs("postProcessVolume")] public Volume vol;
    private Vignette vignette;
    protected override void Init()
    {
        
        Camera.main.TryGetComponent<Volume>(out vol);
            
        if (vol == null)
        {
            Debug.LogError("PostProcessVolume not assigned.");
            return;
        }

        if (vol.profile.TryGet<Vignette>(out vignette))
        {
            vignette = vol.profile.components.Find(x => x is Vignette) as Vignette;
        }
        else
        {
            Debug.LogError("Vignette not found in PostProcessVolume.");
        }

        vignette.intensity.value = 1;
        DOVirtual.Float(1, 0, 2.5f, val =>
        {
            vignette.intensity.value = val;
        });
                
        base.Init();


        LoadAsset();
        SetAudio();
        SetPath();

        camera.position = _pathVector[0];

        OnBtnShut();
        
        // UI_Scene_Button.onBtnShut -= OnBtnShut;
        // UI_Scene_Button.onBtnShut += OnBtnShut;

    }


    private void LoadAsset()
    {
        _tigerGrowlA = Resources.Load<AudioClip>("게임별분류/명화컨텐츠/동양화/" + nameof(_tigerGrowlA));
        _tigerGrowlB = Resources.Load<AudioClip>("게임별분류/명화컨텐츠/동양화/" + nameof(_tigerGrowlB));
        _tigerGrowlC = Resources.Load<AudioClip>("게임별분류/명화컨텐츠/동양화/" + nameof(_tigerGrowlC));
    }

    private void SetAudio()
    {
        _tigerGrowlingAudioSource = gameObject.AddComponent<AudioSource>();
        _tigerGrowlingAudioSource.volume = 0.2f;
        _tigerGrowlingAudioSource.playOnAwake = false;
    }

    private void SetPath()
    {
        _tigerGrowlClips = new AudioClip[3];
        _tigerGrowlClips[0] = _tigerGrowlA;
        _tigerGrowlClips[1] = _tigerGrowlB;
        _tigerGrowlClips[2] = _tigerGrowlC;

        _defaultAnimatorSpeed = mainTigerAnimator.speed;
        mainTigerAnimator.speed = 0;

        _pathVector = new Vector3[3];
        _newVector = new Vector3[2];

        for (var i = 0; i < cameraPath.Length; i++) _pathVector[i] = cameraPath[i].position;

        _skinnedPictureChildren = new Transform[skinnedPicture.transform.childCount];


        for (var i = 0; i < _skinnedPictureChildren.Length; i++)
            _skinnedPictureChildren[i] = skinnedPicture.transform.GetChild(i);

        newBackground.DOFade(0, 0.1f);
        camera.DOLookAt(lookAtA.position, 0.01f);
    }

    private void OnBtnShut()
    {
#if UNITY_EDITOR
        Debug.Log($"{SceneManager.GetActiveScene().name}'s started");
#endif
        StartEasternArtAnim();
    }

    private void StartEasternArtAnim()
    {
        camera.DOPath(_pathVector, 3.5f, PathType.CatmullRom)
            .SetLookAt(lookAtA, true)
            .OnComplete(() =>
            {
                newBackground.maskInteraction = SpriteMaskInteraction.None;
                newBackground.DOFade(1, 1.5f);
                //  _spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
                originalSpriteRenderer.DOFade(0, 1.5f)
                    .OnComplete(() => { originalPicture.SetActive(false); });

                foreach (var obj in _skinnedPictureChildren) obj.gameObject.SetActive(true);

                _newVector[0] = camera.position;
                _newVector[1] = arrivalB.position;


                var currentLookat = new Vector3();

                camera.DOPath(_newVector, 3.5f)
                    .SetEase(Ease.InOutQuint
                    ).OnStart(() =>
                    {
                        DOVirtual.Float(0, 1, 3.8f,
                            reval =>
                            {
                                currentLookat = Vector3.Lerp(lookAtA.position, lookAtB.position, reval);
                                camera.DOLookAt(currentLookat, 0.01f).OnComplete(() =>
                                {
                                    DOVirtual.Float(0, 0, 1f, val => val++).OnComplete(() =>
                                    {
                                        //맨처음에 호랑이가 커지는 애니메이션을 울움소리 미동반, 애니메이션 빠르게 재생
                                        mainTigerAnimator.speed = _defaultAnimatorSpeed;
                                        PlayMainTigerAnimation();
                                        DOVirtual.Float(0, 0, 1.8f,
                                            _ => { mainTigerAnimator.speed = _defaultAnimatorSpeed * 3f; }).OnComplete(
                                            () =>
                                            {
                                                //초반에 Idle상태로 만들기위한 설정, PlayMainTigerAnimation에는 영향없음
                                                mainTigerAnimator.SetBool(LEFT_IDLE, true);
                                                mainTigerAnimator.speed = _defaultAnimatorSpeed;
                                            });
                                    });
                                });
                            });
                    });
            })
            //버튼 클릭 후 시작까지의 대기시간.
            .SetDelay(1.5f);
    }

    private Sequence _pollingSequence;

    private void PlayMainTigerAnimation()
    {
        if (_isMainTigerAnimPlaying) return;
        _isMainTigerAnimPlaying = true;

        // Clear existing sequence if needed
        if (mainTigerSequence != null && mainTigerSequence.IsActive()) mainTigerSequence.Kill();

        mainTigerSequence = DOTween.Sequence();

        // Start with Idle animation
        mainTigerSequence
            .AppendCallback(() => { mainTigerAnimator.speed = _defaultAnimatorSpeed; })
            .AppendInterval(animationInterval - 3f)

            // Left Idle
            .AppendCallback(() =>
            {
#if UNITY_EDITOR
                Debug.Log("LEFT_IDLE");
#endif
                mainTigerAnimator.speed = _defaultAnimatorSpeed;
                _growlingCount = 0;
                mainTigerAnimator.SetBool(LEFT_IDLE, true);
            })
            .AppendInterval(animationInterval)

            // Left Growling
            .AppendCallback(() =>
            {
#if UNITY_EDITOR
                Debug.Log($"Left_GROWLING Duration: {growlingDuration}");
#endif
                _tigerGrowlingAudioSource.clip = _tigerGrowlClips[Random.Range(0, _tigerGrowlClips.Length)];
                mainTigerAnimator.SetBool(LEFT_GROWLING, true);
                mainTigerAnimator.speed = _defaultAnimatorSpeed * 2.7f;
                DOVirtual.Float(0, 1, growlingDuration, _ => { CheckAndPlayAudio(); });
            })
            .AppendInterval(growlingDuration)

            // Reset parameters and prepare for Right Idle
            .AppendCallback(() =>
            {
#if UNITY_EDITOR
                Debug.Log("Parameters reset and preparing for RIGHT_IDLE");
#endif
                InitializeAnimParams();
            })

            // Right Idle
            .AppendCallback(() =>
            {
#if UNITY_EDITOR
                Debug.Log("RIGHT_IDLE");
#endif
                mainTigerAnimator.speed = _defaultAnimatorSpeed;
                _growlingCount = 0;
                mainTigerAnimator.SetBool(RIGHT_IDLE, true);
            })
            .AppendInterval(animationInterval)

            // Right Growling
            .AppendCallback(() =>
            {
#if UNITY_EDITOR
                Debug.Log($"RIGHT_GROWLING Duration: {growlingDuration}");
#endif
                _tigerGrowlingAudioSource.clip = _tigerGrowlClips[Random.Range(0, _tigerGrowlClips.Length)];
                mainTigerAnimator.SetBool(RIGHT_GROWLING, true);
                mainTigerAnimator.speed = _defaultAnimatorSpeed * 2.7f;
                DOVirtual.Float(0, 1, growlingDuration, _ => { CheckAndPlayAudio(); });
            })
            .AppendInterval(growlingDuration) // Wait for growlingDuration seconds

            // Final reset before loop
            .AppendCallback(() =>
            {
#if UNITY_EDITOR
                Debug.Log("Final reset, sequence will restart");
#endif
                mainTigerAnimator.speed = _defaultAnimatorSpeed;
                InitializeAnimParams(); // Ensure all parameters are reset before looping
            })
            .SetLoops(-1, LoopType.Restart);

        mainTigerSequence.Play();
    }

    private int _growlingCount;
    private bool _isGrowling;

    private void CheckAndPlayAudio()
    {
        var
            stateInfo = mainTigerAnimator.GetCurrentAnimatorStateInfo(0); // 0은 base layer를 의미


        if (_growlingCount < 1)
            if (stateInfo.normalizedTime % 1 < 0.1f && stateInfo.normalizedTime % 1 > 0.05f)
                if (!_isGrowling)
                {
                    _isGrowling = true;
                    _growlingCount++;
                    _tigerGrowlingAudioSource.Play();
                    //호랑이 울음횟수 제한을 위한 _roraCount
                    DOVirtual.Float(0, 0, 3.35f, _ => { })
                        .OnComplete(() =>
                        {
                            _isGrowling = false;
                            _growlingCount--;
                        });
                }
    }

    private void InitializeAnimParams()
    {
        mainTigerAnimator.SetBool(RIGHT_IDLE, false);
        mainTigerAnimator.SetBool(LEFT_IDLE, false);
        mainTigerAnimator.SetBool(RIGHT_GROWLING, false);
        mainTigerAnimator.SetBool(LEFT_GROWLING, false);
    }
}