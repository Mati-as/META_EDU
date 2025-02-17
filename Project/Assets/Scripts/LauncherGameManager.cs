using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauncherGameManager : Base_GameManager
{
  protected override void Init()
  {
    base.Init();
    waitForClickableInGameRay = 0.35f;
    
    ManageProjectSettings(100,0.55f);
  }


  /// <summary>
  /// 런쳐에서는 모든레이어클릭가능
  /// </summary>
  protected override void SetLayerMask()
  {
    LayerMask allLayers = ~0;  // 모든 레이어를 포함
    layerMask = allLayers;
  }
}
