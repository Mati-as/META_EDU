using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EasternArt_GameManager : MonoBehaviour
{
    [Header("gameObjs")] public Transform camera;
    public SpriteRenderer originalSpriteRenderer;

    [Space(15f)] [Header("LookAt")] public Transform lookAtA;
    public Transform lookAtB;


    public Transform[] cameraPath;
    public Transform arrivalB;

    private Vector3[] _pathVector;
    private Vector3[] _newVector;

    [Header("Skinned Picture")] public GameObject skinnedPicture;

    private Transform[] _skinnedPictureChildren;

public Animator mainTigerAnimator;
    private float _defaultAnimatorSpeed;
    private Sequence mainTigerSequence;
    private bool _isMainTigerAnimPlaying;
    
    public static readonly int RIGHT_IDLE = Animator.StringToHash("Right"); 
    public static readonly int RIGHT_GROWLING = Animator.StringToHash("RightGrowling"); 
    
    public static readonly int LEFT_IDLE = Animator.StringToHash("Left");
    public static readonly int LEFT_GROWLING = Animator.StringToHash("LeftGrowling");
    public float animationInterval = 10f;

    private AudioSource _tigerGrowlingAudioSource;

    private AudioClip _tigerGrowlA;
    private AudioClip _tigerGrowlB;
    private AudioClip _tigerGrowlC;

    private AudioClip[] _tigerGrowlClips;

    public static event Action OnSkinnedAnimStart ;
    
    private void Awake()
    {

        _tigerGrowlingAudioSource = gameObject.AddComponent<AudioSource>();
        
        _tigerGrowlingAudioSource.volume = 0.2f;
        _tigerGrowlingAudioSource.playOnAwake = false;
      
        _tigerGrowlA = Resources.Load<AudioClip>("게임별분류/명화컨텐츠/동양화/" + nameof(_tigerGrowlA));
        _tigerGrowlB = Resources.Load<AudioClip>("게임별분류/명화컨텐츠/동양화/" + nameof(_tigerGrowlB));
        _tigerGrowlC = Resources.Load<AudioClip>("게임별분류/명화컨텐츠/동양화/" + nameof(_tigerGrowlC));


        _tigerGrowlClips = new AudioClip[3];
        _tigerGrowlClips[0] = _tigerGrowlA;
        _tigerGrowlClips[1] = _tigerGrowlB;
        _tigerGrowlClips[2] = _tigerGrowlC;
            
        _defaultAnimatorSpeed = mainTigerAnimator.speed;
        mainTigerAnimator.speed = 0;
        
        _pathVector = new Vector3[3];
        _newVector = new Vector3[2];

        for (var i = 0; i < cameraPath.Length; i++)
        {
            _pathVector[i] = cameraPath[i].position;
         
        }

        _skinnedPictureChildren = new Transform[skinnedPicture.transform.childCount];
        
        
        for (int i = 0; i < _skinnedPictureChildren.Length; i++)
        {
            _skinnedPictureChildren[i] = skinnedPicture.transform.GetChild(i);
        }

        newBackground.DOFade(0, 0.1f);

    }

    [SerializeField]
    private GameObject originalPicture;
    [SerializeField]
    private SpriteRenderer newBackground;


    
    private void Start()
    {
        camera.DOLookAt(lookAtA.position, 0.01f);

        camera.position = _pathVector[0];

        camera.DOPath(_pathVector, 3.5f, PathType.CatmullRom)
            .SetLookAt(lookAtA, true)
            .OnComplete(() =>
            {
                newBackground.maskInteraction = SpriteMaskInteraction.None;
                newBackground.DOFade(1, 1.5f);
               //  _spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
               originalSpriteRenderer.DOFade(0, 1.5f)
                     .OnComplete(() =>
                 {
                     originalPicture.SetActive(false);
                 });
               
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
                                        mainTigerAnimator.speed = _defaultAnimatorSpeed;
                                        PlayMainTigerAnimation();
                                    });

                                });
                            });
                    });
            })
            .SetDelay(1.5f);
    }

    private Sequence _pollingSequence;
    private void PlayMainTigerAnimation()
    {
        if(_isMainTigerAnimPlaying) return;
        _isMainTigerAnimPlaying = true;
        
        
        //1. start에서 Idle 애니메이션 재생상태..
        mainTigerSequence = DOTween.Sequence();

        //2. animationInterval 종료 후 왼쪽바라보기
        mainTigerSequence
            .Append(DOVirtual.Float(0, 0, animationInterval, val => val++)
                .OnComplete(() =>
                {
#if UNITY_EDITOR
                    Debug.Log("LEFT_IDLE");
#endif
                    
                    mainTigerAnimator.SetBool(LEFT_IDLE, true);
                }))

            //2. animationInterval 종료 후 LeftIdle및 Growling 재생
            .Append(DOVirtual.Float(0, 0, animationInterval, val => val++)
                .OnComplete(() =>
                {
#if UNITY_EDITOR
                    Debug.Log("LEFT_GROWLING");
#endif
                    _tigerGrowlingAudioSource.clip = _tigerGrowlClips[Random.Range(0, _tigerGrowlClips.Length)];
                    _tigerGrowlingAudioSource.Play();

                    _pollingSequence = DOTween.Sequence();
                    _pollingSequence.Append(DOVirtual.Float(0, 1, animationInterval, _ =>
                    {
                        AnimatorStateInfo
                            stateInfo = mainTigerAnimator.GetCurrentAnimatorStateInfo(0); // 0은 base layer를 의미
                        
                        Debug.Log("isGrowling Available checking....");
                        if (stateInfo.normalizedTime % 1 < 0.05f)
                        {
                           
                            Debug.Log("Growling Again");

                            _tigerGrowlingAudioSource.Play();

                            mainTigerAnimator.SetBool(RIGHT_GROWLING, true);
                        }
                    }));
                    
                    mainTigerAnimator.SetBool(LEFT_GROWLING, true);
                }))

            .Append(DOVirtual.Float(0, 0, animationInterval, val => val++)
                .OnComplete(() =>
                {
#if UNITY_EDITOR
                    Debug.Log("IDLE");
#endif
                    _pollingSequence.Kill();
                    InitializeAnimParams();
                }))

            //3. LeftGrowling 종료 후 RightIdle 및 RightGrowling 재생
            .Append(DOVirtual.Float(0, 0, animationInterval, val => val++)
                .OnComplete(() =>
                {
#if UNITY_EDITOR
                    Debug.Log("RIGHT_IDLE");
#endif
                    mainTigerAnimator.SetBool(RIGHT_IDLE, true);
                
                }))
            
            //4. 초기화 및 무한 반복 
            .Append(DOVirtual.Float(0, 0, animationInterval, val => val++)
                .OnComplete(() =>
                {
#if UNITY_EDITOR
                    Debug.Log("RIGHT_GROWLING");
#endif
                    _tigerGrowlingAudioSource.clip = _tigerGrowlClips[Random.Range(0, _tigerGrowlClips.Length)];
                    _tigerGrowlingAudioSource.Play();
                    mainTigerAnimator.SetBool(RIGHT_GROWLING, true);
                    
                    _pollingSequence = DOTween.Sequence();
                    _pollingSequence.Append(DOVirtual.Float(0, 1, animationInterval, _ =>
                    {
                        AnimatorStateInfo
                            stateInfo = mainTigerAnimator.GetCurrentAnimatorStateInfo(0); // 0은 base layer를 의미
                       
                        Debug.Log("isGrowling Available checking....");
                        if (stateInfo.normalizedTime % 1 < 0.05f)
                        {
                            
                            Debug.Log("Growling Again");

                            _tigerGrowlingAudioSource.Play();

                           
                        }
                    }));
                }))
            
            .Append(DOVirtual.Float(0, 0, animationInterval, val => val++)
                .OnComplete(() =>
                {
                    _pollingSequence.Kill();
                    InitializeAnimParams();
                }))
      
           
            .SetLoops(-1,LoopType.Restart);


        mainTigerSequence.Play();
    }

    private void InitializeAnimParams()
    {
        mainTigerAnimator.SetBool(RIGHT_IDLE,false);
        mainTigerAnimator.SetBool(LEFT_IDLE,false);
        mainTigerAnimator.SetBool(RIGHT_GROWLING,false);
        mainTigerAnimator.SetBool(LEFT_GROWLING,false);
    }
}