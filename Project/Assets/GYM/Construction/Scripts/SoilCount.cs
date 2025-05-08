using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;

public class SoilCount : MonoBehaviour
{
    public int soilCount;
    public int orisoilCount;

    [SerializeField] private List<GameObject> soils = new List<GameObject>(7);

    private void Start()
    {
      // orisoilCount = soilCount;

        for (int i = 0; i < 7; i++)
        {
            soils[i] = transform.GetChild(i).gameObject;

        }

    }

    private void OnEnable()
    {
        soilCount = orisoilCount;
    }

    public void leftSoilCount()
    {
        
        int indexToDisable = (orisoilCount - soilCount) / 3;

        for (int i = 0; i < soils.Count; i++)
        {
            soils[i].SetActive(i < (7 - indexToDisable));
        }
    }

}
