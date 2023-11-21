using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class KoreanFlag_EffectController : MonoBehaviour
{
    private ParticleSystem[] particleSystems;

    private Camera _camera;
    private InputAction _mouseClickAction;
    private ParticleSystem[] _particles;
    private WaitForSeconds wait_;

    private AudioSource[] _adSources;
    private AudioSource _audioSource;
    private AudioSource _subAudioSourceA;
    private AudioSource _subAudioSourceB;
    private AudioSource _subAudioSourceC;
    public AudioClip _effectClip;


    private readonly Stack<ParticleSystem> particlePool = new();

    private void Awake()
    {
        _adSources = GetComponents<AudioSource>();
        foreach (var adSource in _adSources) adSource.clip = _effectClip;

        wait_ = new WaitForSeconds(_returnWaitForSeconds);
        _camera = Camera.main;

        _mouseClickAction = new InputAction("MouseClick", binding: "<Mouse>/leftButton", interactions: "press");
        _mouseClickAction.performed += OnMouseClick;

        _particles = GetComponentsInChildren<ParticleSystem>();

        foreach (var ps in _particles) particlePool.Push(ps);
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

    // fadeInDuration은 사실상 playduration과 다름없습니다.
    public float fadeInDuration;
    public float fadeOutDuration;

    private void OnMouseClick(InputAction.CallbackContext context)
    {
        var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(LAYER_NAME)))
            PlayParticle(hit.point);
    }

    public float targetVol;
    private int _count;
    public int emitAmount;
    public int burstAmount;
    public int burstCount;
    private int _currentCountForBurst;

    private void PlayParticle(Vector3 position)
    {
        if (burstCount < _currentCountForBurst)
        {
            _currentCountForBurst = 0;

            for (var i = 0; i < burstAmount; i++)
            {
                var ps = particlePool.Pop();
                ps.transform.position = position;
                ps.gameObject.SetActive(true);
                ps.Play();

                StartCoroutine(ReturnToPoolAfterDelay(ps, ps.main.duration));

                for (var j = 0; j < _adSources.Length; j++)
                    if (!_adSources[i].isPlaying)
                    {
                        FadeInSound(targetVol, fadeInDuration, _adSources[i]);
                        break;
                    }
            }
        }
        else if (particlePool.Count >= emitAmount)
        {
            for (var i = 0; i < emitAmount; i++)
            {
                var ps = particlePool.Pop();
                ps.transform.position = position;
                ps.gameObject.SetActive(true);
                ps.Play();
                _currentCountForBurst++;

                StartCoroutine(ReturnToPoolAfterDelay(ps, ps.main.duration));
            }
        }
        else
        {
            _count = 0;
            foreach (var ps in _particles)
            {
                GrowPool(_particles[_count], 1);
                _count++;
            }

            for (var i = 0; i < emitAmount; i++)
            {
                var ps = particlePool.Pop();
                ps.transform.position = position;
                ps.gameObject.SetActive(true);
                ps.Play();

                StartCoroutine(ReturnToPoolAfterDelay(ps, ps.main.duration));
            }

            Debug.LogWarning("No particles available in pool!");
        }
    }

    private readonly float _returnWaitForSeconds = 5f;

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
        for (var i = 0; i < count; i++)
        {
            var newInstance = Instantiate(original);
            newInstance.gameObject.SetActive(false);
            particlePool.Push(newInstance);
        }
    }

    public void FadeOutSound(float target, float duration, AudioSource audioSource)
    {
        audioSource.DOFade(0f, duration).SetDelay(fadeInDuration).OnComplete(() =>
        {
#if UNITY_EDITOR
            Debug.Log("audioQuit");
#endif
            audioSource.Stop();
        });
    }

    public void FadeInSound(float targetVolume, float duration, AudioSource audioSource)
    {
#if UNITY_EDITOR
        Debug.Log("audioPlay");
#endif
        audioSource.Play();
        audioSource.DOFade(targetVolume, duration).OnComplete(() => { FadeOutSound(0.05f, 0.5f, audioSource); });
    }
}