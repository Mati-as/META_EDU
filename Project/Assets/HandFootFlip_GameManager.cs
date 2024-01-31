using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HandFootFlip_GameManager : IGameManager
{
    private Print[] _prints;
    private Vector3 _rotateVector;

    public Color[] colorOptions; 

    [Header("RayCaster Setting")] [Range(0, 60)]
    public float raycasterMoveInterval;

    [SerializeField] private Transform[] movePath;

    // 파도타기방식(형태) RayCasting 및 색상변환로직 구현용 RayCaster.              
    private GameObject _animalRayCasterParent;
    private Transform[] _animalRayCasters;
    private Vector3[] _pathPos;

    
    
    //쌍이되는 컬러를  String으로 할당하여, 색상이름(string)에 따라 제어.
    private Dictionary<string, Color> _colorPair;
    


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

        Flip();
    }


    //instance ID를 통한 접근 및 제어
    private Dictionary<int, Print> _PrintMap;
    

    protected override void Init()
    {
        base.Init();

        // 색상 배열을 섞는다
        ShuffleColors();

        
        _PrintMap = new Dictionary<int, Print>();
        _colorPair = new Dictionary<string, Color>();
        for (int i = 0; i < colorOptions.Length; i += 2)
        {
            ShuffleColors();
        }

        _rotateVector = new Vector3(0, 0, 180);

        var printsParent = GameObject.Find("Prints");


        if (printsParent == null)
        {
            Debug.LogError("Prints GameObject not found in the scene.");
            return;
        }


        var CHILD_COUNT = printsParent.transform.childCount;

        _prints = new Print[CHILD_COUNT];

        for (var i = 0; i < CHILD_COUNT; i++)
        {
            _prints[i] = new Print
            {
                printObj = printsParent.transform.GetChild(i).gameObject,
                defaultVector = printsParent.transform.GetChild(i).rotation.eulerAngles,
                defaultColor = printsParent.transform.GetChild(i).GetComponent<MeshRenderer>().material.color
                ,defaultColorName = printsParent.transform.GetChild(i).gameObject.name.Substring(5)
            };

            //Print 캐싱, Flip에서는 InstaceID를 기반으로 Prints를 참조 및 제어한다.
            var currentTransform = printsParent.transform.GetChild(i);
            _PrintMap.TryAdd(currentTransform.GetInstanceID(), _prints[i]);
            
            // 연속된 색상 쌍을 딕셔너리에 추가한다. (중복값 허용없고, ColorName에따라 색상변경
            _colorPair.TryAdd(_prints[i].defaultColorName, colorOptions[i]); 
            
         
            
            
            _animalRayCasterParent = GameObject.Find("Animal_RayCaster");
            _animalRayCasters = _animalRayCasterParent.GetComponentsInChildren<Transform>();
            _wait = new WaitForSeconds(0.2f);
            
        }
    }

    private RaycastHit hit;

    private void Flip()
    {
        foreach (var hit in GameManager_Hits)
        {
            var currentInstanceID = hit.transform.GetInstanceID();

            if (_PrintMap.ContainsKey(currentInstanceID))
            {
                if (_PrintMap[currentInstanceID].seq.IsActive()) return;

                _PrintMap[currentInstanceID].seq = DOTween.Sequence();

                if (!_PrintMap[currentInstanceID].isFlipped)
                    _PrintMap[currentInstanceID].seq
                        .Append(hit.transform
                            .DORotate(_rotateVector, 0.6f))
                        .SetEase(Ease.InOutSine);

                else
                    _PrintMap[currentInstanceID].seq
                        .Append(hit.transform
                            .DORotate(_PrintMap[currentInstanceID].defaultVector, 0.6f)
                            .SetEase(Ease.InOutSine));


                _PrintMap[currentInstanceID].isFlipped = !_PrintMap[currentInstanceID].isFlipped;
                _PrintMap[currentInstanceID].seq.Play();
                break;
            }

#if UNITY_EDITOR
            Debug.Log(
                $"Hit Info :  isFliiped {_PrintMap[currentInstanceID].isFlipped}   objName: {hit.transform.gameObject.name} ");
#endif
        }
    }

    private void RayCasterMovePlay()
    {
        _animalRayCasterParent.transform.position = _pathPos[(int)RayCasterMovePosition.Start];
        _animalRayCasterParent.transform
            .DOMove(_pathPos[(int)RayCasterMovePosition.Arrival], 3.2f)
            .OnStart(() => { _rayCastCoroutine = StartCoroutine(RayCasterMoveCoroutine()); })
            .OnComplete(() =>
            {
                {
                    if (_rayCastCoroutine != null)
                        StopCoroutine(_rayCastCoroutine);
                }
            })
            .SetEase(Ease.Linear);
    }


    private Coroutine _rayCastCoroutine;
    private WaitForSeconds _wait;

    private IEnumerator RayCasterMoveCoroutine()
    {
        while (true)
        {
//            Debug.Log("Move And RayCastring");
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
                if (Physics.Raycast(raycasterMoveRay, out hit, 1000f)) Flip();
                //      Debug.Log("Raycastermove hit: " + hit.transform.name);
            }
    }

    private Dictionary<int, MeshRenderer> _meshRendererMap;
    private Dictionary<int, Sequence> _colorSequences;
    private Dictionary<int, Sequence> _moveSequence;
    private Dictionary<int, Sequence> _scaleSequence;
    private MeshRenderer[] _meshRenderers;

    private void RandomlyChangeColor(Ray ray)
    {
        GameManager_Hits = Physics.RaycastAll(ray);

        foreach (var hit in GameManager_Hits)
        {
            var currentInstance = hit.transform.gameObject.GetComponent<MeshRenderer>().GetInstanceID();
            if (_meshRendererMap.ContainsKey(currentInstance))
            {
                // var scaleSeq = DOTween.Sequence();

                var sequence = DOTween.Sequence();

                // 스케일 애니메이션
                // var defaultScale = hit.transform.localScale;
                // var targetScale = defaultScale * _scaleInterval;
                // hit.transform.DOScale(targetScale, 0.53f).SetEase(Ease.InOutSine)
                //     .OnComplete(() => { hit.transform.DOScale(defaultScale, 0.35f).SetEase(Ease.InOutSine).SetDelay(0.1f); });

                if (_colorSequences.ContainsKey(currentInstance))
                {
                    if (!_colorSequences[currentInstance].IsActive())
                    {

                        if (!_colorSequences[currentInstance].IsActive())
                        {
                            if (!_PrintMap[currentInstance].isFlipped)
                            {
                                sequence
                                    .Append(_meshRendererMap[currentInstance].material
                                        .DOColor(_colorPair[_PrintMap[currentInstance].defaultColorName], 0.5f)
                                    );
                            }
                            else
                            {
                                sequence
                                    .Append(_meshRendererMap[currentInstance].material
                                    .DOColor(_PrintMap[currentInstance].defaultColor, 0.5f)
                                    );
                            }
                        }
                        

                        _colorSequences.TryAdd(currentInstance, sequence);
                    }
                }
             
            }
        }
    }
    
    private void ShuffleColors()
    {
        for (int i = 0; i < colorOptions.Length; i++)
        {
            Color temp = colorOptions[i];
            int randomIndex = Random.Range(i, colorOptions.Length);
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