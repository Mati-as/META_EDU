using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class PaintingContent_GameManager : Ex_BaseGameManager
{
    private enum Objs
    {
        SketchFinished
    }

    protected VideoPlayer videoPlayer;
    
    [SerializeField] private float playbackSpeed = 1;
    [SerializeField] private int poolSize = 400;
    
#if UNITY_EDITOR
    [SerializeField] [Range(10, 1000)] private int COUNT_TO_BRUSH;
#else
    private int COUNT_TO_BRUSH = 300;
#endif
 
    private int _currentBrushCount;
    private MeshRenderer _sketchMeshRenderer;

    private Queue<GameObject> _brushPool = new();
    private Dictionary<int, MeshRenderer> _meshRendererMap;
    private Vector3 _brushDefaultSize;

    private int _videoTransformID;
    private bool _isPaintFinished;

    private float _startDelay =0.75f;
    
    private Base_UIManager _baseUIManager;

    protected override void Init()
    {
        BindObject(typeof(Objs));
        _sketchMeshRenderer = GetObject((int)Objs.SketchFinished).GetComponent<MeshRenderer>();
        DOTween.SetTweensCapacity(2000, 2000);
        psResourcePath = string.Empty;
        base.Init();
        InitializePool();
        SetVideo();

        Managers.Sound.Play(SoundManager.Sound.Bgm, "Common/Bgm/Paint");
        SketchFinishFilterSet(false);
       // _defaultPosition = new Vector3();
        //_defaultPosition = transform.position;
        _baseUIManager = UIManagerObj.GetComponent<Base_UIManager>();
    }

    private void SketchFinishFilterSet(bool isActive)
    {
        if (isActive)
        {
            foreach (int key in _meshRendererMap.Keys.ToArray())
                DOVirtual.DelayedCall(1f, () =>
                {
                    ReturnToPool(_meshRendererMap[key].transform.gameObject);
                });

            GetObject((int)Objs.SketchFinished).transform.DOScale(_defaultSizeMap[(int)Objs.SketchFinished], 1f)
                .SetEase(Ease.OutSine);
        }

        else
            GetObject((int)Objs.SketchFinished).transform.DOScale(Vector3.zero, 0.7f).SetEase(Ease.OutSine);
    }

   
    
    
    protected virtual void SetVideo()
    {
        // 비디오 재생관련 세팅.
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.playbackSpeed = playbackSpeed;

        string sceneName = SceneManager.GetActiveScene().name;
        string mp4Path = Path.Combine(Application.streamingAssetsPath, $"MediaArt/{sceneName}.mp4");
        string movPath = Path.Combine(Application.streamingAssetsPath, $"MediaArt/{sceneName}.mov");

        if (File.Exists(mp4Path))
            videoPlayer.url = mp4Path;
        else if (File.Exists(movPath))
            videoPlayer.url = movPath;
        else
        {
            Debug.LogError($"비디오 파일이 없습니다: {mp4Path} 또는 {movPath}");
            return;
        }

        // 필요시, fallback 동작 처리 (예: 대체 화면 활성화 등)
        videoPlayer.Play();

        videoPlayer.SetDirectAudioMute(0, true); // 트랙 0번을 뮤트

        DOVirtual.DelayedCall(_startDelay, () =>
        {
            videoPlayer.Pause();
        });
        BindEvent();
    }

    protected override void OnGameStartButtonClicked()
    {
        _baseUIManager.PopInstructionUIFromScaleZero("그림을 터치해 완성시켜보세요!", 5f);
        base.OnGameStartButtonClicked();
    }


    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;


        if (_isPaintFinished) return;
        //브러쉬 이미지 생성

        foreach (var hit in GameManager_Hits)
        {
            int HitID = hit.transform.GetInstanceID(); 

            if (_brushPool.Count <= 0)
            {
                
            }
            else
            {
                var brush = GetBrushFromPool();
                if (brush != null)
                {
                    brush.transform.position = hit.point;
                    //brush.transform.rotation = Quaternion.LookRotation(hit.normal);
                    _meshRendererMap[brush.transform.GetInstanceID()].material.DOFade(1, 1f).SetEase(Ease.OutSine);
                    brush.transform.localScale = _brushDefaultSize * Random.Range(0.8f, 1.2f);
                    brush.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    brush.transform.DOShakeScale(0.07f, 0.05f);
                    brush.SetActive(true);
                }

                if (_currentBrushCount < COUNT_TO_BRUSH)
                    _currentBrushCount++;
                else
                {
                    _currentBrushCount = 0;
                    _isPaintFinished = true;

                    SketchFinishFilterSet(true);
                    // 브러쉬 개수가 COUNT_TO_BRUSH에 도달하면, 더 이상 생성하지 않음.
                    // 필요시, 브러쉬 개수 조정 로직 추가 가능.

                    Managers.Sound.Stop(SoundManager.Sound.Bgm);
                    videoPlayer.Play();
                    videoPlayer.SetDirectAudioMute(0, false);
                   
                    _baseUIManager.PopInstructionUIFromScaleZero("살아있는 그림을 완성시켰어요!", 5f);


                    Logger.Log("Length of Video: " + videoPlayer.length);
                    float videoLength = (float)videoPlayer.length - _startDelay;
             
                 DOVirtual.DelayedCall(videoLength, () =>
                 {
                     videoPlayer.SetDirectAudioMute(0, true); // 트랙 0번을 뮤트
                     videoPlayer.Stop();
                     videoPlayer.Play();


                     DOVirtual.DelayedCall(1.5f, () =>
                     {
                         Managers.Sound.Play(SoundManager.Sound.Bgm, "Common/Bgm/Paint");
                         _baseUIManager.PopInstructionUIFromScaleZero("그림을 터치해 완성시켜보세요!", 5f);
                         SketchFinishFilterSet(false);
                         _isPaintFinished = false;
                         videoPlayer.Pause();
                     });
                 });
                }
            }
        }
    }

    
    private void InitializePool()
    {
        _brushPool = new Queue<GameObject>();
        _meshRendererMap = new Dictionary<int, MeshRenderer>();

        for (int i = 0; i < poolSize; i++)
        {
            var brushPrefab = Resources.Load<GameObject>("Runtime/FB_Painting/Brush");
            var poolParent = GameObject.Find("@brushPool")?.transform;
            var brushInstance = Instantiate(brushPrefab, poolParent);

            var renderer = brushInstance.GetComponent<MeshRenderer>();
            _meshRendererMap.Add(brushInstance.transform.GetInstanceID(), renderer);
            renderer.material.DOFade(0, 0.000001f);

            brushInstance.SetActive(false);
            _brushPool.Enqueue(brushInstance);
            _brushDefaultSize = brushInstance.transform.localScale; // Assuming uniform scale
        }
    }

    public GameObject GetBrushFromPool()
    {
        if (_brushPool.Count > 0)
        {
            var obj = _brushPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        Debug.LogWarning("Pool empty!");
        return null;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        _brushPool.Enqueue(obj);
    }
}