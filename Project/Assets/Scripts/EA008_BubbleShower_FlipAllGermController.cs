using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;
public class EA008_BubbleShower_FlipAllGermController : Base_GameManager
{
  
    enum PrintType
    {
        Hand,
        Foot,
        Max
    }

    private Vector3[] _appearablePoints;
    
    
    private Transform[] _blackPrints;
    private float _defaultScale;
    private MeshRenderer[] _meshRenderers;
    private Color _defaultColor;
    private Sequence[] _blinkSeqs;
    private int _firstClickedID;
    
    private bool _seqChanged;
    private bool _isDisappearing;
    private bool _isClickable;
    
    public static event Action onAllBlackPrintClicked;
    private EA008_BubbleShower_GameManager _gm;


    private void RoundInit()
    {

        _firstClickedID = -987654321;
        _seqChanged = false;
        _isDisappearing = false;
        _isClickable = false;
    }

    protected override void Init()
    {
        base.Init();
        EA008_BubbleShower_GameManager.onStart -= OnStart;
        EA008_BubbleShower_GameManager.onStart += OnStart;

        EA008_BubbleShower_GameManager.onRoundFinished -= DisappearOnRestart;
        EA008_BubbleShower_GameManager.onRoundFinished += DisappearOnRestart;
            
        
        EA008_BubbleShower_GameManager.roundInit -= RoundInit;
        EA008_BubbleShower_GameManager.roundInit += RoundInit;
        
        _meshRenderers = new MeshRenderer[(int)PrintType.Max];
        _blackPrints = new Transform[transform.childCount];
        _blinkSeqs = new Sequence[(int)PrintType.Max];

        _gm = GameObject.FindWithTag("GameManager").GetComponent<EA008_BubbleShower_GameManager>();
        
;        for (int i = 0; i < transform.childCount; i++)
        {
            _blackPrints[i] = transform.GetChild(i);
            _defaultScale = _blackPrints[i].localScale.x;
            _blackPrints[i].localScale = Vector3.zero;
            _blackPrints[i].gameObject.SetActive(false);
            _meshRenderers[i] = _blackPrints[i].gameObject.GetComponent<MeshRenderer>();
            _defaultColor = _blackPrints[i].gameObject.GetComponent<MeshRenderer>().material.color;
        }
        var appearablePoints = GameObject.Find("BlackPrintsAppearblePoints");
        int childCount = appearablePoints.transform.childCount;
        
        _appearablePoints = new Vector3[childCount];
        for (int i = 0; i < childCount; ++i)
        {
            _appearablePoints[i] = appearablePoints.transform.GetChild(i).position;
        }
           
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EA008_BubbleShower_GameManager.onStart -= OnStart;
        EA008_BubbleShower_GameManager.onRoundFinished -= DisappearOnRestart;
        EA008_BubbleShower_GameManager.roundInit -= RoundInit;
  
    }

    private float  _blackPrintAppearableTime  = 1f;
    public void OnStart()
    {

        StartCoroutine(SequenceAnimations(_blackPrintAppearableTime));
    }

