using System;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using MyCustomizedEditor;
#endif

public class FootstepManager : Base_GameManager
{
 
    private enum FootstepSounds
    {
        Normal1,
        Normal2,
        Found1,
        Found2
    }

    private AudioSource _audioSource;
    public AudioClip[] _audioClips = new AudioClip[4];

    [FormerlySerializedAs("gameManager")] [Header("Reference")] [SerializeField] private GroundGameController gameController;
    [SerializeField] private GroundFootStepData _groundFootStepData;

    public GroundFootStepData GetGroundFootStepData()
    {
        return _groundFootStepData;
    }


    [Space(10)]
    [Header("Footstep Positions")]

#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] firstFootstepsGroup = new GameObject[4];

    public GameObject[] DustGroup1 = new GameObject[4];

    [Space(10)]
#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif

    public GameObject[] secondFootstepsGroup = new GameObject[4];

    public GameObject[] DustGroup2 = new GameObject[4];

    [Space(10)]


#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif


    public GameObject[] thirdFootstepsGroup = new GameObject[4];

    public GameObject[] DustGroup3 = new GameObject[4];

    [Space(10)]
#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] fourthFootstepsGroup = new GameObject[4];

    public GameObject[] DustGroup4 = new GameObject[4];

    [Space(10)]
#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] fifthFootstepsGroup = new GameObject[4];

    public GameObject[] DustGroup5 = new GameObject[4];

    [Space(10)]
#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] sixthFootstepsGroup = new GameObject[4];

    public GameObject[] DustGroup6 = new GameObject[4];

    [Space(10)]
#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] seventhFootstepsGroup = new GameObject[4];

    public GameObject[] DustGroup7 = new GameObject[4];

    [Space(10)]
#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] eighthFootstepsGroup = new GameObject[4];

    public GameObject[] DustGroup8 = new GameObject[4];

    [Space(10)]
#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] ninthFootstepsGroup = new GameObject[4];

    public GameObject[] DustGroup9 = new GameObject[4];

    [Space(10)]
#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] tenthFootstepsGroup = new GameObject[4];

    public GameObject[] DustGroup10 = new GameObject[4];

    [Space(10)]
#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] eleventhFootstepsGroup = new GameObject[4];

    public GameObject[] DustGroup11 = new GameObject[4];

    [Space(10)]
#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] twelfthFootstepsGroup = new GameObject[4];
    public GameObject[] DustGroup12 = new GameObject[4];
    private static readonly int TOTAL_FOOTSTEP_GROUP_COUNT = 12;
    
    private readonly GameObject[][] _footstepGameObjGroups = new GameObject[TOTAL_FOOTSTEP_GROUP_COUNT][];
    private readonly GameObject[][] _dustGameObjGroup = new GameObject[TOTAL_FOOTSTEP_GROUP_COUNT][];
    
    public ReactiveProperty<bool> finishPageTriggerProperty;

    public static int currentFootstepGroupOrder { get; private set; }
    public static int currentFootstepIndexOrder { get; private set; }
    public static event Action OnFootstepClicked;
   
    public UndergroundUIManager undergroundUIManager;
    private Camera _camera;
    private InputAction _mouseClickAction;
    

    protected override void Init()
    {
        base.Init();
        DOTween.Init();
        DOTween.SetTweensCapacity(2000,100);
        
        _audioSource = GetComponent<AudioSource>();
        
        finishPageTriggerProperty = new ReactiveProperty<bool>(false);
        lastElementClickedProperty = new ReactiveProperty<bool>(false);
        SetTransformArray();
        Initialize();

        Underground_PopUpUI_Button.onPopUpButtonEvent -= pageFinishToggle;
        Underground_PopUpUI_Button.onPopUpButtonEvent += pageFinishToggle;
        
    }


    private readonly float _firstFootstepWaitTime = 4.5f;

    protected void Start()
    {
        
      
        
        _camera = Camera.main;

        gameController.currentStateRP
            .Where(currentState => currentState.GameState == IState.GameStateList.StageStart)
            .Delay(TimeSpan.FromSeconds(_firstFootstepWaitTime))
            .Subscribe(_ =>
            {
                firstFootstepsGroup[0].SetActive(true);
                _audioSource.clip = footstepAppearingSound;
                _audioSource.Play();
            });
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        Underground_PopUpUI_Button.onPopUpButtonEvent -= pageFinishToggle;
    }


    public string currentlyClickedObjectName { get; private set; }

    

    private RaycastHit[] hits;

    //public void OnMouseClicked(InputAction.CallbackContext context)
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        hits = Physics.RaycastAll(GameManager_Ray);
        
