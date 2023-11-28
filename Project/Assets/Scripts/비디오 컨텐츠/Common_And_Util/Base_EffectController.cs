using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using System;

public abstract class Base_EffectController : MonoBehaviour
{
   
    public ParticleSystem[] _particles;
    
    [HideInInspector]
    public Camera _camera;
    public InputAction _mouseClickAction;
    protected Stack<ParticleSystem> particlePool;
    public WaitForSeconds wait_;
  
    private int _count;
    public int emitAmount;
    public int burstAmount;
    public int burstCount;
    public float targetVol;
    protected int _burstAudioSize;

    public Ray ray { get; set; }
    public RaycastHit[] hits;
    protected abstract void OnClicked();

    protected virtual void Init()
    {
        particlePool = new Stack<ParticleSystem>();
        
        for (int i = 0; i < burstAmount; i++)
        {
            foreach(ParticleSystem ps in _particles)
            {
                GrowPool(ps);
            }
        }

        Video_Image_Move.OnStep -= OnClicked;
        Video_Image_Move.OnStep += OnClicked;
        
    }

    protected virtual void OnDestroy()
    {
        Video_Image_Move.OnStep -= OnClicked;
    }

    protected void GrowPool(ParticleSystem original, int count = 1)
    {
        for (var i = 0; i < count; i++)
        {
            var newInstance = Instantiate(original);
            newInstance.gameObject.SetActive(false);
            particlePool.Push(newInstance);
        }
    }

    protected void FadeOutSound(AudioSource audioSource, float target = 0.1f, float fadeInDuration = 2.3f,float duration =1f)
    {
        audioSource.DOFade(target, duration).SetDelay(fadeInDuration).OnComplete(() =>
        {
#if UNITY_EDITOR
            Debug.Log("audioQuit");
#endif
            audioSource.Stop();
        });
    }

    protected void FadeInSound( AudioSource audioSource,float targetVolume = 1f,float duration = 0.3f)
    {
#if UNITY_EDITOR
        Debug.Log("audioPlay");
#endif
        audioSource.Play();
        audioSource.DOFade(targetVolume, duration).OnComplete(() => { FadeOutSound(audioSource); });
    }
    



    protected virtual void PlayParticle(Vector3 position, AudioSource[] audioSources, AudioSource[]
            burstAudioSources, ref int currentCountForBurst, bool isBurst = false,
        int emitAmount = 2, int burstCount = 10, int burstAmount = 5, float wait = 3f,bool usePsMainDuration = false)
    {
        

        //UnderFlow를 방지하기 위해서 선제적으로 GrowPool 실행 
        if (particlePool.Count < emitAmount || particlePool.Count < burstCount)
        {
            // 에디터상에서 배치한 순서대로 파티클을 Push하기 위해 for문 사용합니다. 
            for (var i = 0; i < burstAmount; i++)
                foreach (var ps in _particles)
                    GrowPool(ps);

#if UNITY_EDITOR
            Debug.Log("no particles in the pool. creating particles and push...");
#endif
        }

        if (particlePool.Count >= emitAmount)
        {
            if (currentCountForBurst > burstCount && isBurst)
            {
                // if (particlePool.Count <= burstAmount)
                //     foreach (var ps in _particles)
                //         GrowPool(ps);

              
                TurnOnParticle(position, emitAmount, wait,usePsMainDuration);
                FindAndPlayAudio(burstAudioSources);
                currentCountForBurst = 0;
            }
            else
            {
              
                TurnOnParticle(position, emitAmount, wait,usePsMainDuration);
                FindAndPlayAudio(audioSources);
                currentCountForBurst++;
            }
        }
    }


    protected void TurnOnParticle(Vector3 position, int loopCount = 2, float delayToReturn = 3f, bool isWaitTimeManuallySet = false)
    {
        for (var i = 0; i < loopCount; i++)
        {
            ParticleSystem ps = particlePool.Pop();
            ps.transform.position = position;
            ps.gameObject.SetActive(true);
            ps.Play();

           
            if (isWaitTimeManuallySet)
            {
                StartCoroutine(ReturnToPoolAfterDelay(ps, ps.main.startLifetime.constantMax));
            }
            else 
            {
#if UNITY_EDITOR
                Debug.Log($"delayToReturn: {delayToReturn}");
#endif
                StartCoroutine(ReturnToPoolAfterDelay(ps, delayToReturn));
            }

         
        }
        
    }
    
    protected IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, float wait = 3f,bool isWaitTimeManuallySet = false)
    {
        if (wait_ == null)
        {
            if (isWaitTimeManuallySet)
            {
#if UNITY_EDITOR
                Debug.Log($"PS.LIFETIME.CONSTANTMAX : {ps.main.startLifetime.constantMax}");
#endif
                wait_ = new WaitForSeconds(ps.main.startLifetime.constantMax);
            }
            else
            {
                wait_ = new WaitForSeconds(wait);
            }
            
        }
        
        
        yield return new WaitForSeconds(ps.main.startLifetime.constantMax);
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        particlePool.Push(ps); // Return the particle system to the pool
    }
    
    protected void FindAndPlayAudio(AudioSource[] audioSources,bool isBurst = false, bool recursive = false)
    {
        if (!isBurst)
        {
            var availableAudioSource = Array.Find(audioSources, audioSource => !audioSource.isPlaying);

            if (availableAudioSource != null)
            {
                FadeInSound(availableAudioSource);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("No available AudioSource!");
#endif
            }
        }
        

    }




}
