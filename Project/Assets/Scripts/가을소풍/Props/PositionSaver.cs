using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class PositionSaver : MonoBehaviour
{
    private Transform _lookAtPosition;
  
    
    void Start()
    {
        GameManager.onGameStartEvent -= SavePosition;
        GameManager.onGameStartEvent += SavePosition;
        _lookAtPosition = GetComponentInChildren<Transform>();
    }
    void OnDestroy()
    {
        GameManager.onGameStartEvent -= SavePosition;
    }
    
    /// <summary>
    /// ScritableObject에 직접 Transform을 할당할 수 없는 이슈로 인해 다음과 같이 저장.
    /// </summary>
    private void SavePosition()
    {
        AnimalData.SPOTLIGHT_POSITION_FOR_ANIMAL = transform;
        AnimalData.LOOK_AT_POSITION = _lookAtPosition;
    }


}
