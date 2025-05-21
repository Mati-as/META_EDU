using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using TMPro;
using UnityEngine;

public class PlaygroundVer2_UIManager : UI_PopUp
{
    private enum UIObjects
    {
        LeftScore,
        RightScore,
        Timer,
        LeftWin,
        RightWin,
        Count
    }

    private TextMeshProUGUI[] _scoreBoards;
    private PlaygroundBaseVer2GameManager _gm;


    private GameObject _leftScore;
    private GameObject _rightScore;
    private GameObject _timer;

    private Vector3 _leftScoreDefaultScale;
    private Vector3 _rightDefaultScale;

    private RectTransform _rectTimer;
    private RectTransform _rectLeftScore;
    private RectTransform _rectRightScore;

    private bool _isReinitUIPlaying;

    private int _defaultFontSize;

    public static event Action OnReInitUIFinished;
    public override bool InitEssentialUI()
    {
        PlaygroundBaseVer2GameManager.OnTimeOver -= OnTimeOver;
        PlaygroundBaseVer2GameManager.OnTimeOver += OnTimeOver;

        PlaygroundBaseVer2GameManager.OnScoreValueChange -= OnScoreValueChanged;
        PlaygroundBaseVer2GameManager.OnScoreValueChange += OnScoreValueChanged;

        _gm = GameObject.FindWithTag("GameManager").GetComponent<PlaygroundBaseVer2GameManager>();

        _scoreBoards = new TextMeshProUGUI[3];

        BindObject(typeof(UIObjects));
        _leftScore = GetObject((int)UIObjects.LeftScore);
        _scoreBoards[(int)UIObjects.LeftScore] = _leftScore.GetComponent<TextMeshProUGUI>();
        _rectLeftScore = _leftScore.GetComponent<RectTransform>();

        // _leftScoreDefaultScale = _rectLeftScore.localScale;
        // _rectLeftScore.localScale = Vector3.zero;
        // _leftScore.SetActive(false);


        _rightScore = GetObject((int)UIObjects.RightScore);
        _scoreBoards[(int)UIObjects.RightScore] = _rightScore.GetComponent<TextMeshProUGUI>();
        _rectRightScore = _rightScore.GetComponent<RectTransform>();

        // _rightDefaultScale = _rectRightScore.localScale;
        // _rectRightScore.localScale = Vector3.zero;
        // _rightScore.SetActive(false);


        _timer = GetObject((int)UIObjects.Timer);
        _rectTimer = _timer.GetComponent<RectTransform>();
        _scoreBoards[(int)UIObjects.Timer] = _timer.GetComponent<TextMeshProUGUI>();
        
        _defaultFontSize = (int)_scoreBoards[(int)UIObjects.Timer].fontSize;
        return true;
    }

    private void OnDestroy()
    {
        PlaygroundBaseVer2GameManager.OnScoreValueChange -= OnScoreValueChanged;
        PlaygroundBaseVer2GameManager.OnTimeOver -= OnTimeOver;
    }

    private void Update()
    {
        if(!_isReinitUIPlaying) _scoreBoards[(int)UIObjects.Timer].text = _gm.currentTime.ToString() + "초";
    }

    private void OnScoreValueChanged()
    {

        _scoreBoards[(int)UIObjects.LeftScore].text = _gm.scoreLeft.ToString();
        _scoreBoards[(int)UIObjects.RightScore].text = _gm.scoreRight.ToString();
    }

    private void OnTimeOver() => StartCoroutine(OnTimeOverCo());

    IEnumerator OnTimeOverCo()
    {
        if (_isReinitUIPlaying) yield break;
        _isReinitUIPlaying = true;
#if UNITY_EDITOR
        Debug.Log("라운드종료-------");
#endif

        _scoreBoards[(int)UIObjects.Timer].text = "그만!";
        yield return DOVirtual.Float(0, 0, 3f, _ => { }).WaitForCompletion();
        if (_gm.scoreLeft == _gm.scoreRight) _scoreBoards[(int)UIObjects.Timer].text = "비겼어요!";
        else
        {
            _scoreBoards[(int)UIObjects.Timer].text = _gm.scoreLeft > _gm.scoreRight ? "왼쪽 승리!" : "오른쪽 승리!";
        }

       
        yield return DOVirtual.Float(0, 0, 5f, _ => { }).WaitForCompletion();
        
        _scoreBoards[(int)UIObjects.Timer].fontSize = 8;
        _scoreBoards[(int)UIObjects.Timer].text = "놀이를 다시\n준비하고 있어요";
        
        yield return DOVirtual.Float(0, 0, 3f, _ => { }).WaitForCompletion();
        _scoreBoards[(int)UIObjects.Timer].fontSize = _defaultFontSize;
        _scoreBoards[(int)UIObjects.Timer].text = "3";
        
        yield return DOVirtual.Float(0, 0, 1f, _ => { }).WaitForCompletion();
        _scoreBoards[(int)UIObjects.Timer].text = "2";
        
        yield return DOVirtual.Float(0, 0, 1f, _ => { }).WaitForCompletion();
        _scoreBoards[(int)UIObjects.Timer].text = "1";
        
        yield return DOVirtual.Float(0, 0, 1f, _ => { }).WaitForCompletion();
        _scoreBoards[(int)UIObjects.Timer].text = "시작!";
        
        OnReInitUIFinished?.Invoke();
        
        yield return DOVirtual.Float(0, 0, 1.5f, _ => { }).WaitForCompletion();
        _isReinitUIPlaying = false;
     

       
    }

}
