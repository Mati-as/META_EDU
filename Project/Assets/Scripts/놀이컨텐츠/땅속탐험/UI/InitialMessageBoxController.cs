using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UniRx;
using UnityEngine.Serialization;

public class InitialMessageBoxController : MonoBehaviour
{
    [FormerlySerializedAs("gameManager")] [SerializeField] 
    private GroundGameController gameController;

    public float maximizedSize;
    
    private void Start()
    {
        transform.localScale = Vector3.zero;
        
        gameController.currentStateRP
            .Where(CurrentState => CurrentState.GameState == IState.GameStateList.StageStart)
            .Delay(TimeSpan.FromSeconds(1f))
            .Subscribe(_=>
            {
                Debug.Log("Initial Message Out!");
                transform.DOScale(Vector3.one * maximizedSize, 2f)
                    .SetEase(Ease.InOutBounce);
                // {
                //     
                // }
                // LeanTween.scale(gameObject, Vector3.one * maximizedSize, 2)
                //     .setEaseInOutBounce();
            });
        
        gameController.currentStateRP
            .Where(CurrentState => CurrentState.GameState == IState.GameStateList.GameOver)
            .Subscribe(_=>
            {
                Debug.Log("Initial Message Out!");
                transform.DOScale(Vector3.zero, 0.5f)
                    .SetEase(Ease.InOutBounce);
            });
        
    }
}
