using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class UndergroundTutorialExampleBoxUIController : MonoBehaviour
{
  [Header("TextBox Frame Move Settings")] 
   
  [SerializeField]
  private RectTransform frame;
  [SerializeField]
  private RectTransform leftPosition;
  [SerializeField]
  private RectTransform rightPosition;
  
  
   [SerializeField]
  private RectTransform leftRectTransform;
  
  private Vector3 originalPosition; 
  public RectTransform target1; 
  public RectTransform target2;

 
  [SerializeField]
  private RectTransform rightRectTransform;
  public RectTransform rigtMoveTarget1;
  public RectTransform rigtMoveTarget2;

  private Vector3 rightOriginalPosition;
  void Awake()
  {
    TextBoxUIController.TextBoxLeftUIEvent -= MoveKidsToTarget;
    TextBoxUIController.TextBoxRightUIEvent -= MoveRabbitToTarget;

        
    TextBoxUIController.TextBoxLeftUIEvent += MoveKidsToTarget;
    TextBoxUIController.TextBoxRightUIEvent += MoveRabbitToTarget;
;
  }

  private void OnDestroy()
  {
    TextBoxUIController.TextBoxLeftUIEvent -= MoveKidsToTarget;
    TextBoxUIController.TextBoxRightUIEvent -= MoveRabbitToTarget;

  }

  void Start()
  {
   
   rightOriginalPosition = rightRectTransform.anchoredPosition;
    originalPosition = leftRectTransform.anchoredPosition;

    _dustDeafultPosition = dustPosition.anchoredPosition;
  }

  
  
  // private void MoveFrameToRight()
  // {
  //   LeanTween.move(frame, rightPosition.anchoredPosition, 1.5f)
  //     .setOnComplete(() =>
  //       LeanTween.delayedCall(3f, MoveFrameToLeft));
  // }
  //
  // private void MoveFrameToLeft()
  // {
  //   LeanTween.move(frame, rightPosition.anchoredPosition, 1.5f)
  //     .setOnComplete(() =>
  //       LeanTween.delayedCall(3f, MoveFrameToRight));
  // }

  void MoveRabbitToTarget()
  {
    
    MoveDust();
    LeanTween.move(rightRectTransform, rigtMoveTarget1.anchoredPosition, 1f)
      .setOnComplete(() => 
      {
        Invoke("MoveBackRabbit", 1.5f); // 1.5초 뒤에 MoveToTarget2 호출
      });
  }
  private void MoveBackRabbit()
  {
    LeanTween.move(rightRectTransform, rightOriginalPosition, 1f);
  }
  void MoveKidsToTarget()
  {
    
    MoveBackDust();
    LeanTween.move(leftRectTransform, target1.anchoredPosition, 1f)
      .setOnComplete(() => 
      {
        Invoke("MoveToTarget2", 1.5f); // 1.5초 뒤에 MoveToTarget2 호출
      });
  }

 
  void MoveToTarget2()
  {
    
    LeanTween.move(leftRectTransform, target2.anchoredPosition, 1f)
      .setOnComplete(() =>
      {
        
        Invoke("MoveToOriginalPosition", 1f); // 1초 뒤에 MoveToOriginalPosition 호출
      });
  }

  void MoveToOriginalPosition()
  {
    LeanTween.move(leftRectTransform, originalPosition, 1f);
  }


  public RectTransform dustPosition;
 
  public RectTransform dustTargetPosition;
  private Vector3 _dustDeafultPosition;
  private void MoveDust()
  {
    LeanTween.move(dustPosition, dustTargetPosition.anchoredPosition, 1f);
  }

  private void MoveBackDust()
  {
    LeanTween.move(dustPosition, _dustDeafultPosition, 1f);
  }
}
