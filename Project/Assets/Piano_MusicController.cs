using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Piano_MusicController : MonoBehaviour
{
    enum SongList
    {
        WhatIsTheSame,
        SchoolBell,
        Max
    }
    enum KeyList
    {
        Piano1,
        PianoC,
        Piano2,
        PianoD,
        Piano3,
        Piano4,
        PianoG,
        Piano5,
        PianoF,
        Piano6,
        PianoA,
        Piano7,
        Piano8,
        Max
    }

    // about sheet music.
    private readonly char QUARTER_NOTE ='f';
    private readonly char EIGHT_NOTE ='e';
    private string[] _scoreList;
    private string[] _songTitleList;
    
    //private XmlNode soundNode;
    private TextAsset _xmlAsset;
    private XmlDocument _xmlDoc;
    private string _path = "Common/Data/BD005_SheetMusic";

    private Coroutine _songCr;
    [Range(0, 5)] public float interval;


    private ParticleSystem _ps;


    //animationPart - Piano
    private Sequence _seq;
    private float moveAmount = 0.050f;
    private Transform[] _keys;
    private Vector3[] _defaultPositions;
    private Dictionary<string, Transform> _transformMap;
    private Dictionary<string, Vector3> _defaultPosMap;
    
    //animationPart - Parrot
    private float _pathHeight = 1.30f;
    private Transform _parrot;
    private Animator _animator;
    private Vector3[] _currentPath;

    private readonly int FLY = Animator.StringToHash("Fly");
    

    private void Start()
    {
        _parrot = GameObject.Find("Parrot").GetComponent<Transform>();
        _ps = GameObject.Find("CFX_ParticleInducing").GetComponent<ParticleSystem>();
        _animator = _parrot.GetComponent<Animator>();
        
        _scoreList = new string[(int)SongList.Max];
        _songTitleList = new string[(int)SongList.Max];
        _defaultPosMap = new Dictionary<string, Vector3>();
        _transformMap = new Dictionary<string, Transform>();
        _currentPath = new Vector3[3];
        
        _songTitleList = Enum.GetNames(typeof(SongList));
        _keys = new Transform[(int)KeyList.Max];
        _defaultPositions = new Vector3[(int)KeyList.Max];

        for (int i = 0; i < (int)KeyList.Max ; i++)
        {
            _keys[i] = transform.GetChild(i);
            _defaultPositions[i] = _keys[i].position;
            _defaultPosMap.TryAdd(_keys[i].gameObject.name, _defaultPositions[i]);
            _transformMap.TryAdd(_keys[i].gameObject.name, _keys[i]);
        }

        
        SetXML();


        PlaySong(_scoreList[(int)SongList.WhatIsTheSame]);
    }

    private void SetXML()
    {
        Utils.LoadXML(ref _xmlAsset,ref _xmlDoc, _path);

        for (int i = (int)SongList.WhatIsTheSame; i < (int)SongList.Max; i++)
        {
            var score = _xmlDoc.SelectSingleNode($"//StringData[@ID='{i}']");
            _scoreList[i] = score.Attributes["string"].Value;
#if UNITY_EDITOR
            Debug.Log($"Sheet Music Set : {i} : {_scoreList[i]}");
#endif
        }
       

    }

    private void PlaySong(string score)
    {
        _songCr = StartCoroutine(PlaySongCoroutine(score, interval));
    }

    IEnumerator PlaySongCoroutine( string scoreString, float intervalBtwKeys)
    {
      
        for (int i = 0; i < scoreString.Length; i++)
        {
            if (scoreString[i] == QUARTER_NOTE)
            {
                yield return DOVirtual.Float(0, 0, intervalBtwKeys, _ => { }).WaitForCompletion();
            }
            else if (scoreString[i] == EIGHT_NOTE)
            {
                yield return DOVirtual.Float(0, 0, intervalBtwKeys/2, _ => { }).WaitForCompletion();
            }
            else
            {
                
                // ------ 건반 클릭 유도 파티클 관련
                var currentPos =  _defaultPosMap["Piano" + scoreString[i]];
                _ps.gameObject.transform.position = currentPos + _ps.gameObject.transform.up *0.9f + Vector3.back * 0.5f;
                _ps.gameObject.SetActive(true);
                yield return DOVirtual.Float(0, 0, 0.51f, _ => { }).WaitForCompletion();
           
            
                // ------ 앵무새이동
                _currentPath = SetPath(_parrot, currentPos + Vector3.back * 0.3f + Vector3.up * 0.5f);
                _parrot.DOPath(_currentPath, 0.35f).SetEase(Ease.InOutSine).OnStart(() =>
                {
                    _animator.SetBool(FLY,true);
                }).OnComplete(() =>
                {
                    _animator.SetBool(FLY,false);
                });
        
                // ------ 건반 클릭 
                yield return DOVirtual.Float(0, 0, 0.05f, _ => { }).WaitForCompletion();
                PlayKeyAnim(_transformMap["Piano"+scoreString[i]],_defaultPosMap["Piano"+scoreString[i]]);
                yield return DOVirtual.Float(0, 0, 0.05f, _ => { }).WaitForCompletion();
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Piano/Piano" + scoreString[i]);
               
            
                // ------ 클릭 이후
                yield return DOVirtual.Float(0, 0, 0.18f, _ => { }).WaitForCompletion();
                _ps.Stop();
                _ps.gameObject.SetActive(false);
                yield return DOVirtual.Float(0, 0, intervalBtwKeys, _ => { }).WaitForCompletion();
            }

        
        }
        
        StopCoroutine(_songCr);
        
        yield return DOVirtual.Float(0, 0, 1.1f, _ => { }).WaitForCompletion();
        
        PlaySong(_scoreList[(int)SongList.SchoolBell]);
      
    }

    private Vector3[] SetPath(Transform transform,Vector3 arrival)
    {
        var path = new Vector3[3];
        path[0] = transform.position;
        path[1] = (transform.position + arrival) / 2 + _parrot.up * _pathHeight;
        path[2] = arrival;
        return path;

    }
    private void PlayKeyAnim(Transform obj, Vector3 defaultLocation)
    {
        // 시퀀스 중복실행방지용
        if (_seq != null && _seq.IsActive() && _seq.IsPlaying()) return;

        _seq = DOTween.Sequence();

        _seq
            .Append(obj.DOMove(defaultLocation + -transform.up * moveAmount, 0.15f).SetEase(Ease.InOutSine))
            .AppendInterval(0.05f)
            .Append(obj.DOMove(defaultLocation, 0.15f).SetEase(Ease.InOutSine));

    }
}
