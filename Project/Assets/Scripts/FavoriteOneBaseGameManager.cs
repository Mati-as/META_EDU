using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using KoreanTyper;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class FavoriteOneBaseGameManager : Base_GameManager
{
    private enum ColorBall
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple,
        Count
    }

    private enum Position
    {
        OnPlatform, // 동물이 나와서 선택되었다는 애니메이션을 하는 곳
        FruitSpawnPosition //과일을 선택했을때, 과일이 우수수 떨어지는(생성) 되는 위치
    }

    private enum Obj
    {
        Btns,
        Animals, //default위치 등
        Fruits,
        ColorBalls,
        Platform,
        PathAndPosition // 애니메이션 경로 설정 등
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
    private Transform[] _animals;
    private Vector3[] _animalDefaultScales;
    private Quaternion[] _animalDefaultQuaternion;
    private Vector3[] _animalDefaultPositions;
    private Dictionary<int, Animator> _animators; // transform.GetinstanceID, Animator 쌍으로 구성됩니다.

    private readonly int ON_SELECTED = Animator.StringToHash("Selected");
    private readonly int ON_AWAY = Animator.StringToHash("Away");

    private Transform[] _btns;
    private Vector3[] _btnsDefaultPositions;
    private Vector3 _btnDefaultScale;
    private Dictionary<int, MeshRenderer> _meshRenderMap;
    private Dictionary<int, Color> _colorMap; // 클릭한 색깔을 캐싱합니다. 
    
    private Transform[] _fruits;
    private Transform[] _fruitsChildren;
    private Vector3[] _fruitsDefaultPositions;
    private Vector3[] _fruitChildDefaultPositions;
    private Dictionary<int, Quaternion> _fruitDefaultRotationMap;
    
    private Vector3[] _fruitDefaultSizes;
    private Dictionary<int, Vector3> _fruitForEffectDefaultSizeMap;
    private Stack<Transform>[] _fruitPoolForEffect;
    private Stack<Transform> _currentPoolForEffect; // 이펙트로 나오는 과일들을 참조유지하기 위한 스택입니다. 
    private Dictionary<int, Stack<Transform>> _fruitStackMap; // 이펙트 스택을 각 과일에 할당합니다.과일.<GetInstanceID(),Stack>구조 
    private Dictionary<int, Rigidbody> _rbMap;
    
    private Transform[] _positions;
    private Transform[] _colors;
    private int _currentlyClickedBallInstanceID;
    private Transform[] _colorBalls;
    private Transform[] _splats;
    private Vector3 _splatDefaultSize;

    private Vector3 _defaultColorBallSize;
    private Dictionary<string, string> _koreanNameMap;
    private Dictionary<int, Transform> _btnToObjectMap; // 각 버튼에 해당하는 동물과 과일을 참조합니다. 
    private Dictionary<int, Sequence> _seqMap;


    private TextMeshProUGUI[] _TMPs;
    private TextMeshProUGUI _instructionTMP;
    private WaitForSeconds _speedWs;
    private WaitForSeconds _offsetWs;

    // 게임 클릭 로직 관련
    private bool _isClickable; // 나레이션, 동물애니메이션 재생중, 초기화 끝난 후.
    private string _currentAnswerName;
    private int _currentStage = (int)Narration.Narration_Animal; // 현재질문 순서 관리 등
    
    //사운드관련
    private float _narrationPlayDelay = 1.65f;

    protected override void Init()
    {
        base.Init();
        KoreanizeName();
        
        _btnToObjectMap = new Dictionary<int, Transform>();
        _seqMap = new Dictionary<int, Sequence>();
        _animators = new Dictionary<int, Animator>();
        _meshRenderMap = new Dictionary<int, MeshRenderer>();
        _colorBalls = new Transform[(int)ColorBall.Count];
        _colorMap = new Dictionary<int, Color>();
        _fruitStackMap = new Dictionary<int, Stack<Transform>>();
        _currentPoolForEffect = new Stack<Transform>();
        _rbMap = new Dictionary<int, Rigidbody>();
        _fruitDefaultRotationMap = new Dictionary<int, Quaternion>();
        _fruitForEffectDefaultSizeMap = new Dictionary<int, Vector3>();

        _narrations = new string[(int)Narration.MaxCount];
        _narrations[(int)Narration.Narration_Animal] = "내가 가장\n좋아하는 동물은?";
        _narrations[(int)Narration.Narration_Fruit] = "내가 가장\n좋아하는 과일은?";
        _narrations[(int)Narration.Narration_Color] = "내가 가장\n좋아하는 색깔은?";


        InitializeTransforms(Obj.Btns, ref _btns);
        _btnsDefaultPositions = new Vector3[_btns.Length];
        _btnDefaultScale = _btns[0].localScale;
        for (var i = 0; i < _btns.Length; i++) _btnsDefaultPositions[i] = _btns[i].position;

        InitializeTransforms(Obj.ColorBalls, ref _colorBalls);
        _defaultColorBallSize = _colorBalls[0].localScale;
        
        InitializeTransforms(Obj.Animals, ref _animals);
        _animalDefaultPositions = new Vector3[_animals.Length];
        _animalDefaultQuaternion = new Quaternion[_animals.Length];
        _animalDefaultScales = new Vector3[_animals.Length];
        
        for (int i = 0; i < _animals.Length; i++)
        {
            _animalDefaultPositions[i] = _animals[i].position;
            _animalDefaultQuaternion[i] = _animals[i].rotation;
            _animalDefaultScales[i] = _animals[i].localScale;
        }

        InitializeTransforms(Obj.Fruits, ref _fruits);
        _fruitPoolForEffect = new Stack<Transform>[_fruits.Length];
        _fruitsDefaultPositions = new Vector3[_fruits.Length];
        _fruitChildDefaultPositions = new Vector3[_fruits.Length];
        _fruitsChildren = new Transform[_fruits.Length];
        _fruitDefaultSizes = new Vector3[_fruits.Length];
        for (var i = 0; i < _fruits.Length; i++)
        {
            _fruitsDefaultPositions[i] = _fruits[i].position;
            _fruitDefaultSizes[i] = _fruits[i].localScale;
            _fruitPoolForEffect[i] = new Stack<Transform>();
            _fruitsChildren[i] = _fruits[i].GetChild(0);
          
            _fruitChildDefaultPositions[i] = _fruitsChildren[i].position;
            //버튼을 스테이지별로 구분하기위해 InstanceID에 enum값을 더합니다. 
            _fruitStackMap.TryAdd(_btns[i].GetInstanceID() + (int)Stage.Fruit, _fruitPoolForEffect[i]);


            _fruitDefaultRotationMap.TryAdd(_fruits[i].GetInstanceID(), _fruits[i].rotation);
            _fruitDefaultRotationMap.TryAdd(_fruitsChildren[i].GetInstanceID(), _fruitsChildren[i].rotation);
            
            var fruitCount = 25;
            for (var j = 0; j < fruitCount; j++)
            {
                var clone = Instantiate(_fruits[i]);
                var rb = clone.GetComponent<Rigidbody>();
                //버튼 옆 선택가능한 과일과 달리 이펙트 효과를 위한 과일오브젝트이므로, 자유롭게 움직일 수 있도록 Constraints 전부 해제하였습니다
                rb.constraints = RigidbodyConstraints.None; 
                clone.gameObject.SetActive(false);
                _fruitPoolForEffect[i].Push(clone);
            }
        }

        InitializeTransforms(Obj.PathAndPosition, ref _positions);

        foreach (var ball in _colorBalls)
        {
            _colorMap.TryAdd(ball.GetInstanceID(), ball.GetComponent<MeshRenderer>().material.color);
            ball.localScale = Vector3.zero;
            ball.gameObject.SetActive(false);
        }

        // 버튼을 각 개체에 할당합니다.
        for (var i = 0; i < _btns.Length; i++)
        {
           
            var fruitRb = _fruits[i].GetComponent<Rigidbody>();
            var constraints = RigidbodyConstraints.None;
            constraints |= RigidbodyConstraints.FreezePositionX;
            constraints |= RigidbodyConstraints.FreezePositionZ;
            constraints |= RigidbodyConstraints.FreezeRotationX;
            constraints |= RigidbodyConstraints.FreezeRotationZ;
            fruitRb.constraints = constraints;

            _rbMap.TryAdd(_btns[i].GetInstanceID() + (int)Stage.Fruit,fruitRb);
            _btnToObjectMap.TryAdd(_btns[i].GetInstanceID() + (int)Stage.Animal, _animals[i]);
            _btnToObjectMap.TryAdd(_btns[i].GetInstanceID() + (int)Stage.Fruit, _fruits[i]);
            _meshRenderMap.TryAdd(_btns[i].GetInstanceID(), _btns[i].GetComponent<MeshRenderer>());
        }

        foreach (var animal in _animals) _animators.TryAdd(animal.GetInstanceID(), animal.GetComponent<Animator>());

        _TMPs = new TextMeshProUGUI[_btns.Length];

        for (var i = 0; i < _btns.Length; i++)
        {
            _TMPs[i] = Utils.FindChild(_btns[i].gameObject, "TMP", true).GetComponent<TextMeshProUGUI>();
        }
        
        
        var splats = Utils.FindChild(gameObject, "Splats").transform;
        _splats = new Transform[splats.childCount];
        for (var i = 0; i < splats.childCount; i++)
        {
            _splats[i] = splats.GetChild(i);

            if (i < 6) _meshRenderMap.TryAdd(_splats[i].GetInstanceID(), _splats[i].GetComponent<MeshRenderer>());

            
            _splatDefaultSize = _splats[i].localScale;
            _splats[i].transform.localScale = Vector3.zero;
            _splats[i].gameObject.SetActive(false);
        }

        var PlatfromParent = transform.GetChild((int)Obj.Platform);
        _instructionTMP = Utils.FindChild(PlatfromParent.gameObject, "TMP", true).GetComponent<TextMeshProUGUI>();
        _instructionTMP.text = string.Empty;
        foreach (var fruit in _fruits) fruit.gameObject.SetActive(false);
    }

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        foreach (var hit in GameManager_Hits)
            if ((hit.transform.gameObject.name.Contains("Btn") || hit.transform.gameObject.name.Contains("Color"))
                && _isClickable)
            {
                var randomChar = Random.Range('B', 'C' + 1);
                Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_" + randomChar);
                _currentlyClickedBallInstanceID = hit.transform.GetInstanceID();
                _isClickable = false;
                OnSelect(hit.transform);
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
            seq.SetLoops(-1, LoopType.Yoyo);
            seq.OnKill(() =>
            {
                foreach (var tmp in _TMPs) tmp.DOFade(0, 1f);
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
            case (int)Stage.Fruit:
                StartCoroutine(OnSelectCo(transform));
                break;
            case (int)Stage.Color:
                StartCoroutine(OnSelectCo(transform, 1.5f));
                break;
        }
    }

    private IEnumerator OnSelectCo(Transform colorBall, float delay = 1f)
    {
        yield return colorBall.DOShakePosition(0.75f, 0.3f, 20).WaitForCompletion();
        yield return colorBall.DOScale(Vector3.zero, 0.1f).WaitForCompletion();

        Managers.soundManager.Play(SoundManager.Sound.Narration, "Audio/나를알고표현해요/Narration_" + colorBall.gameObject.name);
        for (var i = 0; i < _splats.Length; i++)
        {
            if (i < 6)
            {
                var randomChar = Random.Range('B', 'C' + 1);
                _splats[i].gameObject.SetActive(true);
                if (_colorMap.TryGetValue(_currentlyClickedBallInstanceID, out var value))
                    _meshRenderMap[_splats[i].GetInstanceID()].material.color =value;
                _splats[i].DOScale(_splatDefaultSize, 0.085f).OnStart(() =>
                    {
                        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_A");
                    }).SetEase(Ease.OutCubic)
                    .SetDelay(Random.Range(0f, 1.5f));
               
            }
        }
        
        //delay
        yield return DOVirtual.Float(0, 0, 3f, _ => { }).WaitForCompletion();
        
        foreach (var otherColorBall in _colorBalls)
        {
            otherColorBall.DOShakePosition(0.75f, 0.3f, 20);
        }

        yield return DOVirtual.Float(0, 0, 1f, _ => { }).WaitForCompletion();
        
        foreach (var otherColorBall in _colorBalls)
        {
            otherColorBall.DOScale(Vector3.zero, 0.1f);
            
        }
        for (var i = 6; i < _splats.Length; i++)
        {
            var randomChar = Random.Range('B', 'C' + 1);
            _splats[i].gameObject.SetActive(true);
            _splats[i].DOScale(_splatDefaultSize, 0.085f).SetEase(Ease.OutCubic)
                .SetDelay(Random.Range(0.02f, 2f)).OnStart(() =>
                {
                    Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_A");
                });
       
        }


        var restartGameDelay = 3f;
        yield return DOVirtual.Float(0, 0, restartGameDelay, _ => { }).WaitForCompletion();
        Reinit();
        

    }

    private IEnumerator OnSelectCo(Transform button)
    {
        var currentStageButtonID = button.GetInstanceID() + _currentStage;
        var Obj_BtnCombined = _btnToObjectMap[button.GetInstanceID() + _currentStage];
        var Id_ObjBtnCombined = Obj_BtnCombined.GetInstanceID();
            
        if (_animators.ContainsKey(Id_ObjBtnCombined)) _animators[Id_ObjBtnCombined].SetTrigger(ON_SELECTED);
        foreach (var tmp in _TMPs)
        {
            _seqMap[tmp.GetInstanceID()].Kill();
            _seqMap[tmp.GetInstanceID()] = null;
        }
#if UNITY_EDITOR
        Debug.Log($"OBJ NAME : {Obj_BtnCombined.name}");
#endif
        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/나를알고표현해요/" + Obj_BtnCombined.name);
        
        DOVirtual.Float(0,0,_narrationPlayDelay,_=>{}).OnComplete(()=>
        {
            Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/나를알고표현해요/Narration_" + Obj_BtnCombined.name);
        });
      
    
        button.DOMove(button.position - button.up * 0.075f, 0.8f);
        _btnToObjectMap[currentStageButtonID]
            .DOMove(_positions[(int)Position.OnPlatform].position, 1.5f).WaitForCompletion();
       
        _btnToObjectMap[currentStageButtonID]
            .DOScale(_btnToObjectMap[currentStageButtonID].localScale * 1.5f, 1.5f).SetEase(Ease.InOutBounce).WaitForCompletion();


        if ((Stage)_currentStage == Stage.Fruit)
        {
            Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/Gamemaster Audio - Fun Casual Sounds/Collectibles_Items_Powerup/collect_item_sparkle_pop_04",0.3f);
            
            var rb = _rbMap[currentStageButtonID];
            rb.constraints = RigidbodyConstraints.None;
            
            while (_fruitStackMap[currentStageButtonID].Count > 0)
            {
                var fruit = _fruitStackMap[currentStageButtonID].Pop();
                fruit.position = _positions[(int)Position.FruitSpawnPosition].position;
                fruit.gameObject.SetActive(true);
                fruit.transform.localScale *= 0.75f;
                _currentPoolForEffect.Push(fruit);
                yield return DOVirtual.Float(0, 0, Random.Range(0.05f, 0.25f), _ => { }).WaitForCompletion();
            }
 
        }
         

        yield return DOVirtual.Float(0, 0, 3f, _ => { }).WaitForCompletion();

        if (_currentStage == (int)Stage.Animal)
        {
            foreach (var animal in _animals)
            {
               

                if ((Stage)_currentStage == Stage.Animal) _animators[Id_ObjBtnCombined].SetTrigger(ON_SELECTED);
            }
        }


        if (_currentStage == (int)Stage.Fruit)
        {
            foreach (var fruit in _fruits)
                fruit.DORotateQuaternion(fruit.rotation * Quaternion.Euler(0f, -180f, 0f), 1f)
                    .SetDelay(Random.Range(0f, 0.5f));
        }
          
        
      
        
        yield return DOVirtual.Float(0, 1, 1f, _ => { }).WaitForCompletion();
        
        //delay용도의 Dovirtual.Float가 아님에 주의합니다. 
        yield return DOVirtual.Float(0, 1, 1.55f, value =>
        {
            foreach (var key in _animators.Keys.ToArray())
            {
                _animators[key].speed = value;
            }
        });


        // 선택된 동물,과일 제외 사라지는 애니메이션 재생 --------------------------------------------
        if (_currentStage == (int)Stage.Animal)
            foreach (var animal in _animals)
            {
                var dealyTime = 3f;
                if (_btnToObjectMap[currentStageButtonID].GetInstanceID() == animal.GetInstanceID())
                    // _animators[animal.GetInstanceID()].SetBool(ON_AWAY, true);
                    animal.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutBounce)
                        .OnStart(() =>
                        {
                            Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_A");
                        })
                        .OnComplete(() => animal.gameObject.SetActive(false)).SetDelay(dealyTime);
                else
                    animal.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutBounce)
                        .OnStart(() =>
                        {
                            Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_A");
                        })
                        .OnComplete(() => animal.gameObject.SetActive(false)).SetDelay(Random.Range(0.1f, 1.0f));

                //_animators[animal.GetInstanceID()].SetBool(ON_AWAY,true);
            }
            
        
        
        
        // 선택된 동물,과일 제외 사라지는 애니메이션 재생 --------------------------------------------
        if (_currentStage == (int)Stage.Fruit)
        {
            foreach (var fruit in _fruits)
            {
                var dealyTime =3f;
                if (_btnToObjectMap[currentStageButtonID].GetInstanceID() == fruit.GetInstanceID())
                {
                
                    fruit.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutBounce)
                        .OnStart(()=>{ Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_A");
                        })
                        .OnComplete(() => fruit.gameObject.SetActive(false)).SetDelay(dealyTime);
                }
                else
                {
                    fruit.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutBounce)
                        .OnComplete(() => fruit.gameObject.SetActive(false))
                        .SetDelay(Random.Range(0.1f,1.0f));
                }
            }
             

            foreach (var fruit in _currentPoolForEffect)
            {
                fruit.DOScale(Vector3.zero, 0.125f).SetEase(Ease.InOutBounce).SetDelay(Random.Range(0.1f,1.3f)).OnStart(
                    () =>
                    {
                        fruit.gameObject.SetActive(false);
                        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_A",0.8f);
                    });
            }

            while (_currentPoolForEffect.Count > 0)
            {
                _fruitStackMap[currentStageButtonID].Push(_currentPoolForEffect.Pop());
            }
        }
        

        yield return DOVirtual.Float(0, 0, 3f, _ => { }).WaitForCompletion();

        if (_currentStage == (int)Stage.Animal) 
        {
            TypeIn(_narrations[(int)Narration.Narration_Fruit]);
            DOVirtual.Float(0,0,_narrationPlayDelay,_=>{}).OnComplete(()=>
            {
                Managers.soundManager.Play(SoundManager.Sound.Narration, "Audio/나를알고표현해요/Narration_Fruit");
            });
        }
        if (_currentStage == (int)Stage.Fruit) 
        {
            TypeIn(_narrations[(int)Narration.Narration_Color]);
            DOVirtual.Float(0,0,_narrationPlayDelay,_=>{}).OnComplete(()=>
            {
                Managers.soundManager.Play(SoundManager.Sound.Narration, "Audio/나를알고표현해요/Narration_Color");
            });
        }

        yield return DOVirtual.Float(0, 0, 2f, _ => { }).WaitForCompletion();

        if (_currentStage == (int)Stage.Animal)
            foreach (var fruit in _fruits)
                fruit.gameObject.SetActive(true);

        if (_currentStage == (int)Stage.Fruit)
        {
            foreach (var btn in _btns)
                btn.DOScale(Vector3.zero, 0.12f)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(Random.Range(0.05f, 0.015f))
                    .OnComplete(() => { btn.gameObject.SetActive(false); });

            foreach (var ball in _colorBalls)
            {
                ball.gameObject.SetActive(true);
                ball.DOScale(_defaultColorBallSize, 0.4f)
                    .SetEase(Ease.InOutBounce)
                    .SetDelay(Random.Range(0.6f, 0.7f));
            }
        }


     
        PlayTMPScaleAnimation();
        _isClickable = true;
        StageInit();
            
        
    }

    private void Reinit()
    {
        _isClickable = false;
        _currentStage = 0;
        
        _TMPs[0].text = "병아리";
        _TMPs[1].text = "말";
        _TMPs[2].text = "강아지";
        _TMPs[3].text = "오리";
        _TMPs[4].text = "돼지";
        _TMPs[5].text = "양";

      
        
        for (int i = 0; i < _animals.Length; i++)
        {
            _animals[i].gameObject.SetActive(true);
            _animals[i].position = _animalDefaultPositions[i] ;
            _animals[i].rotation = _animalDefaultQuaternion[i];
            _animals[i].localScale = _animalDefaultScales[i];
        }

        for (int i = 0; i < _fruits.Length; i++)
        {
            _fruits[i].gameObject.SetActive(false);
            _fruits[i].localScale = _fruitDefaultSizes[i];
            
            _fruits[i].position = _fruitsDefaultPositions[i] ;
            _fruitsChildren[i].position = _fruitChildDefaultPositions[i];

            _fruits[i].rotation = _fruitDefaultRotationMap[_fruits[i].GetInstanceID()];
            _fruitsChildren[i].rotation = _fruitDefaultRotationMap[_fruitsChildren[i].GetInstanceID()];
        }

        foreach (var key in _rbMap.Keys.ToArray())
        {
            _rbMap[key].velocity = Vector3.zero;
        }
        
        for (int i = 0; i < _fruits.Length; i++)
        {
            foreach (var fruit in _fruitPoolForEffect[i])
            {
                
                fruit.localScale = _fruitDefaultSizes[i];
            }
           
        }
        for (int i = 0; i < _btns.Length; i++)
        {
            _btns[i].localScale = _btnDefaultScale;
            _btns[i].gameObject.SetActive(true);
        }

 

        
        
        var restartDelay = 2f;
        DOVirtual.Float(0, 0, restartDelay, _ => { }).OnComplete(() =>
        {
            foreach (var splat in _splats)
            {
                splat.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InOutSine);
            }
            
            TypeIn(_narrations[(int)Narration.Narration_Animal]);
            Managers.soundManager.Play(SoundManager.Sound.Narration, "Audio/나를알고표현해요/Narration_Animal");
        });

    }
    
    /// <summary>
    ///     각 Stage가 끝날때, 다음 스테이지로 넘어가기 위한 초기화 작업을 수행합니다.
    /// </summary>
    private void StageInit()
    {
        _currentStage++;
        
        if (_currentStage == (int)Stage.Fruit)
        {
            foreach (var animal in _animals)
            {
                _animators[animal.GetInstanceID()].SetBool(ON_AWAY,false);
            }
            
            _TMPs[0].text = "사과";
            _TMPs[1].text = "오렌지";
            _TMPs[2].text = "바나나";
            _TMPs[3].text = "레몬";
            _TMPs[4].text = "수박";
            _TMPs[5].text = "포도";
        }

        if (_currentStage == (int)Stage.Color)
        {
            _TMPs[0].text = "빨강";
            _TMPs[1].text = "노랑";
            _TMPs[2].text = "주황";
            _TMPs[3].text = "초록";
            _TMPs[4].text = "파랑";
            _TMPs[5].text = "보라";
        }

        for (var i = 0; i < _btns.Length; i++) _btns[i].DOMove(_btnsDefaultPositions[i], 1).SetEase(Ease.InOutExpo);

        
        foreach (var tmp in _TMPs) tmp.DOFade(1, 1f);
       
        
        foreach (var id in _rbMap.Keys.ToArray())
        {
            _rbMap[id].constraints |= RigidbodyConstraints.None;
            _rbMap[id].constraints |= RigidbodyConstraints.FreezePositionX;
            _rbMap[id].constraints |= RigidbodyConstraints.FreezePositionZ;
            _rbMap[id].constraints |= RigidbodyConstraints.FreezeRotationX;
            _rbMap[id].constraints |= RigidbodyConstraints.FreezeRotationZ;
        }
       
    }


    private void InitializeTransforms(Obj objType, ref Transform[] transformsArray)
    {
        var parent = transform.GetChild((int)objType);
        transformsArray = new Transform[parent.childCount];

        for (var i = 0; i < parent.childCount; i++) transformsArray[i] = parent.GetChild(i);
    }

    private void KoreanizeName()
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
        DOVirtual.Float(0,0,_narrationPlayDelay,_=>{}).OnComplete(()=>
        {
            Managers.soundManager.Play(SoundManager.Sound.Narration, "Audio/나를알고표현해요/Narration_Animal");
        });
        PlayTMPScaleAnimation();
    }


    private string[] _narrations;


    public void TypeIn(string message)
    {
        if (_speedWs == null) _speedWs = new WaitForSeconds(0.1f);

        if (_offsetWs == null) _offsetWs = new WaitForSeconds(1f);

        StartCoroutine(TypeInCo(_instructionTMP, message, _offsetWs, _speedWs));
    }

    public IEnumerator TypeInCo(TextMeshProUGUI tmp, string str,
        WaitForSeconds offsetWaitForSeceonds, WaitForSeconds speedWaitForSeconds)
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