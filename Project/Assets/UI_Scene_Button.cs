using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Scene_Button : MonoBehaviour
{
    [SerializeField]
  private Message_anim_controller _animController;
  private Button _button;

  private void Awake()
  {
      _animController = FindActiveMessageAnimController();
  }

  private void Start()
  {
      _button = GetComponent<Button>();
      _button.onClick.AddListener(OnClicked);
 
  }
  
  private Message_anim_controller FindActiveMessageAnimController()
  {
     
      foreach (Transform child in transform.parent)
      {
          if (child.gameObject.activeInHierarchy)
          {
              Message_anim_controller controller = child.GetComponent<Message_anim_controller>();
              if (controller != null)
              {
                  return controller; 
              }
          }
      }
      return null; 
  }

  private void OnClicked()
  {
      if (_animController != null)
      {
          _animController.Animation_Off();
      }
      else
      {
          #if UNITY_EDITOR
          Debug.Log("AnimalController is null");
          #endif
      }
  }
}
