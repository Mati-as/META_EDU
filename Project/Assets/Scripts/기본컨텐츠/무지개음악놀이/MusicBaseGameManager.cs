using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MusicBaseGameManager : Base_GameManager
{

    
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        pre
        GameManager_Hits = Physics.RaycastAll(GameManager_Ray);
#if UNITY_EDITOR
       // Debug.Log("eventAfterAGetRay Invoke");
#endif
    }
}
