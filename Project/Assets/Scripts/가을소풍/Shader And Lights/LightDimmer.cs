using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

public class LightDimmer : MonoBehaviour
{
    [Header("RenderSettings: Ambient")] [Space(10f)]
    public float defaultAmbient;
    public float minAmbient;
    private float _currentAmbient;
    [Header("Directional Light Intensitiy")] [Space(10f)]
    public float defaultIntensity;
    public float decreaseRate;
    public float increaseRate;// Intensity 감소 비율
    public float minIntensity; // 최소 Intensity 값
    public float lightTurningOffTime;
    [Header("Spotlight")] [Space(10f)]
    [SerializeField] 
    private Light spotlight;
    [SerializeField] 
    private Light spotlightDownLeft;
    [SerializeField] 
    private Light spotlightDownRight;
    public float spotMaxIntensity;
    public float spotMaxIntensityDownLeft;
    public float spotMaxIntensityDownRight;
    public float spotMinIntensity;
    private Light dirLight; // Directional Light의 Light 컴포넌트
    private Coroutine lightCoroutineA;
    private Coroutine lightCoroutineB;
    private Coroutine lightCoroutineC;
    private bool _isInitialized;
    private Coroutine _increaseAmbientAndLightIntensityCoroutine;
    private float currentIntensity;
    private float tempLerp;
    private float elapsedForLight;
    
