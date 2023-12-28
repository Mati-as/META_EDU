using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TMP_Text fpsText; // Text ��� TMP_Text�� ���
    private float deltaTime;

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        var fps = 1.0f / deltaTime;
        fpsText.text =  string.Format("{0:0.0} fps", fps);
        SetResolution(1920, 1080, 30);
    }
    
    private void SetResolution(int width, int height, int targetFrame)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrame;
    }
}