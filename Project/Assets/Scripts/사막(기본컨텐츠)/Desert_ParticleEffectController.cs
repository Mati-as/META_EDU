using UnityEngine;
using UnityEngine.InputSystem;

public class Desert_ParticleEffectController : MonoBehaviour
{
    private Camera _camera;
    private InputAction _mouseClickAction;
    private ParticleSystem _particle;

    private void Awake()
    {
        _camera = Camera.main;

        //OnEnable,Disable 또한 반드시 가져갈 것..
        _mouseClickAction = new InputAction("MouseClick", binding: "<Mouse>/leftButton", interactions: "press");
        _mouseClickAction.performed += OnMouseClick;

        _particle = GetComponentInChildren<ParticleSystem>();
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    private void OnEnable()
    {
        _mouseClickAction.Enable();
    }   

    private void OnDisable()
    {
        _mouseClickAction.Disable();
    }

    private readonly string LAYER_NAME = "UI";

    private void OnMouseClick(InputAction.CallbackContext context)
    {
        var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;


        var layerMask = 1 << LayerMask.NameToLayer(LAYER_NAME);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
#if UNITY_EDITOR
            Debug.Log("Particle Effect");
#endif
            _particle.Stop();
            _particle.transform.position = hit.point;
            OnClicked();
        }
    }

    private void OnClicked()
    {
        _particle.Play();
    }
}
//Action 