using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class LightDimmer : MonoBehaviour
{
    public float defaultAmbient;
    public float minAmbient;
    private float _currentAmbient;
    public float defaultIntensity;
    public float decreaseRate;
    public float increaseRate;// Intensity 감소 비율
    public float minIntensity; // 최소 Intensity 값

    [SerializeField] private Light spotlight;
    private Light dirLight; // Directional Light의 Light 컴포넌트


    private void Awake()
    {
        dirLight = GetComponent<Light>();

        spotlight.enabled = false;
        _currentAmbient = defaultAmbient;
        dirLight.intensity = defaultIntensity;
    }

    private void Start()
    {
        RenderSettings.ambientIntensity = defaultAmbient;
        if (dirLight == null)
            Debug.LogError(
                "No Light component found on this object. Please attach this script to a Directional Light.");
    }

  
   
    private Coroutine lightCoroutine;
    private bool _isInitialized;
    private void Update()
    {
        if (GameManager.isGameStarted)
        {
            dirLight.intensity -= decreaseRate * Time.deltaTime; // Intensity 감소
            dirLight.intensity = Mathf.Max(dirLight.intensity, minIntensity); // Intensity 값이 최소값보다 작아지지 않도록 보장

            if (_currentAmbient > 0)
            {
                _currentAmbient -= decreaseRate * Time.deltaTime;
                RenderSettings.ambientIntensity = Mathf.Max(_currentAmbient,minAmbient );
            }
            
           
        }
        

        else if (GameManager.isGameFinished)
        {
            
            dirLight.intensity += increaseRate * Time.deltaTime; // Intensity 감소
            dirLight.intensity = Mathf.Min(dirLight.intensity, defaultIntensity); // Intensity 값이 최소값보다 작아지지 않도록 보장

            
            _currentAmbient += increaseRate * Time.deltaTime;
            RenderSettings.ambientIntensity = Mathf.Min(defaultAmbient,  _currentAmbient);
        }
    }

    public float spotMaxIntensity;
    public float spotMinIntensity;
    public float spotLightChangeSpeed;
    private float tempLerp;

    private float elapsedForLight;
    public float lightChangeTime;
    IEnumerator  TurnOnSpotLight()
    {
        if (elapsedForLight < 1)
        {
            elapsedForLight += Time.deltaTime;
        }
        else
        {
            elapsedForLight = 1;
        }
        
        spotlight.enabled = true;
        elapsedForLight += Time.deltaTime;
        
        var t =  Mathf.Min(1,Mathf.Clamp01(elapsedForLight / lightChangeTime));
            
              // Intensity 감소
          
            float lerp = Lerp2D.EaseInBounce(spotlight.intensity, spotMaxIntensity,t );
            spotlight.intensity = lerp;
            // Intensity 값이 최소값보다 작아지지 않도록 보장
            yield return null;

    }

    IEnumerator  TurnOffSpotLight()
    {
        spotlight.enabled = true;
        if (elapsedForLight > 0)
        {
            elapsedForLight += Time.deltaTime;
        }
        else
        {
            elapsedForLight = 0;
        }
        
      
        
        var t =  Mathf.Min(1,Mathf.Clamp01(elapsedForLight / lightChangeTime));
            
        // Intensity 감소
          
        float lerp = Lerp2D.EaseInBounce(spotMinIntensity, spotlight.intensity,t );
        spotlight.intensity = lerp;
        // Intensity 값이 최소값보다 작아지지 않도록 보장
        yield return null;

    }

    private void OnApplicationQuit()
    {
        dirLight.intensity = defaultIntensity;
        RenderSettings.ambientIntensity = defaultAmbient;
    }
    
    public void TurnOffSpotLightEvent()
    {
        StopCoroutine(TurnOnSpotLight());
        lightCoroutine = StartCoroutine(TurnOffSpotLight());
    }
    public void TurnOnSpotLightEvent()
    {
        StopCoroutine(TurnOffSpotLight());
        lightCoroutine = StartCoroutine(TurnOnSpotLight());
    }
    
 

   
    
}