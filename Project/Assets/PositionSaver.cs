using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PositionSaver : MonoBehaviour
{

    
    public Transform spotlightPosition;
    public Transform lookAtPosition;
    
    void Start()
    {
        GameManager.onGameStartEvent += SavePosition;
    }
    
    /// <summary>
    /// ScritableObject에 직접 Transform을 할당할 수 없는 이슈로 인해 다음과 같이 저장.
    /// </summary>
    private void SavePosition()
    {
        AnimalData.SPOTLIGHT_POSITION_FOR_ANIMAL = spotlightPosition;
        AnimalData.LOOK_AT_POSITION = lookAtPosition;
    }
}
