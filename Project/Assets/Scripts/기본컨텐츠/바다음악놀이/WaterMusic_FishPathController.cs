using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class WaterMusic_FishPathController : MonoBehaviour
{
  private Vector3[] _path;
  private float _duration;

  private void Awake()
  {
    var path = GameObject.Find("Path" + gameObject.name).transform;
    var pathCount = path.childCount;
    _path = new Vector3[pathCount];

    for (int i = 0; i < pathCount; i++)
    {
      _path[i] = path.GetChild(i).position;
    }

    _duration = UnityEngine.Random.Range(5f, 12f);
      
    transform.position = _path[0];
    transform.DOPath(_path, _duration,PathType.CatmullRom).SetEase(Ease.InOutSine)
      .OnStart(() =>
      {
        _duration = UnityEngine.Random.Range(5f, 12f);
      })
      .SetLoops(-1,LoopType.Restart)
      .SetLookAt(0.01f);
  }
}
