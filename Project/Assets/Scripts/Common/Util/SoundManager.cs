
using UnityEngine;
using DG.Tweening;


    public class SoundManager : MonoBehaviour
    {
        
        
        public static void FadeOutSound(AudioSource audioSource, float volumeTarget = 0,float waitTime =0.5f,float outDuration = 0.5f , bool rollBack = false)
        {
            audioSource.DOFade(0f, outDuration).SetDelay(waitTime).OnComplete(() =>
            {
            
#if UNITY_EDITOR
                Debug.Log("audioQuit");
#endif
                if (!rollBack)
                {
                    audioSource.Pause();
                }
                else
                {
                    audioSource.Stop();
                }
                
            });
        }

        public static void FadeInAndOutSound(AudioSource audioSource,float targetVolume = 1f, float inDuration =0.5f,float fadeWaitTime =0.5f, float outDuration =0.5f, bool rollBack =false)
        {

#if UNITY_EDITOR
            Debug.Log("audioPlay");
#endif 
            audioSource.Play();
            audioSource.DOFade(targetVolume, inDuration).OnComplete(() =>
            {
                FadeOutSound(audioSource,0f,fadeWaitTime,outDuration,rollBack);
            });
        }
        
        
    }
