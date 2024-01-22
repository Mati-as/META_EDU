using UnityEngine;
using System;

public abstract class Base_BasicGameManager : MonoBehaviour
{
    public Ray ray_GameManager { get; set; }
    
    public RaycastHit[] hits;
  
    
    private void Start()
    {
        BindEvent();
    }

    protected abstract void OnClicked();
    
    protected virtual void OnDestroy()
    {
        RaySynchronizer.OnGetInputFromUser -= OnClicked;
    }
    
    protected virtual void BindEvent()
    {
        RaySynchronizer.OnGetInputFromUser -= OnClicked;
        RaySynchronizer.OnGetInputFromUser += OnClicked;
    }
    

}
