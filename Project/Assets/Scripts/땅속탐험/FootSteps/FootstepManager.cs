using System;
using System.Collections;
using System.Collections.Generic;
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
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif

    public GameObject[] secondFootstepsGroup   = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] thirdFootstepsGroup    = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] fourthFootstepsGroup   = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] fifthFootstepsGroup    = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] sixthFootstepsGroup    = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] seventhFootstepsGroup  = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] eighthFootstepsGroup   = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] ninthFootstepsGroup    = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] tenthFootstepsGroup    = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] eleventhFootstepsGroup = new GameObject[4];
    [Space(10)]
#if UNITY_EDITOR 
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
#endif
    public GameObject[] twelfthFootstepsGroup  = new GameObject[4];
    private static readonly int TOTAL_FOOTSTEP_GROUP_COUNT = 12;
    private GameObject[][] _footstepGameObjGroups = new GameObject[TOTAL_FOOTSTEP_GROUP_COUNT][];


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
        ActivateNextGroupOfFootsteps();
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
            currentFootstepGroupOrder++;
            _footstepGameObjGroups[currentFootstepGroupOrder][0].SetActive(true);
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

        }
        
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
    }
    
}
