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
        Image_Move.OnGetInputFromUser -= OnClicked;
    }
    
    protected virtual void BindEvent()
    {
        Image_Move.OnGetInputFromUser -= OnClicked;
        Image_Move.OnGetInputFromUser += OnClicked;
    }
    

}
