
using UnityEngine;

namespace 가을소풍
{
    [CreateAssetMenu(fileName = "AnimalShaderScriptableObject", menuName = "Effects/AnimalShaderScriptableObject", order = 0)]
    public class AnimalShaderScriptableObject : ScriptableObject
    {

        [Header("Fresnel Setting")]    [Space(20)]
        [SerializeField]  public float fresnelSpeed;
        [SerializeField] public float minFresnelPower;
        [SerializeField] public float maxFresnelPower;

    
        [Header("Outline Intensity Setting")]    [Space(20)]
        [SerializeField] public float colorIntensityRange; //0~2
        [SerializeField] public float waitTimeForTurningOnGlow;
    }
}
