using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class LightDimmer : MonoBehaviour
{

    private Light dirLight; // Directional Light의 Light 컴포넌트
    public float decreaseRate = 0.1f; // Intensity 감소 비율
    public float minIntensity = 0.04f; // 최소 Intensity 값


    void Start()
    {
        dirLight = GetComponent<Light>(); // 해당 게임 오브젝트의 Light 컴포넌트 가져오기

        if (dirLight == null)
        {
            dirLight.intensity = 1f;
            Debug.LogError("No Light component found on this object. Please attach this script to a Directional Light.");
        }
    }

    void Update()
    {
        if (dirLight != null && dirLight.intensity > minIntensity && GameManager.IsGameStarted == true)
        {
            dirLight.intensity -= decreaseRate * Time.deltaTime; // Intensity 감소
            dirLight.intensity = Mathf.Max(dirLight.intensity, minIntensity); // Intensity 값이 최소값보다 작아지지 않도록 보장
        }
    }
}
