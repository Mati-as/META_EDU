
using UnityEngine;


    [CreateAssetMenu(fileName = "ShaderAndCommon", menuName = "ScriptableObjects/ShaderAndCommon", order = int.MaxValue)]
    public class ShaderAndCommon : ScriptableObject
    {

        [Header("Fresnel Setting")]    [Space(10)]
        [SerializeField]  public float fresnelSpeed;
        [SerializeField] public float minFresnelPower;
        [SerializeField] public float maxFresnelPower;

    
        [Space(20)] [Header("Outline Setting")]    [Space(10)]
        [SerializeField] public float colorIntensityRange; //0~2
        [SerializeField] public float waitTimeForTurningOnGlow;
        [Space(10)]
        [SerializeField] public float outlineTurningOnSpeed;

        [Space(35)] [Header("Common Settings")] [Space(20)]
        public float sizeIcreasingSpeed;

    }

