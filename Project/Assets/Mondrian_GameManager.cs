using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Sequence = DG.Tweening.Sequence;

public class Mondrian_GameManager : IGameManager
{
    [Header("Cube Moving Settings")] private Dictionary<Transform, Transform> _cubeDpArrivalMap;
    private float _cubeMoveCurrentTime;
    private GameObject _movableCubeParent;
    private Transform[] _movableCubes;
    [Range(0,60)]
    public float _cubeMoveInterval;

    private enum RayCasterMovePosition
    {
        Start,
        Arrival,
        Max
    }

    //자식객체에서 중복값을 피해서 가져오기 위한 HashSet설정
    private HashSet<Color> mondrian_CubeColors;

    private GameObject _cubes;
    private Color[] _colors;


    // 파도타기방식(형태) RayCasting 및 색상변환로직 구현용 RayCaster. 
    private GameObject _rayCasterParent;
    private Transform[] _rayCasters;


    private Dictionary<int, MeshRenderer> _meshRendererMap;
    private Dictionary<int, Sequence> _colorSequences;
    private Dictionary<int, Sequence> _moveSequence;
    private MeshRenderer[] _meshRenderers;


    [Header("RayCaster Setting")] [SerializeField]
    private Transform[] movePath;

    private Vector3[] _pathPos;
    [FormerlySerializedAs("interval")] [Range(0, 60)] public float raycasterMoveInterval;
    private bool _isRayCasterMoving;
    private float rayMoveCurrentTime;


    protected override void Init()
    {
        base.Init();


        _cubeDpArrivalMap = new Dictionary<Transform, Transform>();
        
        _meshRendererMap = new Dictionary<int, MeshRenderer>();

        _colorSequences = new Dictionary<int, Sequence>();
        _moveSequence = new Dictionary<int, Sequence>();
        
        mondrian_CubeColors = new HashSet<Color>();
        _pathPos = new Vector3[(int)RayCasterMovePosition.Max];

        _pathPos[0] = movePath[(int)RayCasterMovePosition.Start].position;
        _pathPos[1] = movePath[(int)RayCasterMovePosition.Arrival].position;


        //cube settings.
        _cubes = GameObject.Find("Cubes");
        _meshRenderers = _cubes.GetComponentsInChildren<MeshRenderer>();

        foreach (var meshRenderer in _meshRenderers) AddColor(meshRenderer.material.color);

        SetColorArray(mondrian_CubeColors);
        foreach (var meshRenderer in _meshRenderers)
        {
            var instanceID = meshRenderer.GetInstanceID();
            _meshRendererMap.Add(instanceID, meshRenderer);
         
            Sequence seq = DOTween.Sequence();
            _moveSequence.Add(instanceID,seq);
            _colorSequences.Add(instanceID,seq);
        }

        //raycaster settings.
        _rayCasterParent = GameObject.Find("RayCaster");
        _rayCasters = _rayCasterParent.GetComponentsInChildren<Transform>();
        _wait = new WaitForSeconds(0.2f);


        //movable cube transform set
        _movableCubeParent = GameObject.Find("Objects_Movable_Cube");
        _movableCubes = _movableCubeParent.GetComponentsInChildren<Transform>()
            .Where(x => x != _movableCubeParent.transform)
            .ToArray();

        for (var i = 0; i < _movableCubes.Length; i = i + 2)
        {
            _cubeDpArrivalMap.TryAdd(_movableCubes[i], _movableCubes[i + 1]);
            _cubeDpArrivalMap.TryAdd(_movableCubes[i + 1], _movableCubes[i]);
        }
        //0,1 1,0 // 2,3,3,2 , 4,5,5,4, 6,7,7,6 // 8,9,9,8 
    }


    private void Update()
    {
        _cubeMoveCurrentTime += Time.deltaTime;
        rayMoveCurrentTime += Time.deltaTime;

        if (rayMoveCurrentTime > raycasterMoveInterval)
        {
            RayCasterMovePlay();
            rayMoveCurrentTime = 0;
        }

        if (_cubeMoveCurrentTime > _cubeMoveInterval)
        {

            int randomIndex = (Random.Range(0, _movableCubes.Length) / 2) * 2;
#if UNITY_EDITOR
            Debug.Log($"cube moving!:randomIndex: {randomIndex}");
#endif
            PlayExchangeAnimation(_cubeDpArrivalMap[_movableCubes[randomIndex]]);
            PlayExchangeAnimation(_cubeDpArrivalMap[_movableCubes[randomIndex + 1]]);
            
            _cubeMoveCurrentTime = 0;
        }
    }

