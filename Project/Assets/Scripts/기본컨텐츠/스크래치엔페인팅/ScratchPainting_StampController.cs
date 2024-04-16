using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ScratchPainting_StampController : MonoBehaviour
{
    private Vector3 _defaultSize;
    private ScratchPainting_GameManager _gm;
    
    private void Awake()
    {
        _defaultSize = transform.localScale;
       
        ScratchPainting_GameManager.printInitEvent -= OnRestartInit;
        ScratchPainting_GameManager.printInitEvent += OnRestartInit;
    }

    private void Start()
    {
        if (transform.localScale == Vector3.zero)
        {
            Debug.LogError("Default size is zero. Awake order must be changed");
        }

        _gm = GameObject.FindWithTag("GameManager").GetComponent<ScratchPainting_GameManager>();
        Debug.Assert(_gm!=null);
       

    }
    private void OnEnable()
    {

        if (!HandFootPainting_GameManager.isInit) return;
        
        transform.localScale = Vector3.zero;
        transform.DOScale(_defaultSize, 0.055f).SetEase(Ease.OutBounce);
    }

    private void OnRestartInit()
    {
        transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InBounce).OnComplete(()=>
        {
            gameObject.SetActive(false);
        });
    }



    private void OnDisable()
    {
        if (!ScratchPainting_GameManager.isInit) return;
        _gm.printPool.Enqueue(gameObject);

    }

}
