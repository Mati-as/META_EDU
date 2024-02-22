using System;
using DG.Tweening;
using UnityEngine;
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
    



    [SerializeField] 
    private WaterPlayground_BallSpawner _ballSpawner;
   
    
    
   
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

        //material은 static이기 때문에, 직접적으로 수정하지 않기 위한 tempMat 설정  .
        GetComponents();
        ColorInit(); 
        SetColor();
        SetScale();
    }
    
    
    private void OnTriggerEnter(Collider other)
    {
        _ballSpawner = GameObject.Find("BallSpawner").GetComponent<WaterPlayground_BallSpawner>();
        if (other.transform.gameObject.name == "Hole")
        {
            OnBallIsInTheHole?.Invoke();
            
            // 중복클릭방지
            _collider.enabled = false;

            _path[0] = transform.position;
            _path[1] = other.transform.position - Vector3.down * ballInfo.offset;
            _path[2] = other.transform.position + Vector3.down * ballInfo.depth;

            transform.DOPath(_path, ballInfo.durationIntoHole, PathType.CatmullRom)
                .OnStart(() =>
                {
                    //audio
                })
                .OnComplete(() => { DOVirtual.Float(0, 1, ballInfo.respawnWaitTime, _ => _++)
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

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.gameObject.name == "Wall")
        {

          //audio
        }

        if (other.transform.gameObject.name != "Ground" &&
            other.transform.gameObject.name == "Large"||
            other.transform.gameObject.name == "Small" ||
            other.transform.gameObject.name == "Medium")
        {
           
            //audio
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
        Debug.Log("Ball is Respawned");
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

}
