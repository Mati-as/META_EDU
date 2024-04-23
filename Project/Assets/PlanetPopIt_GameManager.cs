using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
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
    
    private Dictionary<int, Sequence> _movementSeq;
    private Dictionary<int, Sequence> _squishSeq;
    private Dictionary<int, Sequence> _rotatioSeq;
    private Dictionary<int, bool> _isAnimating;
   
    private Dictionary<int, bool> _isSquished;

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
        
        var popItParent = GameObject.Find(nameof(PlanetPopIt));

        _popIts = new Transform[(int)PopIt.Count];
        
        _popIts[(int)PopIt.Left] = popItParent.transform.GetChild((int)PopIt.Left);
        _popIts[(int)PopIt.Right] = popItParent.transform.GetChild((int)PopIt.Right);


        var indexOfPlanetGroup = 0;


        var popItLeft = _popIts[(int)PopIt.Left].GetChild(indexOfPlanetGroup);

        for (var k = 0; k < popItLeft.childCount; k++)
        {
            var planet = popItLeft.GetChild(k);

            var transformID = planet.GetInstanceID();
            _transformMap.Add(transformID, planet);
            _isSquished.Add(transformID, false);
            _isAnimating.Add(transformID, false);
            _defaultSizeMap.Add(transformID, planet.localScale);
        }


        var popItRight = _popIts[(int)PopIt.Right].GetChild(indexOfPlanetGroup);

        for (var k = 0; k < popItRight.childCount; k++)
        {
            var planet = popItRight.GetChild(k);
            
            var transformID = planet.GetInstanceID();
            _transformMap.Add(transformID, planet);
            _isSquished.Add(transformID, false);
            _isAnimating.Add(transformID, false);
            _defaultSizeMap.Add(transformID, planet.localScale);
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

    private void OnPlanetClicked(Transform planet)
    {
        var id = planet.GetInstanceID();
        if (_isAnimating[id]) return;
        
        if (!_isSquished[id])
        {
            Squish(id);
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
        seq.Append(_transformMap[id].DOScale(defaultScale, 0.13f).SetEase(Ease.InCirc));
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
        seq.Append(_transformMap[id].DOScale(squishedSize, 0.1f).SetEase(Ease.InCirc));
        seq.Play();
        _squishSeq[id] = seq;
        _isSquished[id] = true;
    
        StartCoroutine(SetAnimatingBool(id));
    }

    /// <summary>
    /// 애니메이션 재생중 bool값을 딜레이 후 true에서 false로 토글합니다.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator SetAnimatingBool(int id, float delay=1f)
    {
        _isAnimating[id] = true;
        yield return DOVirtual.Float(0, 0, delay, _ => { }).WaitForCompletion();
        _isAnimating[id] = false;
    }


}
