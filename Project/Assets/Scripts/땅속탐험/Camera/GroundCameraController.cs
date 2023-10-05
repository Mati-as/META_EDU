using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

public class GroundCameraController : MonoBehaviour
{

    enum CameraState
    {
        Default,
        Story,
        InPlayStart,
        GameFinished
    }
    [Header("References")] 
    [SerializeField] 
    private GroundGameManager gameManager;
    
    [Space(20f)] [Header("Paramters")]
    public float[] cameraMovingTime;
    
    [Space(20f)] [Header("Positions")]
    public Transform[] cameraPositions;
    
    void Start()
    {
        transform.position = cameraPositions[(int)CameraState.Default].position;

        
        gameManager.currentStateRP
            .Where(_ => gameManager.currentStateRP.Value.Gamestate == IState.GameStateList.GameStart)
            .Subscribe(_ => MoveCamera(
                cameraPositions[(int)CameraState.Story],
                cameraMovingTime[(int)CameraState.Story]));
        
    }

    private void MoveCamera(Transform target,float duration)
    {
        Debug.Log(" tutorial 로 카메라 위치 이동");
        transform.DOMove(target.position, duration);
    }
}
