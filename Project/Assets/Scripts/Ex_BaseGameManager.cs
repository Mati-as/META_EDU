using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;


/// <summary>
/// 0508/25
/// UIManager에 텍스트 및 나레이션 전송용 페이로드 입니다.
/// pub-sub패턴 활용이며 텍스트나 나레이션의 양이 많을경우 활용할 수 있습니다. 
/// </summary>
public class UI_Payload : IPayload
{
    public string Narration//텍스트 내용
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
    
    
    public float DelayAndAutoShutTime//텍스트 내용
    {
        get;
    }
    
    public string Checksum//텍스트 내용, 메세지필터링
    {
        get;
    }

    
    public UI_Payload( string narration, bool isCustomOn = false, bool isPopFromZero = true,float delayAndAutoShutTime = 0.0f,string Checksum ="")
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

    protected bool _init;
    protected Animator mainAnimator;
    protected int CurrentMainMainSequence;

    protected readonly int SEQ_NUM = Animator.StringToHash("seqNum");


    private readonly Stack<ParticleSystem> _particlePool = new();
    private Dictionary<int,Stack<ParticleSystem>> _subParticlePool = new();
    
    protected Dictionary<int, bool> _isClickableMap = new();
    protected Dictionary<int,bool> _isClickedMap = new();
    
    
    protected Dictionary<int, Transform> _tfidTotransformMap = new();
    protected Dictionary<int, int> _enumToTfIdMap = new();
    protected Dictionary<int, int> _tfIdToEnumMap = new();
    

    protected Dictionary<Type, Animator> _animators = new();

    //protected Dictionary<Type,Sequence> _sequences = new();
    protected Dictionary<int, Vector3> _defaultSizeMap = new();
    protected Dictionary<int, Quaternion> _defaultRotationQuatMap = new();
    protected Dictionary<int, Quaternion> _defaultLocalRotationQuatMap = new();
    protected Dictionary<int, Sequence> _sequenceMap = new();
    protected Dictionary<int, Animator> _animatorMap = new();
    protected Dictionary<int,Vector3> _defaultPosMap = new(); //
    //resourceFields
    protected string psResourcePath = string.Empty;

    protected Dictionary<int,string> subPsResourcePathMap = new();
    private const int MAX_SUB_PS_COUNT = 5; 
    
    
    
    protected GameObject UIManagerObj;
  
    

    protected virtual new void Awake()
    {
        Init();
    }

    protected virtual new void Init()
    {
        SetUIManager(); // UIcamera 로드를 먼저하고, base.Init에서 카메라 Rect조정.
       
        SetPool();
        bool isAnimatorAttached = TryGetComponent(out mainAnimator);
        if (!isAnimatorAttached) Logger.Log("게임매니저에 애니메이터 없음.");
        
        
         
        base.Init();


    }

    private void SetUIManager()
    {
        bool isUIManagerLoadedOnRuntime = Managers.UI.ShowCurrentSceneUIManager<GameObject>(out UIManagerObj,SceneManager.GetActiveScene().name);

        if (!isUIManagerLoadedOnRuntime) return; 
        var mainCamera = Camera.main;
        
        // UIManager가 로드된 경우, UICamera를 MainCamera의 Stack에 추가
        var uiCameraObj = GameObject.FindGameObjectWithTag("UICamera");
      
        
        
        Canvas canvas = uiCameraObj.GetComponentInChildren<Canvas>();

        if (canvas != null && uiCameraObj != null)
        {
            // Set the render mode and assign the camera
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCameraObj.GetComponent<Camera>();

            Logger.CoreClassLog("UICamera assigned to Canvas successfully.");
        }
        else
        {
            Logger.LogError("Canvas or Camera not found on UICamera object.");
        }
        if (uiCameraObj != null)
        {
            var uiCamera = uiCameraObj.GetComponent<Camera>();
            if (uiCamera != null)
            {
               uiCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
            
                if (mainCamera != null && mainCamera.cameraType == CameraType.Game)
                {
                    if (!mainCamera.GetUniversalAdditionalCameraData().cameraStack.Contains(uiCamera))
                    {
                        mainCamera.GetUniversalAdditionalCameraData().cameraStack.Add(uiCamera);
                        Logger.ContentTestLog("UICamera가 MainCamera의 Stack에 추가됨.");
                    }
                }
                else
                    Logger.LogError("MainCamera가 없거나 올바르지 않은 타입입니다.");
            }
            else
                Logger.LogError("UICamera 오브젝트에 Camera 컴포넌트가 없습니다.");
        }
        else
            Logger.LogError("UICamera 태그를 가진 오브젝트를 찾을 수 없습니다.");
    }


