using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

public class PlanetPopIt_GameManager : IGameManager
{

    enum PopIt
    {
        Left,
        Right,
        Count
    }

    private Transform[] _popIts;

    private Dictionary<int, Transform> _transformMap;
    private Dictionary<int, Vector3> _defaultSizeMap;
    
    // left,right group을 bool로 구분합니다
    private Dictionary<int, bool> _groupMap;
    private readonly bool IS_LEFT_GROUP = false;
    private readonly bool IS_RIGHT_GROUP = true;
    
    
    private Dictionary<int, Sequence> _movementSeq;
    private Dictionary<int, Sequence> _squishSeq;
    private Dictionary<int, Sequence> _rotatioSeq;
    private Dictionary<int, bool> _isAnimating;
   
    private Dictionary<int, bool> _isSquished;

    /*
     * sun 객체만 별도로 컨트롤할 수 있도록 Transform을 선언합니다.
     * 스케일변경(Squish)하는 애니메이션과 독립적으로 동작합니다.
     */
    
    private readonly int SUN_INDEX =0;
    private Transform _sunLeft;
    private Transform _sunRight;
    private int _squishedCountLeft;
    private int _squishedCountRight;
    
    
    
    private static readonly string PlanetPopIt;
    private static readonly string Planet;
    

    protected override void Init()
    {
        base.Init();
        
        _transformMap = new Dictionary<int, Transform>();
        _defaultSizeMap = new Dictionary<int, Vector3>();
        
        _movementSeq = new Dictionary<int, Sequence>();
        _squishSeq = new Dictionary<int, Sequence>();
        _rotatioSeq = new Dictionary<int, Sequence>();
        
        _isSquished = new Dictionary<int, bool>();
        _isAnimating = new Dictionary<int, bool>();
        _groupMap = new Dictionary<int, bool>();
        
        var popItParent = GameObject.Find(nameof(PlanetPopIt));

        _popIts = new Transform[(int)PopIt.Count];
        
        _popIts[(int)PopIt.Left] = popItParent.transform.GetChild((int)PopIt.Left);
        _popIts[(int)PopIt.Right] = popItParent.transform.GetChild((int)PopIt.Right);


        var indexOfPlanetGroup = 0;


        var popItLeft = _popIts[(int)PopIt.Left].GetChild(indexOfPlanetGroup);

        for (var k = 0; k < popItLeft.childCount; k++)
        {
            if (k == SUN_INDEX)
            {
                _sunLeft = popItLeft.GetChild(k);
            }
            
            var planet = popItLeft.GetChild(k);
            var transformID = planet.GetInstanceID();
            _transformMap.Add(transformID, planet);
            _isSquished.Add(transformID, false);
            _isAnimating.Add(transformID, false);
            _defaultSizeMap.Add(transformID, planet.localScale);
            _groupMap.Add(transformID,IS_LEFT_GROUP);
        }


        var popItRight = _popIts[(int)PopIt.Right].GetChild(indexOfPlanetGroup);

        for (var k = 0; k < popItRight.childCount; k++)
        {
            
            if (k == SUN_INDEX)
            {
                _sunRight = popItRight.GetChild(k);
            }

            var planet = popItRight.GetChild(k);
            
            var transformID = planet.GetInstanceID();
            _transformMap.Add(transformID, planet);
            _isSquished.Add(transformID, false);
            _isAnimating.Add(transformID, false);
            _defaultSizeMap.Add(transformID, planet.localScale);
            _groupMap.Add(transformID,IS_RIGHT_GROUP);
        }
    }

    protected override void OnRaySynced()
    {
        base.OnRaySynced();

        foreach (var hit in GameManager_Hits)
        {
            if (hit.transform.name.Contains(nameof(Planet)))
            {
                OnPlanetClicked(hit.transform);
            }
        }
    }

    private readonly int SQUISH_COUNT_TO_BEGIN_SUN_ANIMATION =5;
    private void OnPlanetClicked(Transform planet)
    {
        var id = planet.GetInstanceID();
        if (_isAnimating[id]) return;
        
        if (!_isSquished[id])
        {
            Squish(id);
            if (_groupMap[id] == IS_RIGHT_GROUP)
            {
                _squishedCountRight++;
                if (_squishedCountRight > SQUISH_COUNT_TO_BEGIN_SUN_ANIMATION 
                    && !_transformMap[id].gameObject.name
                    .Contains("Sun"))
                {
                    _squishedCountRight = 0;
                    BloatAndRotate(_sunRight);
                }
            }
            else
            {
                _squishedCountLeft++;
                if (_squishedCountLeft > SQUISH_COUNT_TO_BEGIN_SUN_ANIMATION 
                    && !_transformMap[id].gameObject.name
                        .Contains("Sun"))
                {
                    BloatAndRotate(_sunLeft);
                    _squishedCountLeft = 0;
                }
            }
        }
        else
        {
            BloatBack(id);
        }
    }
    
    

    private void BloatBack(int id)
    {
        var defaultScale = _defaultSizeMap[id];
        var seq = DOTween.Sequence();
        seq.Append(_transformMap[id].DOScale(defaultScale, 0.09f).SetEase(Ease.InCirc));
        seq.Play();
        _squishSeq[id] = seq;
        _isSquished[id] = false;
        
        StartCoroutine(SetAnimatingBool(id));
    }

    private void Squish(int id, float squishAmount = 0.45f)
    {
   
        var defaultScale = _defaultSizeMap[id];
        var squishedSize = new Vector3(defaultScale.x * squishAmount, defaultScale.y, defaultScale.z);
      
        var seq = DOTween.Sequence();
        seq.Append(_transformMap[id].DOScale(squishedSize, 0.08f).SetEase(Ease.InCirc));
        seq.Play();
        _squishSeq[id] = seq;
        _isSquished[id] = true;
    
        StartCoroutine(SetAnimatingBool(id));
    }

    private void BloatAndRotate(Transform sun)
    {
        var id = sun.GetInstanceID();
        if (_isSquished[id]) BloatBack(id);
        sun.DOLocalRotateQuaternion(sun.localRotation * 
                                    quaternion.Euler(Random.Range(180,360), Random.Range(180,360), Random.Range(180,360)), 1).SetEase(Ease.InOutBounce);
     
        StartCoroutine(SetAnimatingBool(id,1.1f));
    }

    /// <summary>
    /// 애니메이션 재생중 bool값을 딜레이 후 true에서 false로 토글합니다.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator SetAnimatingBool(int id, float delay=0.11f)
    {
        _isAnimating[id] = true;
        yield return DOVirtual.Float(0, 0, delay, _ => { }).WaitForCompletion();
        _isAnimating[id] = false;
    }


}
