using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TESTSCRIPT : MonoBehaviour
{
    private float timer = 0f;
    void Update()
    {
        if (timer < 0.5f)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0f;
            Destroy_obj();
        }
    }

    void Destroy_obj()
    {
        Destroy(this.gameObject);
    }
}
