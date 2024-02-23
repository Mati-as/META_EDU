using System;
using System.Collections.Generic;
using System.Linq;
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

    private enum IngsAppearPos
    {
        InvisibleA,
        VisibleA,
        VisibleB,
        VisibleC,
        VisibleD,
        VisibleE,
        InvisibleB
    }

    private static GameObject s_UIManager;

    private string[] _ingredients;
    private Transform[] _ingredientsOnBigPlate;

    private Transform[] _selectableIngredientsOnSmallPlates;
    private int[] _currentSmallPlateLocationIndex;
    private Vector3[] _ingredientsDefaultScales;
    private Vector3 _previousShrinkingPoint; // 새로운 재료 생성시 위치 참조
    private Vector3[] _path;
    public Transform currentClickedIng;
    private readonly int INGREDIENT_COUNT = (int)Sandwich.Max;
    private int _ingsPickinigOrder = 1;
    private readonly int ING_MAX_COUNT = 5;
    private Dictionary<Transform, Sequence> _pathSeqMap;
    private Dictionary<Transform, Sequence> _shakeSeqMap;
    

    private ParticleSystem _finishMakingPs;

    /*아래 연산자는 두가지 경우에 쓰입니다.
    1. 첫번째, 다섯번째 재료에 빵이 반드시 포함되도록 합니다.
    2. 빵이 중복되게 나오지 않도록 합니다.
    */
    private bool _isBreadOnOption;
    public static bool isGameStart { get; private set; }

    //중복클릭방지
    private bool _isClickable = true;
    private readonly float _clickableDelay = 3.0f;

    private readonly float RESTART_DELAY = 1.5f;

    // OnRoundReady 
    // - 1. 인트로에서 샌드위치가 사라지고 일정 시간이 지났을떄 (일정시간을 파라미터로)
    // - 2. 동물이 샌드위치를 다 먹고 일정 시간이 지났을때


    [Range(0, 1f)] public float fallingSpeed;

    public static event Action onRoundReady;
    public static event Action onSandwichMakingFinish;
    public static event Action onAnimalEatingFinish;

    // animal
    public static event Action onSandwichArrive;


    //Positions
    private Vector3 _moveOutP;
    private Vector3 _ingredientGenerationPosition;
    private Vector3[] _ingredientsAppearPosition;
    private Vector3 defaultLookAt;
    private GameObject _thisSandwich;

    // Events ------------------------------------------------------------------------

    protected override void Init()
    {
        SetPositionValue();
        _ingredientGenerationPosition = GameObject.Find("IngredientGenerationPosition").transform.position;
        _currentSmallPlateLocationIndex = new int[INGREDIENT_COUNT];
        _finishMakingPs = GameObject.Find("CFX_FinishMaking").GetComponent<ParticleSystem>();


        // _shakeSeqs = new List<Sequence>();
        _pathSeqMap = new Dictionary<Transform, Sequence>();
        _shakeSeqMap = new Dictionary<Transform, Sequence>();
        Camera.main.transform.LookAt(defaultLookAt);

        base.Init();

        InitUI();

        GetEnumStrings();
        SetSandwich();
        InitIngredients();


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

        sandwich_AnimalController.onAllFinishAnimOver -= AllFinishAnimOver;
        sandwich_AnimalController.onAllFinishAnimOver += AllFinishAnimOver;
    }

    private bool _isRoundFinished;
    private readonly int NO_VALID_OBJECT = -1;
    private RaycastHit[] _raycastHits;

    protected override void OnRaySynced()
    {
        _raycastHits = Physics.RaycastAll(GameManager_Ray);
        if (!isInitialized) return;
        if (!isStartButtonClicked) return;
        if (_isRoundFinished) return;
        if (!_isClickable)
        {
#if UNITY_EDITOR
            Debug.Log("Can't be clicked : isClickable is false");
#endif
            return;
        }
        
        foreach (var hit in _raycastHits)
        {
          
#if UNITY_EDITOR
            Debug.Log($"작은접시생성위치 갱신: 클릭오브젝트 이름: {hit.transform.gameObject.name}, " +
                      $"클릭오브젝트 위치: {hit.transform.position}" +
                      $"갱신위치: {_previousShrinkingPoint}");
#endif
            var selectedIndex = FindIndexByName(hit.transform.gameObject.name);
            if (selectedIndex == NO_VALID_OBJECT)
            {
#if UNITY_EDITOR
                Debug.Log($"isNotValid: {hit.transform.gameObject.name} : selectedIndex: {selectedIndex}X");
#endif
                return;
            }

            var randomChar = (char)Random.Range('A', 'F' + 1);
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_" + randomChar,
                0.3f);

            var clickedObj = _ingredientsOnBigPlate.FirstOrDefault(x =>
                x.transform.gameObject.name.Contains(hit.transform.gameObject.name.Substring(5)));

            DropBigIng(clickedObj.gameObject, 1f);

            if(hit.transform.gameObject.name.Contains("Plate")) PlayShrinkAnim(hit.transform);

       
    


            SetClickableAfterDelay(_clickableDelay);
            _isClickable = false;

            return;
        }
    }


    protected override void OnStartButtonClicked()
    {
        base.OnStartButtonClicked();

        MoveOutSandwich();
    }


    // methods ------------------------------------------------------------------------
    
    private void SetClickableAfterDelay(float delay)
    {
        DOVirtual.Float(0, 0, delay, _ => { }).OnComplete(() => { _isClickable = true; });
    }

    private void DropBigIng(GameObject gameObj, float delay, float fallingDuration = 0.785f)
    {
        if(_isRoundFinished) return;
        
        var obj = Instantiate(gameObj, _thisSandwich.transform);
        DOVirtual.Float(0, 0, delay, _ => { })
            .OnComplete(() =>
            {
                obj.transform.position = _ingredientGenerationPosition;


                var path = "Audio/기본컨텐츠/Sandwich/SandwichFalling0" + Random.Range(1, 6);
                Managers.Sound.Play(SoundManager.Sound.Effect, path, 0.25f);
#if UNITY_EDITOR
                Debug.Log($"fallingsound : path : {path}");
#endif
                obj.transform.DOShakeRotation(fallingDuration,1,1,1);
                var rb = obj.GetComponent<Rigidbody>();
                SetRbConstraint(rb);
                obj.gameObject.SetActive(true);

                obj.transform.DORotateQuaternion(
                    obj.transform.rotation * Quaternion.Euler(0, Random.Range(20, 340), 0),
                    fallingDuration);

                DOVirtual.Float(0, 0, fallingDuration, _ => { })
                    .OnStart(() => { rb.AddForce(Vector3.down * fallingSpeed, ForceMode.Impulse); })
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
                            
#if UNITY_EDITOR
                            Debug.Log($"_ingsPickinigOrder : {_ingsPickinigOrder}");
#endif
                        }
                    });
            });
    }

    private void SetRbConstraint(Rigidbody rb)
    {
        var constraints = rb.constraints;
        constraints = RigidbodyConstraints.None;
        constraints = RigidbodyConstraints.FreezeRotation;
        constraints = RigidbodyConstraints.FreezePositionX;
        constraints = RigidbodyConstraints.FreezePositionZ;
        rb.constraints = constraints;
    }


    private void PlayShrinkAnim(Transform transform)
    {
        
        transform.GetComponent<Collider>().enabled = false;

        transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
               
                //deactivate delay..다시 생성시 로직을 위해서 약간의 딜레이를 줍니다. 
                DOVirtual.Float(0, 0, 0.5f, _ => { }).OnComplete(() =>
                {
                    transform.gameObject.SetActive(false);
                    
                    _previousShrinkingPoint = transform.position;
                    ScaleUpSingleIng();
                    
                });
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
        var uiInstance =
            Resources.Load<GameObject>("Common/Prefab/UI/Sandwich_UI_Scene");
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

        var posCount = 0;
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

    private void SetPositionValue()
    {
        _moveOutP = GameObject.Find("MoveOut").transform.position;
        posMid = GameObject.Find("SandwichMiddlePoint").transform.position;
        sandwichArrival = GameObject.Find("SandwichArrive").transform.position;
        _cameraLookAtSec = GameObject.Find("CameraLookAtSec").transform.position;
        defaultLookAt = GameObject.Find("MainDefaultLookAt").GetComponent<Transform>().position;
    }

    private GameObject _sandWichOrigin;

    private void SetSandwich()
    {
        _ingredientsOnBigPlate = new Transform[INGREDIENT_COUNT];
        //초기 샌드위치 할당 및 추후 접시위 
        _sandWichOrigin = GameObject.Find("Sandwich");


        _thisSandwich = Instantiate(_sandWichOrigin, transform);
        for (var i = (int)Sandwich.BreadT; i < INGREDIENT_COUNT; i++)
            _ingredientsOnBigPlate[i] = _sandWichOrigin.transform.GetChild(i);

        //샌드위치 복사본의 재료만 Deactivate합니다. 


        _thisSandwich = Instantiate(_sandWichOrigin, transform);
        _thisSandwich.transform.position = _sandWichOrigin.transform.position;
        var copiedIngredientsOnBigPlate = new Transform[INGREDIENT_COUNT];
        for (var i = (int)Sandwich.BreadT; i < INGREDIENT_COUNT; i++)
        {
            copiedIngredientsOnBigPlate[i] = _thisSandwich.transform.GetChild(i);
            copiedIngredientsOnBigPlate[i].gameObject.SetActive(false);
        }
    }

    private void MoveOutSandwich()
    {
        // DOVirtual.Float(0, 0, 0.12f, _ => { }).OnComplete(() =>
        // {
        for (var i = (int)Sandwich.BreadT; i < _ingredientsOnBigPlate.Length; i++)
            _ingredientsOnBigPlate[i]
                .DOMove(_moveOutP, 0.88f)
                .SetEase(Ease.OutSine)
                .SetDelay(0.1f * i + Random.Range(0.5f, 0.7f))
                .OnStart(() =>
                {
                    var randomChar = (char)Random.Range('A', 'F' + 1);
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_" + randomChar,
                        0.3f);
                })
                .OnComplete(() =>
                {
                    // 샌드위치 객체 복사생성으로 인해 필요없음 2/19
                    // _ingredientsOnBigPlate[i].gameObject.SetActive(false);
                });
        // });


        onRoundReady?.Invoke();
    }

    private void OnRoundReady()
    {
        ScaleAllIngs(2.5f);
    }


    // 클릭후 새로운 재료를 만들때 생성 


    // 첫번쨰는 무조건 빵이 포함되도록 섞기위한 불 연산자 입니다. 

    private bool _isFirstRound;
    private Transform[] _initialLocations;
    private void ShuffleIngredients()
    {
        if (!_isFirstRound)
        {
            _initialLocations = new Transform[_selectableIngredientsOnSmallPlates.Length];
            _initialLocations = _selectableIngredientsOnSmallPlates;
            _isFirstRound = true;
        }
       
        
        //첫번째에 빵을 반드시 포함하기위해 1부터 랜덤으로 구성합니다.
        for (var i = 1; i < _selectableIngredientsOnSmallPlates.Length; i++)
        {
            var temp = _initialLocations[i];
            var randomIndex = Random.Range(i, _selectableIngredientsOnSmallPlates.Length);
            _selectableIngredientsOnSmallPlates[i] = _initialLocations[randomIndex];
            _selectableIngredientsOnSmallPlates[randomIndex] = temp;
        }
    }

   
    private void ScaleAllIngs(float delay)
    {
        var count = 0;
        var indices = Enumerable.Range(0, _selectableIngredientsOnSmallPlates.Length).ToList();
        indices = indices.OrderBy(a => Random.value).ToList();
        var shakeSeq = DOTween.Sequence();
        ShuffleIngredients();

     
           
        

  
        {
            for (var i = 0; i < _selectableIngredientsOnSmallPlates.Length; i++)
                if (i < 5)
                    _selectableIngredientsOnSmallPlates[i].position 
                        = _ingredientsAppearPosition[i];
                else
                    _selectableIngredientsOnSmallPlates[i].gameObject.SetActive(false);


            // if (IsBothBreadInvisible())
            // {
            //     SwapIngredients();
            // }


            for (var i = 0; i < _selectableIngredientsOnSmallPlates.Length; i++)
                if (i < 5)
                {
                    _selectableIngredientsOnSmallPlates[i].gameObject.SetActive(true);

                    _selectableIngredientsOnSmallPlates[i].DOScale(_ingredientsDefaultScales[count], 1.3456789f)
                        .OnStart(() =>
                        {
#if UNITY_EDITOR
                            Debug.Log("popup sound");
#endif
                            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/pop", 0.3f);
                        })
                        .SetEase(Ease.InOutBounce).SetDelay(delay + Random.Range(0, 0.5f));

                    shakeSeq.Append(
                        _selectableIngredientsOnSmallPlates[i]
                            .DOShakeRotation(1f, 3f, 1, 15));
                }
                else
                {
                    _selectableIngredientsOnSmallPlates[i].gameObject.SetActive(false);
                }

            isGameStart = true;
        }


        var pathSeq = DOTween.Sequence();
        foreach (var obj in _selectableIngredientsOnSmallPlates)
        {
            pathSeq = DOTween.Sequence();
           
            var path = SetPath(obj.transform.position);
            obj.transform.position = path[0];

            pathSeq.Append(obj.DOPath(path, Random.Range(17f, 20.5f))
                .SetLoops(10, LoopType.Yoyo)
                .SetDelay(Random.Range(0.5f, 1.1f))
                .SetEase((Ease)Random.Range(0, 10)));
            
            shakeSeq.Append(obj.DOShakeRotation(1f, 2f, 1, 10));

            obj.GetComponent<Collider>().enabled = true;

            _pathSeqMap.TryAdd(obj, pathSeq);
            _shakeSeqMap.TryAdd(obj, shakeSeq);
        }


        foreach (var obj in _selectableIngredientsOnSmallPlates)
        {
        
            shakeSeq.Play();
            pathSeq.Play();
        }
    }


    private Vector3[] SetPath(Vector3 originPos)
    {
        var path = new Vector3[2];
        path[0] = originPos + new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
        path[1] = originPos + new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
        return path;
    }


    private bool IsBothBreadInvisible()
    {
        if ((_selectableIngredientsOnSmallPlates[(int)Sandwich.BreadT].position
             == _ingredientsAppearPosition[(int)IngsAppearPos.InvisibleA] &&
             _selectableIngredientsOnSmallPlates[(int)Sandwich.BreadB].position
             == _ingredientsAppearPosition[(int)IngsAppearPos.InvisibleB])
            ||
            (_selectableIngredientsOnSmallPlates[(int)Sandwich.BreadT].position
             == _ingredientsAppearPosition[(int)IngsAppearPos.InvisibleB] &&
             _selectableIngredientsOnSmallPlates[(int)Sandwich.BreadB].position
             == _ingredientsAppearPosition[(int)IngsAppearPos.InvisibleA]
            )) return true;


        return false;
    }

    private void SwapIngredients()
    {
        var invisibleP = _selectableIngredientsOnSmallPlates[(int)Sandwich.BreadT].position;

        _selectableIngredientsOnSmallPlates[(int)Sandwich.BreadT].position
            = _ingredientsAppearPosition[3];

        _selectableIngredientsOnSmallPlates[_currentSmallPlateLocationIndex[3]].position
            = invisibleP;
    }

    private void OnSandwichMakingFinish()
    {
        _isRoundFinished = true;
#if UNITY_EDITOR
        Debug.Log("Making Sandwich is finished");
#endif

        Managers.Sound.Play(SoundManager.Sound.Effect,
            "Audio/기본컨텐츠/Sandwich/OnSandwichMakingFinish0" + Random.Range(1, 5), 0.5f);

        PlayParticle(0.89f);

        DOVirtual.Float(0, 0, 1.5f, _ => { })
            .OnComplete(() =>
            {
                ScaleDown();
            });
       
      
        
        SendSandwichToAnimal();
    }

    private void ScaleDown()
    {
        foreach (var obj in _selectableIngredientsOnSmallPlates)
        {
            obj.DOScale(Vector3.zero, 1.0f)
                .OnStart(() =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Sandwich_Ing_Popup");
                })
                .SetEase(Ease.InOutBounce);
            
        }
    }

    private void PlayParticle(float delay)
    {
        DOVirtual.Float(0, 0, delay, _ => { }).OnComplete(() =>
        {
            _finishMakingPs.Stop();
            _finishMakingPs.Play();
        });
    }

    private Vector3 posMid;
    private Vector3 _cameraLookAtSec;
    private Vector3 sandwichArrival;
    private readonly int FOV_FAR = 12;

    private void SendSandwichToAnimal(float delay = 3f)
    {
        DOVirtual.Float(0, 0, delay, _ => { }).OnComplete(() =>
        {
            DOVirtual.Float(0, 0, 2.8f, _ => { Camera.main.transform.DOLookAt(_thisSandwich.transform.position, 1f); });

            _thisSandwich.transform
                .DOMove(posMid, 1.8f)
                .OnComplete(() =>
                {
                    _thisSandwich.transform.DOMove(sandwichArrival, 2f).OnStart(() =>
                    {
                        DOVirtual.Float(10, FOV_FAR, 2f, fov => { Camera.main.fieldOfView = fov; })
                            .SetEase(Ease.InOutSine)
                            .SetDelay(0.5f)
                            .OnComplete(() =>
                            {
                                Camera.main.transform.DOLookAt(_cameraLookAtSec, 1.0f);
                                onSandwichArrive?.Invoke();
                            });
                    });
                });
        });
    }


    public int FindIndexByName(string nameToFind)
    {
        for (var i = 0; i < _selectableIngredientsOnSmallPlates.Length; i++)
            if (_selectableIngredientsOnSmallPlates[i].name.Contains($"{nameToFind}"))
                return i;
        return -1;
    }

    private List<Sequence> _shakeSeqs;

    /// <summary>
    ///     시작시 모든 재료를 스케일업하는 것이 아닌, 사용자가 클릭한 후 하나씩 스케일업 합니다.
    ///     이미 클릭된 재료중에서만 ScaleUp하도록 합니다.
    /// </summary>
    private bool _isScalingUp;


    public void ScaleUpSingleIng( float delay = 1.88f)
    {
        var obj = _selectableIngredientsOnSmallPlates.FirstOrDefault(x =>
            !x.gameObject.gameObject.name.Contains("Bread")
            && !x.gameObject.activeSelf);
        

        //position은 delay전에 할당하여, 생성 position이 바뀌지 않도록 합니다.
        if (_ingsPickinigOrder <= (int) ING_MAX_COUNT - 2)
        {
            obj = _selectableIngredientsOnSmallPlates.FirstOrDefault(x =>
                !x.gameObject.gameObject.name.Contains("Bread")
                && !x.gameObject.activeSelf);
#if UNITY_EDITOR
            Debug.Log($"재료표출:{obj.gameObject.name} 생성위치 : {_previousShrinkingPoint}");
#endif
            AppendAnim(obj);
           
        }
        
        else
        {
#if UNITY_EDITOR
            Debug.Log($"빵 표출가능 ");
#endif
            obj = _selectableIngredientsOnSmallPlates.FirstOrDefault(x =>
                x.gameObject.gameObject.name.Contains("Bread")
                && !x.gameObject.activeSelf);
            if (obj != null)
            {
#if UNITY_EDITOR
                Debug.Log($"빵 표출: 생성위치 : {_previousShrinkingPoint}");
#endif
                AppendAnim(obj);
            }
            else
            {


                obj = _selectableIngredientsOnSmallPlates
                    .FirstOrDefault(x => !x.gameObject.activeSelf);
                if (obj == null)
                {
                    _isScalingUp = false;
                    return;
                }
                AppendAnim(obj);
            }


           
        }

        if (obj == null)
        {
            _isScalingUp = false;
            return;
        }

   
      
        
   
       

    }

    private void AppendAnim(Transform obj)
    {
        
        if (_pathSeqMap[obj].IsActive())
        {
#if UNITY_EDITOR
            Debug.Log($"이전트윈 킬!");
#endif
            _pathSeqMap[obj].Kill();
        }

        
        
        obj.localScale = Vector3.zero;
        obj.gameObject.SetActive(true);
        
        obj.DOScale(_ingredientsDefaultScales[0], 2f)
            .OnStart(() =>
            {
                _shakeSeqMap[obj].Kill();
                _shakeSeqMap[obj] = DOTween.Sequence();
                _shakeSeqMap[obj].Append(
                    obj.DOShakeRotation(1f, 3f, 1, 15));
                _shakeSeqMap[obj].Play();
                
               
                obj.GetComponent<Collider>().enabled = true;
                
               
                var newPath = new Vector3[2];
                newPath[0] =  _previousShrinkingPoint + new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
                newPath[1] = _previousShrinkingPoint + new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
                obj.position = newPath[0];
                _pathSeqMap[obj] = DOTween.Sequence();
                _pathSeqMap[obj].
                    Append(obj.DOPath(newPath, Random.Range(7f, 10.5f), PathType.CatmullRom)
                    .SetLoops(30, LoopType.Yoyo)
                    .SetDelay(Random.Range(0.1f, 0.5f))       
                    .SetEase((Ease)Random.Range(0, 10)));
       
                _pathSeqMap[obj].Play();
            })
            .SetEase(Ease.InOutBounce)
            .OnComplete(() =>
            {
#if UNITY_EDITOR
                Debug.Log($"scaleup 종료, 최종 생성위치 : {obj.transform.position}");
#endif
                _isScalingUp = false;
            });

    }


    private void AllFinishAnimOver()
    {
        SendBackToDefault();
    }


    private void SendBackToDefault(float delay = 3f)
    {
        DOVirtual.Float(0, 0, delay, _ => { }).OnComplete(() =>
        {
            DOVirtual.Float(0, 0, 0f, _ =>
            {
                Camera.main.transform.DOLookAt(defaultLookAt, 1.5f);
                DOVirtual.Float(
                        FOV_FAR, 10, 1.5f, fov => { Camera.main.fieldOfView = fov; }).SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        Destroy(_thisSandwich);
                        InitForNewRound();
                    });
            });
        });
    }

    private void InitForNewRound()
    {
        
        _isClickable = true;
        _ingsPickinigOrder = 1;
        _isRoundFinished = false;
        
        _thisSandwich = Instantiate(_sandWichOrigin);
        _thisSandwich.transform.position = _sandWichOrigin.transform.position;
     
        foreach (var obj in _selectableIngredientsOnSmallPlates)
        {

            DOTween.KillAll();
        }

                
#if UNITY_EDITOR
        Debug.Log($"게임재시작");
#endif
        ScaleAllIngs(RESTART_DELAY);

        onSandwichMakingRestart?.Invoke();
    }

    public static event Action onSandwichMakingRestart;
}