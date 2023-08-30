using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class LightDimmer : MonoBehaviour
{
    public float defaultAmbient;
    public float defaultIntensity;
    public float decreaseRate; // Intensity 감소 비율
    public float minIntensity; // 최소 Intensity 값

    [SerializeField] private Light spotlight;
    private Light dirLight; // Directional Light의 Light 컴포넌트


    private void Awake()
    {
        dirLight = GetComponent<Light>();

        spotlight.enabled = false;
        dirLight.intensity = defaultIntensity;
    }

    private void Start()
    {
        RenderSettings.ambientIntensity = defaultAmbient;
        if (dirLight == null)
            Debug.LogError(
                "No Light component found on this object. Please attach this script to a Directional Light.");
    }

    private float elapsed;
    public float spotlightWaitTime;
    private Coroutine lightCoroutine;
    private void Update()
    {
        if (GameManager.isGameStarted)
        {
            
            elapsed += Time.deltaTime;
            if (elapsed > spotlightWaitTime) spotlight.enabled = true;

            dirLight.intensity -= decreaseRate * Time.deltaTime; // Intensity 감소
            dirLight.intensity = Mathf.Max(dirLight.intensity, minIntensity); // Intensity 값이 최소값보다 작아지지 않도록 보장

            defaultAmbient -= decreaseRate * Time.deltaTime;
            RenderSettings.ambientIntensity = Mathf.Max(defaultAmbient, minIntensity);



         
        }
    }

    public float spotMaxIntensity;
    public float spotMinIntensity;
    public float spotLightChangeSpeed;
    IEnumerator  TurnOnSpotLight()
    {
     
            spotlight.intensity += spotLightChangeSpeed * Time.deltaTime; // Intensity 감소
            spotlight.intensity = Mathf.Min(dirLight.intensity, spotMaxIntensity); // Intensity 값이 최소값보다 작아지지 않도록 보장
            yield return null;

    }

    IEnumerator  TurnOffSpotLight()
    {
        
            spotlight.intensity -= spotLightChangeSpeed * Time.deltaTime; // Intensity 증가
            spotlight.intensity = Mathf.Max(dirLight.intensity, spotMinIntensity); // Intensity 값이 최소값보다 작아지지 않도록 보장
            yield return null;

    }

    private void OnApplicationQuit()
    {
        dirLight.intensity = defaultIntensity;
        RenderSettings.ambientIntensity = defaultAmbient;
    }
    
    public void TurnOffSpotLightEvent()
    {
        StopCoroutine(TurnOffSpotLight());
        lightCoroutine = StartCoroutine(TurnOnSpotLight());
    }
    public void TurnOnSpotLightEvent()
    {
        StopCoroutine(TurnOnSpotLight());
        lightCoroutine = StartCoroutine(TurnOffSpotLight());
    }
    
 

   
    
}