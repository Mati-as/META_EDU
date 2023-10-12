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
        FirstPage,
        SecondPage,
        ThirdPage,
        FourthPage,
        StageFinished,
        GameFinished,
        GamePaused
    }
    
    [Header("References")] 
    [SerializeField] 
    private GroundGameManager gameManager;

    [SerializeField] private FootstepManager footstepManager;
    
    [Space(20f)] [Header("Paramters")]
    [NamedArrayAttribute(new string[]
    {
        "Default", "Story", "InPlayStart","StageStart",
        "FirstPage","SecondPage","ThirdPage","FourthPage",
        "StageFinished", "GameFinished","StageStart"
        ,"StageFinished","GamePaused"
    })]
    public int[] cameraMovingTime = new int[20];
    [Space(20f)] [Header("Positions")]
    
    [NamedArrayAttribute(new string[]
    {
        "Default", "Story", "InPlayStart","StageStart",
        "FirstPage","SecondPage","ThirdPage","FourthPage",
        "StageFinished", "GameFinished","StageStart",
        "StageFinished","GamePaused"
    })]
    public Transform[] cameraPositions = new Transform[20];
    
    void Start()
    {
        transform.position = cameraPositions[(int)CameraState.Default].position;

        
        gameManager.currentStateRP
            .Where(currentState => currentState.GameState == IState.GameStateList.GameStart)
            .Subscribe(_ =>
            {
                MoveCamera(
                    cameraPositions[(int)CameraState.Story],
                    cameraMovingTime[(int)CameraState.Story]);
            });
        
        gameManager.currentStateRP
            .Where(currentState => currentState.GameState == IState.GameStateList.StageStart)
            .Subscribe(_ =>
            {
                MoveCamera(
                    cameraPositions[(int)CameraState.InPlayStart],
                    cameraMovingTime[(int)CameraState.InPlayStart]);
            });

        footstepManager.finishPageTriggerProperty
            .Where(finishpage => finishpage == true)
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