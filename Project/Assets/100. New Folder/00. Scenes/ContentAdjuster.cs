using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class ContentAdjuster : MonoBehaviour
{
    UI_MetaEduLauncherMaster _metaEduLauncherMaster;
    private readonly Dictionary<string, string> sceneTitles = new Dictionary<string, string>()
    {
        
        //⚠️⛔️ 테스트용 임시 컨텐츠 이름, 컨텐츠 이름 반드시 수정 필요  ⛔️⚠️
        { "EA034", "파티를 해요" },
        { "SideWalk", "차도와 보도는 달라요" },
    
        { "EA035", "풍속화첩 씨름 색칠해요" },
        { "EA036", "풍속화첩 서당 색칠해요" },
        { "EA037", "모나리자 색칠해요" },
        { "EA038", "고흐 초상화 색칠해요" },
        { "EA039", "별이 빛나는 밤에 색칠해요" },
        
        //
        
        
        
        
        { "BB002", "사방치기 놀이" },
        { "BB003", "알록달록 손발을 뒤집어요" },
        { "BB006", "고래와 공놀이를 해요" },
        { "BB008_E", "물고기를 잡아요" },
        { "BB009", "우주의 행성이 움직여요" },
        { "EA006", "허수아비 아저씨를 도와줘요" },
        { "EA009", "몸에 좋은 음식을 찾아요" },
        { "BA001", "송하맹호도 색칠해요" },
        { "BD001", "무지개 건반을 연주해요" },
        { "BD002", "바다에서 연주해요" },
        { "BD004", "악기 연주를 해요" },
        { "BD003", "비즈드럼을 연주해요" },
        { "BC001", "가을낙엽" },
        { "AA004", "밤하늘이 반짝여요" },
        { "AA005", "코스모스가 움직여요" },
        { "AA006", "뒤뚱뒤뚱 펭귄이 와요" },
        { "EA002", "동물을 찾아봐요" },
        { "EA014", "같은 모양을 찾아요" },
        { "EA015", "가을 낙엽 [3세]" },
        { "EA016", "고래와 공놀이를 해요 [3세]" },
        { "EA028", "다양한 모양을 찾아요" },
        { "EA029", "자동차를 주차해요" },
        { "EA024", "무지개 [미디어 아트]" },
        { "EA019", "풍선을 날려요" },
        { "BB004", "손발을 뒤집어요" },
        { "EA004", "다양한 표정을 알아봐요" },
        { "EA008", "거품목욕놀이" },
        { "BA005", "샌드위치를 만들어요" },
        { "EA003", "과일과 야채담기" },
        { "EA003_E", "과일과 야채담기 [ENG]" },
        { "EA025", "북극 [미디어 아트]" },
        { "EA026", "공룡 [미디어 아트]" },
        { "EA018", "자동차를 꾸며요" },
        { "EA017", "일하는 자동차가 있어요" },
        { "EA012", "어떤 자동차 일까요?" },
        { "EA021", "해변 [미디어 아트]" },
        { "EA020", "횡단보도를 안전하게 건너요" },
        { "EA010", "가을 열매를 찾아요" },
        { "EA022", "코스모스  [미디어 아트]" },
        { "EA023", "숲속  [미디어 아트]" },
        { "EA031", "불이 나면 비상구로 대피해요" },
        { "EA030", "가을 곤충은 어디 있을까요?" }
    };

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
        pageA = Utils.FindChild(gameObject, "Page_A", true);
        pageB = Utils.FindChild(gameObject, "Page_B", true);
        prevButton = Utils.FindChild(gameObject, "Button_Prev", true)?.GetComponent<Button>();
        nextButton = Utils.FindChild(gameObject, "Button_Next", true)?.GetComponent<Button>();
        pageNavi = Utils.FindChild(gameObject, "PageNavi", true);
        
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

    public GameObject pageA;
    public GameObject pageB;
    private GameObject[] pageAButtons;
    private GameObject[] pageBButtons;

    public Button prevButton;
    public Button nextButton;

    public GameObject pageNavi; //(중요) 3페이지 이상일 경우 하위 오브젝트 추가 필요

    private int currentPageIndex = 0;
    private int currentPageSlot = 0; // 0 = A, 1 = B

    private List<List<XmlManager.SceneData>> pagedSceneData = new List<List<XmlManager.SceneData>>();


    private Vector2 pageVisiblePos = new Vector2(0f, -193f);
    private Vector2 pageLeftOffscreen = new Vector2(-1920f, -193f);
    private Vector2 pageRightOffscreen = new Vector2(1920f, -193f);

    private bool isSliding = false;


    public void InitPageNavigation()
    {
        pageAButtons = new GameObject[8];
        pageBButtons = new GameObject[8];

        for (int i = 0; i < pageA.transform.childCount; i++)
        {
            pageAButtons[i] = pageA.transform.GetChild(i).gameObject;
        }

        for (int i = 0; i < pageB.transform.childCount; i++)
        {
            pageBButtons[i] = pageB.transform.GetChild(i).gameObject;
        }

        if (pageAButtons.Length != 8 || pageBButtons.Length != 8)
        {
            Debug.LogError("PageA 또는 PageB에 Button이 정확히 8개씩 있어야 합니다.");
            return;
        }

        currentPageIndex = 0;
        currentPageSlot = 0;
        // PageA에 첫 페이지 데이터 채움
        ApplySceneDataToPage(0, pagedSceneData[0]);

        // 위치 조정 (PageA만 화면에 보이게, PageB는 바깥으로 이동)
        pageA.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -193);
        pageB.GetComponent<RectTransform>().anchoredPosition = new Vector2(1920, -193);

        // 네비게이션/버튼 상태 초기화
        UpdateNavigationIndicator(0);
        prevButton.interactable = false;
        nextButton.interactable = (pagedSceneData.Count > 1);

        prevButton.onClick.AddListener(() => SwitchPage(false));
        nextButton.onClick.AddListener(() => SwitchPage(true));

        UpdateNavigationControls();
    }

    void UpdateNavigationIndicator(int activePageIndex)
    {
        for (int i = 0; i < pageNavi.transform.childCount; i++)
        {
            Transform offSlot = pageNavi.transform.GetChild(i);
            Transform onIndicator = offSlot.Find("On");

            if (onIndicator != null)
                onIndicator.gameObject.SetActive(i == activePageIndex);
        }
    }
    private void UpdateNavigationControls()
    {
        int pageCount = pagedSceneData.Count;

        // 페이지 수가 1개 미만이면 모두 숨김
        bool enableNav = pageCount > 1;

        prevButton.interactable = enableNav && currentPageIndex > 0;
        nextButton.interactable = enableNav && currentPageIndex < pageCount - 1;

        if (pageNavi != null)
        {
            pageNavi.gameObject.SetActive(enableNav);
            prevButton.gameObject.SetActive(enableNav);
            nextButton.gameObject.SetActive(enableNav);
        }
    }

    public void SwitchPage(bool goingNext)
    {
        if (isSliding) return; // 애니메이션 중이면 무시

        if (goingNext && currentPageIndex >= pagedSceneData.Count - 1) return;
        if (!goingNext && currentPageIndex <= 0) return;

        isSliding = true; // 슬라이드 시작

        int nextIndex = currentPageIndex + (goingNext ? 1 : -1);
        int nextSlot = 1 - currentPageSlot;

        var currentPage = currentPageSlot == 0 ? pageA : pageB;
        var nextPage = nextSlot == 0 ? pageA : pageB;

        ApplySceneDataToPage(nextSlot, pagedSceneData[nextIndex]);

        var currentRect = currentPage.GetComponent<RectTransform>();
        var nextRect = nextPage.GetComponent<RectTransform>();

        nextRect.anchoredPosition = goingNext ? pageRightOffscreen : pageLeftOffscreen;

        Vector2 outTarget = goingNext ? pageLeftOffscreen : pageRightOffscreen;
        Vector2 inTarget = pageVisiblePos;

        currentRect.DOAnchorPos(outTarget, 0.4f).SetEase(Ease.InOutQuad);
        nextRect.DOAnchorPos(inTarget, 0.4f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            currentPageIndex = nextIndex;
            currentPageSlot = nextSlot;

            UpdateNavigationIndicator(currentPageIndex);
            prevButton.interactable = (currentPageIndex > 0);
            nextButton.interactable = (currentPageIndex < pagedSceneData.Count - 1);

            isSliding = false; // 애니메이션 종료 후 잠금 해제
        });
    }

    public void FilterAndPaginateContent(ContentCategory category)
    {
        var allScenes = XmlManager.Instance.SceneSettings.Values;

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

    void ApplySceneDataToPage(int slotIndex, List<XmlManager.SceneData> sceneList)
    {
        var buttonObjs = slotIndex == 0 ? pageAButtons : pageBButtons;

        for (int i = 0; i < buttonObjs.Length; i++)
        {


            GameObject buttonObj = buttonObjs[i];


            if (i >= sceneList.Count)
            {
                buttonObj.SetActive(false);
                continue;
            }

            var data = sceneList[i];
            buttonObj.SetActive(true); // 반드시 먼저 켜줘야 썸네일이 반영됨

            // 썸네일
            var mask = buttonObj.transform.Find("PictureFrmae_(Mask)");
            if (mask != null && mask.childCount > 0)
            {
                var img = mask.GetChild(0).GetComponent<Image>();
                if (img != null)
                    img.sprite = Resources.Load<Sprite>($"Common/Launcher_ThumbnailImage/{data.Id}");
            }

            // 텍스트
            var textObj = buttonObj.transform.GetChild(2);
            if (textObj != null)
            {
                var text = textObj.GetComponent<Text>();
                if (text != null)
                {
                    if (sceneTitles.TryGetValue(data.Id, out string title))
                        text.text = title;
                    else
                        text.text = data.Id; // fallback: ID 그대로 표시
                }
            }

            // 자물쇠
            var lockFrame = buttonObj.transform.Find("LockFrame");
            if (lockFrame != null)
                lockFrame.gameObject.SetActive(!data.IsActive && !_isTestScene);

            // 버튼 이벤트
            var btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = (data.IsActive || _isTestScene);
                btn.onClick.RemoveAllListeners();
                if (data.IsActive  || _isTestScene)
                {
                    //버튼 관련 효과관련 기능 추가, Active 일때만 동작 -민석 250619
                    var btnImageController = Utils.GetOrAddComponent<CursorImageController>(buttonObj);
                    btnImageController.DefaultScale = buttonObj.transform.localScale;

                    string sceneId = data.Id;
                    btn.onClick.AddListener(() =>
                    {

                        //버튼 씬 실행 기능 수정, 컨펌UI 표출 및 실행 -민석 250619
                        if (_metaEduLauncherMaster == null)
                            _metaEduLauncherMaster = Managers.UI.SceneUI.GetComponent<UI_MetaEduLauncherMaster>();

                        if (sceneTitles.TryGetValue(data.Id, out string title)) _metaEduLauncherMaster.OnSceneBtnClicked(sceneId, title);

                        // SceneManager.LoadScene(sceneId);
                    });
                }
            }

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
