using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Painting_MultipleTexture : IGameManager
{
  public Shader paintShader;
    private Material paintMaterial;
    private RenderTexture renderTexture;
    private MeshRenderer _meshRenderer;
    public Texture2D textureToPaintOn;
    public float brushSize = 0.1f;
 
    [FormerlySerializedAs("burshTexture")] public Texture2D[] burshTextures;// Define an InputAction for painting


     [Header("Shader Setting")] 
     public float burshStrength = 1;

    protected override void Init()
    {
        
        base.Init();
        
        Managers.Sound.Play(SoundManager.Sound.Bgm, "Audio/명화컨텐츠/gnossienne",volume:1.2f);
        
        renderTexture = new RenderTexture(textureToPaintOn.width, textureToPaintOn.height, 0, RenderTextureFormat.ARGB32);
        paintMaterial = new Material(paintShader);
        
        
        // Copy the original texture to the RenderTexture
        Graphics.Blit(textureToPaintOn, renderTexture);

        // Set the material's texture to the RenderTexture
        GetComponent<MeshRenderer>().material.mainTexture = renderTexture;
    }

    public float currentRotation;
    void Paint()
    {
        if (!isStartButtonClicked) return;
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
                // Update the RenderTexture
                RenderTexture temp = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(renderTexture, temp, paintMaterial);
                Graphics.Blit(temp, renderTexture);
             //   RenderTexture.ReleaseTemporary(temp);
            }
        }
    }


    protected override void OnRaySynced()
    {
        base.OnRaySynced();
        if (!isStartButtonClicked) return;
        
        Paint();
    }
}
