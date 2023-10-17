using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEditor;
using UniRx;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;
#if UNITY_EDITOR 
using MyCustomizedEditor;
#endif

public class FootstepManager : MonoBehaviour
{
   
    [Header("Reference")] [SerializeField] private GroundGameManager gameManager;
    [SerializeField]
    private GroundFootStepData _groundFootStepData;
    public GroundFootStepData GetGroundFootStepData()
    {
        return _groundFootStepData;
    }
    
    
    [Space(10)]
    [Header("Footstep Positions")]
   
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] firstFootstepsGroup    = new GameObject[4];
    public GameObject[] DustGroup1 = new GameObject[4];
   
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif

    public GameObject[] secondFootstepsGroup   = new GameObject[4];
    public GameObject[] DustGroup2 = new GameObject[4];
    [Space(10)]
    
    
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    
    
    public GameObject[] thirdFootstepsGroup    = new GameObject[4];
    public GameObject[] DustGroup3 = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] fourthFootstepsGroup   = new GameObject[4];
    public GameObject[] DustGroup4 = new GameObject[4];
    
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] fifthFootstepsGroup    = new GameObject[4];
    public GameObject[] DustGroup5 = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] sixthFootstepsGroup    = new GameObject[4];
    public GameObject[] DustGroup6 = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] seventhFootstepsGroup  = new GameObject[4];
    public GameObject[] DustGroup7 = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] eighthFootstepsGroup   = new GameObject[4];
    public GameObject[] DustGroup8 = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] ninthFootstepsGroup    = new GameObject[4];
    public GameObject[] DustGroup9 = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] tenthFootstepsGroup    = new GameObject[4];
    public GameObject[] DustGroup10 = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] eleventhFootstepsGroup = new GameObject[4];
    public GameObject[] DustGroup11 = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] twelfthFootstepsGroup  = new GameObject[4];
    public GameObject[] DustGroup12 = new GameObject[4];
    
    
    private static readonly int TOTAL_FOOTSTEP_GROUP_COUNT = 12;
    private GameObject[][] _footstepGameObjGroups = new GameObject[TOTAL_FOOTSTEP_GROUP_COUNT][];
    private GameObject[][] _dustGameObjGroup = new GameObject[TOTAL_FOOTSTEP_GROUP_COUNT][];

    
    
    [FormerlySerializedAs("finishPageToggleProperty")] public  ReactiveProperty<bool> finishPageTriggerProperty;
    public static int currentFootstepGroupOrder { get; set; }
   
    public static int currentFootstepIndexOrder { get; private set; }
    
    private void Awake()
    {
        LeanTween.init(2000);
        finishPageTriggerProperty = new ReactiveProperty<bool>(false);
        SetTransformArray();
        Initialize();
    }

    private void Start()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { OnMouseClicked(); });
        trigger.triggers.Add(entry);
        
        
        
        gameManager.currentStateRP
            .Where(currentState => currentState.GameState == IState.GameStateList.StageStart)
            .Subscribe(_ => firstFootstepsGroup[0].SetActive(true));
    }
    

    public static string currentlyClickedObjectName;

    public void OnMouseClicked()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var obj = hit.transform.gameObject;
            var clickedObject = obj;
            var fC = obj.GetComponent<FootstepController>();
            
            currentlyClickedObjectName = fC.animalNameToCall;
            InspectObject(clickedObject);
        }
    }
    
    
    public static event Action OnFootstepClicked;
    void InspectObject(GameObject obj)
    {
     
        Debug.Log("Clicked on: " + obj.name);
        
        FootstepController footstepController = obj.GetComponent<FootstepController>();
        if (footstepController != null)
        {
            currentFootstepGroupOrder = footstepController.footstepGroupOrder - 1;
              
            OnFootstepClicked?.Invoke();
            DoNext();
        }
    }
    

    private void SetTransformArray()
    {
        // 각 FootstepsGroup 배열을 _transformGroups에 할당
        _footstepGameObjGroups[0]  = firstFootstepsGroup;
        _footstepGameObjGroups[1]  = secondFootstepsGroup;
        _footstepGameObjGroups[2]  = thirdFootstepsGroup;
        _footstepGameObjGroups[3]  = fourthFootstepsGroup;
        _footstepGameObjGroups[4]  = fifthFootstepsGroup;
        _footstepGameObjGroups[5]  = sixthFootstepsGroup;
        _footstepGameObjGroups[6]  = seventhFootstepsGroup;
        _footstepGameObjGroups[7]  = eighthFootstepsGroup;
        _footstepGameObjGroups[8]  = ninthFootstepsGroup;
        _footstepGameObjGroups[9]  = tenthFootstepsGroup;
        _footstepGameObjGroups[10] = eleventhFootstepsGroup;
        _footstepGameObjGroups[11] = twelfthFootstepsGroup;


        _dustGameObjGroup[0]  =  DustGroup1;
        _dustGameObjGroup[1]  =  DustGroup2;
        _dustGameObjGroup[2]  =  DustGroup3;
        _dustGameObjGroup[3]  =  DustGroup4;
        _dustGameObjGroup[4]  =  DustGroup5;
        _dustGameObjGroup[5]  =  DustGroup6;
        _dustGameObjGroup[6]  =  DustGroup7;
        _dustGameObjGroup[7]  =  DustGroup8;
        _dustGameObjGroup[8]  =  DustGroup9;
        _dustGameObjGroup[9]  =  DustGroup10;
        _dustGameObjGroup[10] =  DustGroup11;
        _dustGameObjGroup[11] =  DustGroup12;
    }

    private void Initialize()
    {
        currentFootstepIndexOrder = 0;
        foreach (GameObject[] group in _footstepGameObjGroups)
        {
            foreach (GameObject obj in group)
            {
                if (obj != null) 
                {
                    obj.SetActive(false);
                }
            }
        }
      
    }
    private void DoNext()
    {
        if (currentFootstepIndexOrder < _footstepGameObjGroups[currentFootstepGroupOrder].Length - 1)
        {
            OnOtherElementImplemented(_footstepGameObjGroups[currentFootstepGroupOrder]);
            Debug.Log("OnOtherElementImplemented");
        }
        else
        {  
            
            Debug.Log($"currentFootstepGroupOrder : {currentFootstepGroupOrder}");
            OnLastElementImplemented(_footstepGameObjGroups[currentFootstepGroupOrder]);
            Debug.Log("OnLastElementImplemented");
        }
        
        // 10/12/23 순회로직의 경우에는 아래 로직을 사용할 것 
        // foreach (GameObject[] group in _footstepGameObjGroups)
        // {
        
            // foreach (GameObject obj in _footstepGameObjGroups[currentFootstepGroupOrder])
            // {
               
                // 현재 원소가 마지막 원소인 경우
                // if (obj == _footstepGameObjGroups[currentFootstepGroupOrder]
                //         [_footstepGameObjGroups[currentFootstepGroupOrder].Length - 1])
                // {
                //     OnLastElementImplemented(_footstepGameObjGroups[currentFootstepGroupOrder]);
                //     ActivateNextFootstepGroup();
                // }
                // else
                // {
                //     OnOtherElementImplemented(_footstepGameObjGroups[currentFootstepGroupOrder]);
                // }
                
            // }
        // }
    }
    
    /// <summary>
    ///  current Footstep이 사라지는 로직(fade 및 SetActive(false) 되는 로직은
    /// 각 인스턴스화 된 footstepController 스크립트에서 컨트롤합니다.
    /// </summary>
    /// <param name="objGroup"></param>
    void OnLastElementImplemented(GameObject[] objGroup)
    {
        ShakeAndRemoveDust();
        
        
      
        if (currentFootstepGroupOrder != 0 && currentFootstepGroupOrder % 3 == 0)
        {
            pageFinishToggle();
        }

    }

    private void pageFinishToggle()
    {
        Debug.Log("페이지 전환");
        finishPageTriggerProperty.Value = true;
    }

    void ActivateNextGroupOfFootsteps()
    {
        if (currentFootstepGroupOrder < 12 - 1)
        {
            Debug.Log("다음페이지");
            currentFootstepGroupOrder++;
            _turnOnNextGroupFirstFootstepCoroutine = StartCoroutine(TurnOnNextGroupFirstFootstep());
            _isTOGFCorutineStopped = false;
            currentFootstepIndexOrder = 0;
        }
        
        else
        {
            Debug.Log("game Finished!");
           
            
            currentFootstepGroupOrder++;
            
            //화면전환용
            pageFinishToggle();
            
            //state전환용
            gameManager.isGameFinishedRP.Value = true;

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

    private IEnumerator TurnOnNextGroupFirstFootstep()
    {
        if (currentFootstepGroupOrder % 3 == 0)
        {
            yield return GetWaitForSeconds(8.5f);
        }
        else
        {
            yield return GetWaitForSeconds(4f);
        }
        
        _footstepGameObjGroups[currentFootstepGroupOrder][0].SetActive(true);

    }
    
  
    
    /// <summary>
    /// current Footstep이 사라지는 로직(fade 및 SetActive(false) 되는 로직은
    /// 각 인스턴스화 된 footstepController 스크립트에서 컨트롤합니다.
    /// </summary>
    /// <param name="objGroup"></param>
    void OnOtherElementImplemented(GameObject[] objGroup)
    {
        currentFootstepIndexOrder++;
        if (currentFootstepIndexOrder < objGroup.Length)
        {
            objGroup[currentFootstepIndexOrder].SetActive(true);
        }
        else
        {
            OnLastElementImplemented(objGroup);
        }

        ShakeDust();
    }


    void ShakeDust()
    {
        _dustGameObjGroup[currentFootstepGroupOrder][0].
            transform.DOShakePosition(2.2f, 0.3f, 2, 1f);
        
        _dustGameObjGroup[currentFootstepGroupOrder][1].
            transform.DOShakePosition(2.2f, 0.3f, 2, 1f);
        
        _dustGameObjGroup[currentFootstepGroupOrder][2].
            transform.DOShakePosition(2.2f, 0.3f, 2, 1f);
    }

    private SpriteRenderer _spriteRenderer1; 
    private SpriteRenderer _spriteRenderer2;
    private SpriteRenderer _spriteRenderer3; 
    void ShakeAndRemoveDust()
    {
        
        ActivateNextGroupOfFootsteps();
        
        
        _dustGameObjGroup[currentFootstepGroupOrder - 1][0].
            transform.DOShakePosition(3f, 2.2f, 3, 1.2f);
        
        _dustGameObjGroup[currentFootstepGroupOrder - 1][1].
            transform.DOShakePosition(3f, 2.2f, 3, 1.2f);
        
        _dustGameObjGroup[currentFootstepGroupOrder - 1][2].
            transform.DOShakePosition(3f, 2.2f, 3, 1.2f);

        
        
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
