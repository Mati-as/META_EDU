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

    private void Update()
    {
        if (GameManager.IsGameStarted)
        {
            
            elapsed += Time.deltaTime;
            if (elapsed > spotlightWaitTime) spotlight.enabled = true;

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