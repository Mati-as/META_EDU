using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using System;

public class Base_EffectController : MonoBehaviour
{
   
    public ParticleSystem[] _particles;
    
    [HideInInspector]
    public Camera _camera;
    public InputAction _mouseClickAction;
    protected Stack<ParticleSystem> particlePool;
    public WaitForSeconds wait_;
    public float targetVol;
    private int _count;
    public int emitAmount;
    public int burstAmount;
    public int burstCount;
    protected readonly int _burstAudioSize = 10;

    protected virtual void Init()
    {
        particlePool = new Stack<ParticleSystem>();
        foreach(ParticleSystem ps in _particles)
        {
            GrowPool(ps);
        }
     
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
    
    protected IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, float wait = 2f)
    {
        yield return wait_;
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        particlePool.Push(ps); // Return the particle system to the pool
    }


    protected virtual void PlayParticle(Vector3 position, AudioSource[] audioSources, AudioSource[]
            burstAudioSources, ref int currentCountForBurst, bool isBurst = false,
        int emitAmount = 2, int burstCount = 10, int burstAmount = 5, float wait = 3f)
    {
        

        //UnderFlow를 방지하기 위해서 선제적으로 GrowPool 실행 
        if (particlePool.Count < emitAmount || (currentCountForBurst < burstCount && particlePool.Count < burstCount))
        {
            // 에디터상에서 배치한 순서대로 파티클을 Push하기 위해 for문 사용합니다. 
            for (var i = 0; i < 2; i++)
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

                TurnOnParticle(position, burstCount, wait);
                FindAndPlayAudio(burstAudioSources);
                currentCountForBurst = 0;
            }
            else
            {
                TurnOnParticle(position, emitAmount, wait);
                FindAndPlayAudio(audioSources);
                currentCountForBurst++;
            }
        }
    }


    protected void TurnOnParticle(Vector3 position, int loopCount = 2, float delayToReturn = 3f)
    {
        for (var i = 0; i < loopCount; i++)
        {
            var ps = particlePool.Pop();
            ps.transform.position = position;
            ps.gameObject.SetActive(true);
            ps.Play();
#if UNITY_EDITOR
            Debug.Log("enough particles in the pool.");
#endif
            StartCoroutine(ReturnToPoolAfterDelay(ps, delayToReturn));
        }
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
