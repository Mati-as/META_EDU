using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VideoContent_GameManager : MonoBehaviour
{

    private ParticleSystem[] particleSystems;
   
    private Camera _camera;
    private InputAction _mouseClickAction;
    private ParticleSystem[] _particles;
    private WaitForSeconds wait_;
    
    private Stack<ParticleSystem> particlePool = new Stack<ParticleSystem>();
    private void Awake()
    {
        wait_ = new WaitForSeconds(_returnWaitForSeconds);
        _camera = Camera.main;

        _mouseClickAction = new InputAction("MouseClick", binding: "<Mouse>/leftButton", interactions: "press");
        _mouseClickAction.performed += OnMouseClick;

        _particles = GetComponentsInChildren<ParticleSystem>();
        
        foreach (var ps in _particles)
        {
            particlePool.Push(ps);
        }
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

    private RaycastHit _hit;
    private void OnMouseClick(InputAction.CallbackContext context)
    {
        var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(LAYER_NAME)))
        {

            PlayParticle(hit.point);
        }
    }

    private int _count;
    private void PlayParticle(Vector3 position)
    {
        if (particlePool.Count >= 6)
        {
            for(int i =0;i<5;i++)
            {
                ParticleSystem ps = particlePool.Pop(); 
                ps.transform.position = position;
                ps.gameObject.SetActive(true);
                ps.Play();
                
                StartCoroutine(ReturnToPoolAfterDelay(ps, ps.main.duration));
            }
        }
        else
        {
            _count = 0; 
            foreach (var ps in _particles)
            {
                GrowPool(_particles[_count],1);
                _count++;
            }
      
            for(int i =0;i<5;i++)
            {
                ParticleSystem ps = particlePool.Pop(); 
                ps.transform.position = position;
                ps.gameObject.SetActive(true);
                ps.Play();
                
                StartCoroutine(ReturnToPoolAfterDelay(ps, ps.main.duration));
            }
            Debug.LogWarning("No particles available in pool!");
        }
    }

    private float _returnWaitForSeconds = 5f;
    private IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, float delay)
    {
        yield return wait_;
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        particlePool.Push(ps); // Return the particle system to the pool
    }
    
    
    private void GrowPool(ParticleSystem original, int count)
    {
        for (int i = 0; i < count; i++)
        {
            ParticleSystem newInstance = Instantiate(original);
            newInstance.gameObject.SetActive(false);
            particlePool.Push(newInstance);
        }
    }
}
