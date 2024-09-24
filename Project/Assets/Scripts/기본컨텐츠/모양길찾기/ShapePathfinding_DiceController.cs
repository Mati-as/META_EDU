using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class ShapePathfinding_DiceController : MonoBehaviour
{
    private bool _isSoundPlaying;
    private Base_GameManager _baseGameManager;

    private void Awake()
    {
        _baseGameManager = GameObject.FindWithTag("GameManager").GetComponent<Base_GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.name == "Floor" && _baseGameManager.isStartButtonClicked)
        {
            if (!_isSoundPlaying)
            {
                _isSoundPlaying = true;
                Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/BB010/Dice");

                var seq = DOTween.Sequence();
                seq.SetDelay(3.6f);
                seq.AppendCallback(() =>
                {
                    _isSoundPlaying = false;
                });
                seq.Play();
            }
        }
    }
}
