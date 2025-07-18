using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MediaArtTextData
{
    [XmlElement("SceneID")]
    public string sceneId;

    // Title은 XML에 없으므로 생략하거나 나중에 사용할 수 있습니다.
    public string title;

    [XmlElement("Body")]
    public string body;
}
public class UI_MediaArtInScene : UI_PopUp
{

    MediaArtTextLoader mediaArtTextLoader = new();
    private Animator _animator;
    private bool _isOn = false;
    private static readonly int IsOn = Animator.StringToHash("isOn");

    private enum UI
    {
        Btn
    }

    private enum TMPs
    {
        TMP_MainTitle,
        TMP_SmallTitle,
        TMP_Body
        
    }
    public override bool InitOnLoad()
    {
        return base.InitOnLoad();
    }

    
    /// <summary>
    /// 기본적응로 초기화 되어있으나, 해당 UI를 사용하기 위해서는 InitToUse()를 호출해야 합니다.
    /// </summary>
    public void InitToUse()
    {
        BindObject(typeof(UI));
        BindTMP(typeof(TMPs));

        transform.localScale = Vector3.one;
        _animator = GetComponent<Animator>();
        GetObject((int)UI.Btn).BindEvent(() =>
        {
            _isOn = !_isOn;
            _animator.SetBool(IsOn, _isOn);
        });

   
        
        StartCoroutine(mediaArtTextLoader.LoadAnnotationData(() =>
        {
            var data = mediaArtTextLoader.GetTextData(SceneManager.GetActiveScene().name);
            if (data != null)
            {
                Debug.Log($"[{SceneManager.GetActiveScene().name}]\nTitle: {data.title}\nBody: {data.body}");
                GetTMP((int)TMPs.TMP_Body).text = RemoveIndentation(data.body);
            }
            else
            {
                Logger.Log($"[{SceneManager.GetActiveScene().name}] 해당 SceneId에 대한 설명 데이터가 없습니다: " + SceneManager.GetActiveScene().name);
            }
        }));
    }
    
    private string RemoveIndentation(string rawText)
    {
        // 모든 줄 앞 공백 제거
        var lines = rawText.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].TrimStart();
        }

        return string.Join("\n", lines).Trim(); // 양 끝 공백도 제거
    }
}

public class MediaArtTextLoader : MonoBehaviour
{
    private Dictionary<string, MediaArtTextData> _textDataMap;

    public IEnumerator LoadAnnotationData(System.Action onComplete = null)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Painting/Annotations.xml");
        string uri = "file://" + path;

        using (UnityWebRequest uwr = UnityWebRequest.Get(uri))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Annotation XML 로딩 실패: " + uwr.error);
                yield break;
            }

            string xmlText = uwr.downloadHandler.text;

            XmlSerializer serializer = new XmlSerializer(typeof(AnnotationRoot));
            using (StringReader reader = new StringReader(xmlText))
            {
                AnnotationRoot root = (AnnotationRoot)serializer.Deserialize(reader);
                _textDataMap = new Dictionary<string, MediaArtTextData>();
                foreach (var entry in root.Entries)
                {
                    _textDataMap[entry.sceneId] = entry;
                    Logger.Log("Annotation Entry Loaded: " + entry.sceneId);
                }

                Debug.Log($"✅ Annotation.xml 로딩 완료: {_textDataMap.Count}개 항목");
                onComplete?.Invoke();
            }
        }
        
        
    }

    

    
    // ✅ SceneId로 설명 데이터 조회
    public MediaArtTextData GetTextData(string sceneId)
    {
        if (_textDataMap != null && _textDataMap.TryGetValue(sceneId, out var data))
            return data;

        return null;
    }
    
    
    
}


[XmlRoot("Annotations")]
public class AnnotationRoot
{
    [XmlElement("Entry")]
    public List<MediaArtTextData> Entries;
}