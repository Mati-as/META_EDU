using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Random = UnityEngine.Random;

public class WaterPlayground_BallController : MonoBehaviour
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


    private WaterPlayground_DolphinController _dolphinController;
    
    
   
    //클릭 가능 여부 판정을 위해 Collider 할당 및 제어.
    private Collider _collider;
    private Rigidbody _rb;
    public static event Action OnBallIsInTheHole;



 
    
    private Vector3[] _path;
    public Vector3 triggerPosition { get; private set; }
    private float _veggiePositionOffset =0.15f;
    private bool _isRespawning;
    
   

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        _path = new Vector3[3];
        _dolphinController = GameObject.Find("Dolphin").GetComponent<WaterPlayground_DolphinController>();
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

        else if (other.transform.gameObject.name == "Net" && !_isRespawning)

        {
            _isRespawning = true;
            //audio
            
            transform.DOScale(0, 1.5f).SetEase(Ease.InBounce).OnComplete(() =>
            {
                
                gameObject.SetActive(false);
                DOVirtual.Float(0, 1, ballInfo.respawnWaitTime, value => value++)
                    .OnComplete(() => { Respawn(); });
            });
        }
     
        
    }


    private bool _isParticlePlaying;
    private void OnCollisionEnter(Collision other)
    {
   
        if (other.transform.gameObject.name == "Obstacle")
        {
            

            foreach (ContactPoint contact in other.contacts)
            {
                
             
                PlayParticle(contact.point +Vector3.up);
                return;
            }
            
               
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

        colors[(int)BallInfo.BallColor.Red] = ballInfo.colorDef[(int)BallInfo.BallColor.Red];
        colors[(int)BallInfo.BallColor.Yellow] = ballInfo.colorDef[(int)BallInfo.BallColor.Yellow];
        colors[(int)BallInfo.BallColor.Blue] = ballInfo.colorDef[(int)BallInfo.BallColor.Blue];
    }
    
    

    private void SetColor()
    {

        if (this.gameObject.name != "Rainbow")
        {
            _currentColorIndex = Random.Range((int)BallInfo.BallColor.Red, (int)BallInfo.BallColor.Blue + 1);
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
#if UNITY_EDITOR
//        Debug.Log("Ball is Respawned");
#endif

    
        
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
        var ps = Resources.Load<GameObject>("게임별분류/기본컨텐츠/WaterPlayGround/BallHitEffect").GetComponent<ParticleSystem>();


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

        
        var randomChar = (char)Random.Range('A', 'C' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/WaterPlayground/Hit" + randomChar,0.5f);
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
                particlePool.Enqueue(ps);
            });
        
       
    }

}
