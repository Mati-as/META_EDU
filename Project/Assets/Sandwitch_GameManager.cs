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
using Sequence = Unity.VisualScripting.Sequence;
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

    private enum IngsAppearPos
    {
        InvisibleA,
        VisibleA,
        VisibleB,
        VisibleC,
        VisibleD,
        VisibleE,
        InvisibleB,
    }

    private static GameObject s_UIManager;

    private string[] _ingredients;
    private Transform[] _ingredientsOnBigPlate;
    
    private Transform[] _selectableIngredientsOnSmallPlates;
    private int[] _currentSmallPlateLocationIndex;
    private Vector3[] _ingredientsDefaultScales;
    private Vector3 _currentClickedIngPosition; // 새로운 재료 생성시 위치 참조
    public Transform currentClickedIng;
    private readonly int INGREDIENT_COUNT = (int)Sandwich.Max;
    private int _ingsPickinigOrder = 1;
    private readonly int ING_MAX_COUNT = 2;
    
    /*아래 연산자는 두가지 경우에 쓰입니다.
    1. 첫번째, 다섯번째 재료에 빵이 반드시 포함되도록 합니다.
    2. 빵이 중복되게 나오지 않도록 합니다.
    */
    private bool _isBreadOnOption;
    public static bool isGameStart { get; private set; }
    
    //중복클릭방지
    private bool _isClickable = true;
    private float _clickableDelay =7f;


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
        _currentSmallPlateLocationIndex = new int[INGREDIENT_COUNT];
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
        

        if (!_isClickable)
        {
#if UNITY_EDITOR
            Debug.Log($"something still animating... not clickable-------@@@@@");
#endif
            return;
        }

        foreach (var hit in GameManager_Hits)
        {
            currentClickedIng = hit.transform;
            _currentClickedIngPosition = currentClickedIng.position;
            
           var selectedIndex =  FindIndexByName(hit.transform.gameObject.name);
           if (selectedIndex == NO_VALID_OBJECT) return;
            
           PutOnGenerationPosition(_ingredientsOnBigPlate[selectedIndex].gameObject,2f);
           SetCurrentClickedIngPos(hit.transform);
           PlayShrinkAnim(_selectableIngredientsOnSmallPlates[selectedIndex]);
           ScaleUpSingleIng();
            
           _isClickable = false;
           SetClickableAfterDelay(_clickableDelay);
          
          
           return;
        }
        
    }
    

    protected override void OnStartButtonClicked()
    {
        base.OnStartButtonClicked();
        
        MoveOutSandwich();
    }
    

    // methods ------------------------------------------------------------------------

    private void SetCurrentClickedIngPos(Transform ing)
    {
        _currentClickedIngPosition = ing.position;
    }

    private void SetClickableAfterDelay(float delay)
    {
        DOVirtual.Float(0, 0, delay, _ => { }).OnComplete(() =>
        {
            _isClickable = true;
        });
    }
    private void PutOnGenerationPosition(GameObject gameObj,float delay,float fallingDuration= 1.75f)
    {
        var obj = Instantiate(gameObj,_sandWich.transform);
        DOVirtual.Float(0, 0, delay,_ => { })
        .OnComplete(() =>
        {
            obj.transform.position = _ingredientGenerationPosition;
          
            
            obj.transform.DOShakeRotation(fallingDuration,1,1,1);
            var rb = obj.GetComponent<Rigidbody>();
            SetRbConstraint(rb);
            obj.gameObject.SetActive(true);
            DOVirtual.Float(0, 0, fallingDuration,_ => { })
                .OnComplete(() =>
                {
                   
                    rb.constraints = RigidbodyConstraints.FreezeAll;

                    if (_ingsPickinigOrder >= ING_MAX_COUNT)
                    {
                        onSandwichMakingFinish?.Invoke();
                    }
                    else
                    {
                        _ingsPickinigOrder++;
                    }

                });
        
        });

    }

    private void SetRbConstraint(Rigidbody rb)
    {
        var constraints = rb.constraints;
        constraints = RigidbodyConstraints.None;
        constraints = RigidbodyConstraints.FreezePositionX;
        constraints = RigidbodyConstraints.FreezePositionZ;
        rb.constraints = constraints;
    }

 

    private void PlayShrinkAnim(Transform transform)
    {
        if (transform.GetComponent<Collider>().enabled == false) return;
        transform.GetComponent<Collider>().enabled = false;
        
        transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                transform.gameObject.SetActive(false);
            });
       
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

    private GameObject _sandWich;
    private void SetSandwich()
    {
        _ingredientsOnBigPlate = new Transform[INGREDIENT_COUNT];
        //초기 샌드위치 할당 및 추후 접시위 
        _sandWich = GameObject.Find("Sandwich");
        
        for (var i = (int)Sandwich.BreadT; i < INGREDIENT_COUNT; i++)
            _ingredientsOnBigPlate[i] = _sandWich.transform.GetChild(i);
        
    }

    private void MoveOutSandwich()
    {
        for (var i = (int)Sandwich.BreadT; i < INGREDIENT_COUNT; i++)
            _ingredientsOnBigPlate[i]
                .DOMove(_moveOutP , 3.5f)
                .SetEase(Ease.OutSine)
                .SetDelay((0.5f * i) + Random.Range(0.5f, 0.7f))
                .OnComplete(()=>
                {
                    _ingredientsOnBigPlate[i].gameObject.SetActive(false);
                });

        onRoundReady?.Invoke();
    }

    private void OnRoundReady()
    {
        ScaleAllIngs(5f);
    }

    
    // 클릭후 새로운 재료를 만들때 생성 


    private void ScaleAllIngs(float delay, bool sizeZero = false)
    { 
        
        int count = 0;
        List<int> indices = Enumerable.Range(0, _ingredientsAppearPosition.Length).ToList();
        indices = indices.OrderBy(a => Random.value).ToList();


        if (sizeZero)
        {
            foreach (var obj in _selectableIngredientsOnSmallPlates)
            {
         
                obj.gameObject.SetActive(true);
            
                obj.DOScale(Vector3.zero, 2f)
                    .SetEase(Ease.InOutBounce).SetDelay(delay + Random.Range(0, 0.5f));

                obj.DOShakeRotation(1000f, 3f,vibrato:1,randomness:15);

           
                count++;
            }
        }
        else
        {
            foreach (var obj in _selectableIngredientsOnSmallPlates)
            {
                int randomIndex = indices[count];
                obj.position = _ingredientsAppearPosition[randomIndex];
                _currentSmallPlateLocationIndex[count] = randomIndex;
                count++;
            }


            if (IsBothBreadInvisible())
            {
                SwapIngredients();
            }

            count = 0; 
            foreach (var obj in _selectableIngredientsOnSmallPlates)
            {
         
                obj.gameObject.SetActive(true);
            
                obj.DOScale(_ingredientsDefaultScales[count], 2f)
                    .SetEase(Ease.InOutBounce).SetDelay(delay + Random.Range(0, 0.5f));

                obj.DOShakeRotation(1000f, 3f,vibrato:1,randomness:15);

           
                count++;
            }

            isGameStart = true;
        }
       

       
    }


    private bool IsBothBreadInvisible()
    {
        if (((_selectableIngredientsOnSmallPlates[(int)Sandwich.BreadT].position
              == _ingredientsAppearPosition[(int)IngsAppearPos.InvisibleA] &&
              _selectableIngredientsOnSmallPlates[(int)Sandwich.BreadB].position
              == _ingredientsAppearPosition[(int)IngsAppearPos.InvisibleB])
             ||
             (_selectableIngredientsOnSmallPlates[(int)Sandwich.BreadT].position
              == _ingredientsAppearPosition[(int)IngsAppearPos.InvisibleB] &&
              _selectableIngredientsOnSmallPlates[(int)Sandwich.BreadB].position
              == _ingredientsAppearPosition[(int)IngsAppearPos.InvisibleA]
             ))) return true;
        
        
        return false;
        
    }

    private void SwapIngredients()
    {
        Vector3 invisibleP =   _selectableIngredientsOnSmallPlates[(int)Sandwich.BreadT].position;
            
        _selectableIngredientsOnSmallPlates[(int)Sandwich.BreadT].position
            = _ingredientsAppearPosition[3];

        _selectableIngredientsOnSmallPlates[_currentSmallPlateLocationIndex[3]].position
            = invisibleP;
    }
    private void OnSandwichMakingFinish()
    {
#if UNITY_EDITOR
        Debug.Log($"Making Sandwich is finished");
#endif
        DOTween.KillAll();
        ScaleAllIngs(3f,true);
        SendSandwichToAnimal();
    }

    private void SendSandwichToAnimal(float delay = 3f)
    {
        var posUp = GameObject.Find("SandwichUp").transform.position;
        var pos = GameObject.Find("SandwichArrive").transform.position;

        DOVirtual.Float(0, 0, delay, _ => { }).OnComplete(() =>
        {
            _sandWich.transform.DOMove(posUp, 1.8f)
                .OnComplete(() =>
                {
                    _sandWich.transform.DOMove(pos, 3f);
                    
                    DOVirtual.Float(10, 25, 2f, fov =>
                    {
                        Camera.main.fieldOfView = fov;
                    }).SetDelay(0.5f);
                });
       
            DOVirtual.Float(0, 0, 5f, _ =>
            {
                Camera.main.transform.DOLookAt(_sandWich.transform.position, 2.5f);
               
               
            });
        });
    
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

    
    /// <summary>
    /// 시작시 모든 재료를 스케일업하는 것이 아닌, 사용자가 클릭한 후 하나씩 스케일업 합니다.
    /// 이미 클릭된 재료중에서만 ScaleUp하도록 합니다. 
    /// </summary>
    public void ScaleUpSingleIng(float delay = 5f)
    {
        var count = 0;
   
        DOVirtual.Float(0, 0, delay, _ => { }).OnComplete(() =>
        { 
            var pos = currentClickedIng.position;
            foreach (var obj in _selectableIngredientsOnSmallPlates)
            {
                if (!obj.gameObject.activeSelf)
                {    
#if UNITY_EDITOR
                    Debug.Log($" {obj.name} : ActiveStatue{obj.gameObject.activeSelf}");
#endif
                    obj.position = pos;
                    obj.localScale = Vector3.zero;
                    obj.gameObject.SetActive(true);
                    obj.DOScale(_ingredientsDefaultScales[count], 2f)
                        .SetEase(Ease.InOutBounce).SetDelay(Random.Range(0, 0.5f));
                    obj.DOShakeRotation(1000f, 5f, 1);
                    obj.GetComponent<Collider>().enabled = true;
                    return;
                }

                if (obj.transform.position ==_ingredientsAppearPosition[(int)IngsAppearPos.InvisibleA]
                || obj.transform.position ==_ingredientsAppearPosition[(int)IngsAppearPos.InvisibleB])
                {
                    
#if UNITY_EDITOR
                    Debug.Log($"scaleUpAgain: {obj.name}");
#endif

                    obj.position = pos;
                    obj.localScale = Vector3.zero;
                    obj.gameObject.SetActive(true);
                    obj.DOScale(_ingredientsDefaultScales[count], 2f)
                        .SetEase(Ease.InOutBounce).SetDelay(Random.Range(0, 0.5f));
                    obj.DOShakeRotation(1000f, 5f, 1);
                    obj.GetComponent<Collider>().enabled = true;
                    return;
                }
              
                count++;
            }

          
            
        });
    }
    
    
}