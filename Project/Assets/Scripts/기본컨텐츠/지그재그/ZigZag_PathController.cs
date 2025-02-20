using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ZigZag_PathController : Base_GameManager
{
    private enum ZigZagPath
    {
        Triangle,
        Square,
        Count
    }


    private enum Animal
    {
        Dog,
        Rabbit,
        Count
    }

    private enum Slider
    {
        BottomTriangleSlider, //Left To Right gauge
        TopSquareSlider, //Right To Left gauge
        Count
    }

    private readonly Vector3[][] _pathes = new Vector3[(int)ZigZagPath.Count][];

    private Transform[] _animals;
    private Transform _dog;
    private Transform _rabbit;

    private Collider[][] _colliders; //ID, Collider
    private Dictionary<int, int> _colliderOrderMap; //colliderObjID, Order
    private UnityEngine.UI.Slider[] _sliders;
    private readonly float _guageOffset = 0.003f;

    private int _currentIndexForTriangle;
    private int _currentIndexForSquare;

    // click Particle
    private ParticleSystem _psSq;
    private ParticleSystem _psTri;

    private bool _isClickableTri = true;
    private bool _isClickableSq = true;
    private bool _isResetting;
    protected override void Init()
    {
        base.Init();

        _colliderOrderMap = new Dictionary<int, int>();
        _colliders = new Collider[(int)ZigZagPath.Count][];

        _sliders = new UnityEngine.UI.Slider[(int)ZigZagPath.Count];

        _sliders[(int)ZigZagPath.Triangle] = GameObject.Find("SliderTriangle").GetComponent<UnityEngine.UI.Slider>();
        _sliders[(int)ZigZagPath.Square] = GameObject.Find("SliderSquare").GetComponent<UnityEngine.UI.Slider>();

        _psTri = GameObject.Find("CFX_OnRightPathTriangle").GetComponent<ParticleSystem>();
        _psSq = GameObject.Find("CFX_OnRightPathSquare").GetComponent<ParticleSystem>();

        foreach (var slider in _sliders) slider.value = 0f;


        //ZigZag(Triangle)---------------
        var triangleParent = transform.GetChild((int)ZigZagPath.Triangle);
        var trWayPointCount = triangleParent.childCount;

        _pathes[(int)ZigZagPath.Triangle] = new Vector3[trWayPointCount];
        _colliders[(int)ZigZagPath.Triangle] = new Collider[trWayPointCount];

        for (var i = 0; i < trWayPointCount; i++)
        {
            var currentChild = triangleParent.GetChild(trWayPointCount - i - 1);
            _pathes[(int)ZigZagPath.Triangle][i] = currentChild.position;
            _colliderOrderMap.Add(currentChild.GetInstanceID(), i);
            _colliders[(int)ZigZagPath.Triangle][i] = currentChild.GetComponent<Collider>();
        }

        //Square------------------- 
        var squareParent = transform.GetChild((int)ZigZagPath.Square);
        var sqWayPointCount = squareParent.childCount;

        _colliders[(int)ZigZagPath.Square] = new Collider[sqWayPointCount];
        _pathes[(int)ZigZagPath.Square] = new Vector3[sqWayPointCount];
        for (var i = 0; i < sqWayPointCount; i++)
        {
            var currentChild = squareParent.GetChild(i);
            _pathes[(int)ZigZagPath.Square][i] = squareParent.GetChild(i).position;
            _colliderOrderMap.Add(currentChild.GetInstanceID(), i);
            _colliders[(int)ZigZagPath.Square][i] = currentChild.GetComponent<Collider>();
        }

        _dog = transform.Find("Dog");
        _rabbit = transform.Find("Rabbit");

        _animals = new Transform[(int)Animal.Count];
        _animals[(int)Animal.Dog] = _dog;
        _animals[(int)Animal.Rabbit] = _rabbit;

        foreach (var collider in _colliders)
        foreach (var col in collider)
            col.enabled = false;

        _colliders[(int)ZigZagPath.Square][0].enabled = true;
        _colliders[(int)ZigZagPath.Triangle][0].enabled = true;
        StartPathAnim();
    }


    private void StartPathAnim()
    {
        var previousPathIndex = -1;
        var randomNum = -1;
        for (var i = 0; i < (int)Animal.Count; i++)
        {
            if (previousPathIndex == -1)
            {
                randomNum = Random.Range((int)ZigZagPath.Triangle, (int)ZigZagPath.Count);
                previousPathIndex = randomNum;
            }
            else
            {
                randomNum = previousPathIndex == 1 ? 0 : 1;
            }

            var randomPath = _pathes[randomNum];

            _animals[i].position = randomPath[0];
            _animals[i].DOPath(randomPath, Random.Range(15f, 20f)).SetLookAt(-0.01f).SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    //중복실행방지
                    if (!_isResetting)
                    {
                        StartCoroutine(ReSetCoroutine());
                    }
                });
        }
    }

 

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        
        if (_isResetting) return;

        foreach (var hit in GameManager_Hits)
        {
            var hitTransform = hit.transform;
            var id = hitTransform.GetInstanceID();


#if UNITY_EDITOR
            Debug.Log($"클릭객체: {hitTransform.gameObject.name}");
#endif

            if (hit.transform.gameObject.name.Contains("Triangle"))
            {
                if (!_isClickableTri) return;
                _isClickableTri = false;

                OnPathClicked(_sliders[(int)Slider.BottomTriangleSlider], _colliders[(int)ZigZagPath.Triangle],
                    _currentIndexForTriangle, _psTri);
                _currentIndexForTriangle++;


                DOVirtual.Float(0, 0, 1.5f, value => { _isClickableTri = true; });
            }
            else if (hit.transform.gameObject.name.Contains("Square"))
            {
                if (!_isClickableSq) return;
                _isClickableSq = false;

                OnPathClicked(_sliders[(int)Slider.TopSquareSlider], _colliders[(int)ZigZagPath.Square],
                    _currentIndexForSquare, _psSq);

                _currentIndexForSquare++;


                DOVirtual.Float(0, 0, 1.5f, value => { _isClickableSq = true; });
            }
        }
    }


    private IEnumerator ReSetCoroutine(float delay = 2f)
    {
        _isResetting = true;
        yield return DOVirtual.Float(0, 0, delay, _ => { }).WaitForCompletion();
        
        ReSet();
        
        yield return DOVirtual.Float(0, 0, delay, _ => { }).WaitForCompletion();
        
        StartPathAnim();
        _isResetting = false;
    }

    private void ReSet()
    {
        foreach (var collider in _colliders)
        foreach (var col in collider)
            col.enabled = false;

        _currentIndexForSquare = 0;
        _currentIndexForTriangle = 0;

        DOVirtual.Float(_sliders[(int)Slider.BottomTriangleSlider].value, 0, Random.Range(0.7f, 1.4f),
            val => { _sliders[(int)Slider.BottomTriangleSlider].value = val; });

        DOVirtual.Float(_sliders[(int)Slider.TopSquareSlider].value, 0, Random.Range(0.7f, 1.4f),
            val => { _sliders[(int)Slider.TopSquareSlider].value = val; });

        _colliders[(int)ZigZagPath.Square][0].enabled = true;
        _colliders[(int)ZigZagPath.Triangle][0].enabled = true;
    }


    private void OnPathClicked(UnityEngine.UI.Slider slider, Collider[] colliders, int index, ParticleSystem ps)
    {
        var colliderLength = colliders.Length;

        if (index >= colliderLength)
        {
#if UNITY_EDITOR
            Debug.Log("경로도달");
#endif
            return;
        }

        colliders[index].enabled = false;
        ps.Stop();
        var psUpperOffset = 0.1f;
        ps.transform.position = colliders[index].transform.position + Vector3.up * psUpperOffset;
        ps.Play();

        var nextIndex = index + 1;
        if (nextIndex < colliderLength)
        {
            colliders[nextIndex].enabled = true;

            DOVirtual.Float(index / (float)colliderLength, (nextIndex + index * _guageOffset) / colliderLength, 0.9f,
                value =>
                {
                    var clampedVal = Mathf.Clamp(value, 0.05f, value);
                    slider.value = clampedVal;
                }).SetEase(Ease.OutCubic).OnComplete(() =>
            {
#if UNITY_EDITOR
                Debug.Log($"슬라이더이름: {slider.name}, 현재 인덱스: {index} 슬라이더 값{slider.value}");
#endif
            });
        }
    }
}