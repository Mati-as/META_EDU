using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using MyCustomizedEditor;
#endif


public class GroundCameraController : MonoBehaviour
{

    enum CameraState
    {
        Default,
        Story,
        InPlayStart,
        StageStart,
        FirstPage,
        SecondPage,
        ThirdPage,
        FourthPage,
        StageFinished,
        GameFinished,
        GamePaused
    }

    public float pageChangeInterval;
    
    [FormerlySerializedAs("gameManager")]
    [Header("References")] 
    [SerializeField] 
    private GroundGameController gameController;

    [SerializeField] private FootstepManager footstepManager;
    
#if UNITY_EDITOR
    [Space(20f)] [Header("Paramters")]
    [NamedArrayAttribute(new string[]
    {
        "Default", "Story", "InPlayStart","StageStart",
        "FirstPage","SecondPage","ThirdPage","FourthPage",
        "StageFinished", "GameFinished","StageStart"
        ,"StageFinished","GamePaused"
    })]
#endif

    public float[] cameraMovingTime = new float[20];
    [Space(20f)] [Header("Positions")]
    
    
#if UNITY_EDITOR
    [NamedArrayAttribute(new string[]
    {
        "Default", "Story", "InPlayStart","StageStart",
        "FirstPage","SecondPage","ThirdPage","FourthPage",
        "StageFinished", "GameFinished","StageStart",
        "StageFinished","GamePaused"
    })]
#endif

    public Transform[] cameraPositions = new Transform[20];
    
    void Start()
    {
        transform.position = cameraPositions[(int)CameraState.Default].position;

        
        
        gameController.currentStateRP
            .Where(currentState => currentState.GameState == IState.GameStateList.GameStart)
            .Subscribe(_ =>
            {
                MoveCamera(
                    cameraPositions[(int)CameraState.Story],
                    cameraMovingTime[(int)CameraState.Story]);
            });
        
        
        
        gameController.currentStateRP
            .Where(currentState => currentState.GameState == IState.GameStateList.StageStart)
            .Subscribe(_ =>
            {
                MoveCamera(
                    cameraPositions[(int)CameraState.InPlayStart],
                    cameraMovingTime[(int)CameraState.InPlayStart]);
            });

        
        
        footstepManager.finishPageTriggerProperty
            .Where(finishpage => finishpage == true)
            .Delay(TimeSpan.FromSeconds(pageChangeInterval))
            .Subscribe(_ => 
            {
                int calculatedIndex = FootstepManager.currentFootstepGroupOrder / 3 + (int)CameraState.FirstPage;
        
                if (calculatedIndex >= cameraPositions.Length)
                {
                    Debug.LogError($"cameraPositions 배열의 범위를 초과했습니다. 인덱스: {calculatedIndex}, 배열 크기: {cameraPositions.Length}");
                    return;
                }
        
                if (cameraPositions[calculatedIndex] == null)
                {
                    Debug.LogError($"cameraPositions[{calculatedIndex}]는 null입니다.");
                    return;
                }

                MoveCamera(cameraPositions[calculatedIndex], cameraMovingTime[calculatedIndex]);
                footstepManager.finishPageTriggerProperty.Value = false;

                Debug.Log($"현재 페이지{(CameraState)(FootstepManager.currentFootstepGroupOrder / 3 + (int)CameraState.FirstPage)}");
            });
    }

    private void MoveCamera(Transform target,float duration)
    {
        transform.DOMove(target.position, duration);
    }
}


