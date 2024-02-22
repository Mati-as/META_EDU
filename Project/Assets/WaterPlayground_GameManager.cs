using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WaterPlayground_GameManager : IGameManager
{
    private ParticleSystem _clickPs;
    public Queue<ParticleSystem> particlePool;
       
    private RaycastHit[] _hits;
    //  private Rigidbody _currentRigidBody;

    public float forceAmount;
    public float upOffset;

    protected override void Init()
    {
      base.Init();
      SetPool(ref particlePool);
    }
    
    protected override void OnRaySynced()
    {
        base.OnRaySynced();
        
        
        _hits = Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in _hits)
        {
            if(hit.transform.gameObject.name.Contains("water"))
            {
                PlayParticle(hit.point);
            }
            
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>(); // 부딪힌 물체에 Rigidbody 컴포넌트가 있는지 확인합니다.

          
            if (rb != null)
            {
              
                Vector3 forceDirection =  rb.transform.position - hit.point + Vector3.up*upOffset;
                
                rb.AddForce(forceDirection.normalized * forceAmount, ForceMode.Impulse);
            }

            
        }
        
    }
    
    protected virtual void SetPool(ref Queue<ParticleSystem> psQueue)
    {
        psQueue = new Queue<ParticleSystem>();
        var index = 0;
        var ps = Resources.Load<GameObject>("게임별분류/기본컨텐츠/WaterPlayGround/Click").GetComponent<ParticleSystem>();


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
