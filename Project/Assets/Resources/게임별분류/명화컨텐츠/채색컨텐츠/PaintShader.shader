Shader "Custom/PaintShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MouseUV ("Mouse UV", Vector) = (-1,-1,0,0)
        _BrushSize ("Brush Size", Float) = 0.1
        _BrushStrength ("Brush Strength", Float) = 0
        _BrushTex ("Brush Texture", 2D) = "white" {}
        _BrushTexTilingOffset ("Brush Tex Tiling and Offset", Vector) = (1,1,0,0)
        _TextureRotationAngle("Rotation Angle", Float) = 0.0
        
    }
    SubShader
    {
        
    Tags{
        "RenderPipeline"= "UniversalPipeline"
        "RenderType"= "Transparent"
            "RenderQueue"= "Transparent"
        }
    
    Pass
    {
        Name "Universal Forward"
        Tags {"LightMode" = "UniversalForward"} 
        
        Blend SrcAlpha OneMinusSrcAlpha
        
        HLSLPROGRAM
        #pragma prefer_hlslcc gles
        #pragma exclude_renderers d3d11_9x
        
        #pragma vertex vert
        #pragma fragment frag
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        float4 _MouseUV;
        float _BrushStrength;
        Texture2D _MainTex;Texture2D _BrushTex;
        float   _TextureRotationAngle;
        SamplerState sampler_MainTex;
        SamplerState sampler_BrushTex;
        float _BrushSize;
        float4 _BrushTexTilingOffset;

        float4 _MainTex_ST;
        
        struct VertexInput
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct VertexOutput
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

   
        VertexOutput vert (VertexInput v)
        {
            VertexOutput o;
            o.uv = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
            o.vertex = TransformObjectToHClip(v.vertex);
            return o;
        }

        half4 frag (VertexOutput i) : SV_Target
        {
            float2 uv = i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;

            
            
            half4 col = _MainTex.Sample(sampler_MainTex, i.uv);
            float2 brushUV = (uv - _MouseUV.xy) / _BrushSize + 0.5; // 중심을 기준으로 정규화

            // 회전 각도를 라디안으로 변환
            float rad = _TextureRotationAngle * (PI / 180.0);
            // 회전을 위한 2D 회전 행렬 계산
            float2x2 rotationMatrix = float2x2(cos(rad), -sin(rad), sin(rad), cos(rad));
            // 브러시 UV 중심을 (0.5, 0.5)로 이동 후 회전 적용
            
            brushUV = mul(brushUV - 0.5, rotationMatrix) + 0.5;
            
            brushUV = clamp(brushUV, 0.0, 1.0);

            if (brushUV.x == 0.0 || brushUV.x == 1.0 || brushUV.y == 0.0 || brushUV.y == 1.0)
            {
                return col;
            }

            half4 brushColor = _BrushTex.Sample(sampler_BrushTex, brushUV);
            float brushAlpha = clamp(brushColor.a, 0, 1.0);
            if(brushAlpha < 0.1)
            {
                brushAlpha = 0;
            }

            if (distance(i.uv, _MouseUV.xy) < _BrushSize)
            {
                col.a = lerp(col.a, 0, brushAlpha * _BrushStrength);
            }

            return col;
        }
        ENDHLSL
    }
  }
  // yor can tell the unity to use it's bulit in Diffuse shader if your custom shader fails to compile or isn't supported on the use's hardware
  Fallback "Diffuse"
}