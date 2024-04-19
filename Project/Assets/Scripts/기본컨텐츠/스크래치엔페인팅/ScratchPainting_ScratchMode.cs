using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ScratchPainting_ScratchMode : MonoBehaviour
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

    private ScratchPainting_ScratchMode _scratchMode;

    public new float bgmVol;

     [Header("Shader Setting")] 
     public float burshStrength = 1;

     private bool _isPaintable; // 도장직기 중에는 플레아금지 

     private void Awake()
     {
         IGameManager.On_GmRay_Synced -= Paint;
         IGameManager.On_GmRay_Synced += Paint;
         ScratchPainting_GameManager.OnStampingFinished -= OnStampingFinished;
         ScratchPainting_GameManager.OnStampingFinished += OnStampingFinished;
     }

     private void OnDestroy()
     {
         ScratchPainting_GameManager.OnStampingFinished -= OnStampingFinished;
     }

     private void Start()
    {
        InitTexture();
    }

     private void OnStampingFinished()
     {
         _isPaintable = true;
         ActivateTextrue();
     }

     private void ActivateTextrue()
     {
         DOVirtual.Float(0, 0, 6.5f, _ => { })
             .OnComplete(() => { _meshRenderer.material.DOFade(1, 1.5f); });
     }

     /// <summary>
    /// Awake단게로 옮기지말 것,renderTexture Access Deny되는 버그 발생가능성 있음
    /// </summary>
    
    private void InitTexture()
    {
        renderTexture = new RenderTexture(textureToPaintOn.width, textureToPaintOn.height, 0, RenderTextureFormat.ARGB32);
        paintMaterial = new Material(paintShader);
     
        Graphics.Blit(textureToPaintOn, renderTexture);
        
        // Set the material's texture\
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material.mainTexture = renderTexture;
        _meshRenderer.material.DOFade(0, 0.001f);
        
       
        paintMaterial.SetFloat("_BrushStrength",burshStrength );
        paintMaterial.SetTexture("_BrushTex",burshTextures[Random.Range(0,burshTextures.Length)]);
        paintMaterial.SetFloat("_BrushSize", brushSize);

        _width = renderTexture.width;
        _height = renderTexture.height;

    }

    public float currentRotation;
    private RenderTexture _currentlyPaintedTexture;
    private RenderTexture _tempTexture;
    private int _height;
    private int _width;

    void Paint()
    {
        RaycastHit hit;
        if (Physics.Raycast(IGameManager.GameManager_Ray, out hit) && hit.transform == transform)
        {
#if UNITY_EDITOR
            Debug.Log($"Painting (clicked)");
#endif
            Vector2 uv = hit.textureCoord;
            paintMaterial.SetFloat("_TextureRotationAngle", currentRotation);
            paintMaterial.SetVector("_MouseUV", new Vector4(uv.x, uv.y, 0, 0));

            _tempTexture = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, RenderTextureFormat.ARGB32);
            RenderTexture.active = _tempTexture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, _width, _height, 0);
            Graphics.DrawTexture(new Rect(0, 0, _width, _height), renderTexture, paintMaterial);
            GL.PopMatrix();
            RenderTexture.active = null;

            Graphics.CopyTexture(_tempTexture, renderTexture);
            RenderTexture.ReleaseTemporary(_tempTexture);
        }
    }
    


}
