using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

public class ResourceManager : MonoBehaviour
{
    public Dictionary<string, Sprite> _sprites = new();
    public Dictionary<string, SkeletonDataAsset> _skeletons = new();
    public Dictionary<string, Sprite> ThumbnailCache = new();


    public void Init()
    {
        LoadThumbnailImage();
    }
    public void LoadAllThumbnails()
    {
     
        LoadThumbnailImage();
    }


    public void LoadThumbnailImage()
    {
        StartCoroutine(PreloadThumbnailsCoroutine());
    }

    private IEnumerator PreloadThumbnailsCoroutine()
    {
        
        
        List<XmlManager.SceneData> allScenes = XmlManager.Instance.SceneSettings.Values.ToList();
        
        
        foreach (XmlManager.SceneData data in allScenes)
        {
            string sceneId = data.Id;
            string thumbnailImagePath = Path.Combine(Application.streamingAssetsPath, $"Thumbnail_Image/{sceneId}.png");
            string thumbnailUri = "file://" + thumbnailImagePath;

            using (UnityWebRequest uwrThumb = UnityWebRequestTexture.GetTexture(thumbnailUri))
            {
                yield return uwrThumb.SendWebRequest();

                if (uwrThumb.result != UnityWebRequest.Result.Success)
                {
                    Logger.CoreClassLog($"❌ 썸네일 로딩 실패: {sceneId} - {uwrThumb.error}");
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwrThumb);
                  

                    var sprite = Sprite.Create(texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f);
                    

                    ThumbnailCache[sceneId] = sprite;
                 //   Logger.CoreClassLog($"✅ 썸네일 미리 저장 완료: {sceneId}");
                }
            }
        }

        Logger.CoreClassLog($"✅ 전체 썸네일 미리 로딩 완료: {ThumbnailCache.Count}개");
    }


    public T Load<T>(string path) where T : Object
    {
        if (typeof(T) == typeof(Sprite))
        {
            if (_sprites.TryGetValue(path, out var sprite))
                return sprite as T;

            var sp = Resources.Load<Sprite>(path);
            _sprites.Add(path, sp);
            return sp as T;
        }

        if (typeof(T) == typeof(SkeletonDataAsset))
        {
            if (_skeletons.TryGetValue(path, out var sprite))
                return sprite as T;

            var sp = Resources.Load<SkeletonDataAsset>(path);
            _skeletons.Add(path, sp);
            return sp as T;
        }

        return Resources.Load<T>(path);
    }

    public GameObject Instantiate(string path, Transform parent = null)
    {
        var prefab = Load<GameObject>($"Prefabs/{path}");
        if (prefab == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        return Instantiate(prefab, parent);
    }

    public GameObject Instantiate(GameObject prefab, Transform parent = null)
    {
        var go = Object.Instantiate(prefab, parent);
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