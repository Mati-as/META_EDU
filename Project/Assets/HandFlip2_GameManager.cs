using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandFootFlip2_GameManager : IGameManager
{

    enum ColorSide
    {
        ColorA,
        ColorB,
        ColorCount
    }
    private Color _currentUnifiedColor =Color.red;
    private Color _previousUniColor = Color.black;
    private Print[] _prints;
    private int PRINTS_COUNT;
    private Vector3 _rotateVector;

    private bool _isPrintAppearFinished;
   


    //쌍이되는 컬러를  String으로 할당하여, 색상이름(string)에 따라 제어.
    private Dictionary<int, Print> _PrintMap;
    private Dictionary<string, Color> _colorPair;
    private Dictionary<int, MeshRenderer> _meshRendererMap;
    private Dictionary<int, MeshRenderer> _childMeshRendererMap;
    private Dictionary<int, Sequence> _moveSequence;
    private MeshRenderer[] _meshRenderers;
    
    private RaycastHit hit;
    private int COLOR_COUNT = 5;

    [SerializeField] private Color[] colorOptions; //색상조합목록

    private Color[] _currentColorPair;    //현재라운드의 색상 조합 (빨강-파랑, 오렌지-초록...)
    private Color _colorA;
    private Color _colorB;


    protected override void Init()
    {
        base.Init();

        _PrintMap = new Dictionary<int, Print>();
        _colorPair = new Dictionary<string, Color>();
        _meshRendererMap = new Dictionary<int, MeshRenderer>();
        _childMeshRendererMap = new Dictionary<int, MeshRenderer>();
        _rotateVector = new Vector3(180, 0, 0);

        Debug.Log($"(start)color option length: {colorOptions.Length}");
        var printsParent = GameObject.Find("Prints");

        if (printsParent == null)
        {
            Debug.LogError("Prints GameObject not found in the scene.");
            return;
        }

        _currentColorPair = new Color[2];
        SetColor(0);
        
//반드시 게임 오브젝트의 갯수는 짝수..
#if UNITY_EDITOR
Debug.Assert(PRINTS_COUNT % 2 == 0);
#endif
        PRINTS_COUNT = printsParent.transform.childCount;

        _prints = new Print[PRINTS_COUNT];



        for (var i = 0; i < PRINTS_COUNT; i++)
        {
            _prints[i] = new Print
            {
                printObj = printsParent.transform.GetChild(i).gameObject,
                defaultVector = printsParent.transform.GetChild(i).rotation.eulerAngles,
                currentColor = _currentColorPair[ i % (int)ColorSide.ColorCount],
                defaultSize =  printsParent.transform.GetChild(i).gameObject.transform.localScale
            };

            


#if UNITY_EDITOR
            Debug.Log($"colorname: {printsParent.transform.GetChild(i).gameObject.name.Substring(5)}");
#endif



            //Print 캐싱, Flip에서는 InstaceID를 기반으로 Prints를 참조 및 제어한다.
            var currentTransform = printsParent.transform.GetChild(i);
            //Transform Instance ID가 아닌 GameObject의 Instance ID를 참조할것에 주의합니다
            var currentInstanceID = currentTransform.gameObject.GetInstanceID();

            _PrintMap.TryAdd(currentInstanceID, _prints[i]);

            //MeshRenderer 캐싱, Instace ID로 MeshRenderer제어
            MeshRenderer meshRenderer = currentTransform.GetComponent<MeshRenderer>();
            _meshRendererMap.TryAdd(currentInstanceID, meshRenderer);

            meshRenderer.material.color = _currentColorPair[i % (int)ColorSide.ColorCount];
            
            
            meshRenderer = currentTransform.GetChild(0).GetComponentInChildren<MeshRenderer>();
            
            _childMeshRendererMap.TryAdd(currentInstanceID,meshRenderer);
            
            meshRenderer.material.color=_currentColorPair[i % (int)ColorSide.ColorCount];

            printsParent.transform.GetChild(i).gameObject.transform.localScale = Vector3.zero;

        }

        
        UI_Scene_Button.onBtnShut -= OnButtonClicked;
        UI_Scene_Button.onBtnShut += OnButtonClicked;
    }

    private void OnDestroy()
    {
        UI_Scene_Button.onBtnShut -= OnButtonClicked;
    }

    private void SetColor(int round)
    {
        _currentColorPair[(int)ColorSide.ColorA] = colorOptions[round % 3];
        _currentColorPair[(int)ColorSide.ColorB] = colorOptions[round % 3 + 1];

        _colorA = _currentColorPair[(int)ColorSide.ColorA];
        _colorB = _currentColorPair[(int)ColorSide.ColorB];
    }
    protected override void OnRaySynced()
    {
        base.OnRaySynced();

        if (!isStartButtonClicked) return;
        if (!_isPrintAppearFinished) return;
        
        FlipAndChangeColor(GameManager_Ray);
        //  ChangeColor(GameManager_Ray);
    }

    private void OnButtonClicked()
    {
        PrintsAppear();
    }

    private void PrintsAppear()
    {
        foreach(var print in _prints)
        {
            print.printObj.transform
                .DOScale(print.defaultSize, 0.5f)
                .SetEase(Ease.InBounce)
                .SetDelay(Random.Range(1, 1.8f))
                .OnComplete(() =>
                {
                    DOVirtual.Float(0, 0, 2f, _ => { })
                        .OnComplete(() =>
                        {
                            _isPrintAppearFinished = true;
                        });
                });
        }
    }

    
     private void FlipAndChangeColor(Ray ray)
    {
         if(Physics.Raycast(ray ,out hit))
         {
             var currentInstanceID = hit.transform.gameObject.GetInstanceID();
             
            if (_PrintMap.ContainsKey(currentInstanceID) )
            {
                if (_PrintMap[currentInstanceID].seq.IsActive()||_PrintMap[currentInstanceID].isCurrentlyFlipping)
                {
                    Debug.Log("The seq is currently Active! Click later..");
                    return;
                }
            }
             
            _PrintMap[currentInstanceID].seq = DOTween.Sequence();
         
                _PrintMap[currentInstanceID].seq
                    .Append(
                        hit.transform
                            .DOLocalRotate(_rotateVector + _PrintMap[currentInstanceID].printObj.transform.rotation.eulerAngles, 0.38f)
                            .SetEase(Ease.InOutQuint)
                            .OnStart(() =>
                            {
                                
                                //트윈 도중 중복실행 방지
                                _PrintMap[currentInstanceID].isCurrentlyFlipping = true; 
                                
                                char randomChar = (char)Random.Range('A', 'F'+ 1);
                                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFootFlip/Click_"+randomChar,
                                    0.3f);
                                
                                
                                if (_PrintMap[currentInstanceID].currentColor == _colorA)
                                {
                                    _PrintMap[currentInstanceID].currentColor = _colorB;
                                        
                                    Debug.Log("Changing to ColorA");
                                    _meshRendererMap[currentInstanceID].material
                                        .DOColor(_colorB, 0.2f)
                                        .SetDelay(0.235f);
            
                                    _childMeshRendererMap[currentInstanceID].material
                                        .DOColor(_colorB, 0.2f)
                                        .SetDelay(0.235f);
                                }
                                else 
                                {
                                    _PrintMap[currentInstanceID].currentColor = _colorA;
                                    
                                    Debug.Log("Changing to ColorB");
                                    
                                    _meshRendererMap[currentInstanceID].material
                                        .DOColor(_colorA, 0.2f)
                                        .SetDelay(0.235f);
            
                                    _childMeshRendererMap[currentInstanceID].material
                                        .DOColor(_colorA, 0.2f)
                                        .SetDelay(0.235f);
                                }

                              
                            })
                            .OnComplete(() =>
                            {
                                    //트윈 도중 중복실행 방지
                                    _PrintMap[currentInstanceID].isCurrentlyFlipping = false;
        
                            }));
    
            DOVirtual
                .Float(0, 1, 0.08f, _ => { })
                .OnComplete(() =>
                {
                 
                });

        _PrintMap[currentInstanceID].seq.Play();

         }
         else
         {
#if UNITY_EDITOR
Debug.Log("Flipping Failed");        
#endif
         }


     
        
    }
     
     private void ShuffleColors()
     {
         for (var i = 0; i < colorOptions.Length; i++)
         {
             var temp = colorOptions[i];
             var randomIndex = Random.Range(i, colorOptions.Length);
             colorOptions[i] = colorOptions[randomIndex];
             colorOptions[randomIndex] = temp;
         }
     }

    
     public class Print
     {
         public GameObject printObj;
         public bool side;
         public bool isCurrentlyFlipping;
         public Vector3 defaultVector;
         public Vector3 defaultSize;
         
         public Sequence seq;

         public Color currentColor;
     
         
     }
}
