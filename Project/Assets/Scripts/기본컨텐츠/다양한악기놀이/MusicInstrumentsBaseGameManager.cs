using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using 기본컨텐츠.다양한악기놀이;

public class MusicInstrumentsBaseGameManager : Base_GameManager
{
    private Stack<ParticleSystem> _effectContainer;
    private Slider _parrotSlider;
    

    protected override void Init()
    {
        base.Init();
        SensorSensitivity = 0.5f;
        
        
        _effectContainer = new Stack<ParticleSystem>();
    
        SetPool(_effectContainer,"SortedByScene/BasicContents/MusicInstruments/MusicInstruments_CFX_Click");
        _parrotSlider = GameObject.Find("ParrotSlider").GetComponent<Slider>();
     
        var images = _parrotSlider.GetComponentsInChildren<Image>();
        foreach (var image in images)
        {
            image.DOFade(0, 0.1f);
        }
        
        var tmps = _parrotSlider.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var tmp in tmps)
        {
            tmp.DOFade(0, 0.1f);
        }


    }
    
    

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        var images = _parrotSlider.GetComponentsInChildren<Image>();
        foreach (var image in images)
        {
            image.enabled = true;
            image.DOFade(1, 1.2f);
        }
        var tmps = _parrotSlider.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var tmp in tmps)
        {
            tmp.DOFade(1, 1f);
        }

    }

    private void Update()
    {
        _parrotSlider.value -= (Time.deltaTime /20);//감소속도
    }

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        SetClickableWithDelay();
        
        foreach (var hit in GameManager_Hits)
        {
            IMusicInstrumentsIOnClick _iOnClicked;

            hit.transform.TryGetComponent(out _iOnClicked);

            if (_iOnClicked != null)
            {
                DOVirtual.Float(0, 0.009f, 0.2f, val =>
                {
                    _parrotSlider.value += val;
                }).SetEase(Ease.InSine);
                _iOnClicked.OnClicked();
            }

            

        }

        foreach (var hit in GameManager_Hits)
        {
            if (hit.transform.gameObject.name == "Screen")
            {
                var ps = GetFromPool(_effectContainer);
                if (ps == null) return;
                ps.gameObject.SetActive(true);
            
            
                ps.gameObject.transform.position = hit.point;
                ps.Play();
            
                StartCoroutine(ReturnToPoolAfterDelay(ps, _effectContainer));

                return;
            }
          
        }
    }

    private WaitForSeconds _poolReturnWait;
    protected IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, Stack<ParticleSystem> particlePool)
    {
        if (_poolReturnWait == null) _poolReturnWait = new WaitForSeconds(ps.main.startLifetime.constantMax);

        yield return _poolReturnWait;


        ps.Stop();
        ps.Clear();
        ps.gameObject.SetActive(false);
        particlePool.Push(ps); // Return the particle system to the pool
    }
    
    private void SetPool(Stack<ParticleSystem> effectPool, string path, int poolCount = 20)
    {
        for (var poolSize = 0; poolSize < poolCount; poolSize++)
        {
         
            var prefab = Resources.Load<GameObject>(path);

            if (prefab == null)
            {
#if UNITY_EDITOR
                Debug.LogError("this gameObj to pool is null.");
#endif
                return;
            }

            var bead = Instantiate(prefab, transform);
            var ps = bead.GetComponent<ParticleSystem>();
            
            ps.Stop();
            ps.Clear();
            ps.gameObject.SetActive(false);
           
            effectPool.Push(ps);
        }
    }
    
    

    private ParticleSystem GetFromPool(Stack<ParticleSystem> pool)
    {
        if (pool.Count <= 0) return null;

        var ps = pool.Pop();
        return ps;
    }


    
}
