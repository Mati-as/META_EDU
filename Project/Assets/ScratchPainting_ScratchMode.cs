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
      
    }

    public float currentRotation;
    private RenderTexture currentlyPaintedTexture;
    void Paint()
    {
        if (!_isPaintable) return;
        StartCoroutine(PaintCoroutine());
    }

    IEnumerator PaintCoroutine()
    {
        yield return null;
        _isPaintable = false;
        currentRotation = Random.Range(-360,360);
      
        RaycastHit hit;
        if (Physics.Raycast(IGameManager.GameManager_Ray, out hit))
        {
           
            if (hit.transform == transform)
            {
                Vector2 uv = hit.textureCoord;
               
                // Convert to "_MouseUV" for the shader
                paintMaterial.SetFloat("_TextureRotationAngle", currentRotation);
                paintMaterial.SetVector("_MouseUV", new Vector4(uv.x, uv.y, 0, 0));
                
                // 알파블렌딩과 렌더링 순서 간 차이로 인해 클릭 시 한번에 전부 지워지지 않는 버그가 있습니다.
                // 이를 해결하기위해 Blit을 1:n 만큼 수행하여 시각적인 디버그를 완료하였습니다.
                // 추후 렌더링 RnD통한 최적화 필요할 수도 있음 2/5/2024
                // Update the RenderTexture
                currentlyPaintedTexture = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, RenderTextureFormat.ARGB32);
                var temp = currentlyPaintedTexture;
                Graphics.Blit(renderTexture, currentlyPaintedTexture, paintMaterial);
                Graphics.Blit(currentlyPaintedTexture, renderTexture);
                yield return DOVirtual.Float(0, 0, 0.001f, _ => { }).WaitForCompletion();
                //RenderTexture.ReleaseTemporary(temp);
                 
                
                
                // 중복 클릭으로 인한 함수에러 방지를 위한 방어적 코드 추가입니다. 4/1/2024
                _isPaintable = true;

            }
        }
    }



}
