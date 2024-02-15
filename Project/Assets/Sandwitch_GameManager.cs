using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

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
    private Transform[] _initialSandwichIngredients;
    private readonly int INGREDIENT_COUNT = (int)Sandwich.Max;


    //Positions
    private Vector3 _moveOutP;

    // Events ------------------------------------------------------------------------

    protected override void Init()
    {
        base.Init();
        
        GetEnumStrings();
        SetSandwich();
        InitUI();
        SetPos();

        isInitialized = true;
    }


    private void Start()
    {
        Debug.Assert(isInitialized);
        
        StackCamera();
    }


    protected override void OnRaySynced()
    {
    }

    protected override void OnStartButtonClicked()
    {
        base.OnStartButtonClicked();
        
        MoveOut();
    }


    // methods ------------------------------------------------------------------------

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

    private void SetPos()
    {
        _moveOutP = GameObject.Find("MoveOut").transform.position;
    }

    private void SetSandwich()
    {
        //초기 샌드위치 할당
        var sandwich = GameObject.Find("Sandwich");

        _initialSandwichIngredients = new Transform[INGREDIENT_COUNT];

        for (var i = (int)Sandwich.BreadT; i < INGREDIENT_COUNT; i++)
            _initialSandwichIngredients[i] = sandwich.transform.GetChild(i);
    }

    private void MoveOut()
    {
        for (var i = (int)Sandwich.BreadT; i < INGREDIENT_COUNT; i++)
            _initialSandwichIngredients[i]
                .DOMove(_moveOutP, 3.5f)
                .SetEase(Ease.InSine)
                .SetDelay((0.77f * i) + Random.Range(0.5f, 0.7f));
    }
}