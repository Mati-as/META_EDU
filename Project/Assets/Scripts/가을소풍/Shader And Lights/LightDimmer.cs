using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class LightDimmer : MonoBehaviour
{
    public float defaultAmbient;
    public float minAmbient;
    private float _currentAmbient;
    public float defaultIntensity;
    public float decreaseRate;
    public float increaseRate;// Intensity 감소 비율
    public float minIntensity; // 최소 Intensity 값

    [FormerlySerializedAs("lightTurningOffTIme")] [FormerlySerializedAs("spotLightTurnOffSpeed")] public float lightTurningOffTime;
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
        SubscribeGameManagerEvents();
        
        
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
  
    private float tempLerp;

    private float elapsedForLight;
    
    public float lightChangeTime;
    IEnumerator  TurnOnSpotLight()
    {
        
        elapsedForLight = 0f;
        
        while (true)
        {
            if (GameManager.isCorrected)
            {
                elapsedForLight += Time.deltaTime;
           
        
                spotlight.enabled = true;
                elapsedForLight += Time.deltaTime;
        
                var t =  Mathf.Min(1,Mathf.Clamp01(elapsedForLight / lightChangeTime));
            
                // Intensity 감소
          
                float lerp = Lerp2D.EaseInBounce(spotlight.intensity, spotMaxIntensity,t );
                spotlight.intensity = lerp;
                // Intensity 값이 최소값보다 작아지지 않도록 보장
                yield return null;
            }
            else
            {
                yield return null;
            }
          
            
        }
       
    }

    IEnumerator  TurnOffSpotLight()
    {

        elapsedForLight = 0f;
        while (true)
        {
            if (GameManager.isRoundFinished)
            {
                spotlight.enabled = true;
                elapsedForLight += Time.deltaTime;
        
      
        
                var t =  Mathf.Min(1,Mathf.Clamp01(elapsedForLight / lightTurningOffTime));
            
                // Intensity 감소
          
                float lerp = Lerp2D.EaseInBounce(spotMaxIntensity, spotMinIntensity,t );
                spotlight.intensity = lerp;
                // Intensity 값이 최소값보다 작아지지 않도록 보장
                yield return null;
            
                    StopCoroutine( TurnOffSpotLight());
                
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
        UnsubscribeGamaManagerEvents();
    }
    
    // ---------------------------------------------------------

    private void OnGameStart()
    {
    }

    private void OnRoundReady()
    {
        
    }
    

    private void OnRoundStarted()
    {
       
    }

    private void OnCorrect()
    {
        StartCoroutine(TurnOnSpotLight());
    }

    private void OnRoundFinished()
    {
        StartCoroutine(TurnOffSpotLight());
    }

    private void OnGameFinished()
    {
       
    }

    
    private void SubscribeGameManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onGameStartEvent += OnGameStart;
        
        GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onRoundReadyEvent += OnRoundReady;

        GameManager.onCorrectedEvent -= TurnOnSpotLightEvent;
        GameManager.onCorrectedEvent += TurnOnSpotLightEvent;

        GameManager.onRoundFinishedEvent -= TurnOffSpotLightEvent;
        GameManager.onRoundFinishedEvent += TurnOffSpotLightEvent;

        GameManager.onRoundStartedEvent -= OnRoundStarted;
        GameManager.onRoundStartedEvent += OnRoundStarted;
        
        GameManager.onGameFinishedEvent -= OnGameFinished;
        GameManager.onGameFinishedEvent += OnGameFinished;
    }
    
    private void UnsubscribeGamaManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onCorrectedEvent -= TurnOnSpotLightEvent;
        GameManager.onRoundFinishedEvent -= TurnOffSpotLightEvent;
        GameManager.onRoundStartedEvent -= OnRoundStarted;
        GameManager.onGameFinishedEvent -= OnGameFinished;
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