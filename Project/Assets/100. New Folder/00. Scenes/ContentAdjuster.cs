using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class ContentAdjuster : MonoBehaviour
{
    UI_Master _master;
 
    //(확인) 적용된 프리팹의 Acitve 부분이 활성화 된 상태에서 시작
    public enum ModeType
    {
        주제별1학기,
        주제별2학기,
        신체활동,
        예술경험,
        자연탐구,
        의사소통,
        사회관계,
        주제별1월,
        주제별2월,
        주제별3월,
        주제별4월,
        주제별5월,
        주제별6월,
        주제별7월,
        주제별8월,
        주제별9월,
        주제별10월,
        주제별11월,
        주제별12월,
        Test
    }
    public enum ContentCategory
    {
        Physical_Activity,
        Art_Expression,
        Science_Exploration,
        Social_Skills,
        Communication
    }

    public ModeType mode; // 인스펙터에서 선택

    public GameObject Active_month;   // 6개 자식 포함
    public GameObject Inactive_month; // 6개 자식 포함

    private bool _isTestScene =false; //전체활성화 토글용

    private string[] allKeys = new string[]
    {
        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    };

    private void Start()
    {
        // pageA = Utils.FindChild(gameObject, "Page_A", true);
        // pageB = Utils.FindChild(gameObject, "Page_B", true);
        prevButton = Utils.FindChild(gameObject, "Button_Prev", true)?.GetComponent<Button>();
        nextButton = Utils.FindChild(gameObject, "Button_Next", true)?.GetComponent<Button>();
        pageNavis = Utils.FindChild(gameObject, "PageNavi", true);

        _pagePrefab = Resources.Load<GameObject>("UI/Elements/Page");
        
         Utils.FindChild(gameObject, "PageParent", true)?.TryGetComponent(out _pageParent);
         Utils.FindChild(gameObject, "IndicatorParent", true)?.TryGetComponent(out _indicatorParent);
        
        _indicatorPrefab = Resources.Load<GameObject>("UI/Elements/Indicator");
        ApplyContentAdjustments();
    }

    //[주제별]
    public void ApplyContentAdjustments()
    {
        string[] keysToCheck = new string[6];
        int[] monthIndices = new int[6];
        _isTestScene = false;
        
        switch (mode)
        {
            case ModeType.주제별1학기:
                monthIndices = new int[] { 2, 3, 4, 5, 6, 7 }; // Mar~Aug
                break;

            case ModeType.주제별2학기:
                monthIndices = new int[] { 8, 9, 10, 11, 0, 1 }; // Sep~Feb
                break;

            case ModeType.신체활동:
                FilterAndPaginateContent(ContentCategory.Physical_Activity);
                InitPageNavigation();
                return;
            case ModeType.자연탐구:
                FilterAndPaginateContent(ContentCategory.Science_Exploration);
                InitPageNavigation();
                return;
            case ModeType.예술경험:
                FilterAndPaginateContent(ContentCategory.Art_Expression);
                InitPageNavigation();
                return;
            case ModeType.사회관계:
                FilterAndPaginateContent(ContentCategory.Social_Skills);
                InitPageNavigation();
                return;
            case ModeType.의사소통:
                FilterAndPaginateContent(ContentCategory.Communication);
                InitPageNavigation();
                return;
            case ModeType.주제별1월:
                FilterAndPaginateContentByMonth("Jan");
                InitPageNavigation();
                return;
            case ModeType.주제별2월:
                FilterAndPaginateContentByMonth("Feb");
                InitPageNavigation();
                return;
            case ModeType.주제별3월:
                FilterAndPaginateContentByMonth("Mar");
                InitPageNavigation();
                return;
            case ModeType.주제별4월:
                FilterAndPaginateContentByMonth("Apr");
                InitPageNavigation();
                return;
            case ModeType.주제별5월:
                FilterAndPaginateContentByMonth("May");
                InitPageNavigation();
                return;
            case ModeType.주제별6월:
                FilterAndPaginateContentByMonth("Jun");
                InitPageNavigation();
                return;
            case ModeType.주제별7월:
                FilterAndPaginateContentByMonth("Jul");
                InitPageNavigation();
                return;
            case ModeType.주제별8월:
                FilterAndPaginateContentByMonth("Aug");
                InitPageNavigation();
                return;
            case ModeType.주제별9월:
                FilterAndPaginateContentByMonth("Sep");
                InitPageNavigation();
                return;
            case ModeType.주제별10월:
                FilterAndPaginateContentByMonth("Oct");
                InitPageNavigation();
                return;
            case ModeType.주제별11월:
                FilterAndPaginateContentByMonth("Nov");
                InitPageNavigation();
                return;
            case ModeType.주제별12월:
                FilterAndPaginateContentByMonth("Dec");
                InitPageNavigation();
                return;
            
            case ModeType.Test:
                _isTestScene = true;
                FilterAllScenes(); // 전체 씬 가져오는 함수 호출
                InitPageNavigation();
                return;
        }


        for (int i = 0; i < 6; i++)
        {
            string key = allKeys[monthIndices[i]];
            bool isActive = XmlManager.Instance.GetMenuSetting(key);

            Transform activeChild = Active_month.transform.GetChild(i);
            Transform inactiveChild = Inactive_month.transform.GetChild(i);

            activeChild.gameObject.SetActive(isActive);
            inactiveChild.gameObject.SetActive(!isActive);
        }
    }


    //[영역별]
    public ContentCategory selectedCategory = ContentCategory.Science_Exploration;

    private GameObject _pagePrefab; // 페이지 프리팹 (버튼 8개 포함된 비활성화된 상태로 있어야 함)
    private Transform _pageParent;  // 페이지들이 들어갈 부모 오브젝트
    private GameObject _indicatorPrefab; // Indicator 한 칸 (On 포함)
    private Transform _indicatorParent;  // Indicator를 넣을 부모
    private List<GameObject> pages = new();
    private List<GameObject[]> pageButtonsList = new();
    
    private GameObject[] pageAButtons;
    private GameObject[] pageBButtons;

    public Button prevButton;
    public Button nextButton;

    [FormerlySerializedAs("pageNavi")] public GameObject pageNavis; //(중요) 3페이지 이상일 경우 하위 오브젝트 추가 필요

    private int currentPageIndex = 0;
    private int currentPageSlot = 0; // 0 = A, 1 = B

    private List<List<XmlManager.SceneData>> pagedSceneData = new List<List<XmlManager.SceneData>>();


    private Vector2 pageVisiblePos = new Vector2(0f, -193f);
    private Vector2 pageLeftOffscreen = new Vector2(-1920f, -193f);
    private Vector2 pageRightOffscreen = new Vector2(1920f, -193f);

    private bool isSliding = false;


    public void InitPageNavigation()
    {
        // 기존 페이지/인디케이터 제거
        foreach (var p in pages) Destroy(p);
        pages.Clear();
        pageButtonsList.Clear();

        foreach (Transform child in _indicatorParent)
            Destroy(child.gameObject);

        int totalPages = pagedSceneData.Count;

        for (int i = 0; i < totalPages; i++)
        {
            // 페이지 생성
            GameObject newPage = Instantiate(_pagePrefab, _pageParent);
            newPage.name = $"Page_{i}";
            newPage.SetActive(true);

            RectTransform rect = newPage.GetComponent<RectTransform>();
            rect.anchoredPosition = (i == 0) ? pageVisiblePos : pageRightOffscreen;

            // 버튼 수집
            GameObject[] btnArray = new GameObject[8];
            for (int j = 0; j < newPage.transform.childCount; j++)
                btnArray[j] = newPage.transform.GetChild(j).gameObject;

            pages.Add(newPage);
            pageButtonsList.Add(btnArray);

            ApplySceneDataToPage(i, pagedSceneData[i]);

            // Indicator 생성
            GameObject indicator = Instantiate(_indicatorPrefab, _indicatorParent);
            indicator.name = $"Indicator_{i}";
            indicator.transform.Find("On").gameObject.SetActive(i == 0);
        }

        currentPageIndex = 0;
        isSliding = false;

        prevButton.onClick.RemoveAllListeners();
        nextButton.onClick.RemoveAllListeners();
        prevButton.onClick.AddListener(() => SwitchPage(false));
        nextButton.onClick.AddListener(() => SwitchPage(true));

        UpdateNavigationControls();
    }

    void UpdateNavigationIndicator(int activeIndex)
    {
        for (int i = 0; i < _indicatorParent.childCount; i++)
        {
            var onObj = _indicatorParent.GetChild(i).Find("On");
            if (onObj != null)
                onObj.gameObject.SetActive(i == activeIndex);
        }
    }

    private void UpdateNavigationControls()
    {
        int pageCount = pagedSceneData.Count;

        // 페이지 수가 1개 미만이면 모두 숨김
        bool enableNav = pageCount > 1;

        prevButton.interactable = enableNav && currentPageIndex > 0;
        nextButton.interactable = enableNav && currentPageIndex < pageCount - 1;

        if (pageNavis != null)
        {
            pageNavis.gameObject.SetActive(enableNav);
            prevButton.gameObject.SetActive(enableNav);
            nextButton.gameObject.SetActive(enableNav);
        }
    }


    public void SwitchPage(bool goingNext)
    {
        if (isSliding) return;

        int nextIndex = currentPageIndex + (goingNext ? 1 : -1);
        if (nextIndex < 0 || nextIndex >= pages.Count) return;

        isSliding = true;

        var current = pages[currentPageIndex];
        var next = pages[nextIndex];

        Vector2 outTarget = goingNext ? pageLeftOffscreen : pageRightOffscreen;
        Vector2 inTarget = pageVisiblePos;

        current.GetComponent<RectTransform>().DOAnchorPos(outTarget, 0.4f).SetEase(Ease.InOutQuad);
        next.GetComponent<RectTransform>().DOAnchorPos(inTarget, 0.4f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            currentPageIndex = nextIndex;
            isSliding = false;
            UpdateNavigationControls();
        });

        UpdateNavigationIndicator(nextIndex);
    }

    public void FilterAndPaginateContent(ContentCategory category)
    {
        Dictionary<string,XmlManager.SceneData>.ValueCollection allScenes = XmlManager.Instance.SceneSettings.Values;

        string categoryKey = category.ToString();

        var filtered = allScenes
            .Where(x =>
                    x.Category == categoryKey &&
                    x.Month == "None")
            .ToList();

        pagedSceneData.Clear();

        for (int i = 0; i < filtered.Count; i += 8)
        {
            pagedSceneData.Add(filtered.Skip(i).Take(8).ToList());
        }

        Debug.Log($"[{categoryKey}] 영역 콘텐츠 {filtered.Count}개 → {pagedSceneData.Count}페이지 구성 완료");
    }

    public void FilterAndPaginateContentByMonth(string monthKey)
    {
        var allScenes = XmlManager.Instance.SceneSettings.Values;

        // XML에서 이 월이 활성화되어 있는지 확인
        if (!XmlManager.Instance.MenuSettings.TryGetValue(monthKey, out bool isMonthEnabled) || !isMonthEnabled)
        {
            Debug.LogWarning($"[{monthKey}]은 MenuSettingData.xml에서 비활성화된 월입니다.");
            pagedSceneData.Clear();
            return;
        }

        // 월이 일치하고 활성화된 콘텐츠만 필터
        var filtered = allScenes
            .Where(x => x.Month == monthKey).ToList();

        pagedSceneData.Clear();
        for (int i = 0; i < filtered.Count; i += 8)
        {
            pagedSceneData.Add(filtered.Skip(i).Take(8).ToList());
        }

        Debug.Log($"{monthKey} 콘텐츠 {filtered.Count}개 → {pagedSceneData.Count}페이지 구성 완료");
    }

    private void ApplySceneDataToPage(int pageIndex, List<XmlManager.SceneData> sceneList)
    {
        var buttonObjs = pageButtonsList[pageIndex];

        for (int i = 0; i < buttonObjs.Length; i++)
        {
            GameObject buttonObj = buttonObjs[i];

            if (i >= sceneList.Count)
            {
                buttonObj.SetActive(false);
                continue;
            }

            XmlManager.SceneData data = sceneList[i];
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
                lockFrame.gameObject.SetActive(!data.IsActive && !_isTestScene);

            // 클릭 이벤트
            var btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = data.IsActive || _isTestScene;
                btn.onClick.RemoveAllListeners();

                if (data.IsActive || _isTestScene)
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
        GameObject objRef = obj;
        XmlManager.SceneData dataRef = data;

        StartCoroutine(LoadThumbnailImage(objRef, dataRef));
    }

    

    public IEnumerator LoadThumbnailImage(GameObject obj, XmlManager.SceneData sceneData)
    {
        XmlManager.SceneData dataRef = sceneData;
        GameObject objRef = obj;
        string sceneId = dataRef.Id;

        
        // ✅ 1. 캐시에 Sprite가 있는지 먼저 확인
        if (Managers.Resource.ThumbnailCache.TryGetValue(sceneId, out Sprite cachedSprite))
        {
            ApplyThumbnail(objRef, cachedSprite);
            yield break;
        }

        // ✅ 2. 경로 구성
        string thumbnailImagePath = Path.Combine(Application.streamingAssetsPath, $"Thumbnail_Image/{sceneId}.png");
        string thumbnailUri = "file://" + thumbnailImagePath;

        // ✅ 3. 웹 요청 (로컬에서 로딩)
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

                Sprite sprite = Sprite.Create(texture,
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
    /// <summary>
    /// 테스트 전용 전체 씬 로드 
    /// </summary>
    public void FilterAllScenes()
    {
        var allScenes = XmlManager.Instance.SceneSettings.Values.ToList();

        pagedSceneData.Clear();
        for (int i = 0; i < allScenes.Count; i += 8)
        {
            pagedSceneData.Add(allScenes.Skip(i).Take(8).ToList());
        }

        Debug.Log($"[Test 모드] 전체 콘텐츠 {allScenes.Count}개 → {pagedSceneData.Count}페이지 구성 완료");
    }
}
