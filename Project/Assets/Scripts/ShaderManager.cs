using UnityEngine;

public class ShaderManager : MonoBehaviour
{
    [Header("Color Default Settings")] [Space(10f)]
    public float waitTime;

    public Material _animalMaterial;

    public Color startColor;
    public Color inPlayColor;
    public Color tempColor;


    [Space(30f)] [Header("Color Settings")] [Space(10f)]
    public float minBrightness;

    public float maxBrightness;
    public float brightnessIncreasingSpeed;
    private float brightness;
    private float elapsedTime;
    private float lerpProgress; // 추가된 변수


    private void Start()
    {
        tempColor = inPlayColor;

        _animalMaterial.SetColor("_emissionColor", startColor);
    }

    private void Update()
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