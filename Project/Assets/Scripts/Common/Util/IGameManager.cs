using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class IGameManager : MonoBehaviour
{
    public static Ray GameManager_Ray { get; private set; }
    public static RaycastHit[] GameManager_Hits { get; set; }
    public static event Action On_GmRay_Synced;
    private readonly int TARGET_FRAME = 30;

    protected virtual void Awake()
    {
        Init();
    }


    protected virtual void Init()
    {
        BindEvent();
        SetResolution(1920, 1080, TARGET_FRAME);
        PlayNarration();
    }


    private void OnDestroy()
    {
        RaySynchronizer.OnGetInputFromUser -= OnClicked;
        On_GmRay_Synced -= OnRaySynced;
    }


    protected void OnClicked()
    {
        GameManager_Ray = RaySynchronizer.ray_ImageMove;
        GameManager_Hits = Physics.RaycastAll(GameManager_Ray);

#if UNITY_EDITOR
        Debug.Log("On_GmRay_Synced Invoke!");
#endif
        On_GmRay_Synced?.Invoke();
    }


    /// <summary>
    ///     onRaySync 구현 포인트
    ///     1. EffectManager ray와 동기화 (필수)
    ///     EffectManager 내부에서 처리할 로직처리
    ///     2. 나머지 RaySync가 필요한 경우의 게임로직 처리..
    /// </summary>
    protected abstract void OnRaySynced();

    protected void BindEvent()
    {
#if UNITY_EDITOR
        Debug.Log("Ray Sync Subscribed");
#endif
        RaySynchronizer.OnGetInputFromUser -= OnClicked;
        RaySynchronizer.OnGetInputFromUser += OnClicked;

        On_GmRay_Synced -= OnRaySynced;
        On_GmRay_Synced += OnRaySynced;
    }

    protected virtual void PlayNarration()
    {
        //delay for narration.
        DOVirtual.Float(0, 1, 2f, _ => { })
            .OnComplete(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration,
                    "Audio/Narration/" + $"{SceneManager.GetActiveScene().name}", 0.5f);
            });

        Managers.Sound.Play(SoundManager.Sound.Bgm, "Audio/Bgm/" + $"{SceneManager.GetActiveScene().name}", 0.115f);
    }

    private void SetResolution(int width, int height, int targetFrame)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrame;

#if UNITY_EDITOR
        Debug.Log(
            $"Game Info: name: {SceneManager.GetActiveScene().name}, Frame Rate: {TARGET_FRAME}, vSync: {QualitySettings.vSyncCount}");
#endif
    }
}