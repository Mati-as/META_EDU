using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Painting_StarryNightMultipleTexture : Base_GameManager
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

    public new float bgmVol;

     [Header("Shader Setting")] 
     public float burshStrength = 1;

     private static event Action ChangeScene; 
     public Volume vol;
     private Vignette vignette;
     private bool _isSceneChanging;
    protected override void Init()
    {
        BGM_VOLUME = bgmVol;
        gmSensorSensitivity = 0.5f; 
        
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
        
     //   Managers.Sound.Play(SoundManager.Sound.Bgm, "Audio/명화컨텐츠/gnossienne",volume:1.2f);
        

    }


    private void Start()
    {
        InitTexture();
    }
    
    protected override void BindEvent()
    {
        ChangeScene -= OnChangeScene;
        ChangeScene += OnChangeScene;
        base.BindEvent();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ChangeScene -= OnChangeScene;
    }

    private void OnChangeScene()
    {
        DOVirtual.Float(0, 1, 2.5f, val =>
        {
            vignette.intensity.value = val;
            _isSceneChanging = true;
        }).OnComplete(() =>
        {
            SceneManager.LoadScene("AB001FromBA001");
        });
        
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

    public float currentRotation;
    
    private float _elapsed;
    [FormerlySerializedAs("_timeLimitForSceneChange")] public float timeLimitForSceneChange;
    
    private void Update()
    {
        _elapsed += Time.deltaTime;
        if (_elapsed > timeLimitForSceneChange)
        {
            ChangeScene?.Invoke();
            _elapsed = 0; 
        }
        
    }
    void Paint()
    {
        if (!isStartButtonClicked) return;
        if (_isSceneChanging) return; 
        
        
        currentRotation = Random.Range(0,360);
      
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


    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        Paint();
    }
}
