using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class ShapePathfinding_DiceController : MonoBehaviour
{
    private bool _isSoundPlaying;
    private IGameManager _gameManager;

    private void Awake()
    {
        _gameManager = GameObject.FindWithTag("GameManager").GetComponent<IGameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.name == "Floor" && _gameManager.isStartButtonClicked)
        {
            if (!_isSoundPlaying)
            {
                _isSoundPlaying = true;
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BB010/Dice");

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
