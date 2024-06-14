using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using KoreanTyper;
using Unity.VisualScripting;
using Sequence = DG.Tweening.Sequence;

public class FavoriteOne_GameManager : IGameManager
{
    
    private enum Positions
    {
        OnPlatform, // 동물이 나와서 선택되었다는 애니메이션을 하는 곳
    }
    private enum Obj
    {
        Btns, 
        Animals, //default위치 등
        Fruits,
        Platform,
        PathAndPosition// 애니메이션 경로 설정 등
    }
    private enum Stage
    {
        Animal,
        Fruit,
        Color,
        MaxCount
    }

    private enum Narration
    {
        Narration_Animal,
        Narration_Fruit,
        Narration_Color,
        MaxCount
    }
    
    private Vector3[] _defaultPositions;
    
    
    private Transform[] _btns;
    private Vector3[] _btnsDefaultPositions;
    private Transform[] _animals;
    private Transform[] _fruits;
    private Transform[] _positions;
    private Transform[] _colors;
    private Dictionary<string, string> _koreanNameMap;
    private Dictionary<int, Transform> _btnToObjectMap; // 버튼마다 각각 동물,과일,색깔 할당 
    private Dictionary<int, Sequence> _seqMap;
    
    
    private TextMeshProUGUI[] _TMPs;
    private TextMeshProUGUI _instructionTMP;
    private WaitForSeconds _speedWs;
    private WaitForSeconds _offsetWs;
    
    // 게임 클릭 로직 관련
    private bool _isClickable;  // 나레이션, 동물애니메이션 재생중, 초기화 끝난 후.
    private string _currentAnswerName;
    private int _currentStage = (int)Narration.Narration_Animal; // 현재질문 순서 관리 등
    
    protected override void Init()
    {
        base.Init();
        _btnToObjectMap = new Dictionary<int, Transform>();
        _seqMap = new Dictionary<int, Sequence>();
        
        _narrations = new string[(int)Narration.MaxCount];
        _narrations[(int)Narration.Narration_Animal] = "내가 가장\n좋아하는 동물은?";
        _narrations[(int)Narration.Narration_Fruit] ="내가 가장\n좋아하는 과일은?";
        _narrations[(int)Narration.Narration_Color] ="내가 가장\n좋아하는 색깔은?";
        
        
        KoreanizeName();
        
        InitializeTransforms(Obj.Btns, ref _btns);
        _btnsDefaultPositions = new Vector3[_btns.Length];
        for (int i = 0; i < _btns.Length; i++)
        {
            _btnsDefaultPositions[i] = _btns[i].position;
        }
        
        InitializeTransforms(Obj.Animals, ref _animals);
        InitializeTransforms(Obj.Fruits, ref _fruits);
        InitializeTransforms(Obj.PathAndPosition,ref _positions);

        // 버튼을 각 개체에 할당합니다.
        for (int i = 0; i < _btns.Length; i++)
        {
            _btnToObjectMap.TryAdd(_btns[i].GetInstanceID()+ (int)Stage.Animal, _animals[i]);
            _btnToObjectMap.TryAdd(_btns[i].GetInstanceID() + (int)Stage.Fruit, _fruits[i]);
        }

        _TMPs = new TextMeshProUGUI[_btns.Length];
        
        for (int i =0; i<_btns.Length; i++)
        {
            _TMPs[i] = Utils.FindChild(_btns[i].gameObject,"TMP",recursive:true).GetComponent<TextMeshProUGUI>();
        }

        var PlatfromParent = transform.GetChild((int)Obj.Platform);
        _instructionTMP =Utils.FindChild(PlatfromParent.gameObject,"TMP",recursive:true).GetComponent<TextMeshProUGUI>();

        foreach (var fruit in _fruits)
        {
            fruit.gameObject.SetActive(false);
        }
    }

    protected override void OnRaySynced()
    {
        base.OnRaySynced();
        foreach (var hit in GameManager_Hits)
        {
            if (hit.transform.gameObject.name.Contains("Btn") && _isClickable)
            {
                _isClickable = false;
                OnSelect(hit.transform);
            }
        }
    }

    private void PlayTMPScaleAnimation()
    {
        foreach (var tmp in _TMPs)
        {
            var seq = DOTween.Sequence();
            seq.Append(tmp.transform.DOScale(1.1f, 0.2f));
            seq.SetDelay(0.2f);
            seq.Append(tmp.transform.DOScale(0.9f, 0.2f));
            seq.SetLoops(-1,LoopType.Yoyo);
            seq.OnKill(() =>
            {
                foreach (var tmp in _TMPs)
                {
                    tmp.DOFade(0, 1f);
                }
            });
            _seqMap.TryAdd(tmp.GetInstanceID(), seq);
            _seqMap[tmp.GetInstanceID()] = seq;
        }
    }
    private void OnSelect(Transform transform)
    {
        switch (_currentStage)
        {
            case (int)Stage.Animal:
                StartCoroutine(OnSelectCo(transform));
                break;
            case  (int)Stage.Fruit:
                StartCoroutine(OnSelectCo(transform));
                break;
            case (int)Stage.Color:
                StartCoroutine(OnSelectCo(transform));
                break;
            default:
                break;    
        }
    }

