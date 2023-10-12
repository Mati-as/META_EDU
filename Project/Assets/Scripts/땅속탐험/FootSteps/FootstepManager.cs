using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UniRx;

public class FootstepManager : MonoBehaviour
{
    [Header("Reference")] [SerializeField] private GroundGameManager gameManager;
    [Space(10)]
    [Header("Footstep Positions")]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] firstFootstepsGroup    = new GameObject[4]; 
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] secondFootstepsGroup   = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] thirdFootstepsGroup    = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] fourthFootstepsGroup   = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] fifthFootstepsGroup    = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] sixthFootstepsGroup    = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] seventhFootstepsGroup  = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] eighthFootstepsGroup   = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] ninthFootstepsGroup    = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] tenthFootstepsGroup    = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] eleventhFootstepsGroup = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] twelfthFootstepsGroup  = new GameObject[4];


    private static readonly int TOTAL_FOOTSTEP_GROUP_COUNT = 12;
    private GameObject[][] _footstepGameObjGroups = new GameObject[TOTAL_FOOTSTEP_GROUP_COUNT][];

    public static int currentFootstepGroupOrder { get; set; }
   
    public static int currentFootstepIndexOrder { get; private set; }

    
    private void Awake()
    {
        FootstepController.OnButtionClicked -= DoNext;
        FootstepController.OnButtionClicked += DoNext;
        
        SetTransformArray();
        Initialize();
    }

    private void Start()
    {
        gameManager.currentStateRP
            .Where(currentState => currentState.GameState == IState.GameStateList.StageStart)
            .Subscribe(_ => firstFootstepsGroup[0].SetActive(true));
    }
    
    void Update()
    {
        // 마우스 왼쪽 버튼이 눌렸을 때
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Ray가 어떤 GameObject에 충돌했다면
                GameObject clickedObject = hit.transform.gameObject;
                InspectObject(clickedObject);
            }
        }
    }
    void InspectObject(GameObject obj)
    {
     
        Debug.Log("Clicked on: " + obj.name);
        
        FootstepController footstepController = obj.GetComponent<FootstepController>();
        if (footstepController != null)
        {
           
        }
    }
    

    private void OnDestroy()
    {
        FootstepController.OnButtionClicked -= DoNext;
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
        // foreach (GameObject[] group in _footstepGameObjGroups)
        // {
        
            foreach (GameObject obj in _footstepGameObjGroups[currentFootstepGroupOrder])
            {
               
                // 현재 원소가 마지막 원소인 경우
                if (obj == _footstepGameObjGroups[currentFootstepGroupOrder]
                        [_footstepGameObjGroups[currentFootstepGroupOrder].Length - 1])
                {
                    OnLastElementImplemented(_footstepGameObjGroups[currentFootstepGroupOrder]);
                }
                else
                {
                    OnOtherElementImplemented(_footstepGameObjGroups[currentFootstepGroupOrder]);
                }
                
            }
        // }
    }
    
    void OnLastElementImplemented(GameObject[] objGroup)
    {
        objGroup[currentFootstepIndexOrder].SetActive(false);
        currentFootstepIndexOrder = 0;
    }
    
    void OnOtherElementImplemented(GameObject[] objGroup)
    {
        objGroup[currentFootstepIndexOrder].SetActive(false);
        objGroup[currentFootstepIndexOrder + 1].SetActive(true);
        currentFootstepIndexOrder++;
    }
    
}
