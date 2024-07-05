using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;


/// <summary>
/// 사운드 재생과 사운드관련 파라미터를 관리합니다.
/// 설정창의 UI가 참조합니다. 
/// </summary>
public class SoundManager : MonoBehaviour
{
    public enum Sound
    {
        Main,
        Bgm,
        Effect,
        Narration,
        Max
    }

    private float[] _volumes = new float[(int)Sound.Max];
    public float[] VOLUME_MAX = new float[(int)Sound.Max];
    
    private readonly float VOLUME_MAX_MAIN = 1f;
    private readonly float VOLUME_MAX_BGM =0.5f;
    private readonly float VOLUME_MAX_EFFECT = 1f;
    private readonly float VOLUME_MAX_NARRATION = 1f;
    
    
    

    public float[] volumes
    {
        get => _volumes;
        set
        {
            if (_volumes == null || _volumes.Length != value.Length)
            {
                _volumes = new float[value.Length];
            }
            
        }
    }


    [FormerlySerializedAs("_audioSources")]
    public AudioSource[] audioSources;
    private  Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
    private GameObject _soundRoot;
    public void Init()
    {
        volumes = new float[(int)Sound.Max];
        
        if (_soundRoot == null)
        {
            _soundRoot = GameObject.Find("@SoundRoot");
            if (_soundRoot == null)
            {
                audioSources = new AudioSource[(int)Sound.Max];
                VOLUME_MAX = new float[(int)Sound.Max];
                _soundRoot = new GameObject { name = "@SoundRoot" };
                DontDestroyOnLoad(_soundRoot);

                var soundTypeNames = Enum.GetNames(typeof(Sound));
                for (var count = 0; count < soundTypeNames.Length - 1; count++)
                {
                    var go = new GameObject { name = soundTypeNames[count] };
                    audioSources[count] = go.AddComponent<AudioSource>();
                    go.transform.parent = _soundRoot.transform;
                }

                audioSources[(int)Sound.Bgm].loop = true;

                volumes = new float[(int)Sound.Max];
                for (int i = 0; i < (int)Sound.Max; i++)
                {
                    volumes[(int)Sound.Main] = 0.5f;
                    volumes[(int)Sound.Bgm] = 0.3f;
                    volumes[(int)Sound.Effect] = 0.5f;
                    volumes[(int)Sound.Narration] = 0.5f;
                }
                
                for (int i = 0; i < (int)Sound.Max; i++)
                {
                    VOLUME_MAX[(int)Sound.Main] = VOLUME_MAX_MAIN;
                    VOLUME_MAX[(int)Sound.Bgm] = VOLUME_MAX_BGM;
                    VOLUME_MAX[(int)Sound.Effect] = VOLUME_MAX_EFFECT;
                    VOLUME_MAX[(int)Sound.Narration] = VOLUME_MAX_NARRATION;
                }
            }
            
        }
        
    }

    public void Clear()
    {
        foreach (var audioSource in audioSources)
            audioSource.Stop();
        _audioClips.Clear();
    }

    public void SetPitch(Define.Sound type, float pitch = 1.0f)
    {
        var audioSource = audioSources[(int)type];
        if (audioSource == null)
            return;

        audioSource.pitch = pitch;
    }

    public bool Play(Sound type, string path, float volume = 1.0f, float pitch = 1.0f)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        var audioSource = audioSources[(int)type];
        if (path.Contains("Audio/") == false)
            path = string.Format("Audio/{0}", path);

        if(audioSource==null) Debug.LogError("audiosource null exception");
        audioSource.volume = volume;

        if (type == Sound.Bgm)
        {
            var audioClip = Resources.Load<AudioClip>(path);
            if (audioClip == null)
                return false;

            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.clip = audioClip;
            audioSource.pitch = pitch;
            audioSource.Play();
            return true;
        }

        if (type == Sound.Effect)
        {
            var audioClip = GetAudioClip(path);
            if (audioClip == null)
                return false;

            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
            return true;
        }

        if (type == Sound.Narration)
        {
            var audioClip = GetAudioClip(path);
            if (audioClip == null)
            {
#if UNITY_EDITOR
                Debug.Log($"narration clip is null{path}");
#endif
                return false;
                
            }
            

            audioSource.clip = audioClip;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
            return true;
        }

        return false;
    }

    public void Stop(Sound type)
    {
        var audioSource = audioSources[(int)type];
        audioSource.Stop();
    }

    public float GetAudioClipLength(string path)
    {
        var audioClip = GetAudioClip(path);
        if (audioClip == null)
            return 0.0f;
        return audioClip.length;
    }

    private AudioClip GetAudioClip(string path)
    {
        AudioClip audioClip = null;
        if (_audioClips.TryGetValue(path, out audioClip))
            return audioClip;

        audioClip = Resources.Load<AudioClip>(path);
        _audioClips.Add(path, audioClip);
        return audioClip;
    }
    
    
    // Sound관련 메소드 (legacy) 가을낙엽 컨텐츠에서 사용중.
    // 추후 가을소풍 전용으로 사용하도록 클래스 구분하거나 리팩토링 필요 12/18/23
    public  void FadeOut(Sound type,float volumeTarget = 0, float waitTime = 0.3f,
        float outDuration = 0.5f, bool rollBack = false)
    {
        
        var audioSource = audioSources[(int)type];
        audioSource.DOFade(0f, outDuration)
            .SetDelay(waitTime)
            .OnComplete(() =>
            {
                if (!rollBack)
                    audioSource.Pause();
                else
                    audioSource.Stop();
            });
    }


    // Sound관련 메소드 (legacy) 가을낙엽 컨텐츠에서 사용중.
    // 추후 가을소풍 전용으로 사용하도록 클래스 구분하거나 리팩토링 필요 12/18/23
    public static void FadeOutSound(AudioSource audioSource, float volumeTarget = 0, float waitTime = 0.5f,
        float outDuration = 0.5f, bool rollBack = false)
    {
        audioSource.DOFade(0f, outDuration).SetDelay(waitTime).OnComplete(() =>
        {
#if UNITY_EDITOR
           
#endif
            if (!rollBack)
                audioSource.Pause();
            else
                audioSource.Stop();
        });
    }

    public static void FadeInAndOutSound(AudioSource audioSource, float targetVolume = 1f, float inDuration = 0.5f,
        float fadeWaitTime = 0.5f, float outDuration = 0.5f, bool rollBack = false)
    {
#if UNITY_EDITOR
     
#endif
        audioSource.Play();
        audioSource.DOFade(targetVolume, inDuration).OnComplete(() =>
        {
            FadeOutSound(audioSource, 0f, fadeWaitTime, outDuration, rollBack);
        });
    }
}