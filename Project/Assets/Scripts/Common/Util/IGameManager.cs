using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class IGameManager : MonoBehaviour
{
    public static Ray GameManager_Ray { get; private set; }


    /* onRaySynce
        1. EffectManager ray와 동기화 (필수)
             EffectMaager 내부에서 처리할 로직처리
        2. 나머지 게임로직 처리..
     */
    public static event Action On_GmRay_Synced;


    protected virtual void Start()
    {
        PlayNarration();
    }
    
    
    protected abstract void Init();
    

    private void OnDestroy()
    {
  
        RaySynchronizer.OnGetInputFromUser -= OnClicked;
        On_GmRay_Synced -= OnRaySynced;
    }


    protected void OnClicked()
    {
        GameManager_Ray = RaySynchronizer.ray_ImageMove;
        
#if UNITY_EDITOR
     
#endif
        On_GmRay_Synced?.Invoke();
    }

    protected abstract void OnRaySynced();
    
    protected void BindEvent()
    {
#if UNITY_EDITOR
        Debug.Log("Ray Sync 구독완료");
#endif
        RaySynchronizer.OnGetInputFromUser -= OnClicked;
        RaySynchronizer.OnGetInputFromUser += OnClicked;

        On_GmRay_Synced -= OnRaySynced;
        On_GmRay_Synced += OnRaySynced;

    }

    protected virtual void PlayNarration()
    {
#if UNITY_EDITOR
        Debug.Log($"(나레이션) Narration playing.. path : Audio/Narration/{SceneManager.GetActiveScene().name}");
#endif
        DOVirtual.Float(0, 1, 2f, _ => { })
            .OnComplete(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration,
                    "Audio/Narration/" + $"{SceneManager.GetActiveScene().name}", 0.5f);
            });

        Managers.Sound.Play(SoundManager.Sound.Bgm, "Audio/Bgm/" + $"{SceneManager.GetActiveScene().name}", 0.115f);
    }
}