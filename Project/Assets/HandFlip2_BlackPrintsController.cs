using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

public class HandFlip2_BlackPrintsController : IGameManager
{
    enum PrintType
    {
        Hand,
        Foot,
        Max
    }

    private Transform[] _blackPrints;
    private float _defaultScale;
    private MeshRenderer[] _meshRenderers;
    private Color _defaultColor;
    private float _intensity = 1.3f;
    private Sequence[] _blinkSeqs;


    
    public static event Action onAllBlackPrintClicked;
    private HandFlip2_GameManager _gm;
    

    protected override void Init()
    {
        base.Init();
        HandFlip2_GameManager.onStart += OnStart;
        _meshRenderers = new MeshRenderer[(int)PrintType.Max];
        _blackPrints = new Transform[transform.childCount];
        _blinkSeqs = new Sequence[(int)PrintType.Max];

        _gm = GameObject.FindWithTag("GameManager").GetComponent<HandFlip2_GameManager>();
        
;        for (int i = 0; i < transform.childCount; i++)
        {
            _blackPrints[i] = transform.GetChild(i);
            _defaultScale = _blackPrints[i].localScale.x;
            _blackPrints[i].localScale = Vector3.zero;
            _blackPrints[i].gameObject.SetActive(false);
            _meshRenderers[i] = _blackPrints[i].gameObject.GetComponent<MeshRenderer>();
            _defaultColor = _blackPrints[i].gameObject.GetComponent<MeshRenderer>().material.color;
        }

       
    }

    private float  _blackPrintAppearableTime  = 1f;
    public void OnStart()
    {
#if UNITY_EDITOR
        Debug.Log("Button Click Bind{BlackPrintsController}");
#endif
        StartCoroutine(SequenceAnimations(_blackPrintAppearableTime));
    }

    private IEnumerator SequenceAnimations(float delayTime)
    {
        foreach (var print in _blackPrints) print.gameObject.SetActive(true);

        yield return DOVirtual
            .Float(0, _defaultScale, 1,
                scale => { _blackPrints[(int)PrintType.Hand].localScale = Vector3.one * scale; })
            .SetDelay(Random.Range(delayTime, delayTime+0.5f));

        yield return DOVirtual
            .Float(0, _defaultScale, 1,
                scale => { _blackPrints[(int)PrintType.Foot].localScale = Vector3.one * scale; })
            .SetDelay(Random.Range(delayTime, delayTime+0.5f))
            .WaitForCompletion();

      
        
      
            Blink(1.35f);
    }
    
    
    protected override void OnRaySynced()
    {
        base.OnRaySynced();
        
        OnSync(GameManager_Ray);
    }

     
    private RaycastHit hit;
    
    //검은 손발바닥 카운트시, 이름 비교를 통한 중복방지 
    private int _firstClickedID;
    private int count;
    private bool _seqChanged;
    private bool _isDisappearing;

    private void OnSync(Ray ray)
    {
        if (Physics.Raycast(ray, out hit))
        {
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
                    Blink(0.55f);
                    Debug.Log($"clicked ID{_firstClickedID}");

                    //검정색을 클릭하도록 유도하는 급박한 느낌의 사운드 추가 
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/BlackPrint",0.5f);
                }
                else
                {
                    if (!_isDisappearing && hit.transform.gameObject.GetInstanceID() != _firstClickedID)
                    {
                        _isDisappearing = true;
                        Debug.Log($"disAppear! current Clicked ID{hit.transform.gameObject.GetInstanceID()}");
                        Disappear();

                        //사라지는 사운드 추가
                        Managers.Sound.Play(SoundManager.Sound.Effect, "", 0.3f);
                    }
                }
            }
        }
      
    }

   
    private void Blink(float interval)
    {


        Color targetColor = _gm.CurrentColorPair[Random.Range(0,2)];
        
        _blinkSeqs[(int)PrintType.Hand] = DOTween.Sequence();
        _blinkSeqs[(int)PrintType.Foot] = DOTween.Sequence();
        float delay = interval; 
        
        // 손에 대한 깜박임 설정
        _blinkSeqs[(int)PrintType.Hand] = DOTween.Sequence();
        _blinkSeqs[(int)PrintType.Hand]
            .Append(_meshRenderers[(int)PrintType.Hand].material.DOColor(targetColor, interval).SetEase(Ease.Linear))
            .AppendInterval(0.2f) // 지연 시간 추가
            .Append(_meshRenderers[(int)PrintType.Hand].material.DOColor(_defaultColor, interval).SetEase(Ease.Linear))
            .SetLoops(-1, LoopType.Yoyo); // Yoyo 방식으로 무한 반복

        // 발에 대한 깜박임 설정
        _blinkSeqs[(int)PrintType.Foot] = DOTween.Sequence();
        _blinkSeqs[(int)PrintType.Foot]
            .Append(_meshRenderers[(int)PrintType.Foot].material.DOColor(targetColor, interval).SetEase(Ease.Linear))
            .AppendInterval(0.2f) // 지연 시간 추가
            .Append(_meshRenderers[(int)PrintType.Foot].material.DOColor(_defaultColor, interval).SetEase(Ease.Linear))
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
    
}
