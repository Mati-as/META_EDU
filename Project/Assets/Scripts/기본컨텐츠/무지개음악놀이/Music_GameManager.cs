using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Music_GameManager : IGameManager
{

    
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        GameManager_Hits = Physics.RaycastAll(GameManager_Ray);
#if UNITY_EDITOR
       // Debug.Log("eventAfterAGetRay Invoke");
#endif
    }
}
