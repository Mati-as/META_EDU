using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;



/// <summary>
/// 오브젝트 바인딩 기능이 추가.
/// 기본적으로 카메라 무빙, 애니메이션을 총괄할 수 있는 메인 애니메이션 컨트롤러 추가 
/// </summary>
public abstract class Ex_BaseGameManager : Base_GameManager
    
{
    protected Dictionary<Type, Object[]> _objects = new();

    protected bool _init;
    protected Animator mainAnimator;
    protected int _currentSequence;
    protected readonly int SEQ_NUM = Animator.StringToHash("seqNum");
    
    
    private readonly Stack<ParticleSystem> _particlePool = new();
    
    protected Dictionary<int,bool> _isClickableMap = new();
    protected Dictionary<int,Transform> _tfidTotransformMap = new();
    protected Dictionary<int,int> enumToTfIdMap = new();
    protected Dictionary<int,int> _tfIdToEnumMap = new();
    protected Dictionary<Type,Animator> _animators = new();
    //protected Dictionary<Type,Sequence> _sequences = new();
    protected Dictionary<int,Vector3> _defaultSizeMap = new(); 
    protected Dictionary<int,Quaternion> _defaultRotationQuatMap = new(); 
    protected Dictionary<int,Sequence> _sequenceMap = new(); 
    protected Dictionary<int,Animator> _animatorMap = new(); 
    
    protected string psResourcePath = string.Empty;


    protected new virtual void Awake()
    {
        Init();
        
    }
    protected new virtual void Init()
    {
        base.Init();
        
        SetPool();
        var isAnimatorAttached= TryGetComponent(out mainAnimator);
        if(!isAnimatorAttached) Logger.Log("게임매니저에 애니메이터 없음.");
    }

    
    protected void Bind<T>(Type type) where T : Object
    {
        var names = Enum.GetNames(type);
        var objects = new Object[names.Length];
        _objects.Add(typeof(T), objects);

        for (var i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
            {
                var obj = Utils.FindChild(gameObject, names[i], true);
                objects[i] = obj;

                if (obj != null)
                {
                    var transform = obj.transform;
                    _tfidTotransformMap.Add(transform.GetInstanceID(),transform);
                    _tfIdToEnumMap.Add(transform.GetInstanceID(),i);
                    Logger.ContentTestLog($"Key added {transform.GetInstanceID()}:{transform.gameObject.name}");
                    _isClickableMap.Add(transform.GetInstanceID(),false);
                    
                    _defaultSizeMap.Add(i,transform.localScale);
                    _defaultRotationQuatMap.Add(i,transform.rotation);
                    
                    _sequenceMap.Add(i,DOTween.Sequence());
                    
                    obj.transform.TryGetComponent(out Animator animator);
                    if(animator!=null)_animatorMap.Add(i,animator);
                }
            }
            else
            {
                objects[i] = Utils.FindChild<T>(gameObject, names[i], true);
            }

            if (objects[i] == null)
                Debug.Log($"Failed to bind({names[i]})");
        }
    }

    protected void ResetClickable()
    {
        foreach (var key in _isClickableMap.Keys.ToArray())
        {
            _isClickableMap[key] = true;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _objects= new();
    }

    protected void BindObject(Type type)
    {
        Bind<GameObject>(type);
    }

    protected void BindImage(Type type)
    {
        Bind<Image>(type);
    }

    protected void BindText(Type type)
    {
        Bind<TextMeshProUGUI>(type);
    }

    protected void BindButton(Type type)
    {
        Bind<Button>(type);
    }



    protected T Get<T>(int idx) where T : Object
    {
        Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }

    protected GameObject GetObject(int idx)
    {
        return Get<GameObject>(idx);
    }

    protected TextMeshProUGUI GetText(int idx)
    {
        return Get<TextMeshProUGUI>(idx);
    }

    protected Button GetButton(int idx)
    {
        return Get<Button>(idx);
    }

    protected Image GetImage(int idx)
    {
        return Get<Image>(idx);
    }
    

    public static void BindEvent(GameObject go, Action action, Define.UIEvent type = Define.UIEvent.Click)
    {
        var evt = Utils.GetOrAddComponent<UI_EventHandler>(go);

        switch (type)
        {
            case Define.UIEvent.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
                break;
            case Define.UIEvent.Pressed:
                evt.OnPressedHandler -= action;
                evt.OnPressedHandler += action;
                break;
            case Define.UIEvent.PointerDown:
                evt.OnPointerDownHandler -= action;
                evt.OnPointerDownHandler += action;
                break;
            case Define.UIEvent.PointerUp:
                evt.OnPointerUpHandler -= action;
                evt.OnPointerUpHandler += action;
                break;
        }
    }
    
    protected void SetPool()
    {
        if (psResourcePath == string.Empty)
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

    protected void ChangeSeqAnim(int seqNum = 0)
    {
        _currentSequence = seqNum;
        mainAnimator.SetInteger(SEQ_NUM, seqNum);
    }
}
