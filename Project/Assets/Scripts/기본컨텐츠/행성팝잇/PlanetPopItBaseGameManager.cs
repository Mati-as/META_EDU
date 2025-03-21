using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlanetPopItBaseGameManager : Base_GameManager
{
    private enum PopIt
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
    private Dictionary<int, bool> _isclickable =new Dictionary<int, bool>();

    private Quaternion _sunDefaultRotation;
    private Material[] _sunExpressionMat;
    private MeshRenderer[] _sunMeshRenderers;

    private enum SunExpression
    {
        Idle,
        Excited,
        Count
    }

    private SunExpression _currentExpressionLeft = SunExpression.Idle;
    private SunExpression _currentExpressionRight = SunExpression.Idle;

    /*
     * sun 객체만 별도로 컨트롤할 수 있도록 Transform을 선언합니다.
     * 스케일변경(Squish)하는 애니메이션과 독립적으로 동작합니다.
     */

    private readonly int SUN_INDEX = 0;
    private Transform _sunLeft;
    private Transform _sunRight;
    
    private int _squishedCountLeft;
    private int _squishedCountRight;
    private readonly int SQUISH_COUNT_TO_BEGIN_SUN_ANIMATION = 5;

    private static readonly string PlanetPopIt;
    private static readonly string Planet;

  
    
    protected override void Init()
    {
        base.Init();
        _effectContainer = new Stack<ParticleSystem>();
        SetPool(_effectContainer, "SortedByScene/BasicContents/PlanetPopIt/CFX2_PickupStar");
        
        
        var sunCount = 2;
        _sunMeshRenderers = new MeshRenderer[sunCount];

        _sunExpressionMat = new Material[(int)SunExpression.Count];
        var idleMat = Resources.Load<Material>("SortedByScene/BasicContents/PlanetPopIt/M_Sun_Idle");
        var matIdle = Instantiate(idleMat);
        _sunExpressionMat[(int)SunExpression.Idle] = matIdle;

        var excitedMat = Resources.Load<Material>("SortedByScene/BasicContents/PlanetPopIt/M_Sun_Excited");
        var mat = Instantiate(excitedMat);
        _sunExpressionMat[(int)SunExpression.Excited] = mat;

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
                _sunMeshRenderers[(int)PopIt.Left] = popItLeft.GetChild(k).GetComponent<MeshRenderer>();
                _sunLeft = popItLeft.GetChild(k);
                _sunDefaultRotation = _sunLeft.rotation;
            }

            var planet = popItLeft.GetChild(k);
            var transformID = planet.GetInstanceID();
            _transformMap.Add(transformID, planet);
            _isSquished.Add(transformID, false);
            _isclickable.Add(transformID, true);
            _isAnimating.Add(transformID, false);
            _defaultSizeMap.Add(transformID, planet.localScale);
            _groupMap.Add(transformID, IS_LEFT_GROUP);
        }


        var popItRight = _popIts[(int)PopIt.Right].GetChild(indexOfPlanetGroup);

        for (var k = 0; k < popItRight.childCount; k++)
        {
            if (k == SUN_INDEX)
            {
                _sunMeshRenderers[(int)PopIt.Right] = popItRight.GetChild(k).GetComponent<MeshRenderer>();
                _sunRight = popItRight.GetChild(k);
            }

            var planet = popItRight.GetChild(k);

            var transformID = planet.GetInstanceID();
            _transformMap.Add(transformID, planet);
            _isSquished.Add(transformID, false);
            _isAnimating.Add(transformID, false);
            _defaultSizeMap.Add(transformID, planet.localScale);
            _groupMap.Add(transformID, IS_RIGHT_GROUP);
        }
    }

    public override void OnRaySynced()
    {
        base.OnRaySynced();
        if (!PreCheckOnRaySync()) return;
        
        
        foreach (var hit in GameManager_Hits)
        {
            var ps = GetFromPool(_effectContainer);
            if (ps != null)
            {
                ps.gameObject.SetActive(true);
                ps.gameObject.transform.position = hit.point;
                ps.Play();
            }
          
            
            if (hit.transform.name.Contains(nameof(Planet)))
            {
                
                
                OnPlanetClicked(hit.transform);
                
                //sound--------------------
                var randomChar = (char)Random.Range('A', 'F' + 1);
                Managers.Sound.Play(SoundManager.Sound.Effect, $"Audio/BasicContents/HandFootFlip/Click_{randomChar}",
                    0.3f);
                
                //particle------------------
                
                DEV_OnValidClick();
                return;
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
            if (_groupMap[id] == IS_RIGHT_GROUP)
            {
                _squishedCountRight++;
                if (_squishedCountRight > SQUISH_COUNT_TO_BEGIN_SUN_ANIMATION
                    && !_transformMap[id].gameObject.name
                        .Contains("Sun"))
                {
                    _currentExpressionRight++;
                    var intExpression = (int)_currentExpressionRight;
                    intExpression %= (int)SunExpression.Count;
                    _currentExpressionRight = (SunExpression)intExpression;
                    ChangeExpression(PopIt.Right, _currentExpressionRight);

                    BloatAndRotate(_sunRight);

                    _squishedCountRight = 0;
                }
            }
            else
            {
                _squishedCountLeft++;
                if (_squishedCountLeft > SQUISH_COUNT_TO_BEGIN_SUN_ANIMATION
                    && !_transformMap[id].gameObject.name
                        .Contains("Sun"))
                {
                    _currentExpressionLeft++;
                    var intExpression = (int)_currentExpressionLeft;
                    intExpression %= (int)SunExpression.Count;
                    _currentExpressionLeft = (SunExpression)intExpression;
                    ChangeExpression(PopIt.Left, _currentExpressionLeft);


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

    private void Squish(int id, float squishAmount = 0.45f, float sizeUpAmount = 1.30f)
    {
        var defaultScale = _defaultSizeMap[id];
        var squishedSize = new Vector3(defaultScale.x  * squishAmount, defaultScale.y*sizeUpAmount,
            defaultScale.z * sizeUpAmount);

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
        {
            var randomChar = (char)Random.Range('A', 'B' + 1);
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/PlanetPopIt/OnSunRotate" + randomChar, 0.2f);

            sun.DOLocalRotateQuaternion(_sunDefaultRotation *
                                        quaternion.Euler(Random.Range(-45, 45), Random.Range(-45, 45),
                                            Random.Range(-45, 45)), 0.3f).SetEase(Ease.InOutBounce)
                .OnComplete(() => { sun.DORotateQuaternion(_sunDefaultRotation, 0.3f).SetEase(Ease.InOutBack); });

            StartCoroutine(SetAnimatingBool(id, 1.5f));
        }
    }

    private void ChangeExpression(PopIt sunIndex, SunExpression currentExpression)
    {
        _sunMeshRenderers[(int)sunIndex].material = _sunExpressionMat[(int)currentExpression];
    }

    /// <summary>
    ///     애니메이션 재생중 bool값을 딜레이 후 true에서 false로 토글합니다.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator SetAnimatingBool(int id, float delay = 0.75f)
    {
        _isAnimating[id] = true;
        yield return DOVirtual.Float(0, 0, delay, _ => { }).WaitForCompletion();
        _isAnimating[id] = false;
    }
    
    
    
    
    private WaitForSeconds _poolReturnWait;
    private Stack<ParticleSystem> _effectContainer;
    
    
    protected IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, Stack<ParticleSystem> particlePool)
    {
        if (_poolReturnWait == null) _poolReturnWait = new WaitForSeconds(ps.main.startLifetime.constantMax);

        yield return _poolReturnWait;

#if UNITY_EDITOR

#endif
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        particlePool.Push(ps); // Return the particle system to the pool
    }

    private void SetPool(Stack<ParticleSystem> effectPool, string path, int poolCount = 20)
    {
        for (var poolSize = 0; poolSize < poolCount; poolSize++)
        {
            var prefab = Resources.Load<GameObject>(path);

            if (prefab == null)
            {
#if UNITY_EDITOR
                Debug.LogError("this gameObj to pool is null.");
#endif
                return;
            }

            var bead = Instantiate(prefab, transform);
            var ps = bead.GetComponent<ParticleSystem>();

            ps.Stop();
            ps.Clear();
            ps.gameObject.SetActive(false);

            effectPool.Push(ps);
        }
    }


    private ParticleSystem GetFromPool(Stack<ParticleSystem> pool)
    {
        if (pool.Count <= 0) return null;
        var ps = pool.Pop();

        StartCoroutine(ReturnToPoolAfterDelay(ps, _effectContainer));
        return ps;
    }
}