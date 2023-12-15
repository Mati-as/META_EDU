using DG.Tweening;
using UnityEngine;

public class StarryNight_StarlightController : MonoBehaviour
{
    public Light[] spotLights;

    [Header("Color")] public Color[] colors;

    [Header("Duration")] public float duration;
    [Header("Interval")] [Range(0.5f, 20)] public float intervalMin;
    [Range(0.5f, 20)] public float intervalMax;

    private float _determinedInterval;

    [Space(15f)] [Header("Intensity")] public float defaultIntensity;

    [Range(0, 5)] public float intensityMin;
    [Range(0, 5)] public float intensityMax;

    private float _determinedIntensity;

    private void Start()
    {
        Init();
        DOTween.Init();

        _determinedIntensity = Random.Range(intensityMin, intensityMax);
        _determinedInterval = Random.Range(intervalMin, intervalMax);
        var count = 0;
        foreach (var light in spotLights)
        {
            DoVirtualIntensityIncrease(light, count);
            count++;
        }
    }

    private Tween[] tweens; 
    private void Init()
    {
        tweens = new Tween[spotLights.Length];
        var count = 0;
        foreach (var lit in spotLights)
        {
            lit.intensity = defaultIntensity;
            lit.color = colors[count];
            count++;
        }
    }

    private Tween _lightTween;

    private void DoVirtualIntensityIncrease(Light light, int tweenIndex)
    {
        _determinedIntensity = Random.Range(intensityMin, intensityMax);
        _determinedInterval = Random.Range(intervalMin, intervalMax);

        //Tween끼리 간섭하는걸 방지하기 위해, else부분에 offset 추가 
        if (!tweens[tweenIndex].IsActive())
        {
            tweens[tweenIndex] = DOVirtual.Float(defaultIntensity, _determinedIntensity, 0.6f
                    , value => light.intensity = value)
                .OnComplete(() => { DoVirtualIntensityDecrease(light, _determinedIntensity, duration,tweenIndex); })
                //DelayCall을 사용하는 방법도 있다. 
                .SetDelay(_determinedInterval);
        }
        else
        {
            tweens[tweenIndex] = DOVirtual.Float(defaultIntensity, _determinedIntensity, 0.6f
                    , value => light.intensity = value)
                .OnComplete(() => { DoVirtualIntensityDecrease(light, _determinedIntensity, duration,tweenIndex); })
                .SetDelay(_determinedInterval + 5f);
        }
        
    }


    private void DoVirtualIntensityDecrease(Light light, float intensity, float delay,int tweenIndex)
    {
        tweens[tweenIndex] = DOVirtual.Float(intensity, defaultIntensity, 0.8f
                    , value => light.intensity = value)
                .OnComplete(() => DoVirtualIntensityIncrease(light,tweenIndex)
                ).SetDelay(delay);
        
    }
}