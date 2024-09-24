
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

public class AutoClickerEditorTool : EditorWindow
{
    private float clickInterval; // default interval
    private bool isClicking = false;
    private float nextClickTime = 0f;
    private EventSystem eventSystem;

    [MenuItem("Tools/Auto Clicker")]
    public static void ShowWindow()
    {
        GetWindow<AutoClickerEditorTool>("Auto Clicker");
    }

    private RaySynchronizer _raySynchronizer;
    private Base_GameManager _baseGameManager;
    private void OnEnable()
    {
        _raySynchronizer = FindObjectOfType<RaySynchronizer>();
        _baseGameManager = FindObjectOfType<Base_GameManager>();
    }

    private void OnGUI()
    {
      
        GUILayout.Label("Auto Clicker Settings", EditorStyles.boldLabel);

        clickInterval = EditorGUILayout.FloatField("Click Interval (seconds)", clickInterval);

        if (GUILayout.Button(isClicking ? "Stop Clicking" : "Start Clicking"))
        {
            isClicking = !isClicking;
            nextClickTime = (float)EditorApplication.timeSinceStartup + clickInterval;
        }

       
           
        
    }

    private float _elpased;
    private void Update()
    {
        if (isClicking)
        {
            if (_raySynchronizer ==null || _baseGameManager == null)
            {
                _raySynchronizer = FindObjectOfType<RaySynchronizer>();
                _baseGameManager = FindObjectOfType<Base_GameManager>();
            }

            if (_elpased > clickInterval)
            {
                Debug.Log($"auto click working.... clickInterval: {clickInterval}, elapsed : {_elpased}");
                _elpased = 0;
                _raySynchronizer.ShootRay();
                return;
                //_gameManager.OnRaySynced();
            }
            else
            {
                _elpased += Time.deltaTime;
            }
          
           
        }
        
    }

    private void PerformClick()
    {
        if (eventSystem == null)
        {
            Debug.LogWarning("Event System not found!");
            return;
        }

        var mousePosition = new Vector2(Screen.width / 2, Screen.height / 2); // Use center of screen if Event.current is null
        if (Event.current != null)
        {
            mousePosition = Event.current.mousePosition;
        }

        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = mousePosition,
            button = PointerEventData.InputButton.Left
        };

        ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, pointerData, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, pointerData, ExecuteEvents.pointerUpHandler);
    }


    private void OnInspectorUpdate()
    {
        Repaint();
    }
}

#endif

