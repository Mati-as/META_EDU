using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class Underground_PopUpUI_Button : MonoBehaviour
{
    [SerializeField] 
    private GroundGameManager gameManager;

    private Button _button;

    [SerializeField] private AudioSource uiAudioSource;
    public AudioClip buttonSound;

    public static event Action onPopUpButtonEvent; 

    void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);
        isClickable = true;
    }

    private bool isClickable;
    // Popup이벤트 action list
    void OnButtonClicked()
    {
        if (isClickable)
        {
            isClickable = false;
            
            onPopUpButtonEvent?.Invoke();
        
            // 단순 delay및 콜백설정용 DoVirtual..
            DOVirtual.Float(0, 1, 3f, val => val++)
                .OnComplete(() =>
                {
                    isClickable = true;
                });
            
            if (uiAudioSource != null)
            {
                uiAudioSource.clip = buttonSound;
                uiAudioSource.Play();
            }
        }
      

        
    }

}
