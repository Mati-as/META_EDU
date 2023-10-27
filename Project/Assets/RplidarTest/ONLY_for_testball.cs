using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ONLY_for_testball : MonoBehaviour
{
    private float timer = 0f;
    public GameObject Text;
    // Start is called before the first frame update


    //=====0822
    private Vector3 Temp_position;
    //

    private Text tem_text;
    private RectTransform temp_anchoredd;

    void Start()
    {
        //Temp_position = Camera.main.WorldToScreenPoint(this.gameObject.GetComponent<Transform>().position);
        //Mouse.current.WarpCursorPosition(new Vector2(Temp_position.x, Temp_position.y));
        //Debug.Log(Temp_position);
        ////중요, 이렇게 할 경우 오브젝트의 위치에 마우스가 위치하는 것을 확인함
        tem_text = Text.GetComponent<Text>();
        temp_anchoredd = this.GetComponent<RectTransform>();

        Text.GetComponent<Text>().text = (this.GetComponent<RectTransform>().anchoredPosition.x).ToString()
            + "," + (this.GetComponent<RectTransform>().anchoredPosition.y).ToString();

    }

    // Update is called once per frame
    void Update()
    {
        tem_text.text = (temp_anchoredd.anchoredPosition.x).ToString()
            + "," + (temp_anchoredd.anchoredPosition.y).ToString();
    }

}
