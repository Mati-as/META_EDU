using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StarryNight_GameManager : MonoBehaviour
{
    private StarryNight_GameManager _gameManager;

    public static bool isGameStarted { get; set; }
    public static event Action onGameStarted;
    public Volume vol;
    private Vignette vignette;
    private void Start()
    {
        if (_gameManager == null)
        {
            _gameManager = new StarryNight_GameManager();
        }

        Managers.soundManager.Play(SoundManager.Sound.Bgm, "Audio/Bgm/BA002",0.225f);

        Application.targetFrameRate = 60; 
        
               
        GameObject.Find("Intro Cam").TryGetComponent<Volume>(out vol);
            
        if (vol == null)
        {
            Debug.LogError("PostProcessVolume not assigned.");
            return;
        }

        Vignette vignette;
        if (vol.profile.TryGet<Vignette>(out vignette))
        {
            vignette = vol.profile.components.Find(x => x is Vignette) as Vignette;
        }
        else
        {
            Debug.LogError("Vignette not found in PostProcessVolume.");
        }

        vignette.intensity.value = 1;
        DOVirtual.Float(1, 0, 2.5f, val =>
        {
            vignette.intensity.value = val;
        });
    }


    private void OnIntroCamArrived()
    {
        onGameStarted?.Invoke();
    }

}
