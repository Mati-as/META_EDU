using UnityEngine;
using System;
public class PropsController : MonoBehaviour
{

    [SerializeField] private Collider _touchDownBoxController;
    
    

    private void Awake()
    {
        _touchDownBoxController.enabled = false;
        GameManager.onCorrectedEvent -= EnableCollder;
        GameManager.onCorrectedEvent += EnableCollder;
    }

    
    private void EnableCollder()
    {
        _touchDownBoxController.enabled = true;
    }

    private void OnDestroy()
    {
        GameManager.onCorrectedEvent -= EnableCollder;
    }
}
