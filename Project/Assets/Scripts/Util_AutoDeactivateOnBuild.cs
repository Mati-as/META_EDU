using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util_AutoDeactivateOnBuild : MonoBehaviour
{
#if UNITY_EDITOR
    private void Awake()
    {
        gameObject.SetActive(false);
    }

#else
    private void Awake()
    {
        gameObject.SetActive(false);
    }
#endif
}
