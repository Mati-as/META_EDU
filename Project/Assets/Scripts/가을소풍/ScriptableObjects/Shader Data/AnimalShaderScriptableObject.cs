
using UnityEngine;

namespace 가을소풍
{
    [CreateAssetMenu(fileName = "AnimalShaderScriptableObject", menuName = "ScriptableObjects/AnimalShaderScriptableObject", order = int.MaxValue)]
    public class AnimalShaderScriptableObject : ScriptableObject
    {

        [Header("Fresnel Setting")]    [Space(20)]
        [SerializeField]  public float fresnelSpeed;
        [SerializeField] public float minFresnelPower;
        [SerializeField] public float maxFresnelPower;

    
        [Header("Outline Setting")]    [Space(20)]
        [SerializeField] public float colorIntensityRange; //0~2
        [SerializeField] public float waitTimeForTurningOnGlow;
        [Space(10)]
        [SerializeField] public float outlineTurningOnSpeed;
        
        
    }
}
