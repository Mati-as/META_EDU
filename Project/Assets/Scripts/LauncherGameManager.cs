using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 화면비율 조정 등 필수작업 수행 ---------- 042525
/// 초기화 순서 보장을 위해 프리팹 호출 방식으로 생성합니다. 
/// </summary>
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
