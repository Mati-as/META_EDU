using System;
using UnityEngine;

public class LightDimmer : MonoBehaviour
{
    public float defaultAmbient;
    public float defaultIntensity;
    public float decreaseRate; // Intensity 감소 비율
    public float minIntensity; // 최소 Intensity 값

    private Light dirLight; // Directional Light의 Light 컴포넌트


    private void Awake()
    {
        dirLight = GetComponent<Light>();
        
        dirLight.intensity = defaultIntensity;
    }

    private void Start()
    {
        RenderSettings.ambientIntensity = defaultAmbient;
        if (dirLight == null)
        {
            Debug.LogError(
                "No Light component found on this object. Please attach this script to a Directional Light.");
        }
    }

    private void Update()
    {
        if (GameManager.IsGameStarted)
        {
            
            dirLight.intensity -= decreaseRate * Time.deltaTime; // Intensity 감소
            dirLight.intensity = Mathf.Max(dirLight.intensity, minIntensity); // Intensity 값이 최소값보다 작아지지 않도록 보장
            
            defaultAmbient -= decreaseRate * Time.deltaTime;
            RenderSettings.ambientIntensity = Mathf.Max(defaultAmbient, minIntensity); 
        }
    }

    private void OnApplicationQuit()
    {
        dirLight.intensity = defaultIntensity;
        RenderSettings.ambientIntensity = defaultAmbient;
    }
}