    public void AddColor(Color newColor)
    {
        var added = mondrian_CubeColors.Add(newColor);

// #if UNITY_EDITOR
// if (added)
// {
//   Debug.Log("색상추가 성공: " + newColor);
// }
// else
// {
//   Debug.Log("중복X " + newColor);
// }
// #endif
    }

    private void SetColorArray(HashSet<Color> colorSet)
    {
        if (colorSet.Count == 0) Debug.LogError("색상 집합이 비어있습니다.");

        _colors = new Color[colorSet.Count];
        colorSet.CopyTo(_colors);
    }

    private Color GetRandomColor(Color currentColor)
    {
        var randomIndex = Random.Range(0, _colors.Length);
        while (currentColor == _colors[randomIndex]) randomIndex = Random.Range(0, _colors.Length);
        return _colors[randomIndex];
    }

    protected override void OnRaySynced()
    {
        RandomlyChangeColor(GameManager_Ray);
    }

    private float _scaleInterval = 1.00f;

    private void RandomlyChangeColor(Ray ray)
    {
        GameManager_Hits = Physics.RaycastAll(ray);

        foreach (var hit in GameManager_Hits)
        {
            var currentInstance = hit.transform.gameObject.GetComponent<MeshRenderer>().GetInstanceID();
            if (_meshRendererMap.ContainsKey(currentInstance))
            {
#if UNITY_EDITOR

#endif
                var sequence = DOTween.Sequence();

                if (_colorSequences.ContainsKey(currentInstance))
                {
                    if (!_colorSequences[currentInstance].IsActive())
                    {
                        sequence
                            .Append(_meshRendererMap[currentInstance].material
                                .DOColor(GetRandomColor(_meshRendererMap[currentInstance].material.color),
                                    Random.Range(0.55f, 0.75f))
                            );
                        _colorSequences.TryAdd(currentInstance, sequence);
                    }
                }
                else
                {
                    sequence.Append(_meshRendererMap[currentInstance].material
                        .DOColor(GetRandomColor(_meshRendererMap[currentInstance].material.color),
                            Random.Range(0.55f, 0.75f)));
                    _colorSequences.TryAdd(currentInstance, sequence);
                }

                //
                // Vector3 defaultSacle= new Vector3();
                // defaultSacle = hit.transform.localScale;
                // sequence.Play();
                //  hit.transform.DOScale(transform.localScale * _scaleInterval, 0.15f)
                //    .SetEase(Ease.InElastic).OnComplete(() =>
                //    {
                //      hit.transform.DOScale(defaultSacle, 0.15f).SetEase(Ease.OutElastic);
                //    });
                //
                //
            }
        }
    }

    private void RayCasterMovePlay()
    {
        _rayCasterParent.transform.position = _pathPos[(int)RayCasterMovePosition.Start];
        _rayCasterParent.transform
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

    private void RayCasterMove()
    {
        // 각 자식 객체의 위치에서 아래 방향으로 레이를 발사합니다.
        foreach (var childTransform in _rayCasters)
            if (childTransform != transform)
            {
                var raycasterMoveRay = new Ray(childTransform.position, Vector3.down);
                RaycastHit hit;

                // 레이캐스트 발사 (예: 100 유닛 거리까지)
                if (Physics.Raycast(raycasterMoveRay, out hit, 1000f))
                {
                    RandomlyChangeColor(raycasterMoveRay);
              //      Debug.Log("Raycastermove hit: " + hit.transform.name);
                }
            }
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

    public float moveUpAmount;
    private void MoveUp(Transform transform , Transform arrival)
    {
        transform.DOMove((transform.position + moveUpAmount * Vector3.up), 1f);
    }

    private void ExchangePosition(Transform transform)
    {
        transform.DOMove(_cubeDpArrivalMap[transform].position, 2.5f);
    }

    private void MoveDown(Transform transform , Transform arrival)
    {
        transform.DOMove((transform.position + moveUpAmount * Vector3.down), 1f);
    }
    
    private void PlayExchangeAnimation(Transform trans)
    {
        int intID = trans.GetComponent<MeshRenderer>().GetInstanceID();
        
        if(_moveSequence[intID].active) return;
        
        Sequence seq = DOTween.Sequence();
        seq.Append(trans.DOMove((trans.position + moveUpAmount * Vector3.up), 1f));
       
        seq.Append(trans.DOMove(_cubeDpArrivalMap[trans].position, 2.5f)
            .OnStart(()=>
            {
                trans.DORotate(_cubeDpArrivalMap[trans].rotation.eulerAngles, 1f);
            }));
       
    //    seq.Append(trans.DOMove((trans.position + moveUpAmount * Vector3.down), 1f));
       
        _moveSequence[intID]= seq;
       
        seq.Play();


    }
}