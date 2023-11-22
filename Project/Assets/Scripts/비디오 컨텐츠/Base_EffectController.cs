using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class Base_EffectController : MonoBehaviour
{
   
    public ParticleSystem[] _particles;
    
    
    
    [HideInInspector]
    public Camera _camera;
    public InputAction _mouseClickAction;
    public Stack<ParticleSystem> particlePool;
    public WaitForSeconds wait_;

    public void Init()
    {
        particlePool = new();
        foreach(ParticleSystem ps in _particles)
        {
            GrowPool(ps);
        }
     
    }
    public void GrowPool(ParticleSystem original, int count = 1)
    {
        for (var i = 0; i < count; i++)
        {
            var newInstance = Instantiate(original);
            newInstance.gameObject.SetActive(false);
            particlePool.Push(newInstance);
        }
    }

    public void FadeOutSound(AudioSource audioSource, float target = 0.1f, float fadeInDuration = 2.3f,float duration =1f)
    {
        audioSource.DOFade(target, duration).SetDelay(fadeInDuration).OnComplete(() =>
        {
#if UNITY_EDITOR
            Debug.Log("audioQuit");
#endif
            audioSource.Stop();
        });
    }

    public void FadeInSound( AudioSource audioSource,float targetVolume = 1f,float duration = 0.3f)
    {
#if UNITY_EDITOR
        Debug.Log("audioPlay");
#endif
        audioSource.Play();
        audioSource.DOFade(targetVolume, duration).OnComplete(() => { FadeOutSound(audioSource); });
    }
    
    public IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, float wait = 2f)
    {
        yield return wait_;
        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        particlePool.Push(ps); // Return the particle system to the pool
    }
    
 

    public virtual void PlayParticle(Vector3 Position)
    {
        
    }
  



}
