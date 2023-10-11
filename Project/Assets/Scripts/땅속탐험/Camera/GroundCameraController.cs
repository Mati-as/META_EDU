using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class GroundCameraController : MonoBehaviour
{

    enum CameraState
    {
        Default,
        Story,
        InPlayStart,
        StageStart,
        StageFinished,
        GameFinished,
        GamePaused
    }
    
    [Header("References")] 
    [SerializeField] 
    private GroundGameManager gameManager;
    
    [Space(20f)] [Header("Paramters")]
    [NamedArrayAttribute(new string[]
    {
        "Default", "Story", "InPlayStart","StageFinished", "GameFinished","StageStart, StageFinished"
        ,"GamePaused"
    })]
    public int[] cameraMovingTime = new int[7];
    [Space(20f)] [Header("Positions")]
    
    [NamedArrayAttribute(new string[]
    {
        "Default", "Story", "InPlayStart","StageFinished", "GameFinished","StageStart, StageFinished"
        ,"GamePaused"
    })]
    public Transform[] cameraPositions;
    
    void Start()
    {
        transform.position = cameraPositions[(int)CameraState.Default].position;

        
        gameManager.currentStateRP
            .Where(currentState => currentState.GameState == IState.GameStateList.GameStart)
            .Subscribe(_ => MoveCamera(
                cameraPositions[(int)CameraState.Story],
                cameraMovingTime[(int)CameraState.Story]));
        
        gameManager.currentStateRP
            .Where(currentState => currentState.GameState == IState.GameStateList.StageStart)
            .Subscribe(_ => MoveCamera(
                cameraPositions[(int)CameraState.InPlayStart],
                cameraMovingTime[(int)CameraState.InPlayStart]));
        
    }

    private void MoveCamera(Transform target,float duration)
    {
        Debug.Log(" tutorial 로 카메라 위치 이동");
        transform.DOMove(target.position, duration);
    }
}

public class NamedArrayAttribute : PropertyAttribute
{
    public readonly string[] names;
    public NamedArrayAttribute(string[] names) { this.names = names; }
}

[CustomPropertyDrawer(typeof(NamedArrayAttribute))]
public class NamedArrayDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        try
        {
            int pos = int.Parse(property.propertyPath.Split('[', ']')[1]);
            EditorGUI.PropertyField(rect, property, new GUIContent(((NamedArrayAttribute)attribute).names[pos]));
        }
        catch
        {
            EditorGUI.PropertyField(rect, property, label);
        }
    }
}