        foreach (var hit in hits)
        {

            var obj = hit.transform.gameObject;
            var clickedObject = obj;
            FootstepController fc;
            obj.TryGetComponent(out fc);
#if UNITY_EDITOR
            Debug.Log($"raycasted object name :{obj}");
#endif
            if(fc!=null)currentlyClickedObjectName = fc.animalNameToCall;
            InspectObject(clickedObject);
        }
    }

    


    private void InspectObject(GameObject obj)
    {
     

#if UNITY_EDITOR
        Debug.Log("Clicked on: " + obj.name);
#endif
        var footstepController = obj.GetComponent<FootstepController>();
        
        if (footstepController != null)
        {
            currentFootstepGroupOrder = footstepController.footstepGroupOrder - 1;

            if (footstepController.animalNameToCall != null && currentlyClickedObjectName != string.Empty ||
                currentlyClickedObjectName == "")
            {
                undergroundUIManager.popUpUIRectTmp.text = currentlyClickedObjectName;
                OnFootstepClicked?.Invoke();
                DoNext();
                footstepController.OnButtonClicked();
            }   
               
            
            
          
        }
    }


    private void SetTransformArray()
    {
        // 각 FootstepsGroup 배열을 _transformGroups에 할당
        _footstepGameObjGroups[0] = firstFootstepsGroup;
        _footstepGameObjGroups[1] = secondFootstepsGroup;
        _footstepGameObjGroups[2] = thirdFootstepsGroup;
        _footstepGameObjGroups[3] = fourthFootstepsGroup;
        _footstepGameObjGroups[4] = fifthFootstepsGroup;
        _footstepGameObjGroups[5] = sixthFootstepsGroup;
        _footstepGameObjGroups[6] = seventhFootstepsGroup;
        _footstepGameObjGroups[7] = eighthFootstepsGroup;
        _footstepGameObjGroups[8] = ninthFootstepsGroup;
        _footstepGameObjGroups[9] = tenthFootstepsGroup;
        _footstepGameObjGroups[10] = eleventhFootstepsGroup;
        _footstepGameObjGroups[11] = twelfthFootstepsGroup;


        _dustGameObjGroup[0] = DustGroup1;
        _dustGameObjGroup[1] = DustGroup2;
        _dustGameObjGroup[2] = DustGroup3;
        _dustGameObjGroup[3] = DustGroup4;
        _dustGameObjGroup[4] = DustGroup5;
        _dustGameObjGroup[5] = DustGroup6;
        _dustGameObjGroup[6] = DustGroup7;
        _dustGameObjGroup[7] = DustGroup8;
        _dustGameObjGroup[8] = DustGroup9;
        _dustGameObjGroup[9] = DustGroup10;
        _dustGameObjGroup[10] = DustGroup11;
        _dustGameObjGroup[11] = DustGroup12;
    }

    private void Initialize()
    {
        currentFootstepIndexOrder = 0;
        foreach (var group in _footstepGameObjGroups)
        foreach (var obj in group)
            if (obj != null)
                obj.SetActive(false);
    }

    private void DoNext()
    {
        if (currentFootstepIndexOrder < _footstepGameObjGroups[currentFootstepGroupOrder].Length - 1)
        {
            OnOtherElementImplemented(_footstepGameObjGroups[currentFootstepGroupOrder]);
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log($"currentFootstepGroupOrder : {currentFootstepGroupOrder}");
#endif
            OnLastElementImplemented(_footstepGameObjGroups[currentFootstepGroupOrder]);
        }

    }

    /// <summary>
    ///     current Footstep이 사라지는 로직(fade 및 SetActive(false) 되는 로직은
    ///     각 인스턴스화 된 footstepController 스크립트에서 컨트롤합니다.
    /// </summary>
    /// <param name="objGroup"></param>
    public ReactiveProperty<bool> lastElementClickedProperty; // UI재생을 위한 참조를 위해 선언

    private void OnLastElementImplemented(GameObject[] objGroup)
    {
        //함수 마지막에서 다시 초기화.
        lastElementClickedProperty.Value = true;
        lastElementClickedProperty.Value = false;


        if (currentFootstepGroupOrder % 2 == 0)
            _audioSource.clip = _audioClips[(int)FootstepSounds.Found1];
        else
            _audioSource.clip = _audioClips[(int)FootstepSounds.Found2];
        _audioSource.Play();


        ShakeAndRemoveDust();
    }


    private void pageFinishToggle()
    {
        
        if (currentFootstepGroupOrder != 0 && currentFootstepGroupOrder % 3 == 0)
        {
            finishPageTriggerProperty.Value = true;
#if UNITY_EDITOR
            Debug.Log("페이지 전환");
#endif
        }
        
        if (currentFootstepGroupOrder % 3 == 0)
        {
            //페이지 전환 시 interval == duration
            DOVirtual.Float(0,1,13f,val=>val++)
                .OnComplete(() =>
                {
                    _footstepGameObjGroups[currentFootstepGroupOrder][0].SetActive(true);
                    
                    _audioSource.clip = footstepAppearingSound;
                    _audioSource.Play();
                });
        }
        else
        {
            //다음 동물로 넘어갈 때의 interval == duration
            DOVirtual.Float(0,1,6.0f,val=>val++)
                .OnComplete(() =>
                {
                    _footstepGameObjGroups[currentFootstepGroupOrder][0].SetActive(true);
                    
                    _audioSource.clip = footstepAppearingSound;
                    _audioSource.Play();
                });
        }
        
    }

    private void ActivateNextGroupOfFootsteps()
    {
        if (currentFootstepGroupOrder < 12 - 1)
        {
#if UNITY_EDITOR
            Debug.Log("다음페이지");
#endif
           
            currentFootstepGroupOrder++;
           
            _isTOGFCorutineStopped = false;
            currentFootstepIndexOrder = 0;
             // _turnOnNextGroupFirstFootstepCoroutine = StartCoroutine(TurnOnNextGroupFirstFootstep());
        }

        else
        {
#if UNITY_EDITOR
            Debug.Log("game Finished!");
#endif
          


            currentFootstepGroupOrder++;

            //화면전환용
            pageFinishToggle();

            //state전환용
            //11/15/23 -> Bind with Button Event 
            //gameManager.isGameFinishedRP.Value = true;

            if (_turnOnNextGroupFirstFootstepCoroutine != null && !_isTOGFCorutineStopped)
            {
                StopCoroutine(_turnOnNextGroupFirstFootstepCoroutine);
                _isTOGFCorutineStopped = true;
            }
        }
    }

    private Coroutine _turnOnNextGroupFirstFootstepCoroutine;
    private bool _isTOGFCorutineStopped;
    private readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new();

    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds)) waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        return waitForSecondsCache[seconds];
    }

    

    public AudioClip footstepAppearingSound;

    /// <summary>
    ///     current Footstep이 사라지는 로직(fade 및 SetActive(false) 되는 로직은
    ///     각 인스턴스화 된 footstepController 스크립트에서 컨트롤합니다.
    /// </summary>
    /// <param name="objGroup"></param>
    private void OnOtherElementImplemented(GameObject[] objGroup)
    {
        if (currentFootstepGroupOrder % 2 == 0)
            _audioSource.clip = _audioClips[(int)FootstepSounds.Normal1];
        else
            _audioSource.clip = _audioClips[(int)FootstepSounds.Normal2];

        _audioSource.Play();


        currentFootstepIndexOrder++;

        if (currentFootstepIndexOrder < objGroup.Length)
            objGroup[currentFootstepIndexOrder].SetActive(true);
        else
            OnLastElementImplemented(objGroup);

        ShakeDust();
    }

    public float shakeStrength; //other than the every last step.

    private void ShakeDust()
    {
        _dustGameObjGroup[currentFootstepGroupOrder][0].transform.DOShakePosition(2.2f, shakeStrength, 2, 1f);

        _dustGameObjGroup[currentFootstepGroupOrder][1].transform.DOShakePosition(2.2f, shakeStrength, 2, 1f);

        _dustGameObjGroup[currentFootstepGroupOrder][2].transform.DOShakePosition(2.2f, shakeStrength, 2, 1f);
    }

    private SpriteRenderer _spriteRenderer1;
    private SpriteRenderer _spriteRenderer2;
    private SpriteRenderer _spriteRenderer3;

    private void ShakeAndRemoveDust()
    {
        ActivateNextGroupOfFootsteps();


        _dustGameObjGroup[currentFootstepGroupOrder - 1][0].transform.DOShakePosition(3f, 2.2f, 3, 1.2f);

        _dustGameObjGroup[currentFootstepGroupOrder - 1][1].transform.DOShakePosition(3f, 2.2f, 3, 1.2f);

        _dustGameObjGroup[currentFootstepGroupOrder - 1][2].transform.DOShakePosition(3f, 2.2f, 3, 1.2f);


        _spriteRenderer1 =
            _dustGameObjGroup[currentFootstepGroupOrder - 1][0].GetComponent<SpriteRenderer>();

        _spriteRenderer1.DOFade(0, 2)
            .OnComplete(() => _dustGameObjGroup[currentFootstepGroupOrder - 1][0]
                .SetActive(false));


        _spriteRenderer2 =
            _dustGameObjGroup[currentFootstepGroupOrder - 1][1].GetComponent<SpriteRenderer>();

        _spriteRenderer2.DOFade(0, 2)
            .OnComplete(() => _dustGameObjGroup[currentFootstepGroupOrder - 1][1]
                .SetActive(false));


        _spriteRenderer3 =
            _dustGameObjGroup[currentFootstepGroupOrder - 1][2].GetComponent<SpriteRenderer>();

        _spriteRenderer3.DOFade(0, 2.2f)
            .OnComplete(() =>
            {
                _dustGameObjGroup[currentFootstepGroupOrder - 1][2]
                    .SetActive(false);
            });
    }
}