using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Basic_Music_ClockwisePartilceController : MonoBehaviour
{
    public GameObject pathToRotate;
    public Transform lookAt;
    private Vector3[] _path;
    public float rotateDuration;
    void Start()
    {
        int pathCount = pathToRotate.transform.childCount;

        _path = new Vector3[pathCount];
        
        for (int i = 0; i < pathCount; i++)
        {
            _path[i] = pathToRotate.transform.GetChild(i).position;
        }

        transform.position = _path[0];
      


        DoPath();

    }

    private void DoPath()
    {
        transform.DOPath(_path, rotateDuration, PathType.CatmullRom)
            .OnUpdate(() =>
            {
                transform.DOLookAt(lookAt.position, 0.01f);
            })
            .OnComplete(() => DoPath());
    }
}
