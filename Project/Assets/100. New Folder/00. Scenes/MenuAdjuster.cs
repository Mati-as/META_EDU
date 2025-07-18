using UnityEngine;
using UnityEngine.UI;

public class MenuAdjuster : MonoBehaviour
{
    public GameObject Toggle_Monthly;
    public Toggle[] toggles; // Jan ~ Dec 순서로 12개 연결
    public Button saveButton; // 저장 버튼 (선택적)

    private string[] monthKeys = new string[] {
        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    };

    void Start()
    {
        // 토글 배열 초기화
        toggles = Toggle_Monthly.GetComponentsInChildren<Toggle>();

        if (toggles.Length != 12)
        {
            Debug.LogError("ToggleMonthly 하위에 Toggle이 12개가 아님 확인 필요");
            return;
        }

        // 초기화
        InitToggleStates();

        // 이벤트 리스너 추가
        for (int i = 0; i < toggles.Length; i++)
        {
            int index = i; // 클로저 문제 방지
            toggles[i].onValueChanged.AddListener((value) =>
            {
                SaveSingleToggleToXml(index, value);
            });
        }

        // 저장 버튼 연결 (선택 사항)
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveToggleStatesToXml);
        }
    }

    void InitToggleStates()
    {
        var xml = XmlManager.Instance;

        for (int i = 0; i < toggles.Length; i++)
        {
            string key = monthKeys[i];
            toggles[i].isOn = xml.GetMenuSetting(key);
        }
    }

    void SaveToggleStatesToXml()
    {
        var xml = XmlManager.Instance;

        for (int i = 0; i < toggles.Length; i++)
        {
            string key = monthKeys[i];
            bool value = toggles[i].isOn;
            xml.SetMenuSetting(key, value);
        }

        Debug.Log("모든 토글 상태 저장 완료");
    }

    void SaveSingleToggleToXml(int index, bool value)
    {
        string key = monthKeys[index];
        XmlManager.Instance.SetMenuSetting(key, value);

        Debug.Log($"[{key}] 상태 저장됨: {value}");
    }
}