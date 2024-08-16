using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = System.Random;

public class ShapePathfinding_GameManager : IGameManager
{
    /*
     *1.랜덤으로 쉐이프, 컬러 할당하기
     *2.주사위 판별 로직 만들기
     *3.주사위 던지기 로직 만들기 (시간초) 버튼 클릭방식으로
     *4.카운트 및 돌아가는 로직만들기
     *5.이펙트 추가
     *6.사운드 추가
     *7.클릭성공시 애니메이션 추가
     */
    private Dictionary<Pathfinder_GameObject, string> _gameObjectNames;


    //TransformID, Meshrenderer캐싱으로 클릭시 ID만으로 접근
    private Dictionary<int, MeshRenderer> _meshRenderMap;
    private Dictionary<string, string> _koreanMap; //영어-한국어 쌍 , UI 표시용

    private Transform[][] _steps;
    private Vector3 _defaultScale;
    private MeshRenderer[][] _shapeMeshRenderers;
    private MeshRenderer[][] _colorMeshRenderers;

    private Material[] _shapeMat;
    private Material[] _colorMat;

    private Transform _dice;

    private Transform _diceBtnLeft;
    private Transform _diceBtnRight;

    private Vector3 _btnDefaultPosLeft;
    private Vector3 _btnDefaultPosRight;


    private ParticleSystem _correctPs;

    private string _currentShape; //주사위 결과에 따라 맞춰야하는 Shape

    private readonly float _rollHeight = 3.5f;
    private Vector3[] _diceRollPath;
    private bool _isDiceBtnClickable = true;
    private bool _isStepClickable;

    private Vector3 _diceDetector; //해당위치에서 아래로 Ray발사 및 모양판별 


    //  private static event Action _OnDiceBtnClicked;

    private int _columnCount;
    private int _rowCount;
    
    
    //UI Tmp
    private TextMeshProUGUI TMP_Instruction;
    

    private enum Pathfinder_GameObject
    {
        Steps,
        Btn_Dice,
        Dice,
        DiceDetector,
        Max
    }

    private enum ShapeMat
    {
        Circle,
        Square,
        Triangle,
        Heart,
        MaxCount
    }

    private enum ColorMat
    {
        Red,
        Blue,
        Green,
        Violet,
        MaxCount
    }


    protected override void Init()
    {
        base.Init();

        CacheEnumNames();
        SetObjects();

        LoadResource();

        SetSteps();
        SuffleAndSet();

        TMP_Instruction = GameObject.Find("TMP_Instruction").GetComponent<TextMeshProUGUI>();
        TMP_Instruction.text = string.Empty;
    }

    private readonly float BTN_DOWN_DEPTH = 0.036f;

    public override void OnRaySynced()
    {
        base.OnRaySynced();
        if (!isStartButtonClicked) return;

        PressButtion();
        PlayStepAnim();
    }


    private int _currentClickedId;

    private void PlayStepAnim()
    {
        foreach (var hit in GameManager_Hits)
            if (hit.transform.gameObject.name.Contains("Step"))
            {
                var currentID = hit.transform.GetInstanceID();
                _currentClickedId = currentID; // 중복실행방지 위해 방어적코드. 
                if (_meshRenderMap.ContainsKey(currentID))
                {
#if UNITY_EDITOR
                    Debug.Log(
                        $"this mat name: {_meshRenderMap[currentID].material.name},  currentShape(dice)Name: {_currentShape}");
#endif
                    if (!_meshRenderMap.ContainsKey(currentID) || _meshRenderMap[currentID] == null) return;

                    if (_meshRenderMap[currentID].material.name.Contains(_currentShape))
                    {
#if UNITY_EDITOR
                        Debug.Log("Correct Step --- step starting to move...");
#endif

                     
               

                        if (_isStepClickable)
                        {
                            var randomChar = (char)UnityEngine.Random.Range('A', 'F' + 1);
                            Managers.soundManager.Play(SoundManager.Sound.Effect, $"Audio/BB008_U/Click_{randomChar}");
                            
                            
                            _correctPs.Stop();
                            _correctPs.transform.position = hit.transform.position;
                            _correctPs.Play();


                            _isStepClickable = false;
                            hit.transform.DOScale(_defaultScale * 1.5f, 0.2f).SetEase(Ease.InBounce)
                                .OnComplete(() =>
                                {
                                    hit.transform.DOScale(_defaultScale, 0.21f).SetEase(Ease.InOutSine)
                                        .OnComplete(() => { _isStepClickable = true; });
                                });
                        }
                    }
                }

                else
                {
#if UNITY_EDITOR
                    Debug.Log("no meshRenderer with that transform ID...");
#endif
                }
            }
    }

