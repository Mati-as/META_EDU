using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauncherRaySynachronizer : Base_GameManager
{
  protected override void Init()
  {
    base.Init();
    waitForClickableFloat = 0.35f;
    
    ManageProjectSettings(100,1f);
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
