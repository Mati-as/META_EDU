using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SuperMaxim.Core.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
///     0508/25
///     UIManager에 텍스트 및 나레이션 전송용 페이로드 입니다.
///     pub-sub패턴 활용이며 텍스트나 나레이션의 양이 많을경우 활용할 수 있습니다.
/// </summary>
public class UI_Payload : IPayload
{
    public string Narration //텍스트 내용
    {
        get;
    }

    public bool IsCustom // 조건(Checksum)없이 UI를 사용하고 싶은경우
    {
        get;
    }

    public bool IsPopFromZero // 처음에 size가 제로였다가 커지는 애니메이션을 사용하고 싶은경우활용, false인 경우 original 사이즈에서 애니메이션 재생
    {
        get;
    }


    public float DelayAndAutoShutTime //텍스트 내용
    {
        get;
    }

    public string Checksum //텍스트 내용, 메세지필터링
    {
        get;
    }


    public UI_Payload(string narration, bool isCustomOn = false, bool isPopFromZero = true,
        float delayAndAutoShutTime = 0.0f, string Checksum = "")
    {
        IsCustom = isCustomOn;
        Narration = narration;
        IsPopFromZero = isPopFromZero;
        DelayAndAutoShutTime = delayAndAutoShutTime;
        this.Checksum = Checksum;
    }
}

/// <summary>
///     오브젝트 바인딩 기능이 추가.
///     기본적으로 카메라 무빙, 애니메이션을 총괄할 수 있는 메인 애니메이션 컨트롤러 추가
/// </summary>
public abstract class Ex_BaseGameManager : Base_GameManager
{
    protected Dictionary<Type, Object[]> _objects = new();


    #region (GameManager 부착) 메인 애니메이션 컨트롤 관리

    protected bool _init;
    protected Animator mainAnimator; //Gamemanager에 부착된 Animator 사용중
    protected int currentMainMainSequence;

    protected readonly int SEQ_NUM = Animator.StringToHash("seqNum");
    private static readonly int Finish = Animator.StringToHash("Finish");

    #endregion
    
    #region 클릭 및 각종 Fx이펙트 풀 관리
    //클릭이펙트 사용을 위한 풀, 현재 경로와 Playeffect로 사용하도록 구현되어있음 subEffect 사용으로 모든 이펙트 사용가능.
    private readonly Stack<ParticleSystem> _particlePool = new();
    private readonly Dictionary<int, Stack<ParticleSystem>> _subParticlePool = new();

    protected string psResourcePath = string.Empty;

    protected Dictionary<int, string> subPsResourcePathMap = new();
    private const int MAX_SUB_PS_COUNT = 5;

    #endregion



    protected virtual new void Awake()
    {
        Init();
    }

    protected virtual new void Start()
    {
    }

    protected virtual new void Init()
    {
        SetPool();
        bool isAnimatorAttached = TryGetComponent(out mainAnimator);
        if (!isAnimatorAttached) Logger.Log("게임매니저에 애니메이터 없음.");


        base.Init();
    }


    protected void ResetClickable(bool isClickable = true)
    {
        foreach (int key in _isClickableMapByTfID.Keys.ToArray())
            //            Logger.ContentTestLog($"{key} : {isClickable}");
            _isClickableMapByTfID[key] = isClickable;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _objects = new Dictionary<Type, Object[]>();
    }


    protected override sealed void OnRaySyncedByGameManager()
    {
        base.OnRaySyncedByGameManager();
    }


    protected void  SetPool()
    {
        if (psResourcePath.IsNullOrEmpty())
        {
            Logger.ContentTestLog("effect 미사용");
            return;
        }

        var particlePrefab = Resources.Load<GameObject>(psResourcePath);

        for (int i = 0; i < 100; i++)
        {
            var ps = Instantiate(particlePrefab, Vector3.zero, Quaternion.identity).GetComponent<ParticleSystem>();
            ps.gameObject.SetActive(false);
            _particlePool.Push(ps);
        }
    }

    protected ParticleSystem GetFromPool()
    {
        if (_particlePool.Count > 0)
        {
            var ps = _particlePool.Pop();

            ps.gameObject.SetActive(true);
            return ps;
        }

        SetPool();
        var newPs = _particlePool.Pop();

        newPs.gameObject.SetActive(true);
        return newPs;
    }

    protected WaitForSeconds _poolReturnWait;

    protected IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps)
    {
        if (_poolReturnWait == null) _poolReturnWait = new WaitForSeconds(ps.main.startLifetime.constantMax);

        yield return _poolReturnWait;
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        _particlePool.Push(ps); // Return the particle system to the pool
    }

    protected void PlayParticleEffect(Vector3 pos)
    {
        var currentPS = GetFromPool();
        currentPS.transform.position = pos;
        currentPS.Play();
        StartCoroutine(ReturnToPoolAfterDelay(currentPS));
    }

    protected void PlaySubParticleEffect(int subParticleIndex, Vector3 pos)
    {
        var currentPS = GetFromSubPsPool(subParticleIndex);
        currentPS.transform.position = pos;
        currentPS.Play();
        StartCoroutine(ReturnToSubPoolAfterDelay(subParticleIndex, currentPS));
    }

    protected void ChangeThemeSeqAnim(int seqNum = 0)
    {
        mainAnimator.SetInteger(SEQ_NUM, seqNum);
    }

    protected void TriggerFinish()
    {
        mainAnimator.SetTrigger(Finish);
    }


    protected ParticleSystem GetFromSubPsPool(int subParticleIndex)
    {
        if (_particlePool.Count > 0)
        {
            var ps = _subParticlePool[subParticleIndex].Pop();

            ps.gameObject.SetActive(true);
            return ps;
        }

        SetSubPsPool(subParticleIndex);
        var newPs = _subParticlePool[subParticleIndex].Pop();

        newPs.gameObject.SetActive(true);
        return newPs;
    }

    protected void SetSubPsPool(int subParticleIndex)
    {
        _subParticlePool.TryAdd(subParticleIndex, new Stack<ParticleSystem>());

        for (int currentSubPsOrder = 0; currentSubPsOrder < MAX_SUB_PS_COUNT; currentSubPsOrder++)
        {
            if (!subPsResourcePathMap.ContainsKey(subParticleIndex))
            {
                Logger.ContentTestLog("Sub effect 미사용");
                continue;
            }

            var particlePrefab = Resources.Load<GameObject>(subPsResourcePathMap[subParticleIndex]);

            for (int currentPoolSize = 0; currentPoolSize < 100; currentPoolSize++)
            {
                var ps = Instantiate(particlePrefab, Vector3.zero, Quaternion.identity).GetComponent<ParticleSystem>();
                ps.gameObject.SetActive(false);
                _subParticlePool[subParticleIndex].Push(ps);
            }
        }
    }

    protected IEnumerator ReturnToSubPoolAfterDelay(int subParticleIndex, ParticleSystem ps)
    {
        if (_poolReturnWait == null) _poolReturnWait = new WaitForSeconds(ps.main.startLifetime.constantMax);

        yield return _poolReturnWait;
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        _subParticlePool[subParticleIndex].Push(ps); // Return the particle system to the pool
    }

    /// <summary>
    ///     1. 7인모드에서 버튼으로 플레이하는 경우 아래 함수를 오버라이드 해서 구현가능
    /// </summary>
    /// <param name="btnId"></param>
    protected virtual void OnBtnClickEvent(int btnId)
    {
        // Override this method to handle button click events
        // Example: Debug.Log($"Button {btnId} clicked");
    }
}