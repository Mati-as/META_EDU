using DG.Tweening;
using UnityEngine;

public class Playgorund_Ball_Base : MonoBehaviour
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

    //Sound Settings
    private AudioClip sound;
    private AudioSource[] _audioSources;
    private int _audioSize = 5;


    //클릭 가능 여부 판정을 위해 Collider 할당 및 제어.
    private Collider _collider;


    private void Start()
    {
        Init();
    }

    private void Init()
    {
        _path = new Vector3[3];

        SetAudio();

        //material은 static이기 때문에, 직접적으로 수정하지 않기 위한 tempMat 설정  .
        GetComponents();
        SetColor();
        SetScale();
    }


    protected virtual void OnGoal()
    {
    }

    protected virtual void OnHitWall()
    {
    }

    protected virtual void OnEnterHole()
    {
    }

    private Vector3[] _path;

    private void OnTriggerEnter(Collider other)
    {
        if (size == (int)BallInfo.BallSize.Small)
        {
            // 중복클릭방지
            _collider.enabled = false;

            _path[0] = transform.position;
            _path[1] = other.transform.position - Vector3.down*ballInfo.offset;
            _path[2] = other.transform.position + Vector3.down * ballInfo.depth;

            transform.DOPath(_path, ballInfo.durationIntoHole, PathType.CatmullRom)
                .OnComplete(() =>
                {
                    transform.DOScale(0, 1f).SetEase(Ease.InBounce);
                });
        }
    }

    private void OnTriggerExit(Collider other)
    {
    }


    private void SetAudio()
    {
        _audioSources = new AudioSource[ballInfo.audioSize];

        for (var i = 0; i < _audioSources.Length; i++)
        {
            _audioSources[i] = gameObject.AddComponent<AudioSource>();
            _audioSources[i].clip =
                Resources.Load<AudioClip>("기본컨텐츠/PlayGround/Audio/ball_" + $"{gameObject.name}");
            ;
            _audioSources[i].volume = ballInfo.volume;
            _audioSources[i].spatialBlend = 0f;
            _audioSources[i].outputAudioMixerGroup = null;
            _audioSources[i].playOnAwake = false;

            //  _audioSources[i].pitch = Random.Range(1 - interval, 1 + interval);
        }
    }

    private void GetComponents()
    {
        var tempMat = GetComponent<Material>();
        _material = tempMat;
        _meshRenderer = GetComponent<MeshRenderer>();

        _collider = GetComponent<Collider>();
    }

    private void SetColor()
    {
        if (colors == null) colors = new Color[3];

        colors[(int)BallInfo.BallColor.Red] = ballInfo.colorDef[(int)BallInfo.BallColor.Red];
        colors[(int)BallInfo.BallColor.Yellow] = ballInfo.colorDef[(int)BallInfo.BallColor.Yellow];
        colors[(int)BallInfo.BallColor.Blue] = ballInfo.colorDef[(int)BallInfo.BallColor.Blue];

        _currentColorIndex = Random.Range((int)BallInfo.BallColor.Red, (int)BallInfo.BallColor.Blue + 1);
        _color = colors[_currentColorIndex];

        _meshRenderer.material.color = _color;
    }

    private void SetScale()
    {
        transform.localScale =
            ballInfo.ballSizes[size] * Vector3.one *
            Random.Range(1 - ballInfo.sizeRandomInterval, 1 + ballInfo.sizeRandomInterval);
    }
}