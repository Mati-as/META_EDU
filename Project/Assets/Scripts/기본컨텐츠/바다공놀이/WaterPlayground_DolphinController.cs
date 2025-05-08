using DG.Tweening;
using UnityEngine;

public class WaterPlayground_DolphinController : MonoBehaviour
{
    private enum PathOrder
    {
        Start,
        Mid,
        Arrival,
        Max
    }

    private enum PathName
    {
        A,
        B,
        C,
        Idle,
        Max
    }

    private Transform _pathAParent;
    private Transform _pathBParent;
    private Transform _pathCParent;
    private Transform _idlePathParent;

    private MeshRenderer _dolphinBallMeshRenderer;
    private Vector3[][] _pathes;
    private Vector3[] _pathA;
    private Vector3[] _pathB;
    private Vector3[] _pathC;
    private Vector3[] _pathIdle;

    private Vector3 _defaultSize;
    public bool isOnPath { get; private set; }
    public bool isIdleTweening { get; private set; }

    private Sequence _seq;
    private float _waitTimeWhenIdlePathPlaying = 1.5f;
    private float _elapsed;
    [Range(0, 60)] public float _dolphinIdleInterval;

    //ballController 참조
    private GameObject _dolphinBall;
    public Color currentBallInTheHoleColor { get; set; }
    private WaterPlayground_BallController _ballController;
    private int _currentPathIndex;

    private void Start()
    {
        WaterPlayground_BallController.OnBallIsInTheHole -= OnBallIn;
        WaterPlayground_BallController.OnBallIsInTheHole += OnBallIn;

        _pathes = new Vector3[(int)PathName.Max][];
        _pathA = new Vector3[(int)PathOrder.Max];
        _pathB = new Vector3[(int)PathOrder.Max];
        _pathC = new Vector3[(int)PathOrder.Max];
        _pathIdle = new Vector3[(int)PathOrder.Max];

        SetPathes();


        _defaultSize = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    private void SetPathes()
    {
        _pathAParent = GameObject.Find("DolphinPathA").transform;
        _pathBParent = GameObject.Find("DolphinPathB").transform;
        _pathCParent = GameObject.Find("DolphinPathC").transform;
        _idlePathParent = GameObject.Find("DolphinPathIdle").transform;


        if (gameObject.name == "IdleDolphin")
        {
        }
        else
        {
            _dolphinBall = GameObject.Find("DolphinBall");
            _dolphinBallMeshRenderer = _dolphinBall.GetComponent<MeshRenderer>();
        }

        _pathes[(int)PathName.A] = _pathA;
        _pathes[(int)PathName.B] = _pathB;
        _pathes[(int)PathName.C] = _pathC;
        _pathes[(int)PathName.Idle] = _pathIdle;

        for (var i = 0; i < (int)PathOrder.Max; i++)
        {
            _pathA[i] = _pathAParent.GetChild(i).position;
            _pathB[i] = _pathBParent.GetChild(i).position;
            _pathC[i] = _pathCParent.GetChild(i).position;
            _pathIdle[i] = _idlePathParent.GetChild(i).position;
        }
    }

    private void OnBallIn(int _)
    {
        _currentPathIndex = Random.Range(0, 3);
        _currentPath = _pathes[_currentPathIndex];
        PlayDolphinAnim(_currentPath);
    }

    private void Update()
    {
        if (!isIdleTweening)
            _elapsed += Time.deltaTime;
        else
            _elapsed = 0;

        if (_elapsed > _dolphinIdleInterval)
        {
#if UNITY_EDITORw
Debug.Log("idle path");
#endif
            if (gameObject.name == "IdleDolphin")
            {
                isIdleTweening = true;
                PlayDolphinAnim(_pathIdle, 1.5f);
            }

            _elapsed = 0;
        }
    }

    private Vector3[] _currentPath;

    private void PlayDolphinAnim(Vector3[] path, float duration = 2.88f, float extraDelay = 0f)
    {
        //Tweening Start
        if (isOnPath) return;
        isOnPath = true;
        if (gameObject.name == "IdleDolphin" && path != _pathes[(int)PathName.Idle]) return;
        if (gameObject.name != "IdleDolphin")
        {
                 
            var mat = _dolphinBallMeshRenderer.material;
            mat.color = currentBallInTheHoleColor;
            _dolphinBallMeshRenderer.material = mat;
        }
        


#if UNITY_EDITOR
        Debug.Log("Ball Dolphin Anim");
#endif

        

        _seq = DOTween.Sequence();

        transform.localScale = Vector3.zero;
        transform.position = path[(int)PathOrder.Start];
        transform.DOLookAt(path[(int)PathOrder.Mid], 0.01f);

        _seq.Append(transform.DOScale(_defaultSize, 0.55f)
            .OnStart(() =>
            {

                if (gameObject.name != "IdleDolphin")
                {
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BB006/DolphinA" );
                
                    var randomChar = (char)Random.Range('A', 'B' + 1);
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BB006/DolphinLaugh" + randomChar,0.5f);
                }
           
              
                transform.DOLookAt(path[(int)PathOrder.Mid], 0.05f);
                transform.DOPath(path, duration, PathType.CatmullRom)
                    .SetEase(Ease.InOutSine)
                    .OnStart(() =>
                    {
#if UNITY_EDITOR
//                        Debug.Log($"pathStart---------------------Name: {(PathName)_currentPathIndex}");
#endif
                        
                      
                        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BB006/DolphinB" ,1.0f);
                        DOVirtual.Float(0, 0, 1f, _ => { })
                            .OnComplete(() => { transform.DOLookAt(path[(int)PathOrder.Arrival], 1.45f); });
                    })
                    .SetDelay(1.55f + extraDelay)
                    .OnComplete(() =>
                    {
                        // delay
                        DOVirtual.Float(0, 0, 0.5f, _ => { })
                            .OnComplete(() =>
                            {
                                isOnPath = false;
                                isIdleTweening = false;
                            });
                    });
            })
            .SetEase(Ease.InOutSine)
            .SetDelay(extraDelay + 1.09f));

        _seq.Play();
    }
}