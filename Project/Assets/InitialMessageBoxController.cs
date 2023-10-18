using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class InitialMessageBoxController : MonoBehaviour
{
    [SerializeField] 
    private GroundGameManager gameManager;
    
    
    private void Start()
    {
        transform.localScale = Vector3.zero;
        
        gameManager.currentStateRP
            .Where(CurrentState => CurrentState.GameState == IState.GameStateList.StageStart)
            .Delay(TimeSpan.FromSeconds(1f))
            .Subscribe(_=>
            {
                Debug.Log("Initial Message Out!");
                LeanTween.scale(gameObject, Vector3.one * 4f, 2)
                    .setEaseInOutBounce();
            });
        
        gameManager.currentStateRP
            .Where(CurrentState => CurrentState.GameState == IState.GameStateList.GameOver)
            .Subscribe(_=>
            {
                Debug.Log("첫번째 '밟자국을 밟아봐' 메세지 삭제");
                LeanTween.scale(gameObject, Vector3.zero, 1)
                    .setEaseInOutBounce();
            });
        
    }
}
