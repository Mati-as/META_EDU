using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDownPositionSaver : MonoBehaviour
{
    private void Awake()
    {
        AnimalData.TOUCH_DOWN_POSITION = transform;
    }
}
