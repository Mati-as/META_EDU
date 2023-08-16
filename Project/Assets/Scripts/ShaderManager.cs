using UnityEngine;

public class ShaderManager : MonoBehaviour
{
    [Header("Color Default Settings")]
    [Space(10f)]
    public float waitTime;
    private float elapsedTime;
    public Material _animalMaterial;
    
    public Color startColor;
    public Color inPlayColor;
    public Color tempColor;


    [Space(30f)]

    [Header("Color Settings")]
    [Space(10f)]

    public float minBrightness;
    public float maxBrightness;
    public float brightnessIncreasingSpeed;
    private float brightness;
    private float lerpProgress = 0f; // 추가된 변수

    


    void Start()
    {
        tempColor = inPlayColor;
        startColor *= 0;
        _animalMaterial.SetColor("_emissionColor", startColor);
    }

    void Update()
    {
        if (GameManager.IsGameStarted)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime > waitTime)
            {
                lerpProgress += Time.deltaTime * brightnessIncreasingSpeed; // 진행도 업데이트
                brightness = Mathf.Lerp(minBrightness, maxBrightness, lerpProgress);

                tempColor = inPlayColor * brightness;
                _animalMaterial.SetColor("_emissionColor", tempColor);
            }
        }
    }
}
