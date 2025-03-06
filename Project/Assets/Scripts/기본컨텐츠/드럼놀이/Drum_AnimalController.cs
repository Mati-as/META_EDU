
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
///기능요약
///1. 랜덤 시간 간격으로 동물의 움직임을 컨트롤
///2. 카메라 떨림기능, camera.Main사용
/// </summary>
public class Drum_AnimalController : MonoBehaviour
{
  
    private float _elapsed;
    [Range(1,30)]
    public float _interval = 10;

    private Vector3 _animalMoveStartPoint;
    private Vector3 _animalMoveArrivalPoint;
    private Sequence _seq;

    private void Awake()
    {
        var animalMovePosition = GameObject.Find("AnimalMovePositions").GetComponent<Transform>();
        _animalMoveStartPoint = animalMovePosition.GetChild(0).position;
        _animalMoveArrivalPoint = animalMovePosition.GetChild(1).position;

        transform.position = _animalMoveStartPoint;
    }

    // Update is called once per frame
    private bool _isAnimationg;
    void Update()
    {

        if (!_isAnimationg)
        {
            _elapsed += Time.deltaTime;
        }
      

        if (_elapsed > _interval && !_isAnimationg)
        {
            _isAnimationg = true;
            StartCoroutine(AnimalMoveCoroutine());
        }
    }

    private IEnumerator AnimalMoveCoroutine()
    {
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Drum/Herd", 0.3f);
        
        var delayBeforeCameraShaking = 1.5f;
        yield return DOVirtual.Float(0, 0, delayBeforeCameraShaking,_  => { }).WaitForCompletion();
        
        transform.position = _animalMoveStartPoint;
        Camera.main.DOShakePosition(6f, 0.01f, 5);
    
        var delayBeforeAnimalsAppear = 1f;
        yield return DOVirtual.Float(0, 0, delayBeforeAnimalsAppear,_  => { }).WaitForCompletion();
        
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Drum/Elephant", 0.3f);
        transform.DOMove(_animalMoveArrivalPoint, 5f).OnComplete(() =>
        {
            Managers.Sound.Stop(SoundManager.Sound.Effect);
            _interval = Random.Range(20, 25);
        });
        
    }
}


