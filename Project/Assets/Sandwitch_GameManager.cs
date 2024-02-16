using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class Sandwitch_GameManager : IGameManager
{
    private enum Sandwich
    {
        BreadT,
        Egg,
        Tomato,
        Lettuce,
        Cheese,
        Bacon,
        BreadB,
        Max
    }

    private static GameObject s_UIManager;

    private string[] _ingredients;
    private Transform[] _ingredientsOnBigPlate;
    
    private Transform[] _selectableIngredientsOnSmallPlates;
    private Vector3[] _ingredientsDefaultScales;
    private readonly int INGREDIENT_COUNT = (int)Sandwich.Max;



    // OnRoundReady 
    // - 1. 인트로에서 샌드위치가 사라지고 일정 시간이 지났을떄 (일정시간을 파라미터로)
    // - 2. 동물이 샌드위치를 다 먹고 일정 시간이 지났을때
    public static event Action onRoundReady;
    public static event Action onSandwichMakingFinish;
    public static event Action onAnimalEatingFinish;
    
    


    //Positions
    private Vector3 _moveOutP;
    private Vector3 _ingredientGenerationPosition;
    private Vector3[] _ingredientsAppearPosition;

    // Events ------------------------------------------------------------------------

    protected override void Init()
    {
        _ingredientGenerationPosition = GameObject.Find("IngredientGenerationPosition").transform.position;
       
        base.Init();
        
        InitUI();
        
        GetEnumStrings();
        SetSandwich();
        InitIngredients();
        SetPos();

        isInitialized = true;
    }


    private void Start()
    {
        Debug.Assert(isInitialized);
        
        StackCamera();
    }

    protected override void BindEvent()
    {
        base.BindEvent();

        onRoundReady -= OnRoundReady;
        onRoundReady += OnRoundReady;

        onSandwichMakingFinish -= OnSandwichMakingFinish;
        onSandwichMakingFinish += OnSandwichMakingFinish;
    }

    private readonly int NO_VALID_OBJECT = -1;
    protected override void OnRaySynced()
    {
        if (!isInitialized) return;
        if (!isStartButtonClicked) return;

        foreach (var hit in GameManager_Hits)
        {
           var selectedIndex =  FindIndexByName(hit.transform.gameObject.name);
           if (selectedIndex == NO_VALID_OBJECT) return; 
#if UNITY_EDITOR
           Debug.Log($"selectedIndex,name : {selectedIndex},: {hit.transform.gameObject.name}");
#endif
            
           PutOnGenerationPosition(_ingredientsOnBigPlate[selectedIndex]);
           PlayShrinkAnim(_selectableIngredientsOnSmallPlates[selectedIndex]);
        }
        
    }
    

    protected override void OnStartButtonClicked()
    {
        base.OnStartButtonClicked();
        
        MoveOutSandwich();
    }


    // methods ------------------------------------------------------------------------

    private void PutOnGenerationPosition(Transform transform)
    {
        transform.position = _ingredientGenerationPosition;
        transform.gameObject.SetActive(true);
        transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }

    private void PlayShrinkAnim(Transform transform)
    {
        if (transform.GetComponent<Collider>().enabled == false) return;
        transform.GetComponent<Collider>().enabled = false;
        
        transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutBack)
            .OnComplete(() => { transform.gameObject.SetActive(false); });
       
    }
    private void GetEnumStrings()
    {
        _ingredients = new string[(int)Sandwich.Max];
        _ingredients = Enum.GetNames(typeof(Sandwich));
    }

    private void StackCamera()
    {
        var uiCamera = s_UIManager.GetComponentInChildren<Camera>();

        if (Camera.main.TryGetComponent<UniversalAdditionalCameraData>(out var mainCameraData))
            mainCameraData.cameraStack.Add(uiCamera);
        else
            Debug.LogError("Main camera does not have UniversalAdditionalCameraData component.");
    }

    private void InitUI()
    {
        Debug.Log("UI Init");
        var uiInstance = Resources.Load<GameObject>("Prefab/UI/UI_" + SceneManager.GetActiveScene().name);
        var root = GameObject.Find("@Root");

        if (s_UIManager != null)
        {
            Debug.LogError("more than two UIManagers");
            return;
        }

        s_UIManager = Instantiate(uiInstance, root.transform);
    }

    private void InitIngredients()
    {
        //유저가 선택하는 샌드위치 할당 
        var ingOnPlate = GameObject.Find("IngredientsOnSmallPlate");

        _selectableIngredientsOnSmallPlates = new Transform[INGREDIENT_COUNT];
        _ingredientsAppearPosition = new Vector3[INGREDIENT_COUNT];
        _ingredientsDefaultScales = new Vector3[INGREDIENT_COUNT];

        int posCount = 0;
        for (var i = (int)Sandwich.BreadT; i < INGREDIENT_COUNT; i++)
        {
            _selectableIngredientsOnSmallPlates[i] = ingOnPlate.transform.GetChild(i);
            _ingredientsDefaultScales[i] = _selectableIngredientsOnSmallPlates[i].localScale;

       

            _ingredientsAppearPosition[posCount] = ingOnPlate.transform.GetChild(i).position;
            posCount++;
            
            _selectableIngredientsOnSmallPlates[i].localScale = Vector3.zero;
            _selectableIngredientsOnSmallPlates[i].gameObject.SetActive(false);
        }

    }
    private void SetPos()
    {
        _moveOutP = GameObject.Find("MoveOut").transform.position;
    }

    private void SetSandwich()
    {
        _ingredientsOnBigPlate = new Transform[INGREDIENT_COUNT];
        //초기 샌드위치 할당 및 추후 접시위 
        var sandwich = GameObject.Find("Sandwich");
        
        for (var i = (int)Sandwich.BreadT; i < INGREDIENT_COUNT; i++)
            _ingredientsOnBigPlate[i] = sandwich.transform.GetChild(i);
        
    }

    private void MoveOutSandwich()
    {
        for (var i = (int)Sandwich.BreadT; i < INGREDIENT_COUNT; i++)
            _ingredientsOnBigPlate[i]
                .DOMove(_moveOutP, 3.5f)
                .SetEase(Ease.InSine)
                .SetDelay((0.5f * i) + Random.Range(0.5f, 0.7f))
                .OnComplete(()=>
                {
                    _ingredientsOnBigPlate[i].gameObject.SetActive(false);
                });

        onRoundReady?.Invoke();
    }

    private void OnRoundReady()
    {
        ScaleUpIngs(5f);
    }

    private void ScaleUpIngs(float delay)
    {

        int count = 0;
        List<int> indices = Enumerable.Range(0, _ingredientsAppearPosition.Length).ToList();
        indices = indices.OrderBy(a => Random.value).ToList();

        foreach (var obj in _selectableIngredientsOnSmallPlates)
        {
            obj.gameObject.SetActive(true);
            
            int randomIndex = indices[count];
            obj.position = _ingredientsAppearPosition[randomIndex];

            obj.DOScale(_ingredientsDefaultScales[randomIndex], 2f)
                .SetEase(Ease.InOutBounce).SetDelay(delay + Random.Range(0, 0.5f));

            obj.DOShakeRotation(1000f, 5f,vibrato:1);
            
            count++;
        }
    }

    private void OnSandwichMakingFinish()
    {
        DOTween.KillAll();
    }
    
    public int FindIndexByName(string nameToFind)
    {
        for (int i = 0; i < _selectableIngredientsOnSmallPlates.Length; i++)
        {
            if (_selectableIngredientsOnSmallPlates[i].name == nameToFind)
            {
                return i; 
            }
        }
        return -1;
    }
}