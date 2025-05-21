using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate360 : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0, 210 * Time.deltaTime, 0);
    }


}
