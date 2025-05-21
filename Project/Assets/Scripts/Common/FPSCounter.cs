using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private TextMeshProUGUI _fpsText; // Text ��� TMP_Text�� ���
    private float deltaTime;

    private void Start()
    {

        _fpsText = GetComponent<TextMeshProUGUI>();

        //gameObject.SetActive(false);
     
        
    }
    private void Update()
    {

        _fpsText = GetComponent<TextMeshProUGUI>();
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        var fps = 1.0f / deltaTime;
       
        _fpsText.text =  string.Format("{0:0.0} fps", fps);

    }
    
 
}