using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();
    public Dictionary<string, SkeletonDataAsset> _skeletons = new Dictionary<string, SkeletonDataAsset>();

    public void Init()
    {
    }

    public T Load<T>(string path) where T : Object
    {
        if (typeof(T) == typeof(Sprite))
        {
            if (_sprites.TryGetValue(path, out Sprite sprite))
                return sprite as T;

            Sprite sp = Resources.Load<Sprite>(path);
            _sprites.Add(path, sp);
            return sp as T;
        }
        else if (typeof(T) == typeof(SkeletonDataAsset))
        {
            if (_skeletons.TryGetValue(path, out SkeletonDataAsset sprite))
                return sprite as T;

            SkeletonDataAsset sp = Resources.Load<SkeletonDataAsset>(path);
            _skeletons.Add(path, sp);
            return sp as T;
        }

        return Resources.Load<T>(path);
    }

    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject prefab = Load<GameObject>($"Prefabs/{path}");
        if (prefab == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        return Instantiate(prefab, parent);
    }

    public GameObject Instantiate(GameObject prefab, Transform parent = null)
    {
        GameObject go = Object.Instantiate(prefab, parent);
        go.name = prefab.name;
        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        Object.Destroy(go);
    }
}
