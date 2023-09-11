using UnityEngine;
using System;
public class PropsController : MonoBehaviour
{

    [SerializeField] private Collider _touchDownBoxController;
    
    

    private void Awake()
    {
        _touchDownBoxController.enabled = false;
        GameManager.isCorrectedEvent -= EnableCollder;
        GameManager.isCorrectedEvent += EnableCollder;
    }

    
    private void EnableCollder()
    {
        _touchDownBoxController.enabled = true;
    }

    private void OnDestroy()
    {
        GameManager.isCorrectedEvent -= EnableCollder;
    }
}