    private IEnumerator SequenceAnimations(float delayTime)
    {
        foreach (var print in _blackPrints) print.gameObject.SetActive(true);

        int handPosIndex = Random.Range(0,_appearablePoints.Length);
        _blackPrints[(int)PrintType.Hand].position = _appearablePoints[handPosIndex];
        
        yield return DOVirtual
            .Float(0, _defaultScale, 1,
                scale => { _blackPrints[(int)PrintType.Hand].localScale = Vector3.one * scale; })
            .OnStart(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect,
                    "Audio/기본컨텐츠/HandFlip2/BlackAppear", 0.3f);
            })
            .OnComplete(() =>
            {
                DOVirtual.Float(0, 0, 2.5f, _ => { })
                    .OnComplete(() => { _isClickable = true; });
            })
            .SetDelay(Random.Range(delayTime, delayTime + 0.5f));

        int footPosIndex = Random.Range(0,_appearablePoints.Length);
        
        while (footPosIndex == handPosIndex)
        {
            footPosIndex = Random.Range(0,_appearablePoints.Length);
        }
        
        _blackPrints[(int)PrintType.Foot].position = _appearablePoints[footPosIndex];
        
        yield return DOVirtual
            .Float(0, _defaultScale, 1,
                scale => { _blackPrints[(int)PrintType.Foot].localScale = Vector3.one * scale; })
            .SetDelay(Random.Range(delayTime, delayTime + 0.5f))
            .OnComplete(() => { _isClickable = true;})
            .OnStart(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect,
                    "Audio/기본컨텐츠/HandFlip2/BlackAppear", 0.3f);
            })
            .WaitForCompletion();


        Blink();
    }
    
    
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        
        OnSync(GameManager_Ray);
    }

     
    private RaycastHit hit;
    
    //검은 손발바닥 카운트시, 이름 비교를 통한 중복방지 

    private void OnSync(Ray ray)
    {
        if (Physics.Raycast(ray, out hit))
        {
            if (!_isClickable)
            {
#if UNITY_EDITOR
 //Debug.Log("Black Print isn't currently Clickable!-----------------------------------");
#endif
                return;
            }
            
            if (hit.transform.gameObject.name.ToLower().Contains("black"))
            {
                if (!_seqChanged)
                {
                  
                    _firstClickedID = hit.transform.gameObject.GetInstanceID();

                    for (var i = 0; i < 2; i++)
                    {
                        _blinkSeqs[i].Kill();
                    }
                    if (_seqChanged) return;
                    
                    _seqChanged  = true;

                    Blink(0.15f,0.15f);
                    Debug.Log($"clicked ID{_firstClickedID}");

                    //검정색을 클릭하도록 유도하는 급박한 느낌의 사운드 추가 
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/BlackPrint",0.5f);
                }
                else
                {
                    if (!_isDisappearing && hit.transform.gameObject.GetInstanceID() != _firstClickedID&&_firstClickedID!=0)
                    {
                        _isDisappearing = true;
                        Debug.Log($"disAppear! current Clicked ID{hit.transform.gameObject.GetInstanceID()}");
                        Disappear();

                        //사라지는 사운드 추가
                        Managers.Sound.Play(SoundManager.Sound.Effect,
                            "Audio/Gamemaster Audio - Fun Casual Sounds/User_Interface_Menu/ui_menu_button_click_05", 0.3f);
                    }
                      
                    
                }
            }
        }
      
    }

   
    private void Blink(float duration=0.9f,float interval =0.15f)
    {


        Color targetColor = _gm.CurrentColorPair[Random.Range(0,2)] *   1.35f;
        
        _blinkSeqs[(int)PrintType.Hand] = DOTween.Sequence();
        _blinkSeqs[(int)PrintType.Foot] = DOTween.Sequence();
        float delay = interval; 
        
        // 손에 대한 깜박임 설정
        _blinkSeqs[(int)PrintType.Hand] = DOTween.Sequence();
        _blinkSeqs[(int)PrintType.Hand]
            .Append(_meshRenderers[(int)PrintType.Hand].material.DOColor(targetColor, duration).SetEase(Ease.Linear))
            .AppendInterval(interval) // 지연 시간 추가
            .Append(_meshRenderers[(int)PrintType.Hand].material.DOColor(_defaultColor, duration).SetEase(Ease.Linear))
            .SetLoops(-1, LoopType.Yoyo); // Yoyo 방식으로 무한 반복

        // 발에 대한 깜박임 설정
        _blinkSeqs[(int)PrintType.Foot] = DOTween.Sequence();
        _blinkSeqs[(int)PrintType.Foot]
            .Append(_meshRenderers[(int)PrintType.Foot].material.DOColor(targetColor, duration).SetEase(Ease.Linear))
            .AppendInterval(interval) // 지연 시간 추가
            .Append(_meshRenderers[(int)PrintType.Foot].material.DOColor(_defaultColor, duration).SetEase(Ease.Linear))
            .SetLoops(-1, LoopType.Yoyo); // Yoyo 방식으로 무한 반복

        _blinkSeqs[(int)PrintType.Hand].Play();
        _blinkSeqs[(int)PrintType.Foot].Play();

     
    }

    private void Disappear()
    {
        onAllBlackPrintClicked?.Invoke();
        
        DOVirtual.Float(_defaultScale,0, 1,
            scale => { _blackPrints[(int)PrintType.Hand].localScale = Vector3.one * scale; });


        DOVirtual
            .Float(_defaultScale, 0, 1,
                scale => { _blackPrints[(int)PrintType.Foot].localScale = Vector3.one * scale; });

    }
    
    private void DisappearOnRestart()
    {
        
        DOVirtual.Float(_blackPrints[(int)PrintType.Hand].localScale.x,0, 1,
            scale => { _blackPrints[(int)PrintType.Hand].localScale = Vector3.one * scale; });


        DOVirtual
            .Float(_blackPrints[(int)PrintType.Hand].localScale.x, 0, 1,
                scale => { _blackPrints[(int)PrintType.Foot].localScale = Vector3.one * scale; });

    }
    
}
