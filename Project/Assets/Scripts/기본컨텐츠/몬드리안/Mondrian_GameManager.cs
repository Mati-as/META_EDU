using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Mondrian_GameManager : IGameManager
{
    //events
    public static event Action onSmallCubeExplosion;
    public static event Action onBigCubeExplosion;
    [Header("Reference")] private Mondrian_FlowerController mondrianFlowerController;
    private Mondrian_BigFlowerController mondrianBigFlowerController;

    [Header("Cube Moving Settings")] private Dictionary<Transform, Transform> _cubeDpArrivalMap;
    private GameObject _movableCubeParent;
    private Transform[] _movableCubes;
    [Range(0, 60)] public float _cubeMoveInterval;

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
    private Dictionary<int, Sequence> _scaleSequence;
    private MeshRenderer[] _meshRenderers;


    [Header("RayCaster Setting")] [SerializeField]
    private Transform[] movePath;

    private Vector3[] _pathPos;

    [FormerlySerializedAs("interval")] [Range(0, 60)]
    public float raycasterMoveInterval;

    private bool _isRayCasterMoving;
    private float rayMoveCurrentTime;

    private ParticleSystem _explosionParticle;


    protected override void Init()
    {
        base.Init();

        DOTween.Init().SetCapacity(200, 150);
        mondrianFlowerController =
            GameObject.Find("Mondrian_FlowerController").GetComponent<Mondrian_FlowerController>();

        mondrianBigFlowerController =
            GameObject.Find("Mondrian_Big_FlowerController").GetComponent<Mondrian_BigFlowerController>();

        var particle = Resources.Load<GameObject>("게임별분류/기본컨텐츠/Mondrian/" + "CFX_explosionPs");
        _explosionParticle = Instantiate(particle).GetComponent<ParticleSystem>();
        _explosionParticle.Stop();

        _cubeDpArrivalMap = new Dictionary<Transform, Transform>();

        _meshRendererMap = new Dictionary<int, MeshRenderer>();

        _colorSequences = new Dictionary<int, Sequence>();
        _moveSequence = new Dictionary<int, Sequence>();
        _scaleSequence = new Dictionary<int, Sequence>();

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

            var seq = DOTween.Sequence();
            _moveSequence.Add(instanceID, seq);
            var seq1 = DOTween.Sequence();
            _colorSequences.Add(instanceID, seq1);
            var seq2 = DOTween.Sequence();
            _scaleSequence.Add(instanceID, seq2);
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
        rayMoveCurrentTime += Time.deltaTime;

        if (rayMoveCurrentTime > raycasterMoveInterval)
        {
            RayCasterMovePlay();
            rayMoveCurrentTime = 0;
            raycasterMoveInterval = Random.Range(23.5f, 30);
        }

//         if (_cubeMoveCurrentTime > _cubeMoveInterval)
//         {
//             var randomIndex = Random.Range(0, _movableCubes.Length) / 2 * 2;
// #if UNITY_EDITOR
//             Debug.Log($"cube moving!:randomIndex: {randomIndex}");
// #endif
//             PlayExchangeAnimation(_cubeDpArrivalMap[_movableCubes[randomIndex]]);
//             PlayExchangeAnimation(_cubeDpArrivalMap[_movableCubes[randomIndex + 1]]);
//
//             _cubeMoveCurrentTime = 0;
//             _cubeMoveInterval = Random.Range(30, 60);
//         }
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
        base.OnRaySynced();
        if (!isStartButtonClicked) return;
        RandomlyChangeColor(GameManager_Ray);
        PlayExplosionAnimation();
    }

    private readonly float _scaleInterval = 1.02f;


    private void RandomlyChangeColor(Ray ray)
    {
        GameManager_Hits = Physics.RaycastAll(ray);

        
        foreach (var hit in GameManager_Hits)
        {
            var currentInstance = hit.transform.gameObject.GetComponent<MeshRenderer>().GetInstanceID();
            if (_meshRendererMap.ContainsKey(currentInstance))
            {
                
                char randomChar = (char)Random.Range('A', 'D'+ 1);
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Mondrian/Click_" + randomChar, 0.2f);
             
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
                        sequence
                            .Append(_meshRendererMap[currentInstance].material
                                .DOColor(GetRandomColor(_meshRendererMap[currentInstance].material.color),
                                    Random.Range(0.2f, 0.35f))
                            );

                        _colorSequences.TryAdd(currentInstance, sequence);
                    }
                }
                else
                {
                    sequence.Append(_meshRendererMap[currentInstance].material
                        .DOColor(GetRandomColor(_meshRendererMap[currentInstance].material.color),
                            Random.Range(0.2f, 0.3f)));

                    _colorSequences.TryAdd(currentInstance, sequence);
                }
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
                if (Physics.Raycast(raycasterMoveRay, out hit, 1000f)) RandomlyChangeColor(raycasterMoveRay);
                //      Debug.Log("Raycastermove hit: " + hit.transform.name);
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


    private void PlayExplosionAnimation()
    {
        //확률 및 애니메이션 중복재생 방지..
        if (Random.Range(0, 10f) < 7.5f) return;


        var scaleSeq = DOTween.Sequence();
        foreach (var hit in GameManager_Hits)
        {
            //작은큐브인 경우------------------------------------------
            if (hit.transform.localScale.z < 1.1f)
            {
                if (mondrianFlowerController._onGrowing) return;
                var currentInstance = hit.transform.gameObject.GetComponent<MeshRenderer>().GetInstanceID();
                Collider currentCollider = hit.transform.gameObject.GetComponent<Collider>();
                if (_scaleSequence.ContainsKey(currentInstance))
                {
#if UNITY_EDITOR
                    Debug.Log("sequence is already active..try later");
#endif
                    if (_scaleSequence[currentInstance].IsActive()) return;
                }
                else
                {
                    _scaleSequence[currentInstance].Kill();
                }

                if (_meshRendererMap.ContainsKey(currentInstance))
                {
                    //DOScale동작 중, Transform 참조 잃어버리는 경우 방지를 위해 로컬 Transform 변수 선언
                    var currentTransform = hit.transform;
                    // 더블클릭시 스케일이 중복되어 움직이는것(시퀀스 에러)을 방지합니다.

                    //중복클릭방지용 콜라이더 비활성화.. sequnece 간헐적 오류로 인해 방어적 코드작성 
                    currentCollider.enabled = false;
                    
                    // 스케일 애니메이션
                    var defaultScale = hit.transform.localScale;
                    var targetScale = defaultScale * _scaleInterval;
                    scaleSeq.Append(currentTransform.DOScale(targetScale, 0.05f).SetEase(Ease.InOutSine)
                        .OnComplete(() =>
                            {
                                currentTransform
                                    .DOScale(0f, 0.45f).SetEase(Ease.InOutSine).SetDelay(0.1f)
                                    .OnStart(() =>
                                    {
                                        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Mondrian/OnExplosion", 0.4f);
                                        
                                        if (!_explosionParticle.IsAlive() && hit.transform.localScale.x < 1.5f)
                                        {
                                            _explosionParticle.transform.position = currentTransform.position;
                                            _explosionParticle.Play();
                                        }


                                        mondrianFlowerController.flowerAppearPosition = currentTransform.position;
                                        onSmallCubeExplosion?.Invoke();
                                    })
                                    .OnComplete(() =>
                                    {
                                        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Mondrian/OnFlowerAppear", 0.4f);
                                        
                                       
                                        currentTransform
                                            .DOScale(defaultScale, 0.45f).SetEase(Ease.InOutSine)
                                            .SetDelay(Random.Range(5, 10)).OnComplete(() =>
                                            {
                                                currentCollider.enabled = true;
                                            }); //respawnTime
                                            

                                       
                                    });
                            }
                        ));

                    _scaleSequence[currentInstance] = scaleSeq;
                }
            }
            else //큰 큐브인 경우------------------------------------------
            {
                if (mondrianBigFlowerController._onGrowing) return;
                var currentInstance = hit.transform.gameObject.GetComponent<MeshRenderer>().GetInstanceID();

                //중복클릭방지용 콜라이더 비활성화.. sequnece 간헐적 오류로 인해 방어적 코드작성
                Collider currentCollider = hit.transform.gameObject.GetComponent<Collider>();
                currentCollider.enabled = false;
                
                if (_scaleSequence.ContainsKey(currentInstance))
                {
#if UNITY_EDITOR
                    Debug.Log("sequence is already active..try later");
#endif
                    if (_scaleSequence[currentInstance].IsActive()) return;
                }
                else
                {
                    _scaleSequence[currentInstance].Kill();
                }

                if (_meshRendererMap.ContainsKey(currentInstance))
                {
                    //DOScale동작 중, Transform 참조 잃어버리는 경우 방지를 위해 로컬 Transform 변수 선언
                    var currentTransform = hit.transform;

                    // 더블클릭시 스케일이 중복되어 움직이는것(시퀀스 에러)을 방지합니다.
                    if (_scaleSequence.ContainsKey(currentInstance))
                    {
                        if (_scaleSequence[currentInstance].IsActive()) return;
                    }
                    else
                    {
                        _scaleSequence[currentInstance].Kill();
                    }

                   

                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Mondrian/OnBunchFlowersAppear", 0.4f);
                    
                    // 스케일 애니메이션
                    var defaultScale = hit.transform.localScale;
                    var targetScale = defaultScale * _scaleInterval;
                    scaleSeq.Append(currentTransform.DOScale(targetScale, 0.05f).SetEase(Ease.InOutSine)
                        .OnComplete(() =>
                        {
                            currentTransform
                                .DOScale(0f, 0.35f).SetEase(Ease.InOutSine).SetDelay(0.1f)
                                .OnStart(() =>
                                {
                                    mondrianBigFlowerController.flowerAppearPosition = currentTransform.position;
                                    onBigCubeExplosion?.Invoke();
                                })
                                .OnComplete(() =>
                                {
                                    
                                    currentTransform
                                        .DOScale(defaultScale, 0.35f).SetEase(Ease.InOutSine)
                                        .SetDelay(Random.Range(5, 10))
                                        .OnComplete(() =>
                                        {
                                            currentCollider.enabled = true;
                                        }); //respawnTime
                                });
                        }));

                    _scaleSequence[currentInstance] = scaleSeq;
                }
            }

            scaleSeq.Play();
            break;
        }
    }


    private void PlayVanishAnimation()
    {
    }
}