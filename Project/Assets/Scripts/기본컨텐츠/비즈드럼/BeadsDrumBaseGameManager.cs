using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;

public class BeadsDrumBaseGameManager : Base_GameManager
{
    [SerializeField]
    [Range(0,10)]
    public float clickRadius;

    public static event Action OnLeftDrumClicked;
    public static event Action OnRightDrumClicked;
    
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        if (Physics.Raycast(GameManager_Ray, out GameManager_Hits[0]))
        {
            Collider[] hitColliders = Physics.OverlapSphere(GameManager_Hits[0].point, clickRadius);
            foreach (var hitCollider in hitColliders)
            {
                IBeadOnClicked clickable = hitCollider.GetComponent<IBeadOnClicked>();
                if (clickable != null)
                {
                    clickable.OnClicked();
                }
            }
            
            
            foreach (var hit in GameManager_Hits)
            {
                if (hit.transform.gameObject.name.Contains("DrumLeft"))
                {
                    var randomChar = (char)Random.Range('A', 'B' + 1);
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/비즈드럼/Drum"+randomChar);
                    OnLeftDrumClicked?.Invoke();
                    return;
                }
                if (hit.transform.gameObject.name.Contains("DrumRight"))
                {
                    var randomChar = (char)Random.Range('A', 'B' + 1);
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/비즈드럼/Drum"+randomChar);
                    OnRightDrumClicked?.Invoke();
                    return;
                }
                
                if (hit.transform.gameObject.name.Contains("Frame"))
                {
                    var randomChar = (char)Random.Range('A', 'C' + 1);
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/비즈드럼/Plastic" + randomChar);
                    return;
                }
                
              

            }
            
            if (GameManager_Hits[0].transform.gameObject.name.Contains("Background"))
            {
                var randomChar = (char)Random.Range('A', 'D' + 1);
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/비즈드럼/Bubble"+randomChar,0.25f);
                  
            }
        }
        
    }
}
