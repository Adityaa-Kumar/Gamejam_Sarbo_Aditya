Shader "Custom/ASCIIShader"
{
    Properties
    {
        _FontTex ("Font Atlas", 2D) = "white" {}
        _Columns ("Column Count", Float) = 100
        _CharCount ("Character Count", Float) = 10
        _CharWidth ("Character Width in Atlas", Range(0, 1)) = 0.1
        _CharHeight ("Character Height in Atlas", Range(0, 1)) = 0.1
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "ASCIIPass"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            TEXTURE2D(_FontTex);
            SAMPLER(sampler_FontTex);
            
            float _Columns;
            float _CharCount;
            float _CharWidth;
            float _CharHeight;
            
            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                
                // Calculate aspect ratio
                float aspectRatio = _ScreenParams.y / _ScreenParams.x;
                float rows = _Columns * aspectRatio;
                
                // Determine which ASCII cell we're in
                float2 cellCount = float2(_Columns, rows);
                float2 cellID = floor(uv * cellCount);
                float2 cellUV = frac(uv * cellCount);
                
                // Sample the center of the cell from the scene
                float2 sampleUV = (cellID + 0.5) / cellCount;
                float4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, sampleUV);
                
                // Calculate luminance
                float luminance = dot(sceneColor.rgb, float3(0.299, 0.587, 0.114));
                
                // Map luminance to character index
                float charIndex = floor((1.0 - luminance) * (_CharCount - 1));
                charIndex = clamp(charIndex, 0, _CharCount - 1);
                
                // Calculate UV within the specific character cell in the atlas
                // Assuming horizontal strip layout
                float2 charOffset = float2(charIndex * _CharWidth, 0);
                float2 fontUV = charOffset + (cellUV * float2(_CharWidth, _CharHeight));
                
                // Sample from font atlas
                float4 fontSample = SAMPLE_TEXTURE2D(_FontTex, sampler_FontTex, fontUV);
                
                // For SDF fonts, use the alpha channel or distance field value
                float charValue = fontSample.a;
                
                // If alpha is empty, try red channel (some SDF fonts use R)
                if (charValue < 0.01) charValue = fontSample.r;
                
                // Apply threshold for SDF
                charValue = smoothstep(0.4, 0.6, charValue);
                
                // Colored ASCII output
                return float4(sceneColor.rgb * charValue, 1.0);
            }
            ENDHLSL
        }
    }
}
