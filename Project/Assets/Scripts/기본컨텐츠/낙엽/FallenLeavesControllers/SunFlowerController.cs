using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class SunFlowerController : MonoBehaviour
{
    private readonly int SUNFLOWER_DIRECTION_SHADER_ID = Shader.PropertyToID("_WindDirection");
    private readonly int TOTAL_WIND_AMOUNT = Shader.PropertyToID("_TotalWindAmount");

    [FormerlySerializedAs("_material")] [SerializeField]
    private  MeshRenderer _meshRenderer;
   
    private Vector2 _direction;

    private float windDirectionToProperty;
    // Update is called once per frame
    void Update()
    {
        _direction =  ConvertVector3ToVector2(ParticleEventController.randomDirection);
        windDirectionToProperty = ConvertVectorDirectionToMappedAngle(_direction);
        if (ParticleEventController.isWindBlowing)
        {
            SetShaderProperty(_meshRenderer);
        }
    }
    
    float MapValue(float value, float oldMin, float oldMax, float newMin, float newMax)
    {
        return Mathf.Lerp(newMin, newMax, Mathf.InverseLerp(oldMin, oldMax, value));
    }
    
    float ConvertVectorDirectionToMappedAngle(Vector2 v)
    {
       
        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        if(angle < 0)
        {
            angle += 360;  // 각도를 양수로 만들기
        }

        // 0 ~ 180 사이의 값을 0 ~ 0.5로 매핑
        return MapValue(angle, 0, 360, 0, 0.5f);
    }

    private Vector2 ConvertVector3ToVector2(Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.z);
    }


    private void SetShaderProperty(MeshRenderer meshRenderer)
    {
#if UNITY_EDITOR
        Debug.Log("해바라기 쉐이더 프로퍼티 변경");
        #endif
        meshRenderer.material.SetFloat(SUNFLOWER_DIRECTION_SHADER_ID,windDirectionToProperty);
        meshRenderer.material.SetFloat(TOTAL_WIND_AMOUNT,Random.Range(0.6f,1f));
    }
}

