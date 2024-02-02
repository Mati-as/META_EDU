using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Music_GameManager : IGameManager
{

    
    protected override void OnRaySynced()
    {
        base.OnRaySynced();
        GameManager_Hits = Physics.RaycastAll(GameManager_Ray);
#if UNITY_EDITOR
       // Debug.Log("eventAfterAGetRay Invoke");
#endif
    }
}
