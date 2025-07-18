using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Recommendations : UI_PopUp
{
    private readonly List<List<XmlManager.SceneData>> _sceneData = new();
    private readonly List<GameObject[]> pageButtonsList = new();
    private UI_Master _master;


    private enum Btns
    {
        RecommendationA,
        RecommendationB,
        RecommendationC,
        RecommendationD,
        Max
    }
    public override bool InitOnLoad()
    {
        BindButton(typeof(Btns));

        FilterRandomRecommendations(); // 1. 추천 콘텐츠 먼저 준비
        InitPageNavigation();          // 2. 버튼에 바인딩

        Logger.CoreClassLog("추천 UI 초기화 완료-----------------------");
        return true;
    }
    public void InitPageNavigation()
    {
        if (_sceneData.Count == 0 || _sceneData[0].Count < 4)
        {
            Debug.LogWarning("추천 데이터가 부족합니다.");
            return;
        }

        BindSceneDataToButton(GetButton((int)Btns.RecommendationA).gameObject, _sceneData[0][0]);
        BindSceneDataToButton(GetButton((int)Btns.RecommendationB).gameObject, _sceneData[0][1]);
        BindSceneDataToButton(GetButton((int)Btns.RecommendationC).gameObject, _sceneData[0][2]);
        BindSceneDataToButton(GetButton((int)Btns.RecommendationD).gameObject, _sceneData[0][3]);
    }
    
    
    public void FilterRandomRecommendations()
    {
        var allScenes = XmlManager.Instance.SceneSettings.Values
            .Where(x => x.IsActive)
            .ToList();

        if (allScenes.Count < 4)
        {
            Debug.LogWarning("추천할 콘텐츠가 4개 미만입니다.");
            return;
        }

        var randomFour = allScenes.OrderBy(x => Random.value).Take(4).ToList();

        _sceneData.Clear();
        _sceneData.Add(randomFour);
    }
    private void BindSceneDataToButton(GameObject buttonObj, XmlManager.SceneData data)
    {
        buttonObj.SetActive(true);

        GetThumbnailImage(buttonObj, data);

        // 텍스트 설정
        var textObj = buttonObj.transform.GetChild(2);
        if (textObj != null)
        {
            var text = textObj.GetComponent<Text>();
            text.text = !string.IsNullOrEmpty(data.Title) ? data.Title : data.Id;
        }

        // 자물쇠
        var lockFrame = buttonObj.transform.Find("LockFrame");
        if (lockFrame != null)
            lockFrame.gameObject.SetActive(!data.IsActive);

        // 클릭 이벤트
        var btn = buttonObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = data.IsActive;
            btn.onClick.RemoveAllListeners();

            if (data.IsActive)
            {
                var btnImageController = Utils.GetOrAddComponent<CursorImageController>(buttonObj);
                btnImageController.DefaultScale = buttonObj.transform.localScale;

                string sceneId = data.Id;
                btn.onClick.AddListener(() =>
                {
                    SceneManager.LoadSceneAsync(sceneId);
                });
            }
        }
    }

    private void ApplySceneDataToPage(int pageIndex, List<XmlManager.SceneData> sceneList)
    {
        var buttonObjs = pageButtonsList[pageIndex];

        for (int i = 0; i < buttonObjs.Length; i++)
        {
            var buttonObj = buttonObjs[i];

            if (i >= sceneList.Count)
            {
                buttonObj.SetActive(false);
                continue;
            }

            var data = sceneList[i];
            buttonObj.SetActive(true);

            GetThumbnailImage(buttonObj, data);

            // 텍스트 설정
            var textObj = buttonObj.transform.GetChild(2);
            if (textObj != null)
            {
                var text = textObj.GetComponent<Text>();
                text.text = !string.IsNullOrEmpty(data.Title) ? data.Title : data.Id;
            }

            // 자물쇠
            var lockFrame = buttonObj.transform.Find("LockFrame");
            if (lockFrame != null)
                lockFrame.gameObject.SetActive(!data.IsActive);

            // 클릭 이벤트
            var btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = data.IsActive;
                btn.onClick.RemoveAllListeners();

                if (data.IsActive)
                {
                    var btnImageController = Utils.GetOrAddComponent<CursorImageController>(buttonObj);
                    btnImageController.DefaultScale = buttonObj.transform.localScale;

                    string sceneId = data.Id;
                    btn.onClick.AddListener(() =>
                    {
                        if (_master == null)
                            _master = Managers.UI.SceneUI.GetComponent<UI_Master>();

                        _master.OnSceneBtnClicked(sceneId, data.Title);
                    });
                }
            }
        }
    }

    public void GetThumbnailImage(GameObject obj, XmlManager.SceneData data)
    {
        // 🛠️ 지역 변수에 참조를 고정해서 클로저 이슈 방지
        var objRef = obj;
        var dataRef = data;

        StartCoroutine(LoadThumbnailImage(objRef, dataRef));
    }

    public IEnumerator LoadThumbnailImage(GameObject obj, XmlManager.SceneData sceneData)
    {
        var dataRef = sceneData;
        var objRef = obj;
        string sceneId = dataRef.Id;


        // ✅ 1. 캐시에 Sprite가 있는지 먼저 확인
        if (Managers.Resource.ThumbnailCache.TryGetValue(sceneId, out var cachedSprite))
        {
            ApplyThumbnail(objRef, cachedSprite);
            yield break;
        }

        // ✅ 2. 경로 구성
        string thumbnailImagePath = Path.Combine(Application.streamingAssetsPath, $"Thumbnail_Image/{sceneId}.png");
        string thumbnailUri = "file://" + thumbnailImagePath;

        // ✅ 3. 웹 요청 (로컬에서 로딩)
        using (var uwrThumb = UnityWebRequestTexture.GetTexture(thumbnailUri))
        {
            yield return uwrThumb.SendWebRequest();

            if (uwrThumb.result != UnityWebRequest.Result.Success)
                Logger.CoreClassLog($"❌ 썸네일 로딩 실패: {sceneId} - {uwrThumb.error}");
            else
            {
                var texture = DownloadHandlerTexture.GetContent(uwrThumb);

                var sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f); // Optional: pixelsPerUnit 설정

                // ✅ 4. 캐시에 저장
                Managers.Resource.ThumbnailCache[sceneId] = sprite;

                // ✅ 5. 적용
                ApplyThumbnail(objRef, sprite);
            }
        }
    }


    private void ApplyThumbnail(GameObject obj, Sprite sprite)
    {
        var mask = obj.transform.Find("PictureFrmae_(Mask)");
        if (mask != null && mask.childCount > 0)
        {
            var img = mask.GetChild(0).GetComponent<Image>();
            if (img != null)
                img.sprite = sprite;
        }
    }
}