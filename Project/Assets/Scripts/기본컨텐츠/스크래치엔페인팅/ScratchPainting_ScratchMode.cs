using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class ScratchPainting_ScratchMode : IGameManager
{
     [SerializeField]
    private Shader paintShader;
    private Material paintMaterial;
    private Texture2D renderTexture;
    private MeshRenderer _meshRenderer;
    [SerializeField]
    private Texture2D textureToPaintOn;
    public float brushSize = 0.1f;
    [FormerlySerializedAs("burshTexture")] [FormerlySerializedAs("burshTextures")] [SerializeField]
    private Texture2D brushTexture;// Define an InputAction for painting

    private ScratchPainting_ScratchMode _scratchMode;

    public new float bgmVol;

  [Header("Shader Setting")] 
     public float brushStrength = 1;

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
     Color _glowDefaultColor;
         
     private Sequence _glowSeq;
     private SpriteRenderer _outlineSpRenderer;

     private void Start()
    {
        InitTexture();
        
        _outlineSpRenderer = GameObject.Find("GlowTexture").GetComponent<SpriteRenderer>();
        _glowDefaultColor = _outlineSpRenderer.material.color;
        _outlineSpRenderer.enabled = false;
    }

     
     private void OnStampingFinished()
     {
         _isPaintable = true;
         ActivateTextrue();
         
         BlinkOutline();
         _outlineSpRenderer.DOFade(0,  0.001f).OnComplete(() =>
         {
             _outlineSpRenderer.DOFade(1,  2f);
         });
       
         _outlineSpRenderer.enabled = true;
     }

     private void ActivateTextrue()
     {
         DOVirtual.Float(0, 0, 6.5f, _ => { })
             .OnComplete(() => { _meshRenderer.material.DOFade(1, 1.5f); });
     }
 
     private void BlinkOutline()
     {
#if UNITY_EDITOR
         Debug.Log("Outline Glowing~~!@##~!@#@!~$@~#");
#endif
         _glowSeq = DOTween.Sequence();
         
         _glowSeq.Append(_outlineSpRenderer.material.DOColor(_glowDefaultColor * 2f, 1.0f));
         _glowSeq.AppendInterval(0.1f);
         _glowSeq.Append(_outlineSpRenderer.material.DOColor(_glowDefaultColor * 0.5f, 0.7f));
         _glowSeq.AppendInterval(0.5f);
         _glowSeq.SetLoops(-1, LoopType.Yoyo);

         _glowSeq.Play();
     }

     /// <summary>
    /// Awake단게로 옮기지말 것,renderTexture Access Deny되는 버그 발생가능성 있음
    /// </summary>
     
    private Texture2D paintTexture;
     private void InitTexture()
     {
         // If textureToPaintOn has mipmaps, ensure paintTexture does as well.
         bool hasMipMaps = textureToPaintOn.mipmapCount > 1;
         paintTexture = new Texture2D(textureToPaintOn.width, textureToPaintOn.height, textureToPaintOn.format, hasMipMaps);

         // Copy the texture, including mipmaps if present
         Graphics.CopyTexture(textureToPaintOn, paintTexture);

         _meshRenderer = GetComponent<MeshRenderer>();
         _meshRenderer.material.mainTexture = paintTexture;
         _meshRenderer.material.DOFade(0, 0.001f);

         // No need to use paintMaterial if we're not using RenderTextures anymore.
     }

    private void Paint()
    {
        if (!_isPaintable) return;

        // Assume GameManager_Hits is an array of RaycastHit from the game manager
        foreach (var hit in GameManager_Hits)
        {
            if (hit.transform == transform)
            {
#if UNITY_EDITOR
                Debug.Log($"Painting (clicked)");
#endif
                Vector2 uv = hit.textureCoord;

                // Convert textureCoord to pixel coordinates
                int x = (int)(uv.x * paintTexture.width);
                int y = (int)(uv.y * paintTexture.height);
                int brushSizePixels = (int)(brushSize * Mathf.Max(paintTexture.width, paintTexture.height));

                // Paint a circle at the hit position
                for (int i = -brushSizePixels; i <= brushSizePixels; i++)
                {
                    for (int j = -brushSizePixels; j <= brushSizePixels; j++)
                    {
                        if (i * i + j * j <= brushSizePixels * brushSizePixels)
                        {
                            int px = Mathf.Clamp(x + i, 0, paintTexture.width - 1);
                            int py = Mathf.Clamp(y + j, 0, paintTexture.height - 1);

                            // Use brush color here if needed instead of Color.clear
                            Color brushColor = brushTexture.GetPixel(0, 0);

                            Color existingColor = paintTexture.GetPixel(px, py);
                            Color finalColor = Color.Lerp(existingColor, brushColor, brushStrength);

                            paintTexture.SetPixel(px, py, finalColor);
                        }
                    }
                }

                // Apply all SetPixel changes
                paintTexture.Apply(updateMipmaps: true);
            }
        }
    }


}
