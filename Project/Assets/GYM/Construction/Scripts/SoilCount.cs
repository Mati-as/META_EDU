using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;

public class SoilCount : MonoBehaviour
{
    Construction_GameManager manager;
    public int soilCount;

    public int inputCount = 0;
    public int maxInputs = 21;

    private Vector3 startScale = new Vector3(15f, 14f, 14f);
    private Vector3 endScale = new Vector3(-0.1f, -0.1f, -0.1f);


    private void Start()
    {
        manager = FindObjectOfType<Construction_GameManager>();
    }

    public void SoilDecreaseStep()
    {
        if (inputCount < maxInputs)
        {
            inputCount++;

            float t = inputCount / (float)maxInputs;
            Vector3 newScale = Vector3.Lerp(startScale, endScale, t);
            manager.ExcavatorStageSoil.transform.localScale = newScale;
        }
    }

}
   

   
      