    private void PressButtion()
    {
        foreach (var hit in GameManager_Hits)
            if (hit.transform.gameObject.name.Contains(_gameObjectNames[Pathfinder_GameObject.Btn_Dice]))
            {
                if (_isDiceBtnClickable)
                {
                    if (hit.transform.gameObject.name.Contains("Left"))
                        _diceBtnLeft.DOMove(_btnDefaultPosLeft + Vector3.down * BTN_DOWN_DEPTH, 0.22F)
                            .SetEase(Ease.InSine)
                            .OnComplete(() =>
                            {
                                _diceBtnLeft.DOMove(_btnDefaultPosLeft, 0.14F).SetEase(Ease.InOutBack)
                                    .SetDelay(0.07F);
                            });
                    else
                        _diceBtnRight.DOMove(_btnDefaultPosRight + Vector3.down * BTN_DOWN_DEPTH, 0.22F)
                            .OnComplete(() =>
                            {
                                _diceBtnRight.DOMove(_btnDefaultPosRight, 0.14F).SetEase(Ease.InOutBack)
                                    .SetDelay(0.07F);
                            });

                    //버튼 클릭 효과음. 
                    Managers.soundManager.Play(SoundManager.Sound.Effect,
                        "Audio/Gamemaster Audio - Fun Casual Sounds/User_Interface_Menu/ui_menu_button_beep_19");

                    RollDice();
                }

                return;
            }
    }

    private void CacheEnumNames()
    {
        _gameObjectNames = new Dictionary<Pathfinder_GameObject, string>();
        _koreanMap = new Dictionary<string, string>();
        
        _koreanMap.Add("Triangle","세모");
        _koreanMap.Add("Square","네모");
        _koreanMap.Add("Circle","동그라미");
        _koreanMap.Add("Heart","하트");

        _gameObjectNames = Enum.GetValues(typeof(Pathfinder_GameObject))
            .Cast<Pathfinder_GameObject>()
            .ToDictionary(e => e, e => e.ToString());
    }


    private void LoadResource()
    {
        var psPrefab = Resources.Load<ParticleSystem>("게임별분류/기본컨텐츠/ShapePathFinder/CFX_Correct");
        _correctPs = Instantiate(psPrefab);

        _colorMat = new Material[(int)ColorMat.MaxCount];
        _shapeMat = new Material[(int)ShapeMat.MaxCount];

        // 컬러머터리얼 리소스 로딩 ----------------------------------------------
        var matRed = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Red");
        _colorMat[(int)ColorMat.Red] = matRed;

        var matBlue = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Blue");
        _colorMat[(int)ColorMat.Blue] = matBlue;

        var matGreen = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Green");
        _colorMat[(int)ColorMat.Green] = matGreen;

        var matViolet = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Violet");
        _colorMat[(int)ColorMat.Violet] = matViolet;


        // 도형머터리얼 리소스 로딩 ----------------------------------------------
        var matCircle = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Circle");
        _shapeMat[(int)ShapeMat.Circle] = matCircle;

        var matSquare = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Square");
        _shapeMat[(int)ShapeMat.Square] = matSquare;

        var matTriangle = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Triangle");
        _shapeMat[(int)ShapeMat.Triangle] = matTriangle;

        var matHeart = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Heart");
        _shapeMat[(int)ShapeMat.Heart] = matHeart;
    }


