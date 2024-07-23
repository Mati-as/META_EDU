using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


public class Playground_GameManager : IGameManager
{
    private RaycastHit[] _hits;
  //  private Rigidbody _currentRigidBody;

    public float forceAmount;
    public float upOffset;

    
    /// <summary>
    /// 중복클릭으로인한 addforce 중복 영향 방지로직
    /// </summary>
    private Dictionary<int, bool> isBallClickable;

    private WaitForSeconds _delay = new WaitForSeconds(0.22f);
    

    protected override void Init()
    {
        DEFAULT_SENSITIVITY = 0.1f;
        SHADOW_MAX_DISTANCE = 80f;
        base.Init();
        isBallClickable = new Dictionary<int, bool>();
    }

    public override void OnRaySynced()
    {
        base.OnRaySynced();
        if (!isStartButtonClicked) return;

        _hits = Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in _hits)
        {
            if (hit.transform.gameObject.name == "Small" ||
                hit.transform.gameObject.name == "Medium" ||
                hit.transform.gameObject.name == "Large")
            {
                var ballId = hit.transform.GetInstanceID();
                
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/Playground/Ball", 0.3f);
                var rb = hit.collider.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    if (!isBallClickable.ContainsKey(ballId))
                    {
                        var forceDirection = rb.transform.position - hit.point + Vector3.up * upOffset;
                        rb.AddForce(forceDirection.normalized * forceAmount, ForceMode.Impulse);
                        isBallClickable.TryAdd(ballId, false);
                        
                    }
                    else if (isBallClickable[ballId])
                    {
                        var forceDirection = rb.transform.position - hit.point + Vector3.up * upOffset;
                        rb.AddForce(forceDirection.normalized * forceAmount, ForceMode.Impulse);
                    }
                    else
                    {
#if UNITY_EDITOR
                        Debug.Log("ball is not clickable");
#endif  
                    
                    }

                    StartCoroutine(ResetClickableAfterDelay(ballId)
                    );
                }
            }
        }
    }

    private IEnumerator ResetClickableAfterDelay(int ballId)
    {
        isBallClickable[ballId] = false;
        yield return _delay;
        isBallClickable[ballId] = true;
#if UNITY_EDITOR
        Debug.Log($"now ball clickable {ballId}");
#endif  

    }
}
