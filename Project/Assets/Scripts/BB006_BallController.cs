using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Random = UnityEngine.Random;

public class BB006_BallController : MonoBehaviour
{
  [SerializeField] private BallInfo ballInfo;

    //Size Settings.
    [Range(0, 2)] public int size;

    //Color Settings.
    private static Color[] colors;
    private Color _color;
    private int _currentColorIndex;

    private Material _material;
    private MeshRenderer _meshRenderer;
    public Queue<ParticleSystem> particlePool;
    private ParticleSystem _hitPs;


    [SerializeField] 
    private WaterPlayground_BallSpawner _ballSpawner;


    private BB006_DolphinController _dolphinController;
    
    
   
    //클릭 가능 여부 판정을 위해 Collider 할당 및 제어.
    private Collider _collider;
    private Rigidbody _rb;
    public static event Action OnBallIsInTheHole;



 
    
    private Vector3[] _path;
    public Vector3 triggerPosition { get; private set; }
    private float _veggiePositionOffset =0.15f;
    private bool _isRespawning;
    private int currentActivePsCount;
   

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        _path = new Vector3[3];
        _dolphinController = GameObject.Find("Dolphin_Model").GetComponent<BB006_DolphinController>();
        _ballSpawner = GameObject.Find("BallSpawner").GetComponent<WaterPlayground_BallSpawner>();
        //material은 static이기 때문에, 직접적으로 수정하지 않기 위한 tempMat 설정  .
        GetComponents();
        ColorInit(); 
        SetColor();
        SetScale();
        SetEffectPool(ref particlePool);
    }
    
    
    private void OnTriggerEnter(Collider other)
    {
      
       

        
        if (other.transform.gameObject.name == "Hole")
        {
            
            var randomChar = (char)Random.Range('A', 'C' + 1);
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/WaterPlayground/Hole" + randomChar,0.5f);
            
            
             _dolphinController.currentBallInTheHoleColor = _color;
            
            // 중복클릭방지
            _collider.enabled = false;

            _path[0] = transform.position;
            _path[1] = other.transform.position - Vector3.down * ballInfo.offset;
            _path[2] = other.transform.position + Vector3.down * ballInfo.depth;

            
            
            transform.DOPath(_path, ballInfo.durationIntoHole, PathType.CatmullRom)
                .OnStart(() =>
                {
                    OnBallIsInTheHole?.Invoke();
                    //audio
                })
                .OnComplete(() => { DOVirtual.Float(0, 1, ballInfo.respawnWaitTime, _ => _++)
                    .OnStart(() =>
                    {
                        
                    })
                    .OnComplete(() =>
                    {
                        Respawn();
                    }); 
                });
        }

       
       
     
        
    }


    private bool _isParticlePlaying;
    private float _velOffset = 0.01f; // 일정이상 속도로 충돌했을때만 소리 및 파티클이 재생되도록 하기 위한 오프셋값입니다. 
    private void OnCollisionEnter(Collision other)
    {
        
        var velproportionalVolume = _velOffset *(Mathf.Abs(_rb.velocity.x) + Mathf.Abs(_rb.velocity.y) + Mathf.Abs(_rb.velocity.z))/3;
 
        if (other.transform.gameObject.name == "Obstacle")
        {
            foreach (ContactPoint contact in other.contacts)
            {
                        
#if UNITY_EDITOR
//                Debug.Log($"Obstacle particle played");
#endif
                PlayParticle(contact.point +Vector3.up *2.0f);
                return;
            }
               
        }
        if (other.transform.gameObject.name.Contains("Ball"))
        {
            
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/WaterPlayground/Ball", 
                0.5f );
               
            
        }
       


        if (velproportionalVolume > 0.25f)
        {
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/WaterPlayground/Ball", 
                velproportionalVolume );
        }
        
         
 
     
    }

  

    private void GetComponents()
    {
        var tempMat = GetComponent<Material>();
        _material = tempMat;
        _meshRenderer = GetComponent<MeshRenderer>();

        _collider = GetComponent<Collider>();

        _rb = GetComponent<Rigidbody>();
    }

    private void ColorInit()
    {
        if (colors == null) colors = new Color[3];

        colors[(int)BallInfo.BallColor.Pink] = ballInfo.colorDef[(int)BallInfo.BallColor.Pink];
        colors[(int)BallInfo.BallColor.Yellow] = ballInfo.colorDef[(int)BallInfo.BallColor.Yellow];
        colors[(int)BallInfo.BallColor.Blue] = ballInfo.colorDef[(int)BallInfo.BallColor.Blue];
    }
    
    

    private void SetColor()
    {

        if (this.gameObject.name != "Rainbow")
        {
            _currentColorIndex = Random.Range((int)BallInfo.BallColor.Pink, (int)BallInfo.BallColor.Blue + 1);
            _color = colors[_currentColorIndex];

            _meshRenderer.material.color = _color;
        }
       
    }

    private void SetScale()
    {
        transform.localScale =
            ballInfo.ballSizes[size] * Vector3.one *
            Random.Range(1 - ballInfo.sizeRandomInterval, 1 + ballInfo.sizeRandomInterval);
    }


    private int _currentPosition;
    private void Respawn()
    {

    
        
        gameObject.SetActive(true);
        _collider.enabled = true;
        transform.DOScale(ballInfo.ballSizes[size], 1.35f)
            .SetEase(Ease.OutBounce)
            .OnComplete(() =>
            { //단순 중복방지 로직
                _isRespawning = false;
            }); 
        
        SetColor();
        
        _currentPosition = Random.Range(0, 3);
        transform.position = _ballSpawner.spawnPositions[_currentPosition].position;
        _rb.AddForce(_ballSpawner.spawnPositions[_currentPosition].up * ballInfo.respawnPower, ForceMode.Impulse);
       
      
    }

    private void SetEffectPool(ref Queue<ParticleSystem> psQueue)
    {
        psQueue = new Queue<ParticleSystem>();
        var index = 0;
        var ps = Resources.Load<GameObject>("SortedByScene/BasicContents/WaterPlayGround/BallHitEffect").GetComponent<ParticleSystem>();


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
        if (_isParticlePlaying) return;
        if (currentActivePsCount > 5) return;
       
        currentActivePsCount++;
        
        var randomChar = (char)Random.Range('A', 'C' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/WaterPlayground/Hit" + randomChar,0.5f);
        _isParticlePlaying = true;
        var ps = particlePool.Dequeue();
        ps.gameObject.SetActive(true);
        ps.Stop();
        ps.transform.position = point;
        ps.Play();
    
        DOVirtual.Float(0, 0, ps.main.duration, _ =>{})
            .OnComplete(() =>
            {
                DOVirtual.Float(0, 0, ps.main.duration, _ => { }).OnComplete(() =>
                {
                    _isParticlePlaying = false;
                });
                
                ps.gameObject.SetActive(false);
                currentActivePsCount--;
                particlePool.Enqueue(ps);
            });
        
       
    }

}
