using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class AttendanceCheck_GameManager : IGameManager
{
  
  
  private TextMeshPro[] _nameOnTrains;
  private AttendanceCheck_UIManager _uiManager;
  private LineRenderer _lr;  
  
  private Animator _trainAnimator;
  private readonly int TRAIN_MOVE = Animator.StringToHash("Move");

  protected override void Init()
  {
    base.Init();

    var trainParent = transform.GetChild(0);
    var childCount = trainParent.childCount;
    _nameOnTrains = new TextMeshPro[childCount];
  
    for (int i = 1; i < childCount; i++) //인덱스 0은 TrainHead...Continue필요
    {
      var child = trainParent.GetChild(i);
      
      _nameOnTrains[i] = child.GetChild(0).GetComponent<TextMeshPro>();
    }

    _trainAnimator = GameObject.Find("Scene2_NameTrain").GetComponent<Animator>();
  
  }

  protected override void BindEvent()
  {
    base.BindEvent();
    
    AttendanceCheck_UIManager.OnNameInputFinished -= OnNameInputFinished;
    AttendanceCheck_UIManager.OnNameInputFinished += OnNameInputFinished;
  }

  void OnDestroy()
  {
    AttendanceCheck_UIManager.OnNameInputFinished -= OnNameInputFinished;
  }

  private void OnNameInputFinished()
  {
   
    _uiManager = GameObject.Find("Scene1_NameBoard").GetComponent<AttendanceCheck_UIManager>();
    if(_uiManager==null) Debug.LogError("_uiManager is null");
  
    var trainCount = transform.GetChild(0).childCount;
    Debug.Log($"list name count : {_uiManager.text_namesOnList.Length}");
    // 0번 인덱스는 TrainHead기 때문에 Continue
    for (int i = 1; i <  _uiManager.text_namesOnList.Length; i++) //_uiManager.activeNameCount
    {
      if(_nameOnTrains[i%trainCount] == null)
      {
        Debug.Log($"_nameOnTrains[{i}] is null");
        continue;
      }

      if(_uiManager.text_namesOnList[i%trainCount] == null)
      {
        Debug.Log($"_uiManager.namesOnList[{i}] is null");
        continue;
      }
      
      _nameOnTrains[i%trainCount].text = _uiManager.text_namesOnList[(i-1)%trainCount].text;
    }


    DOVirtual.Float(0, 0, 5f, _ => { }).OnComplete(() =>
    {
      _trainAnimator.SetBool(TRAIN_MOVE, true);
    });

  }
}
