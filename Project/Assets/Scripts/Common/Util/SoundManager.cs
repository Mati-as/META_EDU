using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    
    public enum Sound
    {
        Bgm,
        Effect,
        Speech,
        Max,
    }

    private readonly AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.Max];
    private readonly Dictionary<string, AudioClip> _audioClips = new();
    private GameObject _soundRoot;
    public void Init()
    {
        if (_soundRoot == null)
        {
            _soundRoot = GameObject.Find("@SoundRoot");
            if (_soundRoot == null)
            {
                _soundRoot = new GameObject { name = "@SoundRoot" };
                DontDestroyOnLoad(_soundRoot);

                var soundTypeNames = Enum.GetNames(typeof(Define.Sound));
                for (var count = 0; count < soundTypeNames.Length - 1; count++)
                {
                    var go = new GameObject { name = soundTypeNames[count] };
                    _audioSources[count] = go.AddComponent<AudioSource>();
                    go.transform.parent = _soundRoot.transform;
                }

                _audioSources[(int)Define.Sound.Bgm].loop = true;
            }
        }
    }

    public void Clear()
    {
        foreach (var audioSource in _audioSources)
            audioSource.Stop();
        _audioClips.Clear();
    }

    public void SetPitch(Define.Sound type, float pitch = 1.0f)
    {
        var audioSource = _audioSources[(int)type];
        if (audioSource == null)
            return;

        audioSource.pitch = pitch;
    }

    public bool Play(Define.Sound type, string path, float volume = 1.0f, float pitch = 1.0f)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        var audioSource = _audioSources[(int)type];
        if (path.Contains("Audio/") == false)
            path = string.Format("Audio/{0}", path);

        audioSource.volume = volume;

        if (type == Define.Sound.Bgm)
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

        if (type == Define.Sound.Effect)
        {
            var audioClip = GetAudioClip(path);
            if (audioClip == null)
                return false;

            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
            return true;
        }

        if (type == Define.Sound.Speech)
        {
            var audioClip = GetAudioClip(path);
            if (audioClip == null)
                return false;

            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.clip = audioClip;
            audioSource.pitch = pitch;
            audioSource.Play();
            return true;
        }

        return false;
    }

    public void Stop(Define.Sound type)
    {
        var audioSource = _audioSources[(int)type];
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
    public static void FadeOutSound(AudioSource audioSource, float volumeTarget = 0, float waitTime = 0.5f,
        float outDuration = 0.5f, bool rollBack = false)
    {
        audioSource.DOFade(0f, outDuration).SetDelay(waitTime).OnComplete(() =>
        {
#if UNITY_EDITOR
            Debug.Log("audioQuit");
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
        Debug.Log("audioPlay");
#endif
        audioSource.Play();
        audioSource.DOFade(targetVolume, inDuration).OnComplete(() =>
        {
            FadeOutSound(audioSource, 0f, fadeWaitTime, outDuration, rollBack);
        });
    }
}