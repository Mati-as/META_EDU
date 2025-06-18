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

public class Ex_MonoBehaviour : MonoBehaviour
{

    protected virtual void Awake()
    {
        Init();
    }
    
    protected Dictionary<int, bool> _isClickableMap = new();
    protected Dictionary<int,bool> _isClickedMap = new();
    
    
    protected Dictionary<int, Transform> _tfidTotransformMap = new();
    protected Dictionary<int, int> _enumToTfIdMap = new();
    protected Dictionary<int, int> _tfIdToEnumMap = new();
    

    protected Dictionary<Type, Animator> _animators = new();

    //protected Dictionary<Type,Sequence> _sequences = new();
    protected Dictionary<int, Vector3> _defaultSizeMap = new();
    protected Dictionary<int, Quaternion> _defaultRotationQuatMap = new();
    protected Dictionary<int, Sequence> _sequenceMap = new();
    protected Dictionary<int, Animator> _animatorMap = new();
    protected Dictionary<int,Vector3> _defaultPosMap = new(); //
    protected Dictionary<Type, Object[]> _objects = new();

    protected void OnDestroy()
    { 
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

    protected virtual void Init()
    {
        
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
}