    private void SetObjects()
    {
        _meshRenderMap = new Dictionary<int, MeshRenderer>();

        _dice = transform.Find(Pathfinder_GameObject.Dice.ToString());

        _diceBtnLeft = transform.Find(Pathfinder_GameObject.Btn_Dice.ToString()).GetChild(0);
        _diceBtnRight = transform.Find(Pathfinder_GameObject.Btn_Dice.ToString()).GetChild(1);

        _btnDefaultPosLeft = _diceBtnLeft.position;
        _btnDefaultPosRight = _diceBtnRight.position;
        _diceDetector = transform.Find(Pathfinder_GameObject.DiceDetector.ToString()).position;

        //주사위 돌리는 과정은 고정경로.. 
        _diceRollPath = new Vector3[3];
        _diceRollPath[0] = _dice.transform.position;
        _diceRollPath[1] = _rollHeight * Vector3.up + _diceRollPath[0];
        _diceRollPath[2] = _diceRollPath[0] + Vector3.up * 2f; // 주사위 경로진행시 땅에 충돌방지를 위한 오프셋 설정입니다. 
    }

    private void SetSteps()
    {
        var stepParent = GameObject.Find("Steps").transform;

        _columnCount = stepParent.childCount;
        _steps = new Transform[_columnCount][];
        _shapeMeshRenderers = new MeshRenderer[_columnCount][];
        _colorMeshRenderers = new MeshRenderer[_columnCount][];

        _rowCount = stepParent.GetChild(0).childCount;

        for (var i = 0; i < _columnCount; i++)
        {
            _steps[i] = new Transform[_rowCount];
            _shapeMeshRenderers[i] = new MeshRenderer[_rowCount];
            _colorMeshRenderers[i] = new MeshRenderer[_rowCount];

            for (var k = 0; k < _rowCount; k++)
            {
                _shapeMeshRenderers[i][k] = stepParent.GetChild(i).GetChild(k).GetComponent<MeshRenderer>();
                _colorMeshRenderers[i][k] = stepParent.GetChild(i).GetChild(k).GetChild(0).GetComponent<MeshRenderer>();
                _steps[i][k] = stepParent.GetChild(i).GetChild(k);


                // shape으로만 게임로직 판별합니다. 
                _meshRenderMap.TryAdd(_steps[i][k].GetInstanceID(), _shapeMeshRenderers[i][k]);
            }
        }

        _defaultScale = _steps[0][0].localScale; //참고) Scale은 모든 발판이 동일합니다. 
    }

    // 컬럼마다 랜덤하게 색상과, 모양을 섞어 하나씩 배치하는 메소드입니다
    private void SuffleAndSet()
    {
        var rng = new Random();

        for (var i = 0; i < _columnCount; i++)
        {
            var thisShapeMat = new Material[_shapeMat.Length];
            var thisColorMatArray = new Material[_colorMat.Length];

            Array.Copy(_shapeMat, thisShapeMat, _shapeMat.Length);
            Array.Copy(_colorMat, thisColorMatArray, _colorMat.Length);


            Shuffle(thisShapeMat, rng);
            Shuffle(thisColorMatArray, rng);

            for (var k = 0; k < _rowCount; k++)
            {
                _shapeMeshRenderers[i][k].material = thisShapeMat[k];
                _colorMeshRenderers[i][k].material = thisColorMatArray[k];
            }
        }
    }

