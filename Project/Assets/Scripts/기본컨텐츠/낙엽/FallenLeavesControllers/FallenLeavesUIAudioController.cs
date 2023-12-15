using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallenLeavesUIAudioController : MonoBehaviour
{
    private AudioSource _audioSource;

    [SerializeField]
    private AudioClip[] narrationAudioClips;
   

    [SerializeField]
    private float intervalBtwNarrations;
    public event Action OnFirstAudioFinished;
    private readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new();
    
    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds)) waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        return waitForSecondsCache[seconds];
    }

    private enum Narration
    {
        intro,
        howToPlay
    }

    
    // 유니티 루프
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        OnFirstAudioFinished -= PlaySecondAudio;
        OnFirstAudioFinished += PlaySecondAudio;
    }

    private void Start()
    {
        PlayFirstAudio();
      
    }


    private void OnDestroy()
    {
        OnFirstAudioFinished -= PlaySecondAudio;
    }

    // 메소드 목록
    private Coroutine _narrationAudioCoroutine;
    private void PlayFirstAudio()
    {
        if (_audioSource != null)
        {
            _audioSource.clip = narrationAudioClips[(int)Narration.intro];
            _audioSource.Play();
        }
        _narrationAudioCoroutine = StartCoroutine(CheckAudioHasFinishedPlaying());
        Debug.Log("두번째 오디오 코루틴 시작");
    }

   

    private IEnumerator CheckAudioHasFinishedPlaying()
    {
        while (_audioSource.isPlaying)
        {
            Debug.Log("재생중");
            yield return null;
        }
        
        yield return GetWaitForSeconds(intervalBtwNarrations);
        OnFirstAudioFinished?.Invoke();
        Debug.Log("두번째 오디오 이벤트 발생");
        yield return GetWaitForSeconds(0.1f);
        StopCoroutine(_narrationAudioCoroutine);
    }

    private void PlaySecondAudio()
    {
        if (_audioSource != null)
        {
            _audioSource.clip =  _audioSource.clip = narrationAudioClips[(int)Narration.howToPlay];
            _audioSource.Play();
        }
    }
}