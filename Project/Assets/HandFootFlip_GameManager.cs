using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class HandFootFlip_GameManager : IGameManager
{
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

    private Vector3[] _pathPos;

    private int COLOR_COUNT = 5;


    //쌍이되는 컬러를  String으로 할당하여, 색상이름(string)에 따라 제어.
    private Dictionary<string, Color> _colorPair;

    private Dictionary<int, MeshRenderer> _meshRendererMap;
    private Dictionary<int, Sequence> _colorSequences;
    private Dictionary<int, Sequence> _moveSequence;
    private Dictionary<int, Sequence> _scaleSequence;
    private MeshRenderer[] _meshRenderers;


    private bool _isRayCasterMoving;
    private float rayMoveCurrentTime;

    private ParticleSystem _explosionParticle;

    private enum RayCasterMovePosition
    {
        Start,
        Arrival,
        Max
    }

    protected override void OnRaySynced()
    {
        base.OnRaySynced();

        if (!isStartButtonClicked) return;
        FlipAndChangeColor(GameManager_Ray);
        //  ChangeColor(GameManager_Ray);
    }


    //instance ID를 통한 접근 및 제어
    private Dictionary<int, Print> _PrintMap;


    protected override void Init()
    {
        base.Init();

        onRaycasterMoveFinish += OnRayCasterMoveFin;
        onRaycasterMoveFinish -= OnRayCasterMoveFin;

        _pathPos = new Vector3[(int)RayCasterMovePosition.Max];

        _pathPos[0] = movePath[(int)RayCasterMovePosition.Start].position;
        _pathPos[1] = movePath[(int)RayCasterMovePosition.Arrival].position;


        _PrintMap = new Dictionary<int, Print>();
        _colorPair = new Dictionary<string, Color>();
        _meshRendererMap = new Dictionary<int, MeshRenderer>();
        _rotateVector = new Vector3(0, 0, -180);


        _colorSequences = new Dictionary<int, Sequence>();
    }

    private void OnDestroy()
    {
        onRaycasterMoveFinish -= OnRayCasterMoveFin;
    }

    private void Update()
    {
        rayMoveCurrentTime += Time.deltaTime;
        if (rayMoveCurrentTime > raycasterMoveInterval)
        {
            RayCasterMovePlay();
            rayMoveCurrentTime = 0;
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

            // 연속된 색상 쌍을 딕셔너리에 추가한다. (중복값 허용없고, ColorName에따라 색상변경
            _colorPair.TryAdd(_prints[i].defaultColorName, colorOptions[i % COLOR_COUNT]);
#if UNITY_EDITOR
            Debug.Log(
                $"colorOption name:{i} : {colorOptions[i % COLOR_COUNT]}");
#endif

            //Print 캐싱, Flip에서는 InstaceID를 기반으로 Prints를 참조 및 제어한다.
            var currentTransform = printsParent.transform.GetChild(i);
            //Transform Instance ID가 아닌 GameObject의 Instance ID를 참조할것에 주의합니다
            var currentInstanceID = currentTransform.gameObject.GetInstanceID();

            _PrintMap.TryAdd(currentInstanceID, _prints[i]);

            //MeshRenderer 캐싱, Instace ID로 MeshRenderer제어
            _meshRendererMap.TryAdd(currentInstanceID, currentTransform.GetComponent<MeshRenderer>());


            // _colorSequences.TryAdd(currentInstanceID, DOTween.Sequence());

            _animalRayCasterParent = GameObject.Find("Animal_RayCaster");
            _animalRayCasters = _animalRayCasterParent.GetComponentsInChildren<Transform>();
            _wait = new WaitForSeconds(0.18f);
        }
    }

    private RaycastHit[] hits;


    private void FlipAndChangeColor(Ray ray)
    {
        hits = Physics.RaycastAll(ray);

        foreach (var hit in hits)
        {
            var currentInstanceID = hit.transform.gameObject.GetInstanceID();


            if (_PrintMap.ContainsKey(currentInstanceID))
            {
                if (_PrintMap[currentInstanceID].seq.IsActive()) return;
            }
            else
            {
                return;
            }

            _PrintMap[currentInstanceID].seq = DOTween.Sequence();

            if (_isAnimalMoving)
            {
                if (!_PrintMap[currentInstanceID].isFlipped)
                {
                    _PrintMap[currentInstanceID].seq
                        .Append(
                            hit.transform
                                .DOLocalRotate(_rotateVector + _PrintMap[currentInstanceID].defaultVector, 0.3f)
                                .SetEase(Ease.InOutQuint));

                    _meshRendererMap[currentInstanceID].material
                        .DOColor(_colorPair[_PrintMap[currentInstanceID].defaultColorName], 0.2f)
                        .SetDelay(0.235f);
                
                    hit.transform.gameObject.GetComponentInChildren<MeshRenderer>().material
                        .DOColor(_colorPair[_PrintMap[currentInstanceID].defaultColorName], 0.2f)
                        .SetDelay(0.235f);

                }

                else
                {
                    _PrintMap[currentInstanceID].seq
                        .Append(hit.transform
                            .DOLocalRotate(_PrintMap[currentInstanceID].defaultVector, 0.3f)
                            .SetEase(Ease.InOutQuint)
                        );
                


                    _meshRendererMap[currentInstanceID].material
                        .DOColor(_PrintMap[currentInstanceID].defaultColor, 0.2f)
                        .SetDelay(0.235f);
                
                    hit.transform.gameObject.GetComponentInChildren<MeshRenderer>().material
                        .DOColor(_PrintMap[currentInstanceID].defaultColor, 0.2f)
                        .SetDelay(0.235f);
                }
                
                DOVirtual
                    .Float(0, 1, 0.08f, _ => { })
                    .OnComplete(() =>
                    {
                        _PrintMap[currentInstanceID].isFlipped = !_PrintMap[currentInstanceID].isFlipped;
                    });
            }
            else
            {
                _PrintMap[currentInstanceID].seq
                    .Append(
                        hit.transform
                            .DOLocalRotate(_rotateVector + _PrintMap[currentInstanceID].defaultVector, 0.3f)
                            .SetEase(Ease.InOutQuint));

                _meshRendererMap[currentInstanceID].material
                    .DOColor(_colorPair[_PrintMap[currentInstanceID].defaultColorName], 0.2f)
                    .SetDelay(0.235f);
                
                hit.transform.gameObject.GetComponentInChildren<MeshRenderer>().material
                    .DOColor(_colorPair[_PrintMap[currentInstanceID].defaultColorName], 0.2f)
                    .SetDelay(0.235f);
            }
        
            
            
            


            _PrintMap[currentInstanceID].seq.Play();

     

#if UNITY_EDITOR
#endif

            break;
        }
    }

    private static event Action onRaycasterMoveFinish;

    private void UnifyColor()
    {
        foreach (var key in _colorPair.Keys.ToList()) _colorPair[key] = colorOptions[1];
        foreach (var key in _PrintMap.Keys.ToList()) _PrintMap[key].isFlipped = false;
    }


    private void OnRayCasterMoveFin()
    {
        ShuffleColors();
#if UNITY_EDITOR
        Debug.Log(
            "RayCasterMoveFin Invoked!");
#endif
        for (var i = 0; i < PRINTS_COUNT; i++)
            // 연속된 색상 쌍을 딕셔너리에 추가한다. (중복값 허용없고, ColorName에따라 색상변경
            _colorPair.TryAdd(_prints[i].defaultColorName, colorOptions[i % COLOR_COUNT]);
        
        
        
        foreach (var key in _PrintMap.Keys.ToList()) _PrintMap[key].isFlipped = false;
    }

    private bool _isAnimalMoving;
    private void RayCasterMovePlay()
    {
        UnifyColor();

        _isAnimalMoving = true;
        _animalRayCasterParent.transform.position = _pathPos[(int)RayCasterMovePosition.Start];
        _animalRayCasterParent.transform
            .DOMove(_pathPos[(int)RayCasterMovePosition.Arrival], 3.2f)
            .OnStart(() => { _rayCastCoroutine = StartCoroutine(RayCasterMoveCoroutine()); })
            .OnUpdate(() => { rayMoveCurrentTime = 0; })
            .OnComplete(() =>
            {
                
                    if (_rayCastCoroutine != null)
                        StopCoroutine(_rayCastCoroutine);

                    onRaycasterMoveFinish?.Invoke();
                    _isAnimalMoving = false;
            })
            .SetEase(Ease.Linear);
    }


    private Coroutine _rayCastCoroutine;
    private WaitForSeconds _wait;

    private IEnumerator RayCasterMoveCoroutine()
    {
        while (true)
        {
            RayCasterMove();
            yield return _wait; // 0.2초 간격으로 대기
        }
    }

    private void RayCasterMove()
    {
        // 각 자식 객체의 위치에서 아래 방향으로 레이를 발사합니다.
        foreach (var childTransform in _animalRayCasters)
            if (childTransform != transform)
            {
                var raycasterMoveRay = new Ray(childTransform.position, Vector3.down);
                RaycastHit hit;

                // 레이캐스트 발사 (예: 100 유닛 거리까지)
                if (Physics.Raycast(raycasterMoveRay, out hit, 1000f))
                {
                    FlipAndChangeColor(raycasterMoveRay);
                    //  ChangeColor(raycasterMoveRay);
#if UNITY_EDITOR
//Debug.Log("Raycastermove hit: " + hit.transform.name);
#endif
                }
            }
    }


//     private void ChangeColor(Ray ray)
//     {
//         GameManager_Hits = Physics.RaycastAll(ray);
//
//         foreach (var hit in GameManager_Hits)
//         {
// #if UNITY_EDITOR
//
// #endif
//             var currentInstanceID = hit.transform.gameObject.GetInstanceID();
//             if (_meshRendererMap.ContainsKey(currentInstanceID))
//             {
//                 // var scaleSeq = DOTween.Sequence();
//                 var sequence = DOTween.Sequence();
//
//
//                 // 스케일 애니메이션
//                 // var defaultScale = hit.transform.localScale;
//                 // var targetScale = defaultScale * _scaleInterval;
//                 // hit.transform.DOScale(targetScale, 0.53f).SetEase(Ease.InOutSine)
//                 //     .OnComplete(() => { hit.transform.DOScale(defaultScale, 0.35f).SetEase(Ease.InOutSine).SetDelay(0.1f); });
//
//                 if (_colorSequences.ContainsKey(currentInstanceID))
//                 {
//                     if (!_colorSequences[currentInstanceID].IsActive() || _colorSequences[currentInstanceID] == null)
//                     {
//                         if (!_PrintMap[currentInstanceID].isFlipped)
//                         {
// #if UNITY_EDITOR
//                             Debug.Log($"Changing to Pair Color : {_colorPair[_PrintMap[currentInstanceID].defaultColorName]}");
// #endif
//                             sequence
//                                 .Append(_meshRendererMap[currentInstanceID].material
//                                     .DOColor(_colorPair[_PrintMap[currentInstanceID].defaultColorName], 0.5f)
//                                 );
//                         }
//                         else
//                         {
// #if UNITY_EDITOR
//                             Debug.Log("Default Color");
// #endif
//                             sequence
//                                 .Append(_meshRendererMap[currentInstanceID].material
//                                     .DOColor(_PrintMap[currentInstanceID].defaultColor, 0.5f)
//                                 );
//                         }
//                     }
//
//
//                     _colorSequences.TryAdd(currentInstanceID, sequence);
//                     sequence.Play();
//                 }
//                 else
//                 {
// #if UNITY_EDITOR
// #endif
//                 }
//             }
//
//         }
//     }

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
        public Vector3 defaultVector;
        public Sequence seq;
        public Color defaultColor;
        public string defaultColorName;

        public bool type;
        public const bool HAND = false;
        public const bool FOOT = true;
    }
}