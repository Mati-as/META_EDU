using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Playground_Ball_Base : MonoBehaviour
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
    private AudioSource[] _xylophoneAudioSources;
    private AudioSource[] holeSoundSource;
    private AudioSource[] netSoundSource;
    private AudioSource[] otherEffectSource;
    private int _audioSize = 5;

    [Header("Reference")] 

    private Playground_VegetationController _vegetaionController;


    private Playground_BallSpawner _ballSpawner;
   
    
    
   
    //클릭 가능 여부 판정을 위해 Collider 할당 및 제어.
    private Collider _collider;
    private Rigidbody _rb;
    public static event Action OnBallIsOnTheNet;
    public static event Action OnBallIsInTheHole;

   

 
    
    private Vector3[] _path;
    public Vector3 triggerPosition { get; private set; }
    private float _veggiePositionOffset =0.15f;
    private bool _isRespawning;

    private void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        _path = new Vector3[3];

        SetAudio();
        //material은 static이기 때문에, 직접적으로 수정하지 않기 위한 tempMat 설정  .
        GetComponents();
        ColorInit(); 
        SetColor();
        SetScale();

        _vegetaionController = GameObject.Find("Vegetation").GetComponent<Playground_VegetationController>();
        if(_vegetaionController ==null) Debug.LogError("There's no Vegetation Controller");

        _ballSpawner = GameObject.Find("BallSpawner").GetComponent<Playground_BallSpawner>();
        if(_ballSpawner ==null) Debug.LogError("There's no Vegetation Controller");
    }
    
    
    public virtual void OnTriggerEnter(Collider other)
    {
        if (size == (int)BallInfo.BallSize.Small && other.transform.gameObject.name == "Hole")
        {
            _vegetaionController._veggieGrowPosition = other.transform.position + Vector3.up * _veggiePositionOffset;
            OnBallIsInTheHole?.Invoke();
            
            // 중복클릭방지
            _collider.enabled = false;

            _path[0] = transform.position;
            _path[1] = other.transform.position - Vector3.down * ballInfo.offset;
            _path[2] = other.transform.position + Vector3.down * ballInfo.depth;

            transform.DOPath(_path, ballInfo.durationIntoHole, PathType.CatmullRom)
                .OnStart(() =>
                {
                    FindAndPlayAudio(holeSoundSource);
                })
                .OnComplete(() => { DOVirtual.Float(0, 1, ballInfo.respawnWaitTime, value => value++)
                    .OnComplete(() =>
                    {
                        Respawn();
                    }); 
                });
        }

        else if (other.transform.gameObject.name == "Net" && !_isRespawning)

        {
            _isRespawning = true;
            FindAndPlayAudio(netSoundSource);
            
            transform.DOScale(0, 1.5f).SetEase(Ease.InBounce).OnComplete(() =>
            {
                
                gameObject.SetActive(false);
                DOVirtual.Float(0, 1, ballInfo.respawnWaitTime, value => value++)
                    .OnComplete(() => { Respawn(); });
            });
        }
     
        
    }

    public virtual void OnCollisionEnter(Collision other)
    {
        if (other.transform.gameObject.name == "Wall")
        {

            FindAndPlayAudio(_xylophoneAudioSources);
        }

        if (other.transform.gameObject.name != "Ground" &&
            other.transform.gameObject.name == "Large"||
            other.transform.gameObject.name == "Small" ||
            other.transform.gameObject.name == "Medium")
        {
            FindAndPlayAudio(_audioSources);

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

        if (!gameObject.name.Contains("Rainbow"))
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
       
        FindAndPlayAudio(otherEffectSource);
      
    }
    
    private void FindAndPlayAudio(AudioSource[] audioSources, bool recursive = false,
        float volume = 0.8f,string audioPath = null)
    {
#if UNITY_EDITOR
      //  Debug.Log("sound play (walls)");
#endif
            var availableAudioSource = Array.Find(audioSources, audioSource => !audioSource.isPlaying);
            if (availableAudioSource != null) FadeInSound(availableAudioSource, volume,audioPath:audioPath);

#if UNITY_EDITOR
#endif
        
    }

    private void FadeOutSound(AudioSource audioSource, float target = 0.1f, float fadeInDuration = 2.3f,
        float duration = 1f)
    {
        audioSource.DOFade(target, duration).SetDelay(fadeInDuration).OnComplete(() =>
        {
#if UNITY_EDITOR
#endif
            audioSource.Stop();
        });
    }

    private void FadeInSound(AudioSource audioSource, float targetVolume = 1f, float duration = 0.3f,
        string audioPath = null)
    {
#if UNITY_EDITOR
#endif
        if (audioPath != null)
        {
            audioSource.clip =  Resources.Load<AudioClip>(audioPath);
            audioSource.Play();
        }
        
        audioSource.Play();
        audioSource.DOFade(targetVolume, duration).OnComplete(() => { FadeOutSound(audioSource); });
    }
    
    
    private void SetAudio()
    {
        InitializeAudioSourceArray(ref _audioSources, "Audio/Playground/Ball", 
            ballInfo.volume, GetPitchBySize(size));
        InitializeAudioSourceArray(ref _xylophoneAudioSources, "Audio/Playground/Xylophone", ballInfo.volume, 
            GetXylophonePitchBySize(size));

        //작은공만 hole 에 접근가능 하므로..
        if (size == (int)BallInfo.BallSize.Small)
        {
            InitializeAudioSourceArray(ref holeSoundSource, "Audio/Playground/Hole", 0.3f, 1.25f);
        }

        InitializeAudioSourceArray(ref netSoundSource, "Audio/Playground/Net", 0.5f, 1f, 4);
        InitializeAudioSourceArray(ref otherEffectSource, "Audio/Playground/Spawn", 0.5f, 1f, 5);
    }

    private void InitializeAudioSourceArray(ref AudioSource[] audioSources, string resourcePath, float volume, float pitch, int arraySize = -1)
    {
        if (arraySize == -1)
        {
            arraySize = ballInfo.audioSize;
        }

        audioSources = new AudioSource[arraySize];
        
        AudioClip clip = Resources.Load<AudioClip>(resourcePath);

        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            SetupAudioSource(audioSources[i], clip, volume, pitch);
        }
    }

    private void SetupAudioSource(AudioSource source, AudioClip clip, float volume, float pitch)
    {
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.outputAudioMixerGroup = null;
        source.playOnAwake = false;
        source.loop = false;
        source.pitch = pitch;
    }

    private float GetPitchBySize(int size)
    {
        if (size == (int)BallInfo.BallSize.Small)
            return 1.3f;
        if (size == (int)BallInfo.BallSize.Medium || size == (int)BallInfo.BallSize.Large)
            return 0.8f;
        return 1f; // Default pitch
    }

    private float GetXylophonePitchBySize(int size)
    {
        if (size == (int)BallInfo.BallSize.Small)
            return 1.25f;
        if (size == (int)BallInfo.BallSize.Medium)
            return 1.1f;
        if (size == (int)BallInfo.BallSize.Large)
            return 0.95f;
        return 1f; // Default pitch
    }
    
    
    
    
    //   private void SetAudio()
    // {
    //     _audioSources = new AudioSource[ballInfo.audioSize];
    //     _xylophoneAudioSources =  new AudioSource[ballInfo.audioSize];
    //     holeSoundSource =  new AudioSource[ballInfo.audioSize];
    //     netSoundSource = new AudioSource[2];
    //     
    //     for (var i = 0; i < _audioSources.Length; i++)
    //     {
    //         _audioSources[i] = gameObject.AddComponent<AudioSource>();
    //         _audioSources[i].clip = Resources.Load<AudioClip>("Audio/Playground/Ball");
    //         _audioSources[i].volume = ballInfo.volume;
    //         _audioSources[i].spatialBlend = 0f;
    //         _audioSources[i].outputAudioMixerGroup = null;
    //         _audioSources[i].playOnAwake = false;
    //         _audioSources[i].loop = false;
    //
    //         if (size ==(int)BallInfo.BallSize.Small)
    //         {
    //             _audioSources[i].pitch = 1.3f;
    //         }
    //         else if (size ==(int)BallInfo.BallSize.Medium)
    //         {
    //             _audioSources[i].pitch = 0.8f;
    //         }
    //         else if (size ==(int)BallInfo.BallSize.Large)
    //         {
    //             _audioSources[i].pitch = 0.8f;
    //         }
    //         
    //     }
    //     
    //     
    //     
    //     for (var i = 0; i < _audioSources.Length; i++)
    //     {
    //         _xylophoneAudioSources[i] = gameObject.AddComponent<AudioSource>();
    //         _xylophoneAudioSources[i].clip = Resources.Load<AudioClip>("Audio/Playground/Xylophone");
    //         _xylophoneAudioSources[i].volume = ballInfo.volume;
    //         _xylophoneAudioSources[i].spatialBlend = 0f;
    //         _xylophoneAudioSources[i].outputAudioMixerGroup = null;
    //         _xylophoneAudioSources[i].playOnAwake = false;
    //
    //         if (size ==(int)BallInfo.BallSize.Small)
    //         {
    //             _xylophoneAudioSources[i].pitch = 1.25f;
    //         }
    //         else if (size ==(int)BallInfo.BallSize.Medium)
    //         {
    //             _xylophoneAudioSources[i].pitch = 1.1f;
    //         }
    //         else if (size ==(int)BallInfo.BallSize.Large)
    //         {
    //             _xylophoneAudioSources[i].pitch = 0.95f;
    //             
    //         }
    //         
    //     }
    //
    //     if (size == (int)BallInfo.BallSize.Small)
    //     {
    //         for (var i = 0; i < _audioSources.Length; i++)
    //         {
    //             holeSoundSource[i] = gameObject.AddComponent<AudioSource>();
    //             holeSoundSource[i].clip = Resources.Load<AudioClip>("Audio/Playground/Hole");
    //             holeSoundSource[i].volume = 0.3f;
    //             holeSoundSource[i].spatialBlend = 0f;
    //             holeSoundSource[i].outputAudioMixerGroup = null;
    //             holeSoundSource[i].playOnAwake = false;
    //
    //             if (size ==(int)BallInfo.BallSize.Small)
    //             {
    //                 holeSoundSource[i].pitch = 1.25f;
    //             }
    //             
    //         }
    //     }
    //     
    //     
    //     for (var i = 0; i < 2; i++)
    //     {
    //         netSoundSource[i] = gameObject.AddComponent<AudioSource>();
    //          netSoundSource[i].clip = Resources.Load<AudioClip>("Audio/Playground/Net");
    //          netSoundSource[i].volume = 0.5f;
    //          netSoundSource[i].spatialBlend = 0f;
    //          netSoundSource[i].outputAudioMixerGroup = null;
    //          netSoundSource[i].playOnAwake = false;
    //     }
    //    
    //    
    // }
      
      
}