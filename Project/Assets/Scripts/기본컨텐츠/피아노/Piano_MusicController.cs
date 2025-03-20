using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Piano_MusicController : Base_GameManager
{
    private enum SongList
    {
        WhatIsTheSame,
        SchoolBell,
        ThreeBears,
        Max
    }

    private enum KeyList
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

    //Camera
    private Vector3[] _cameraPath;

    // about sheet music.
    private readonly char QUARTER_NOTE = 'f';
    private readonly char EIGHT_NOTE = 'e';
    private string[] _scoreList;
    private string[] _songTitleList;

    // 동요악보를 숫자형태로 아래 파일, "BD005_SheetMusic.xml"에 저장
    private TextAsset _xmlAsset;
    private XmlDocument _xmlDoc;
    private  string _path = "Common/Data/BD005_SheetMusic";

    private Coroutine _songCr;
    [Range(0, 5)] public float interval;


    private ParticleSystem _ps;

    //click
    private Sequence _userSeq;

    //animationPart - Piano
    private Sequence _seq;
    private readonly float MOVE_AMOUNT = 0.050f;
    private Transform[] _keys;
    private Vector3[] _defaultPositions;
    private Dictionary<string, Transform> _transformMap;
    private Dictionary<string, Vector3> _defaultPosMap;
    private readonly float INTERVAL_BTW_SONGS = 2f;


    //animationPart - Parrot
    private readonly float PATH_HEIGHT = 0.25f;
    private readonly float JUMP_HEIGHT = 0.6f;
    private Transform _feetSprite;
    private Animator _animator;
    private Vector3[] _currentPath;

    private readonly int FLY = Animator.StringToHash("Fly");

    //파티클 및 발자국 객체 위치 조정용 
    private readonly float BACK_OFFSET = 2.1f;
    private readonly float UP_OFFSET = 0.125f;
    private readonly float UP_OFFSET_PS = 0.1f;


    //animationPart - event
    private static event Action OnSongFinished;
    private SongList _currentSong;


    protected override void BindEvent()
    {
        base.BindEvent();
        OnSongFinished -= PlayNextSong;
        OnSongFinished += PlayNextSong;
    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();

        var cameraLookAt = GameObject.Find("CameraLookAt").transform.position;
       
        Camera.main.transform.DOPath(_cameraPath, 3f).SetEase(Ease.InOutSine)
            .OnUpdate(() => { Camera.main.transform.DOLookAt(cameraLookAt, 0.01f); })
            .OnComplete(() =>
            {
                _currentSong = SongList.WhatIsTheSame;
                PlaySong(_scoreList[(int)SongList.WhatIsTheSame]);
            });
    }


    private ParticleSystem _clickPs;
    protected override void Init()
    {
        base.Init();
        var psPrefab = Resources.Load<ParticleSystem>("SortedbyGame/BasicContents/Piano/CFX_PianoClick");
        _clickPs = Instantiate(psPrefab);
        _clickPs.Stop();
        
        _feetSprite = GameObject.Find("FeetSprite").GetComponent<Transform>();
        _ps = GameObject.Find("CFX_ParticleInducing").GetComponent<ParticleSystem>();
        _animator = _feetSprite.GetComponent<Animator>();
        
        

        _scoreList = new string[(int)SongList.Max];
        _songTitleList = new string[(int)SongList.Max];
        _defaultPosMap = new Dictionary<string, Vector3>();
        _transformMap = new Dictionary<string, Transform>();
        _currentPath = new Vector3[3];

        _songTitleList = Enum.GetNames(typeof(SongList));
        _keys = new Transform[(int)KeyList.Max];
        _defaultPositions = new Vector3[(int)KeyList.Max];

        for (var i = 0; i < (int)KeyList.Max; i++)
        {
            _keys[i] = transform.GetChild(i);
            _defaultPositions[i] = _keys[i].position;
            _defaultPosMap.TryAdd(_keys[i].gameObject.name, _defaultPositions[i]);
            _transformMap.TryAdd(_keys[i].gameObject.name, _keys[i]);
        }


        SetXML();


        _cameraPath = new Vector3[3];
        var path = GameObject.Find("CameraPathIntro").transform;
        _cameraPath[0] = path.GetChild(0).position;
        _cameraPath[1] = path.GetChild(1).position;
        _cameraPath[2] = Camera.main.transform.position;
        Camera.main.transform.position = _cameraPath[0];
        var cameraLookAt = GameObject.Find("CameraLookAt").transform.position;
        Camera.main.transform.DOLookAt(cameraLookAt, 0.01f);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnSongFinished -= PlayNextSong;
    }


    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        
        foreach (var hit in GameManager_Hits)
        {
            var clickedName = hit.transform.gameObject.name;
            if (clickedName.Contains("Piano") && _defaultPosMap.ContainsKey(hit.transform.gameObject.name))
            {
                PlayKeyAnimByUser(hit.transform, _defaultPosMap[hit.transform.gameObject.name]);
                if (SceneManager.GetActiveScene().name == "BD005_UserPlay")
                {
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/Piano/"+clickedName,0.05f);
                }
            }
              

    
        }
    }

    private void SetXML()
    {
        Utils.LoadXML(ref _xmlAsset, ref _xmlDoc, _path,ref _path);

        for (var i = (int)SongList.WhatIsTheSame; i < (int)SongList.Max; i++)
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

    private IEnumerator PlaySongCoroutine(string scoreString, float intervalBtwKeys)
    {
        for (var i = 0; i < scoreString.Length; i++)
            if (scoreString[i] == QUARTER_NOTE)
            {
                yield return DOVirtual.Float(0, 0, intervalBtwKeys, _ => { }).WaitForCompletion();
            }
            else if (scoreString[i] == EIGHT_NOTE)
            {
                yield return DOVirtual.Float(0, 0, intervalBtwKeys / 2, _ => { }).WaitForCompletion();
            }
            else
            {
                yield return DOVirtual.Float(0, 0, intervalBtwKeys, _ => { }).WaitForCompletion();

                var posToMove = _defaultPosMap["Piano" + scoreString[i]];

                // ------ 1.건반 클릭 유도 파티클 관련------------------------------------------------
                var psPos = posToMove + _ps.gameObject.transform.up * UP_OFFSET_PS + Vector3.back * BACK_OFFSET;
                _ps.gameObject.transform.position = psPos;
                _ps.gameObject.SetActive(true);
                yield return DOVirtual.Float(0, 0, 0.35f, _ => { }).WaitForCompletion();


                // ------ 2.앵무새이동--------------------------------------------------------------
                var arrival = posToMove + Vector3.back * BACK_OFFSET + Vector3.up * UP_OFFSET;

                _currentPath = SetPath(_feetSprite, arrival, PATH_HEIGHT);
                yield return _feetSprite.DOPath(_currentPath, 0.15f).SetEase(Ease.InOutSine)
                    .OnStart(() =>
                    {
                        //_feetSprite.DOLookAt(posToMove + Vector3.back * 5f + Vector3.up * 3f, 0.1f);
                        _animator.SetBool(FLY, true);
                    }).OnComplete(() => { _animator.SetBool(FLY, false); }).WaitForCompletion();


                // ------ 3.앵무새 도착한 위치에서 제자리점프----------------------------------------------
                yield return DOVirtual.Float(0, 0, 0.35f, _ => { }).WaitForCompletion();
                _currentPath = SetPath(_feetSprite, arrival, JUMP_HEIGHT);
                yield return _feetSprite.DOPath(_currentPath, 0.10f).SetEase(Ease.InOutSine).WaitForCompletion();


                // ------ 4.건반 클릭 & 파티클재생 -------------------------------------------------------------------
                yield return DOVirtual.Float(0, 0, 0.01f, _ => { }).WaitForCompletion();
                PlayKeyAnim(_transformMap["Piano" + scoreString[i]], _defaultPosMap["Piano" + scoreString[i]]);
                yield return DOVirtual.Float(0, 0, 0.01f, _ => { }).WaitForCompletion();
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/Piano/Piano" + scoreString[i]);
                _clickPs.transform.position = arrival;
                _clickPs.Play();

                

                // ------ 5.클릭 이후-------------------------------------------------------------------
                yield return DOVirtual.Float(0, 0, 0.08f, _ => { }).WaitForCompletion();
                _ps.Stop();
                _ps.gameObject.SetActive(false);
            }

        yield return DOVirtual.Float(0, 0, INTERVAL_BTW_SONGS, _ => { }).WaitForCompletion();

        //다음곡 재생
        _currentSong++;
        StopCoroutine(_songCr);
        OnSongFinished?.Invoke();
    }

    private void PlayNextSong()
    {
        if (_currentSong != SongList.Max)
        {
            PlaySong(_scoreList[(int)_currentSong]);
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("모든곡 종료!");
#endif
        }
    }

    private Vector3[] SetPath(Transform transform, Vector3 arrival, float height)
    {
        var path = new Vector3[3];
        path[0] = transform.position;
        path[1] = (transform.position + arrival) / 2 + _feetSprite.up * height;
        path[2] = arrival;
        return path;
    }

    private void PlayKeyAnim(Transform obj, Vector3 defaultLocation)
    {
        // 시퀀스 중복실행방지용
        if (_seq != null && _seq.IsActive() && _seq.IsPlaying()) return;

        _seq = DOTween.Sequence();

        _seq
            .Append(obj.DOMove(defaultLocation + -transform.up * MOVE_AMOUNT, 0.15f).SetEase(Ease.InOutSine))
            .AppendInterval(0.05f)
            .Append(obj.DOMove(defaultLocation, 0.15f).SetEase(Ease.InOutSine));
    }

    private void PlayKeyAnimByUser(Transform obj, Vector3 defaultLocation)
    {
        // 시퀀스 중복실행방지용
        if (_userSeq != null && _userSeq.IsActive() && _userSeq.IsPlaying()) return;

        _userSeq = DOTween.Sequence();

        _userSeq
            .Append(obj.DOMove(defaultLocation + -transform.up * MOVE_AMOUNT, 0.15f).SetEase(Ease.InOutSine))
            .AppendInterval(0.05f)
            .Append(obj.DOMove(defaultLocation, 0.15f).SetEase(Ease.InOutSine));
    }
}