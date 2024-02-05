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


     [Header("Shader Setting")] 
     public float burshStrength = 1;

    protected override void Init()
    {
        
        base.Init();
        
        Managers.Sound.Play(SoundManager.Sound.Bgm, "Audio/명화컨텐츠/gnossienne",volume:1.2f);
        SetInputSystem();
        
        renderTexture = new RenderTexture(textureToPaint.width, textureToPaint.height, 0, RenderTextureFormat.ARGB32);
        paintMaterial = new Material(paintShader);
        
        
        // Copy the original texture to the RenderTexture
        Graphics.Blit(textureToPaint, renderTexture);

        // Set the material's texture to the RenderTexture
        GetComponent<MeshRenderer>().material.mainTexture = renderTexture;
    }

    public float currentRotation;
    private readonly int BLIT_COUNT = 3;
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
                
                // 알파블렌딩과 렌더링 순서 간 원인미상 버그로 인해 클릭 시 한번에 전부 지워지지 않는 버그가 있습니다.
                // 이를 해결하기위해 Blit을 1:n 만큼 수행하여 시각적인 디버그를 완료하였습니다.
                // 추후 렌더링 RnD통한 최적화 필요할 수도 있음 2/5/2024
                for (int i = 0; i < BLIT_COUNT; i++)
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

    private void SetInputSystem()
    {
        // Initialize the paint action
        paintAction = new InputAction(binding: "<Mouse>/leftButton", interactions: "Press");
        paintAction.performed += ctx => Paint();
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
    
    protected override void OnRaySynced()
    {
        base.OnRaySynced();
        if (!isStartButtonClicked) return;
        
        Paint();
    }
}
