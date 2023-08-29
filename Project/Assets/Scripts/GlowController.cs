using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowController : MonoBehaviour
{
    public float waitTime;
    private float _elapsed;
    private SkinnedMeshRenderer _meshRenderer;
    void Awake()
    {
        _meshRenderer = GetComponent<SkinnedMeshRenderer>();
        _meshRenderer.enabled = false;
    }

 
    void Update()
    {
        if (GameManager.isGameStarted)
        {
            _elapsed += Time.deltaTime;
            if (_elapsed > waitTime)
            {
                _meshRenderer.enabled = true;
            }
        }
    }
}