    protected void Bind<T>(Type type) where T : Object
    {
        string[] names = Enum.GetNames(type);
        var objects = new Object[names.Length];
        _objects.Add(typeof(T), objects);

        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
            {
                var obj = Utils.FindChild(gameObject, names[i], true);
                objects[i] = obj;

                if (obj != null)
                {
                    var transform = obj.transform;
                    _tfidTotransformMap.Add(transform.GetInstanceID(), transform);
                    _tfIdToEnumMap.Add(transform.GetInstanceID(), i);
               
                    _isClickableMap.Add(transform.GetInstanceID(), false);
                    _isClickedMap.Add(transform.GetInstanceID(), false);
           
                    _enumToTfIdMap.Add(i, transform.GetInstanceID());
                    _defaultSizeMap.Add(i, transform.localScale);
                    
                    _defaultRotationQuatMap.Add(i, transform.rotation);
                    _defaultLocalRotationQuatMap.Add(i, transform.localRotation);
                    _defaultPosMap.Add(i, transform.position);
                    _sequenceMap.Add(i, DOTween.Sequence());

                    obj.transform.TryGetComponent(out Animator animator);
                    
                    //                    Logger.ContentTestLog($"Key added {transform.GetInstanceID()}:{transform.gameObject.name}");
                    if (animator != null) _animatorMap.Add(i, animator);
                }
            }
            else
                objects[i] = Utils.FindChild<T>(gameObject, names[i], true);

            if (objects[i] == null)
                Debug.Log($"Failed to bind({names[i]})");
        }
    }

    protected void ResetClickable(bool isClickable = true)
    {
        foreach (int key in _isClickableMap.Keys.ToArray())
        {
//            Logger.ContentTestLog($"{key} : {isClickable}");
            _isClickableMap[key] = isClickable;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _objects = new Dictionary<Type, Object[]>();
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
        else
        {
            SetPool();
            var newPs = _particlePool.Pop();

            newPs.gameObject.SetActive(true);
            return newPs;
        }

      
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
    
    protected void PlaySubParticleEffect(int subParticleIndex,Vector3 pos)
    {
        var currentPS = GetFromSubPsPool(subParticleIndex);
        currentPS.transform.position = pos;
        currentPS.Play();
        StartCoroutine(ReturnToSubPoolAfterDelay(subParticleIndex,currentPS));
    }

    protected void ChangeThemeSeqAnim(int seqNum = 0)
    {
        mainAnimator.SetInteger(SEQ_NUM, seqNum);
    }
    
    
    

    protected ParticleSystem GetFromSubPsPool(int subParticleIndex)
    {
        if (_particlePool.Count > 0)
        {
            var ps = _subParticlePool[subParticleIndex].Pop();

            ps.gameObject.SetActive(true);
            return ps;
        }
        else
        {
            SetSubPsPool(subParticleIndex);
            var newPs = _subParticlePool[subParticleIndex].Pop();

            newPs.gameObject.SetActive(true);
            return newPs;
        }


    }
    
    protected void SetSubPsPool(int subParticleIndex)
    {
        _subParticlePool.TryAdd(subParticleIndex, new Stack<ParticleSystem>());
        
        for (int currentSubPsOrder = 0 ; currentSubPsOrder < MAX_SUB_PS_COUNT; currentSubPsOrder++)
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

}