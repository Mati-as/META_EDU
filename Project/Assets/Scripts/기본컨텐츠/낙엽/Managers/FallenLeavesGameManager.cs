using System;
using UnityEngine;
using DG.Tweening;

public class FallenLeavesGameManager : MonoBehaviour
{
    public int TARGET_FRAME;

   public static int gameProgressSpeed;

    private void Start()
    {
        TARGET_FRAME = 60;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        DOTween.SetTweensCapacity(2000,200);
    }
    

}