    // Fisher–Yates shuffle방식 구현
    private void Shuffle<T>(T[] array, Random rng)
    {
        var n = array.Length;

        for (var i = n - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            var temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    /*
     * 아래는 RollDice 애니메이션 순서 설명입니다
     * 1.주사위를 랜덤한 회전각도로 돌립니다.
     * 2.던진위치로 다시 돌아옵니다. Delay 0.3sec
     * 3.디텍더로 어떤면의 주사위가 나왔는지 검사합니다. Delay 0.5sec
     */
    private void RollDice()
    {
        _isDiceBtnClickable = false;
        _isStepClickable = false;

        var seq = DOTween.Sequence();


        seq.Append(_dice.DORotateQuaternion(_dice.rotation *
                                            Quaternion.Euler(UnityEngine.Random.Range(300, 480),
                                                UnityEngine.Random.Range(300, 480),
                                                UnityEngine.Random.Range(300, 480)), 0.9f).SetEase(Ease.InOutSine)
            .OnStart(() => { _dice.DOPath(_diceRollPath, 1.1f).SetEase(Ease.InExpo);}));
        
        seq.AppendInterval(2.2f);
        
        seq.Append(_dice.DOMove(_diceRollPath[0], 0.2f)
            .OnStart(() =>
            {
                Managers.soundManager.Play(SoundManager.Sound.Effect,
                    "Audio/Gamemaster Audio - Fun Casual Sounds/Comedy_Cartoon/beep_zap_fun_03", 0.5f);
            })
            .OnComplete(() =>
            {
                DOVirtual.Float(0, 0, 0.5f, _ => { }).OnComplete(() =>
                {
                    _isStepClickable = true;
                    DetectDice();
                });
                DOVirtual.Float(0, 0, 5f, _ => { }).OnComplete(() =>
                {
                    _isDiceBtnClickable = true;
                });
                
                
            }));
        seq.AppendInterval(1.5f);
        seq.AppendCallback(() =>
        {
            foreach (var key in _meshRenderMap.Keys.ToArray())
            {
                if (_meshRenderMap[key].material.name.Contains(_currentShape))
                {
                    var stepScaleSeq= DOTween.Sequence();
                    stepScaleSeq.Append(_meshRenderMap[key].transform.DOScale(_defaultScale * 0.7f, 0.15f));
                    stepScaleSeq.AppendInterval(0.11f);
                    stepScaleSeq.Append(_meshRenderMap[key].transform.DOScale(_defaultScale, 0.15f));
                    stepScaleSeq.SetLoops(12, LoopType.Restart);
        
                }   
            }
            
            TextAnimPlay();
            seq.Play();
        });
    }
   

    private void TextAnimPlay()
    {
        TMP_Instruction.transform.localScale = Vector3.zero;
        TMP_Instruction.text = _koreanMap[_currentShape];

        var tmpScaleSeq = DOTween.Sequence();
        tmpScaleSeq.AppendCallback(() =>
        {
            TMP_Instruction.transform.DOScale(Vector3.one, 0.3f);
        });

        tmpScaleSeq.AppendInterval(3f);
        tmpScaleSeq.AppendCallback(() =>
        {
            TMP_Instruction.transform.DOScale(Vector3.zero, 0.3f);
        });
        tmpScaleSeq.Play();

    }



    private RaycastHit[] _surfaceDetectHits;

    private void DetectDice()
    {
        // Perform the raycast
        _surfaceDetectHits = Physics.RaycastAll(_diceDetector, Vector3.down, 0.7f);
        {
            foreach (var hit in _surfaceDetectHits)
            {
                if (hit.transform.gameObject.name.Contains("Surface_"))
                {
                    //머터리얼 이름과 비교하기위해 string 수정. 
                    _currentShape = hit.transform.gameObject.name.Substring("Surface_".Length);
#if UNITY_EDITOR
                    Debug.Log($"_currentShape = {_currentShape} ");
                    return;
#endif
                }
                
#if UNITY_EDITOR
                Debug.Log("no surface with Collider ");
                Debug.Log($"hit name:  {hit.transform.gameObject.name} ");
#endif
            }
        }

        
    }

    private void OnDrawGizmos()
    {
        if (_diceDetector != null)
        {
            Gizmos.color = Color.blue; // Set the color of the Gizmos
            var direction = Vector3.down * 0.7f; // Direction and length of the ray
            Gizmos.DrawRay(_diceDetector, direction); // Draw the ray in the Scene View

            // Optionally, draw a small sphere at the origin
            Gizmos.DrawSphere(_diceDetector, 0.1f);
        }
    }
    
        


}