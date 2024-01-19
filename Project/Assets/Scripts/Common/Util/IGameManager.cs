using System;
using UnityEngine;

public abstract class IGameManager : MonoBehaviour
{
    public static Ray GameManager_Ray { get; private set; }


    /* onRaySynce
        1. EffectManager ray와 동기화 (필수)
             EffectMaager 내부에서 처리할 로직처리
        2. 나머지 게임로직 처리..
     */
    public static event Action On_GmRay_Synced;


    protected abstract void Init();
    

    private void OnDestroy()
    {
  
        Image_Move.OnGetInputFromUser -= OnClicked;
        On_GmRay_Synced -= OnRaySynced;
    }


    protected void OnClicked()
    {
        GameManager_Ray = Image_Move.ray_ImageMove;
        
#if UNITY_EDITOR
        
#endif
        On_GmRay_Synced?.Invoke();
    }

    protected abstract void OnRaySynced();
    
    protected void BindEvent()
    {
#if UNITY_EDITOR
        Debug.Log("구독완료");
#endif
        Image_Move.OnGetInputFromUser -= OnClicked;
        Image_Move.OnGetInputFromUser += OnClicked;

        On_GmRay_Synced -= OnRaySynced;
        On_GmRay_Synced += OnRaySynced;

    }
}