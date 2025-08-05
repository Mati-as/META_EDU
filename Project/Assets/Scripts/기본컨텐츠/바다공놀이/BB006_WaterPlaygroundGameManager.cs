using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class BB006_WaterPlaygroundGameManager : Base_GameManager
{
    private ParticleSystem _clickPs;
    public Queue<ParticleSystem> particlePool;
       
    private RaycastHit[] _hits;
    private RaycastHit _hitForPs; //클릭시 이펙트 효과전용
    
    public float forceAmount;
    public float upOffset;
    
   
  
    public Vector3 particleUpOffset;
    protected override void Init()
    {
      SetPool(ref particlePool);
      SensorSensitivity = 0.18f;
      //BGM_VOLUME = 0.2f;
      base.Init();
      ManageProjectSettings(150,0.15f);
        
    }

    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();
        initialMessage= "공을 구멍에 넣어 돌고래를 불러볼까요?";
         BaseInGameUIManager.PopInstructionUIFromScaleZero(initialMessage);
        Managers.Sound.Play(SoundManager.Sound.Narration, "OnGameStartNarration/" + SceneManager.GetActiveScene().name + "_intronarration");
    }

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (!isStartButtonClicked) return;
        
        _hits = Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in _hits)
        {
            
            
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>(); // 부딪힌 물체에 Rigidbody 컴포넌트가 있는지 확인합니다.

          
            if (rb != null)
            {
              
                Vector3 forceDirection =  rb.transform.position - hit.point + Vector3.up*upOffset;
                
                var randomChar = (char)Random.Range('A', 'D' + 1);
                Managers.Sound.Play(SoundManager.Sound.Effect, 
                    "Audio/BasicContents/WaterPlayground/Click" + randomChar,0.35f);
                rb.AddForce(forceDirection.normalized * forceAmount, ForceMode.Impulse);

                DEV_OnValidClick();
            }
            
        }
        
   
        
        foreach (var hit in _hits)
        {
            if(hit.transform.gameObject.name.Contains("WaterCollider"))
            {
                PlayParticle(hit.point + particleUpOffset);//offset
                return;
            }
            
        }
     
    }
    
    protected virtual void SetPool(ref Queue<ParticleSystem> psQueue)
    {
        psQueue = new Queue<ParticleSystem>();
        var index = 0;
        var ps = Resources.Load<GameObject>("SortedByScene/BasicContents/WaterPlayGround/Click").GetComponent<ParticleSystem>();


        psQueue.Enqueue(ps);
        ps.gameObject.SetActive(false);
            

        // Optionally, if you need more instances than available, clone them
        while (psQueue.Count < 20)
        {
            var newPs = Instantiate(ps, transform);
            newPs.gameObject.SetActive(false);
            psQueue.Enqueue(newPs);
        }
    }

    private void PlayParticle(Vector3 point)
    {
        if (particlePool.Count <= 0) return;
        
        var ps = particlePool.Dequeue();
        ps.gameObject.SetActive(true);
        ps.Stop();
        ps.transform.position = point;
        ps.Play();
    
        DOVirtual.Float(0, 0, ps.main.duration, _ =>{})
            .OnComplete(() =>
            {
                ps.gameObject.SetActive(false);
                particlePool.Enqueue(ps);
            });
        
       
    }

}
