using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Painting_EasteranArtMultipleTexture : IGameManager
{
     [SerializeField]
    private Shader paintShader;
    private Material paintMaterial;
    private RenderTexture renderTexture;
    private MeshRenderer _meshRenderer;
    [SerializeField]
    private Texture2D textureToPaintOn;
    public float brushSize = 0.1f;
    [SerializeField]
    private Texture2D[] burshTextures;// Define an InputAction for painting


    private static event Action ChangeScene; 
     [Header("Shader Setting")] 
     public float burshStrength = 1;
     public Volume vol;
     private Vignette vignette;
     private bool _isSceneChanging; // 씬 이동중 로직 충돌방지
    protected override void Init()
    {
        Camera.main.TryGetComponent<Volume>(out vol);
        
        if (vol == null)
        {
            Debug.LogError("PostProcessVolume not assigned.");
            return;
        }

        if (vol.profile.TryGet<Vignette>(out vignette))
        {
            vignette = vol.profile.components.Find(x => x is Vignette) as Vignette;
        }
        else
        {
            Debug.LogError("Vignette not found in PostProcessVolume.");
        }

        
        base.Init();
        
     //  Managers.Sound.Play(SoundManager.Sound.Bgm, "Audio/명화컨텐츠/gnossienne",volume:1.2f)
    }

    protected override void BindEvent()
    {
        ChangeScene -= OnChangeScene;
        ChangeScene += OnChangeScene;
        base.BindEvent();
    }

    protected  void OnDestroy()
    {
        ChangeScene -= OnChangeScene;
    }

    private void OnChangeScene()
    {
        DOVirtual.Float(0, 1, 3f, val =>
        {
            _isSceneChanging = false;
            vignette.intensity.value = val;
        }).OnComplete(() =>
        {
            SceneManager.LoadScene("AB002FromBA001");
        });
        
    }


    private float _elapsed;
    private readonly float _timeLimitForSceneChange =60;
    private void Update()
    {
        _elapsed += Time.deltaTime;
        if (_elapsed > _timeLimitForSceneChange)
        {
            
            ChangeScene?.Invoke();
            _elapsed = 0; 
        }
        
    }


    private void Start()
    {
        InitTexture();
    }
    
    
    /// <summary>
    /// Awake단게로 옮기지말 것,renderTexture Access Deny되는 버그 발생가능성 있음
    /// </summary>
    private void InitTexture()
    {
        renderTexture = new RenderTexture(textureToPaintOn.width, textureToPaintOn.height, 0, RenderTextureFormat.ARGB32);
        paintMaterial = new Material(paintShader);
        
        
     
        Graphics.Blit(textureToPaintOn, renderTexture);

        // Set the material's texture
        GetComponent<MeshRenderer>().material.mainTexture = renderTexture;
    }

    
    private float _clickInterval = 0.12f;
    private WaitForSeconds _clickWait;
    private bool _isClickable =true;
    private void SetClickable()
    {
        StartCoroutine(SetClickableCo());
    }

    private IEnumerator SetClickableCo()
    {
        _isClickable = false;
        
        if (_clickWait == null)
        {
            _clickWait = new WaitForSeconds(_clickInterval);
        }

        yield return _clickWait;

        _isClickable = true;

    }
    
    
    public float currentRotation;
    void Paint()
    {

        if (!isStartButtonClicked || _isSceneChanging) return;
        currentRotation = Random.Range(0,360);
      
        var randomChar = (char)Random.Range('A', 'C' + 1);
        Managers.soundManager.Play(SoundManager.Sound.Effect, 
            "Audio/명화컨텐츠/BA001/Click" + randomChar,Random.Range(0.003f,0.009f));
        
        var randomCharWater = (char)Random.Range('A', 'B' + 1);
        Managers.soundManager.Play(SoundManager.Sound.Effect, 
            "Audio/명화컨텐츠/BA001/Water" + randomCharWater,Random.Range(0.07f,0.10f));
        
        RaycastHit hit;
        if (Physics.Raycast(GameManager_Ray, out hit))
        {
           
            if (hit.transform == transform)
            {
                Vector2 uv = hit.textureCoord;
                paintMaterial.SetFloat("_TextureRotationAngle", currentRotation);
                paintMaterial.SetFloat("_BrushStrength",burshStrength );
                paintMaterial.SetTexture("_BrushTex",burshTextures[Random.Range(0,burshTextures.Length)]);
              
                // Convert to "_MouseUV" for the shader
                paintMaterial.SetVector("_MouseUV", new Vector4(uv.x, uv.y, 0, 0));
                paintMaterial.SetFloat("_BrushSize", brushSize);
                
                
#if UNITY_EDITOR
                
                Debug.Log($"Mouse UV: {uv.x}, {uv.y}");
                Debug.Log($"Brush Size: {brushSize}");
                Debug.Log($"Brush Strength: {burshStrength}");
                Debug.Log($"Brush Strength: {burshStrength}");
                
#endif

                // 알파블렌딩과 렌더링 순서 간 차이로 인해 클릭 시 한번에 전부 지워지지 않는 버그가 있습니다.
                // 이를 해결하기위해 Blit을 1:n 만큼 수행하여 시각적인 디버그를 완료하였습니다.
                // 추후 렌더링 RnD통한 최적화 필요할 수도 있음 2/5/2024
                for (int i = 0; i < 3; i++)
                {
                    // Update the RenderTexture
                    RenderTexture temp = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, RenderTextureFormat.ARGB32);
                    Graphics.Blit(renderTexture, temp, paintMaterial);
                    Graphics.Blit(temp, renderTexture);
                    //RenderTexture.ReleaseTemporary(temp);
                }
           
            }
        }
    }

    private void ResetDelayWithDelay()
    {
        StartCoroutine(ResetClickableWithDelayCo());
    }
    IEnumerator ResetClickableWithDelayCo()
    {
        if(!PreCheck()) yield break;
        _isPaintable = false;
        if(!PreCheck()) yield break;
        yield return _waitForPaint;
        if(!PreCheck()) yield break;
        _isPaintable = true;
    }

    //08/12/2024  타겟PC에서 센서 동작 이슈로 일정 딜레이를 넣고있습니다.
    // 약 0.12초보다 빠르게 페인팅을 하는경우, 원인미상 이슈로 텍스쳐에러가 발생합니다. 추후 RnD로 버그해결 필요합니다.
    private bool _isPaintable =true;
    private WaitForSeconds _waitForPaint = new WaitForSeconds(0.125f);

    public override void OnRaySynced()
    {
        base.OnRaySynced();
        if (!isStartButtonClicked) return;
        if(!PreCheck()) return;
        if (!_isPaintable)
        {
#if UNITY_EDITOR
            Debug.Log("It's not clickable");
#endif
            return;
        }
        
        ResetDelayWithDelay();
        
        Paint();
    }
}
