
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "ShaderAndCommon", menuName = "ScriptableObjects/ShaderAndCommon", order = int.MaxValue)]
    public class ShaderAndCommon : ScriptableObject
    {

        [Header("Fresnel Setting")]    [Space(10)]
        [Range(0,2)] [SerializeField]  public float fresnelSpeed;
        [Range(0,10)][SerializeField] public float minFresnelPower;
        [Range(0,10)][SerializeField] public float maxFresnelPower;

    
        [Space(20)] [Header("Outline Setting")]    [Space(10)]
        [Range(0,2)][SerializeField] public float colorIntensityRange; //0~2
        [Range(0,20)][SerializeField] public float waitTimeForTurningOnGlow;
        [Space(10)]
        [Range(0,1)][SerializeField] public float outlineTurningOnSpeed;
        [Range(0,1)][SerializeField] public float outlineTurningOffSpeed;

        [Space(20)] [Header("Outline Color Setting")] [Space(10)]
        public Color RANODOM_COLOR_A;
        public Color RANODOM_COLOR_B;
        public Color RANODOM_COLOR_C;
     
        [FormerlySerializedAs("sizeIcreasingSpeed")]
        [Space(35)] [Header("Common Settings")] [Space(20)]
        [Range(0,2)]public float sizeIncreasingSpeed;
        [Range(0,2)]public float sizeDecreasingSpeed;

    }

