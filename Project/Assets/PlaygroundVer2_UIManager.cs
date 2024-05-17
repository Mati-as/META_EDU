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
        LeftWin,
        RightWin
    }

    private TextMeshProUGUI[] _scoreBoards;
    private PlaygroundVer2_GameManager _gm;
  

    private GameObject _leftScore;
    private GameObject _rightScore;
    
    private Vector3 _leftScoreDefaultScale;
    private Vector3 _rightDefaultScale;
    
    private RectTransform _rectLeftScore;
    private RectTransform _rectRightScore;
    
    public override bool Init()
    {

        PlaygroundVer2_GameManager.OnScoreValueChange -= OnScoreValueChanged;
        PlaygroundVer2_GameManager.OnScoreValueChange += OnScoreValueChanged;
        
        _gm = GameObject.FindWithTag("GameManager").GetComponent<PlaygroundVer2_GameManager>();

        _scoreBoards = new TextMeshProUGUI[2];
        
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
        
        return true;
    }

    private void OnDestroy()
    {
        PlaygroundVer2_GameManager.OnScoreValueChange -= OnScoreValueChanged;
    }

    private void OnScoreValueChanged()
    {
#if UNITY_EDITOR
        Debug.Log("점수반영-------");
#endif
        _scoreBoards[(int)UIObjects.LeftScore].text = _gm.scoreLeft.ToString();
        _scoreBoards[(int)UIObjects.RightScore].text = _gm.scoreRight.ToString();
    }
    
  
    
}
