using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Music_GameManager : Base_BasicGameManager
{
    public static event Action eventAfterAGetRay; 
    
    protected override void OnClicked()
    {
        hits = Physics.RaycastAll(ray_GameManager);
        eventAfterAGetRay?.Invoke();
        
    }
}