    private IEnumerator OnSelectCo(Transform btnCombinedObj)
    {

        // switch (_currentStage)
        // {
        //     case (int)Stage.Animal:
        //         break;
        //     case  (int)Stage.Fruit:
        //         break;
        //     case (int)Stage.Color:
        //         break;
        //     default:
        //         break;    
        // }
        
        foreach (var tmp in _TMPs)
        {
            _seqMap[tmp.GetInstanceID()].Kill();
            _seqMap[tmp.GetInstanceID()] = null;
        }

        btnCombinedObj.DOMove(btnCombinedObj.position - btnCombinedObj.up * 0.075f, 0.8f);
        _btnToObjectMap[btnCombinedObj.GetInstanceID() + _currentStage]
            .DOMove(_positions[(int)Positions.OnPlatform].position, 2f);
     
        yield return DOVirtual.Float(0, 0, 3f, _ => { }).WaitForCompletion();
        
        
        foreach (var animal in _animals)
        {
            animal.DORotateQuaternion(animal.rotation * Quaternion.Euler(0f, -180f, 0f), 1f)
                .SetDelay(Random.Range(0f,0.5f));
        }
         
        
        if (_currentStage == (int)Stage.Fruit)
        {
            foreach (var fruit in _fruits)
            {
                fruit.DORotateQuaternion(fruit.rotation * Quaternion.Euler(0f, -180f, 0f), 1f)
                    .SetDelay(Random.Range(0f,0.5f));
            }

        }
        yield return DOVirtual.Float(0, 0, 1f, _ => { }).WaitForCompletion();
        
        foreach (var animal in _animals)
        {
            animal.DOMove(animal.forward * 1f, 5f).SetEase(Ease.InOutSine)
                .OnComplete(()=>animal.gameObject.SetActive(false));
        }

        if (_currentStage == (int)Stage.Fruit)
        {
            foreach (var fruit in _fruits)
            {
                fruit.DOMove(fruit.forward * 1f, 5f).SetEase(Ease.InOutSine)
                    .OnComplete(()=>fruit.gameObject.SetActive(false));
            }

        }
     
        yield return DOVirtual.Float(0, 0, 3f, _ => { }).WaitForCompletion();
        
        if (_currentStage == (int)Stage.Animal) TypeIn(_narrations[(int)Narration.Narration_Fruit]);
        if (_currentStage == (int)Stage.Fruit) TypeIn(_narrations[(int)Narration.Narration_Color]);
        
        yield return DOVirtual.Float(0, 0, 3f, _ => { }).WaitForCompletion();

        if (_currentStage == (int)Stage.Animal)
        {
            foreach (var fruit in _fruits)
            {
                fruit.gameObject.SetActive(true);
            }
        }
        

        ReInit();
        PlayTMPScaleAnimation();
        _isClickable = true;
        _currentStage++;
    }

    private void ReInit()
    {
        
        if (_currentStage == (int)Stage.Animal)
        {
            _TMPs[0].text = "사과";
            _TMPs[1].text = "오렌지";
            _TMPs[2].text = "바나나";
            _TMPs[3].text = "레몬";
            _TMPs[4].text = "수박";
            _TMPs[5].text = "포도";  
        }
        
        if (_currentStage == (int)Stage.Fruit)
        {
            _TMPs[0].text = "빨강";
            _TMPs[1].text = "노랑";
            _TMPs[2].text = "주황";
            _TMPs[3].text = "초록";
            _TMPs[4].text = "파랑";
            _TMPs[5].text = "보라";  
        }

        
        
        for (int i = 0; i < _btns.Length; i++)
        {
            _btns[i].DOMove(_btnsDefaultPositions[i], 1).SetEase(Ease.InOutExpo);
        }
        foreach (var tmp in _TMPs)
        {
            tmp.DOFade(1, 1f);
        }
    }

 
    
    void InitializeTransforms(Obj objType, ref Transform[] transformsArray)
    {
        var parent = transform.GetChild((int)objType);
        transformsArray = new Transform[parent.childCount];

        for (int i = 0; i < parent.childCount; i++)
        {
            transformsArray[i] = parent.GetChild(i);
          
        }
    }

    void KoreanizeName()
    {
        _koreanNameMap = new Dictionary<string, string>();
        _koreanNameMap.TryAdd("Horse", "말");
        _koreanNameMap.TryAdd("Chick", "병아리");
        _koreanNameMap.TryAdd("Dog", "강아지");
        _koreanNameMap.TryAdd("Duck", "오리");
        _koreanNameMap.TryAdd("Pig", "돼지");
        _koreanNameMap.TryAdd("Lamb", "양");

    }

    protected override void OnStartButtonClicked()
    {
        base.OnStartButtonClicked();
        TypeIn(_narrations[(int)Narration.Narration_Animal]);
        PlayTMPScaleAnimation();
    }


    private string[] _narrations;
    
    
    public void TypeIn(string message)
    {
        if (_speedWs == null)
        {
            _speedWs = new WaitForSeconds(0.1f);
        }

        if (_offsetWs == null)
        {
            _offsetWs = new WaitForSeconds(1f);
        }
        
        StartCoroutine(TypeInCo(_instructionTMP,message,_offsetWs,_speedWs));
    }
        
    public IEnumerator TypeInCo(TextMeshProUGUI tmp,string str,
        WaitForSeconds offsetWaitForSeceonds,WaitForSeconds speedWaitForSeconds)
    {
        tmp.text = ""; 
        yield return offsetWaitForSeceonds; 
            
        var strTypingLength = str.GetTypingLength();
        for (var i = 0; i <= strTypingLength; i++)
        {
            tmp.text = str.Typing(i); 
            yield return speedWaitForSeconds;
        }
        
        
        yield return offsetWaitForSeceonds;
        _isClickable = true;
        yield return new WaitForNextFrameUnit();

    }
}
