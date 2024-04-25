using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mono.Cecil;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Twister_GameManager : IGameManager
{
    private Transform[][] _spotTransforms;

    private Dictionary<int, SpriteRenderer> _spriteMap;
    private Sprite[] _spotSsprites;
    private Dictionary<int, TextMeshProUGUI> _textMap;
    
    
    private Dictionary<int, Sequence> _scaleSeq;
    private Vector3 _defaultSize;
    
    
    private Dictionary<int, bool> _isOnSteppedMap;
    
    // 트위스터 스팟 클릭시 ,bool값이 true,false로 진동하는 것이 아닌, 지속클릭시 true로만 유지되도록하기위한 코루틴 선언입니다.
    private Dictionary<int, Coroutine> _coroutineMap;

    private int _stepRowCount;
    private int _stepColumnCount;
    private readonly string SPRITE_PATH = "게임별분류/기본컨텐츠/Twister/T_Twister";

    private int _currentSpotGroupIndex;
    private int _currentSpotElementIndex;

    
    //애니메이션 중복재생 및 인덱스 중복증가 방지용
    private bool _isMovingOnToNextGroup;

    
    private bool _isEverySpotClickedInGroup;
    
    protected override void Init()
    {
        base.Init();
        
        _spriteMap = new Dictionary<int, SpriteRenderer>();
        _textMap = new Dictionary<int, TextMeshProUGUI>();
        _isOnSteppedMap = new Dictionary<int, bool>();
        _scaleSeq = new Dictionary<int, Sequence>();
        _spotSsprites = new Sprite[3];
        _coroutineMap = new Dictionary<int, Coroutine>();
        
        _spotSsprites[0]  = Resources.Load<Sprite>(SPRITE_PATH + "A");
        _spotSsprites[1]  = Resources.Load<Sprite>(SPRITE_PATH + "B");
        _spotSsprites[2]  = Resources.Load<Sprite>(SPRITE_PATH + "C");
        
        BindObjects();
       
    }

    private void Start()
    {
        SetTwisterSpots();
        DoScaleAnimation(_currentSpotGroupIndex,_currentSpotElementIndex);
    }

    private void BindObjects()
    {
        _stepColumnCount = transform.childCount;
        _spotTransforms = new Transform[_stepColumnCount][];
        
        for (int c = 0; c < _stepColumnCount; c++)
        {
            //3,2구조로 행렬구조로 치환시 가변적인 형태임에 주의합니다.
            _stepRowCount = transform.GetChild(c).childCount;
            _spotTransforms[c] = new Transform[_stepRowCount];
            
            for(int r = 0; r < _stepRowCount ;r++ )
            {
                _spotTransforms[c][r] = transform.GetChild(c).GetChild(r);

                var transformID = _spotTransforms[c][r].GetInstanceID();
                
                _spriteMap.Add(transformID, _spotTransforms[c][r].GetComponent<SpriteRenderer>());
                _textMap.Add(transformID, _spotTransforms[c][r].GetComponentInChildren<TextMeshProUGUI>());
                _isOnSteppedMap.Add(transformID,false);
            }
        }

        _defaultSize = _spotTransforms[0][0].localScale;
        _defaultRotation = _spotTransforms[0][0].localRotation;
    }

    private void Reset()
    {
        _currentSpotGroupIndex = 0;
        SetTwisterSpots();
        foreach (var spots in _spotTransforms)
        {
            foreach (var spot in spots)
            {
                spot.DOScale(_defaultSize, 1f).SetEase(Ease.InOutBounce).SetDelay(Random.Range(2.0f,2.5f));
            }
        }

        DOVirtual.Float(0, 0, 3f, _ => { }).OnComplete(() =>
        {
            DoScaleAnimation(_currentSpotGroupIndex,_currentSpotElementIndex);
        });
    }

    private Quaternion _defaultRotation;
    private void SetTwisterSpots()
    {
      
        // 그룹마다 손손손,발발발과 같이 손발이 각각 두개이상 나오는것을 방지하기 위한 변수 선언입니다.
        // 씬에있는 객체에 할당하지 않고, 로직판별로만 사용합니다. (객체할당의 경우 _textMap 변수를 활용
        for (int c = 0; c < _stepColumnCount; c++)
        {
            Utils.Shuffle(_spotSsprites);
            string[] textListForEachGroup = new string[3];
            
            
            _stepRowCount = transform.GetChild(c).childCount;// 그룹마다 크기가 다르므로 자식개수 체크 로직 필요 
            for(int r = 0; r < _stepRowCount ;r++ )
            {  
                
                var id = _spotTransforms[c][r].GetInstanceID();
                
                _spotTransforms[c][r].localRotation =_defaultRotation * Quaternion.Euler(0, 0,Random.Range(-25,25));

                //마지막 인덱스이고, 0,1인덱스가 "손""손" 혹은 "발""발"일 경우.
                if (r == 2 && textListForEachGroup[0] == textListForEachGroup[1])
                {
                    _textMap[id].text = textListForEachGroup[0] == "손" ? "발" : "손";
                }
                else
                {
                    _textMap[id].text = Random.Range(0, 100) > 50 ? "손" : "발";
                    
                    textListForEachGroup[r] = _textMap[id].text;
                }

                _spriteMap[id].sprite = _spotSsprites[r];

            }
        }
        
    }

    protected override void OnRaySynced()
    {
        base.OnRaySynced();

        foreach (var hit in GameManager_Hits)
            if (hit.transform.gameObject.name.Contains("T_Twister"))
            {

                var id = hit.transform.GetInstanceID();
              
                IsOnStepCheck(id);
                
                if (_currentSpotElementIndex + 1 < _spotTransforms[_currentSpotGroupIndex].Length)
                {
                    _currentSpotElementIndex++;
                    DoScaleAnimation(_currentSpotGroupIndex,_currentSpotElementIndex);
                }
                else
                {
                    CheckAndMoveOnToNextGroup();
                }
          
              
                       
            }
    }

    private void IsOnStepCheck(int id)
    {
        if (_coroutineMap.ContainsKey(id))
        {
            StopCoroutine(_coroutineMap[id]);
        }
                
        var _cr = StartCoroutine(IsStepOnCheck(id));
        _coroutineMap.TryAdd(id, _cr);

    }

    private void CheckAndMoveOnToNextGroup()
    {

        
        foreach (var spot in _spotTransforms[_currentSpotGroupIndex])
        {
            var spotId = spot.GetInstanceID();
            if (!_isOnSteppedMap[spotId]) return;
        }

        if (_isMovingOnToNextGroup) return;
        _isMovingOnToNextGroup = true;
        
        for (int i = 0; i < _spotTransforms[_currentSpotGroupIndex].Length; i++)
        {

            _spotTransforms[_currentSpotGroupIndex][i].DOScale(Vector3.zero, 1f).SetEase(Ease.InOutBounce);
            var spotId = _spotTransforms[_currentSpotGroupIndex][i].GetInstanceID();
            _scaleSeq[spotId].Kill();
            _scaleSeq[spotId] = null;
        }

        DOVirtual.Float(0, 0, 1f,_ => { }).OnComplete(() =>
        {
            // 초기화로직 -----------------------------------------------------------
            _currentSpotGroupIndex++;
            _currentSpotElementIndex = 0;
            _isEverySpotClickedInGroup = false;
            _isMovingOnToNextGroup = false;
            
            if (_currentSpotGroupIndex >= _stepColumnCount)
            {
                Reset();
                return;
            }
            
            DoScaleAnimation(_currentSpotGroupIndex,_currentSpotElementIndex);
        });
     
    }


  
    private void DoScaleAnimation(int groupIndex, int elementIndex)
    {
        if (_isEverySpotClickedInGroup) return;
        
        Sequence scaleSeq = DOTween.Sequence();
        scaleSeq.Append(_spotTransforms[groupIndex][elementIndex].DOScale(_defaultSize * 1.1f, 0.5f)).SetEase(Ease.InOutSine);
        scaleSeq.AppendInterval(0.1f);
        scaleSeq.Append(_spotTransforms[groupIndex][elementIndex].DOScale(_defaultSize * 0.9f, 0.5f)).SetEase(Ease.InOutSine);
        scaleSeq.SetLoops(-1,LoopType.Yoyo);

        var id = _spotTransforms[groupIndex][elementIndex].GetInstanceID();
        _scaleSeq[id] = scaleSeq;

        
    
    }


    /// <summary>
    ///     지속적으로 발판위에 플레이어가 올라가있는지 체크하기 위해, 발판에 물체가 없는경우 약간의 딜레이를 주어 논리값을 수정합니다.
    /// </summary>
    /// <returns></returns>
    private IEnumerator IsStepOnCheck(int id, float delay = 0.6f)
    {
        _isOnSteppedMap[id] = true;
        yield return DOVirtual.Float(0, 0, delay, _ => { }).WaitForCompletion();
        _isOnSteppedMap[id] = false;
    }
    
    
    
    // private void Shuffle<T>(T[] array) where T : UnityEngine.Object
    // {
    //     for (int i = 0; i < array.Length; i++)
    //     {
    //         T temp = array[i];
    //         int randomIndex = Random.Range(i, array.Length);
    //         array[i] = array[randomIndex];
    //         array[randomIndex] = temp;
    //     }
    // }

    
}
