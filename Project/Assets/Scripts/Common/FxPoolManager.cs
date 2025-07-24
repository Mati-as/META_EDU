using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FxPoolManager 
{
    private readonly Stack<ParticleSystem> _particlePool = new();
    private readonly Dictionary<int, Stack<ParticleSystem>> _subParticlePool = new();

    private readonly string _psResourcePath;
    private readonly Dictionary<int, string> _subPsResourcePathMap;
    private readonly MonoBehaviour _owner;
    private const int MAX_SUB_PS_COUNT = 5;
    private WaitForSeconds _poolReturnWait;

    public FxPoolManager(MonoBehaviour owner, string psResourcePath, Dictionary<int, string> subPsResourcePathMap)
    {
        _owner = owner;
        _psResourcePath = psResourcePath;
        _subPsResourcePathMap = subPsResourcePathMap;
        InitMainPool();

        if(_subPsResourcePathMap == null || _subPsResourcePathMap.Count == 0)
        {
            Logger.ContentTestLog("Sub effect 미사용");
            return;
        }
        foreach (var key in _subPsResourcePathMap.Keys.ToArray())
        {
            InitSubPool(key);
        }
    }

    private void InitMainPool()
    {
        if (string.IsNullOrEmpty(_psResourcePath))
        {
            Logger.ContentTestLog("effect 미사용");
            return;
        }

        var particlePrefab = Resources.Load<GameObject>(_psResourcePath);

        for (int i = 0; i < 100; i++)
        {
            var ps = Object.Instantiate(particlePrefab).GetComponent<ParticleSystem>();
            ps.gameObject.SetActive(false);
            _particlePool.Push(ps);
        }
    }


    private ParticleSystem GetFromMainPool()
    {
        if (_particlePool.Count == 0) InitMainPool();

        var ps = _particlePool.Pop();
        ps.gameObject.SetActive(true);
        return ps;
    }

    private IEnumerator ReturnToMainPoolAfterDelay(ParticleSystem ps)
    {
        if (_poolReturnWait == null)
            _poolReturnWait = new WaitForSeconds(ps.main.startLifetime.constantMax);

        yield return _poolReturnWait;

        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        _particlePool.Push(ps);
    }


    #region Sub 파티클 관련 (상위에 있는 메인 이펙트 제외한 나머지 이펙트를 관리하기 위한 메소드)

    private ParticleSystem GetFromSubPool(int index)
    {
        if (!_subParticlePool.ContainsKey(index) || _subParticlePool[index].Count == 0) InitSubPool(index);

        var ps = _subParticlePool[index].Pop();
        ps.gameObject.SetActive(true);
        return ps;
    }

    private void InitSubPool(int index)
    {
        if (!_subPsResourcePathMap.ContainsKey(index))
        {
            Logger.ContentTestLog("Sub effect 미사용");
            return;
        }

        if (!_subParticlePool.ContainsKey(index))
            _subParticlePool[index] = new Stack<ParticleSystem>();

        var prefab = Resources.Load<GameObject>(_subPsResourcePathMap[index]);

        for (int i = 0; i < 100; i++)
        {
            var ps = Object.Instantiate(prefab).GetComponent<ParticleSystem>();
            ps.gameObject.SetActive(false);
            _subParticlePool[index].Push(ps);
        }
    }

    public void PlayMainParticle(Vector3 pos)
    {
        var ps = GetFromMainPool();
        ps.transform.position = pos;
        ps.Play();
        _owner.StartCoroutine(ReturnToMainPoolAfterDelay(ps));
    }

    public void PlaySubParticle(int index, Vector3 pos)
    {
        var ps = GetFromSubPool(index);
        ps.transform.position = pos;
        ps.Play();
        _owner.StartCoroutine(ReturnToSubPoolAfterDelay(index, ps));
    }

    private IEnumerator ReturnToSubPoolAfterDelay(int index, ParticleSystem ps)
    {
        if (_poolReturnWait == null)
            _poolReturnWait = new WaitForSeconds(ps.main.startLifetime.constantMax);

        yield return _poolReturnWait;

        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        _subParticlePool[index].Push(ps);
    }

    #endregion
}