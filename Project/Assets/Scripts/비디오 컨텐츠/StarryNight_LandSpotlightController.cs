using System;
using DG.Tweening;
using UnityEngine;

public class StarryNight_LandSpotlightController : MonoBehaviour
{
    [Header("Referrence")] [SerializeField]
    private StarryNight_MoonController _starryNight_MoonController;


    private void Start()
    {
        transform.position = pathTargets[0].position;
        _defaultPosition = transform.position;
        
        StarryNight_MoonController.OnPathStart -= StartPathAnimation;
        StarryNight_MoonController.OnPathStart += StartPathAnimation;
    }


    private void OnDestroy()
    {
        StarryNight_MoonController.OnPathStart -= StartPathAnimation;
    }
    

    public Transform[] pathTargets;
    private Vector3 _defaultPosition;

    private void StartPathAnimation()
    {
        var path = new Vector3[3];
        path[0] = pathTargets[0].position;
        path[1] = pathTargets[1].position;
        path[2] = pathTargets[2].position;

        // Path 설정 (여기에서는 원형 경로를 설정)
        transform.DOPath(path, _starryNight_MoonController.randomDuration, PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .OnComplete(() => { transform.position = _defaultPosition; });
    }
}