using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class Basic_Music_ClockwisePartilceController : MonoBehaviour
{
    public GameObject pathToRotate;
    public Transform lookAt;
    private Vector3[] _path;
    public float rotateDuration;
    private ParticleSystem _ps;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();

        var pathCount = pathToRotate.transform.childCount;

        _path = new Vector3[pathCount];

        for (var i = 0; i < pathCount; i++) _path[i] = pathToRotate.transform.GetChild(i).position;

        transform.position = _path[0];

        
        Music_BubbleController.onPatternStart -= DoPath;
        Music_BubbleController.onPatternStart += DoPath;

    }


    private void OnDestroy()
    {
        Music_BubbleController.onPatternStart -= DoPath;
    }


    private void DoPath()
    {
        transform.DOPath(_path, rotateDuration, PathType.CatmullRom)
            .OnStart(() =>
            {
                _ps.Play();
            })
            .OnUpdate(() => { transform.DOLookAt(lookAt.position, 0.01f); })
            .OnComplete(() =>
            {
                transform.position = _path[0];
                _ps.Stop();
                
            });
    }
}