    private Dictionary<float, WaitForSeconds> waitForSecondsCache = new Dictionary<float, WaitForSeconds>();

    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds))
        {
            waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        }

        return waitForSecondsCache[seconds];
    }

    private void StopCoroutineWithNullCheck()
    {
        if (_increaseAmbientAndLightIntensityCoroutine != null &&
            _decreaseAmbientAndLightIntensityCoroutine != null)
        {
            StopCoroutine(_increaseAmbientAndLightIntensityCoroutine);
            StopCoroutine(_decreaseAmbientAndLightIntensityCoroutine);
        }
    }


    //Unity Loop
    private void Awake()
    {

        StopCoroutineWithNullCheck();
        dirLight = GetComponent<Light>();
        _currentAmbient = defaultAmbient;
        dirLight.intensity = defaultIntensity;
    }

 

    private void Start()
    {
        SubscribeGameManagerEvents();
      
        RenderSettings.ambientIntensity = defaultAmbient;
        if (dirLight == null)
            Debug.LogError(
                "No Light component found on this object. Please attach this script to a Directional Light.");
        
        
        InitializeAndOffLight();
    }
  

    
    
    //Methods
    private void InitializeAndOffLight()
    {
        spotlight.intensity = 0f; 
        spotlightDownRight.intensity = 0f; 
        spotlightDownLeft.intensity = 0f; 
        
        spotlight.enabled = false;
        spotlightDownRight.enabled = false;
        spotlightDownLeft.enabled = false;
    
    }
    

    private Coroutine _decreaseAmbientAndLightIntensityCoroutine;

    private IEnumerator IncreaseAmbientAndLightIntensity()
    {
        while (true)
        {
            dirLight.intensity += increaseRate * Time.deltaTime; // Intensity 감소
            dirLight.intensity = Mathf.Min(dirLight.intensity, defaultIntensity); // Intensity 값이 최소값보다 작아지지 않도록 보장
            
            _currentAmbient += increaseRate * Time.deltaTime;
            RenderSettings.ambientIntensity = Mathf.Min(defaultAmbient,  _currentAmbient);
        
         
            if (_currentAmbient > defaultAmbient)
            {
                StopCoroutine(_increaseAmbientAndLightIntensityCoroutine);
            }
            
            yield return null;
        }
    }
       
   
    private IEnumerator DecreaseAmbientAndLightIntensity()
    {

        currentIntensity = dirLight.intensity;
        while (true)
        {
            currentIntensity -= decreaseRate * Time.deltaTime; // Intensity 감소
            dirLight.intensity = currentIntensity;
            dirLight.intensity = Mathf.Max(dirLight.intensity, minIntensity); // Intensity 값이 최소값보다 작아지지 않도록 보장


            if (_currentAmbient > 0)
            {
                _currentAmbient -= decreaseRate * Time.deltaTime;
                RenderSettings.ambientIntensity = Mathf.Max(_currentAmbient,minAmbient );
            }
            
            if (currentIntensity < minIntensity)
            {
                Debug.Log($"Amibent 감소중지. 현재 amibient {_currentAmbient}");
                StopCoroutine(_decreaseAmbientAndLightIntensityCoroutine);
            }
            
            yield return null;
        }
      
    }
   
   
    
    [FormerlySerializedAs("lightChangeTime")] 
    public float lightChangingDuration;
    IEnumerator TurnOnSpotLight(Light light,float lightChangingDuration,float spotMaxIntensity)
    {
        
        elapsedForLight = 0f;
        bool isLightEnabled = false;
        
        while (true)
        {
            if (GameManager.isCorrected)
            {
                if (!isLightEnabled)
                {
                    light.enabled = true;
                    isLightEnabled = true;
                }
               
                elapsedForLight += Time.deltaTime;
                float t =  elapsedForLight /lightChangingDuration;
            
                // Intensity 감소
                float lerp = Lerp2D.EaseInBounce(light.intensity, spotMaxIntensity,t );
                light.intensity = lerp;
                // Intensity 값이 최소값보다 작아지지 않도록 보장
                yield return null;
                
                if (light.intensity >= spotMaxIntensity)
                {
                    Debug.Log("조명 코루틴 탈출 및 종료");
                    yield break;
                }
            }
            else
            {
                yield return null;
            }
          
            
        }
       
    }

    IEnumerator TurnOffSpotLight(Light light)
    {

        elapsedForLight = 0f;
        while (true)
        {
            if (GameManager.isRoundFinished)
            {
                light.enabled = true;
                elapsedForLight += Time.deltaTime;
        
      
        
                float t =  elapsedForLight / lightTurningOffTime;
            
                // Intensity 감소
          
                float lerp = Lerp2D.EaseInBounce(spotMaxIntensity, spotMinIntensity,t );
                light.intensity = lerp;
                // Intensity 값이 최소값보다 작아지지 않도록 보장
                yield return null;
            
                    StopCoroutine( TurnOffSpotLight(light));
                
            }
            else
            {
                yield return null;
            }
         
        }
        

    }

    private void OnApplicationQuit()
    {
        dirLight.intensity = defaultIntensity;
        RenderSettings.ambientIntensity = defaultAmbient;
    }
    
 
    

    private void OnDestroy()
    {
        dirLight.intensity = defaultIntensity;
        RenderSettings.ambientIntensity = defaultAmbient;
        UnsubscribeGamaManagerEvents();
    }
    
    // ---------------------------------------------------------

    private void OnGameStart()
    {
        _decreaseAmbientAndLightIntensityCoroutine =   StartCoroutine(DecreaseAmbientAndLightIntensity());
    }
    

    private void OnGameFinished()
    {
        _increaseAmbientAndLightIntensityCoroutine = StartCoroutine(IncreaseAmbientAndLightIntensity());
    }

    
    private void SubscribeGameManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onGameStartEvent += OnGameStart;
      

        GameManager.onCorrectedEvent -= TurnOnSpotLightEvent;
        GameManager.onCorrectedEvent += TurnOnSpotLightEvent;

        GameManager.onRoundFinishedEvent -= TurnOffSpotLightEvent;
        GameManager.onRoundFinishedEvent += TurnOffSpotLightEvent;

    
        GameManager.onGameFinishedEvent -= OnGameFinished;
        GameManager.onGameFinishedEvent += OnGameFinished;
    }
    
    private void UnsubscribeGamaManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        // GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onCorrectedEvent -= TurnOnSpotLightEvent;
        GameManager.onRoundFinishedEvent -= TurnOffSpotLightEvent;
        // GameManager.onRoundStartedEvent -= OnRoundStarted;
        GameManager.onGameFinishedEvent -= OnGameFinished;
    }

    public void TurnOffSpotLightEvent()
    {
        StopCoroutine(TurnOnSpotLight(spotlight,lightChangingDuration,spotMaxIntensity));
        StopCoroutine(TurnOnSpotLight(spotlightDownLeft,lightChangingDuration,spotMaxIntensityDownLeft));
        StopCoroutine(TurnOnSpotLight(spotlightDownRight,lightChangingDuration,spotMaxIntensityDownRight));
    
        lightCoroutineA = StartCoroutine(TurnOffSpotLight(spotlight));
        lightCoroutineB = StartCoroutine(TurnOffSpotLight(spotlightDownLeft));
        lightCoroutineC = StartCoroutine(TurnOffSpotLight(spotlightDownRight));
    }
    public void TurnOnSpotLightEvent()
    {
        StopCoroutine(TurnOffSpotLight(spotlight));
        StopCoroutine(TurnOffSpotLight(spotlightDownLeft));
        StopCoroutine(TurnOffSpotLight(spotlightDownRight));
        
        lightCoroutineA = StartCoroutine(TurnOnSpotLight(spotlight,lightChangingDuration,spotMaxIntensity));
        lightCoroutineB = StartCoroutine(TurnOnSpotLight(spotlightDownLeft,lightChangingDuration,spotMaxIntensityDownLeft));
        lightCoroutineC = StartCoroutine(TurnOnSpotLight(spotlightDownRight,lightChangingDuration,spotMaxIntensityDownRight));
    }
    
}