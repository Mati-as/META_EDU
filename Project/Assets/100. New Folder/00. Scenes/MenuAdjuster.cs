using UnityEngine;
using UnityEngine.UI;

public class MenuAdjuster : MonoBehaviour
{
    public GameObject Toggle_Monthly;
    public Toggle[] toggles; // Jan ~ Dec 순서로 12개 연결
    public Button saveButton;             // 저장 버튼 (인스펙터에 직접 연결)

    private string[] monthKeys = new string[]
    {
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

        // XML에서 초기값 읽어와 토글 설정
        InitToggleStates();

        // 저장 버튼 연결
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

        Debug.Log("메뉴 설정 저장");
    }

}