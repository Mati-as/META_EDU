using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Painting_PaintableTextureController : IGameManager
{
    public Shader paintShader;
    private Material paintMaterial;
    private RenderTexture renderTexture;
    private MeshRenderer _meshRenderer;
    public Texture2D textureToPaint;
    public float brushSize = 0.1f;
    public InputAction paintAction;
    public Texture2D burshTexture;// Define an InputAction for painting


     [Header("Shader Seting")] 
     public float burshStrength = 1;
    void Start()
    {
     
        
        SetInputSystem();
        
        renderTexture = new RenderTexture(textureToPaint.width, textureToPaint.height, 0, RenderTextureFormat.ARGB32);
        paintMaterial = new Material(paintShader);
        
        
        // Copy the original texture to the RenderTexture
        Graphics.Blit(textureToPaint, renderTexture);

        // Set the material's texture to the RenderTexture
        GetComponent<MeshRenderer>().material.mainTexture = renderTexture;

        
    }
    
    void StartPainting()
    {
        RaycastHit hit;
        if (Physics.Raycast(GameManager_Ray, out hit))
        {
           
            if (hit.transform == transform)
            {
                Vector2 uv = hit.textureCoord;
                
                paintMaterial.SetFloat("_BrushStrength",burshStrength );
                paintMaterial.SetTexture("_BrushTex",burshTexture);
              
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

    private void SetInputSystem()
    {
        // Initialize the paint action
        paintAction = new InputAction(binding: "<Mouse>/leftButton", interactions: "Press");
        paintAction.performed += ctx => StartPainting();
        paintAction.Enable();
    }
    
    private void OnEnable()
    {
        paintAction.Enable();
    }

    private void OnDisable()
    {
        paintAction.Disable();
    }
}
