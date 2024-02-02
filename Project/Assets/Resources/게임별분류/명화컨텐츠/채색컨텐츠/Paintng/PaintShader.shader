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
}