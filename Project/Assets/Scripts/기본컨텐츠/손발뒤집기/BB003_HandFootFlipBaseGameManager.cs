using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class BB003_HandFootFlipBaseGameManager : Base_GameManager
{
    private enum RayCasterMovePosition
    {
        Start,
        Arrival,
        Max
    }

    private RaycastHit hit;

    private Color _currentUnifiedColor = Color.red;
    private Color _previousUniColor = Color.black;
    private Print[] _prints;
    private int PRINTS_COUNT;
    private Vector3 _rotateVector;

    [SerializeField] private Color[] colorOptions;


    [Header("RayCaster Setting")] [Range(0, 60)]
    public float raycasterMoveInterval;

    [SerializeField] private Transform[] movePath;

    // 파도타기방식(형태) RayCasting 및 색상변환로직 구현용 RayCaster.              
    private GameObject _animalRayCasterParent;
    private Transform[] _animalRayCasters;
    private Transform _animals;

    private Vector3[] _pathPos;

    private int COLOR_COUNT = 5;

    //instance ID를 통한 접근 및 제어
    private Dictionary<int, Print> _printMap;

    //쌍이되는 컬러를  String으로 할당하여, 색상이름(string)에 따라 제어.
    private Dictionary<string, Color> _colorPair;
    private Dictionary<int, MeshRenderer> _meshRendererMap;
    private Dictionary<int, MeshRenderer> _childMeshRendererMap;
    private Dictionary<int, Sequence> _moveSequence;
    private Dictionary<int, Sequence> _scaleSequence;
    private MeshRenderer[] _meshRenderers;


    private bool _isRayCasterMoving;
    private float _animalMoveCurrentTime;

    private ParticleSystem _explosionParticle;
    private Collider _animalCollider;


    public override void OnRaySynced()
    {
        
        if (_isAnimalMoving) return;
        if (!PreCheckOnRaySync()) return;
        if (SceneManager.GetActiveScene().name != "BB003") return;
        
        
        
        FlipAndChangeColor(GameManager_Ray);
        DEV_OnValidClick();
        
        
        //  ChangeColor(GameManager_Ray);
    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        initialMessage= "손발을 터치해 뒤집어 보세요!";
        _uiManagerCommonBehaviorController.ShowInitialMessage(initialMessage);
        Managers.Sound.Play(SoundManager.Sound.Narration, "OnGameStartNarration/" + SceneManager.GetActiveScene().name + "_intronarration");
    }

    protected override void Init()
    {
        base.Init();

        onRaycasterMoveFinish -= OnRayCasterMoveFin;
        onRaycasterMoveFinish += OnRayCasterMoveFin;


        _pathPos = new Vector3[(int)RayCasterMovePosition.Max];

        _pathPos[0] = movePath[(int)RayCasterMovePosition.Start].position;
        _pathPos[1] = movePath[(int)RayCasterMovePosition.Arrival].position;


        _printMap = new Dictionary<int, Print>();
        _colorPair = new Dictionary<string, Color>();
        _meshRendererMap = new Dictionary<int, MeshRenderer>();
        _childMeshRendererMap = new Dictionary<int, MeshRenderer>();
        _rotateVector = new Vector3(180, 0, 0);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        onRaycasterMoveFinish -= OnRayCasterMoveFin;
       
    }

    private void Update()
    {
        if (!isStartButtonClicked) return;

        if (!_isAnimalMoving) _animalMoveCurrentTime += Time.deltaTime;


        if (_animalMoveCurrentTime > raycasterMoveInterval)
        {
            _isAnimalMoving = true;
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFootFlip/Alert");

            DOVirtual.Float(0, 0, 0.8f, _ => { }).OnComplete(() => { RayCasterMovePlay(); });
            _animalMoveCurrentTime = 0;
        }
    }


    private void Start()
    {
        Debug.Log($"(start)color option length: {colorOptions.Length}");
        var printsParent = GameObject.Find("Prints");

        if (printsParent == null)
        {
            Debug.LogError("Prints GameObject not found in the scene.");
            return;
        }


        COLOR_COUNT = colorOptions.Length;
        PRINTS_COUNT = printsParent.transform.childCount;

        _prints = new Print[PRINTS_COUNT];

        // 색상 배열을 섞는다
        ShuffleColors();

        for (var i = 0; i < PRINTS_COUNT; i++)
        {
            _prints[i] = new Print
            {
                printObj = printsParent.transform.GetChild(i).gameObject,
                defaultVector = printsParent.transform.GetChild(i).rotation.eulerAngles,
                defaultColor = printsParent.transform.GetChild(i).GetComponent<MeshRenderer>().material.color,
                defaultColorName = printsParent.transform.GetChild(i).gameObject.name.Substring(5),
                isFlipped = false
            };
#if UNITY_EDITOR
            Debug.Log($"colorname: {printsParent.transform.GetChild(i).gameObject.name.Substring(5)}");
#endif
            // 연속된 색상 쌍을 딕셔너리에 추가한다. (중복값 허용없고, ColorName에따라 색상변경
            _colorPair.TryAdd(_prints[i].defaultColorName, colorOptions[i % COLOR_COUNT]);


            //Print 캐싱, Flip에서는 InstaceID를 기반으로 Prints를 참조 및 제어한다.
            var currentTransform = printsParent.transform.GetChild(i);
            //Transform Instance ID가 아닌 GameObject의 Instance ID를 참조할것에 주의합니다
            var currentInstanceID = currentTransform.gameObject.GetInstanceID();

            _printMap.TryAdd(currentInstanceID, _prints[i]);

            //MeshRenderer 캐싱, Instace ID로 MeshRenderer제어
            _meshRendererMap.TryAdd(currentInstanceID, currentTransform.GetComponent<MeshRenderer>());
            _childMeshRendererMap.TryAdd(currentInstanceID,
                currentTransform.GetChild(0).GetComponentInChildren<MeshRenderer>());

            _animals = GameObject.Find("HandFlippable_Animal").GetComponent<Transform>();
        }
    }

    private void FlipAndChangeColor(Ray ray)
    {
        if (Physics.Raycast(ray, out hit))
        {
            var currentInstanceID = hit.transform.gameObject.GetInstanceID();

            if (_printMap.ContainsKey(currentInstanceID))
                if (_printMap[currentInstanceID].seq.IsActive() || _printMap[currentInstanceID].isNowFlipping)
                {
                    Debug.Log("The seq is currently Active! Click later..");
                    return;
                }

            _printMap[currentInstanceID].seq = DOTween.Sequence();

            _printMap[currentInstanceID].seq
                .Append(
                    hit.transform
                        .DOLocalRotate(
                            _rotateVector + _printMap[currentInstanceID].printObj.transform.rotation.eulerAngles, 0.38f)
                        .SetEase(Ease.InOutQuint)
                        .OnStart(() =>
                        {
                            var randomChar = (char)Random.Range('A', 'F' + 1);
                            Managers.Sound.Play(SoundManager.Sound.Effect,
                                "Audio/BasicContents/HandFootFlip/Click_" + randomChar);

                            _printMap[currentInstanceID].isNowFlipping = true;

                            if (!_printMap[currentInstanceID].isFlipped)
                            {
                                Debug.Log("Changing to Pair");
                                _meshRendererMap[currentInstanceID].material
                                    .DOColor(_colorPair[_printMap[currentInstanceID].defaultColorName], 0.2f)
                                    .SetDelay(0.235f);

                                _childMeshRendererMap[currentInstanceID].material
                                    .DOColor(_colorPair[_printMap[currentInstanceID].defaultColorName], 0.2f)
                                    .SetDelay(0.235f);
                            }
                            else
                            {
                                Debug.Log("Changing to Default");

                                _meshRendererMap[currentInstanceID].material
                                    .DOColor(_printMap[currentInstanceID].defaultColor, 0.2f)
                                    .SetDelay(0.235f);

                                _childMeshRendererMap[currentInstanceID].material
                                    .DOColor(_printMap[currentInstanceID].defaultColor, 0.2f)
                                    .SetDelay(0.235f);
                            }
                        })
                        .OnComplete(() =>
                        {
                            if (_isAnimalMoving)
                                //객체가 두번 뒤집어 지지않도록 딜레이를 주어 bool값을 변화시킵니다. 
                                DOVirtual.Float(0, 1, 1.25f, _ => { }).OnComplete(() =>
                                {
                                    _printMap[currentInstanceID].isNowFlipping = false;
                                });
                            else
                                _printMap[currentInstanceID].isNowFlipping = false;
                        }));

            DOVirtual
                .Float(0, 1, 0.08f, _ => { })
                .OnComplete(() =>
                {
                    _printMap[currentInstanceID].isFlipped = !_printMap[currentInstanceID].isFlipped;
                });

            _printMap[currentInstanceID].seq.Play();
        }
    }


    public void FlipAndChangeColor(Transform collidedTransform)
    {
#if UNITY_EDITOR
        Debug.Log("filpping by Collider!");
#endif

        var currentInstanceID = collidedTransform.gameObject.GetInstanceID();

        if (_printMap.ContainsKey(currentInstanceID))
            if (_printMap[currentInstanceID].seq.IsActive() || _printMap[currentInstanceID].isNowFlipping)
            {
#if UNITY_EDITOR
                Debug.Log("The seq is currently Active! Click later..");
#endif
                return;
            }

        _printMap[currentInstanceID].seq = DOTween.Sequence();

        _printMap[currentInstanceID].seq
            .Append(
                collidedTransform
                    .DOLocalRotate(_rotateVector + _printMap[currentInstanceID].printObj.transform.rotation.eulerAngles,
                        0.38f)
                    .SetEase(Ease.InOutQuint)
                    .OnStart(() =>
                    {
                        var randomChar = (char)Random.Range('A', 'F' + 1);
                        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFootFlip/Click_" + randomChar);

                        _printMap[currentInstanceID].isNowFlipping = true;

                        if (!_printMap[currentInstanceID].isFlipped)
                        {
                            Debug.Log("Changing to Pair");
                            _meshRendererMap[currentInstanceID].material
                                .DOColor(_colorPair[_printMap[currentInstanceID].defaultColorName], 0.2f)
                                .SetDelay(0.235f);

                            _childMeshRendererMap[currentInstanceID].material
                                .DOColor(_colorPair[_printMap[currentInstanceID].defaultColorName], 0.2f)
                                .SetDelay(0.235f);
                        }
                        else
                        {
                            Debug.Log("Changing to Default");

                            _meshRendererMap[currentInstanceID].material
                                .DOColor(_printMap[currentInstanceID].defaultColor, 0.2f)
                                .SetDelay(0.235f);

                            _childMeshRendererMap[currentInstanceID].material
                                .DOColor(_printMap[currentInstanceID].defaultColor, 0.2f)
                                .SetDelay(0.235f);
                        }
                    })
                    .OnComplete(() =>
                    {
                        if (_isAnimalMoving)
                            //객체가 두번 뒤집어 지지않도록 딜레이를 주어 bool값을 변화시킵니다. 
                            DOVirtual.Float(0, 1, 1.25f, _ => { }).OnComplete(() =>
                            {
                                _printMap[currentInstanceID].isNowFlipping = false;
                            });
                        else
                            _printMap[currentInstanceID].isNowFlipping = false;
                    }));

        DOVirtual
            .Float(0, 1, 0.08f, _ => { })
            .OnComplete(() => { _printMap[currentInstanceID].isFlipped = !_printMap[currentInstanceID].isFlipped; });

        _printMap[currentInstanceID].seq.Play();


#if UNITY_EDITOR
#endif
    }

    private static event Action onRaycasterMoveFinish;

    private void UnifyColor()
    {
        ShuffleColors();
        _currentUnifiedColor = colorOptions[0];

        //처음 while문 조건 통과를 위한 구성.

        //이전 컬러와 중복 피하도록 구성
        while (_currentUnifiedColor == _previousUniColor)
        {
            ShuffleColors();
            _currentUnifiedColor = colorOptions[0];
        }


        foreach (var key in _colorPair.Keys.ToList()) _colorPair[key] = _currentUnifiedColor;

        foreach (var print in _prints) print.isFlipped = false;

        _previousUniColor = _currentUnifiedColor;
    }


    private void OnRayCasterMoveFin()
    {
        _colorPair.Clear();


        for (var i = 0; i < PRINTS_COUNT; i++)
        {
            _colorPair.TryAdd(_prints[i].defaultColorName, colorOptions[i % COLOR_COUNT]);
            Debug.Log($"suffle Paircolor again...: Color info: {colorOptions[i % COLOR_COUNT]}");
        }
    }

    private bool _isAnimalMoving;
    private readonly float _animalMoveDuration = 6.0f;

    private void RayCasterMovePlay()
    {
        UnifyColor();


        /*
         isNowFlipping이 true 인 경우, 유저클릭과 동물움직임 동시 실행 시, Flip함수가 동작하지 않습니다.
        따라서 아래와 같이, _printMap의 bool값을 수정해줍니다. (isNowFlipping 조건 참고)
        */
        foreach (var print in _printMap.Values.ToList()) print.isNowFlipping = false;

        _animals.transform.position = _pathPos[(int)RayCasterMovePosition.Start];
        _animals.transform
            .DOMove(_pathPos[(int)RayCasterMovePosition.Arrival], _animalMoveDuration)
            .OnStart(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFootFlip/Herd");

                DOVirtual.Float(0, 1, 0.35f, _ => { })
                    .OnStart(() =>
                    {
                        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFootFlip/Giggle_A");
                        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFootFlip/Giggle_B");
                    })
                    .OnComplete(() =>
                    {
                        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFootFlip/Elephant");
                        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFootFlip/Elephant_B");
                    });


                DOVirtual.Float(0, 1, 2.3f, _ => { })
                    .OnComplete(() => { _isAnimalMoving = false; });


                //동물이 모두 이동하기전 관련 초기화 로직 수행
                DOVirtual.Float(0, 1, _animalMoveDuration - 0.15f, _ => { })
                    .OnComplete(() =>
                    {
                        onRaycasterMoveFinish?.Invoke();
                        _isAnimalMoving = false;
                        Managers.Sound.FadeOut(SoundManager.Sound.Effect);
                    });
            })
            .OnUpdate(() => { _animalMoveCurrentTime = 0; });
    }


    private Coroutine _rayCastCoroutine;
    private WaitForSeconds _wait;

    // private IEnumerator RayCasterMoveCoroutine()
    // {
    //     while (true)
    //     {
    //         RayCasterMove();
    //         yield return _wait; // 0.2초 간격으로 대기
    //     }
    // }

    //     콜라이더 로직 변경으로 인한 로직 삭제 2/7/24
    //     private void RayCasterMove()
    //     {
    //         // 각 자식 객체의 위치에서 아래 방향으로 레이를 발사합니다.
    //         foreach (var childTransform in _animalRayCasters)
    //             if (childTransform != transform)
    //             {
    //                 var raycasterMoveRay = new Ray(childTransform.position, Vector3.down);
    //                 RaycastHit hit;
    //
    //                 // 레이캐스트 발사 (예: 100 유닛 거리까지)
    //                 if (Physics.Raycast(raycasterMoveRay, out hit, 100))
    //                 {
    //                     FlipAndChangeColor(raycasterMoveRay);
    //                     //  ChangeColor(raycasterMoveRay);
    // #if UNITY_EDITOR
    // //Debug.Log("Raycastermove hit: " + hit.transform.name);
    // #endif
    //                 }
    //             }
    //    }


    private void ShuffleColors()
    {
        for (var i = 0; i < colorOptions.Length; i++)
        {
            var temp = colorOptions[i];
            var randomIndex = Random.Range(i, colorOptions.Length);
            colorOptions[i] = colorOptions[randomIndex];
            colorOptions[randomIndex] = temp;
        }
    }

    public class Print
    {
        public GameObject printObj;
        public bool isFlipped;
        public bool isNowFlipping;
        public Vector3 defaultVector;
        public Sequence seq;
        public Color defaultColor;
        public string defaultColorName;

        public bool type;
        public const bool HAND = false;
        public const bool FOOT = true;
    }
}