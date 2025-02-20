using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ScratchPainting_StampController : MonoBehaviour
{
    private Vector3 _defaultSize;
    private ScratchPaintingBaseGameManager _gm;
    
    private void Awake()
    {
        _defaultSize = transform.localScale;
       
        ScratchPaintingBaseGameManager.printInitEvent -= OnRestartInit;
        ScratchPaintingBaseGameManager.printInitEvent += OnRestartInit;
    }

    private void Start()
    {
        if (transform.localScale == Vector3.zero)
        {
            Debug.LogError("Default size is zero. Awake order must be changed");
        }

        _gm = GameObject.FindWithTag("GameManager").GetComponent<ScratchPaintingBaseGameManager>();
        Debug.Assert(_gm!=null);
       

    }
    private void OnEnable()
    {

        if (!HandFootPaintingBaseGameManager.isInit) return;
        
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
        if (!ScratchPaintingBaseGameManager.isInit) return;
        _gm.printPool.Enqueue(gameObject);

    }

}
