// Advanced Emission Flash + Outline Shader
// Has separate controls for flash timing
Shader "Custom/AdvancedEmissionFlash" 
{
    Properties 
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _EmissionMap ("Emission Map", 2D) = "black" {}
        
        // Flash Properties
        _FlashSpeed ("Flash Speed", Range(0.1, 10)) = 2.0
        _FlashMin ("Flash Min", Range(0, 1)) = 0.0
        _FlashMax ("Flash Max", Range(0, 5)) = 2.0
        _FlashColor ("Flash Color", Color) = (1,1,1,1)
        _FlashRiseTime ("Flash Rise Time", Range(0.1, 2)) = 0.5
        _FlashFallTime ("Flash Fall Time", Range(0.1, 2)) = 0.5
        _FlashDelay ("Flash Delay", Range(0, 5)) = 1.0
        
        // Outline
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
    }
    
    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        
        // Outline Pass
        Pass 
        {
            Cull Front
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata 
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f 
            {
                float4 pos : SV_POSITION;
                float flash : TEXCOORD0;
            };
            
            float _OutlineWidth;
            float4 _OutlineColor;
            float _FlashSpeed;
            float _FlashMin;
            float _FlashMax;
            
            v2f vert(appdata v) 
            {
                v2f o;
                
                // Calculate flash pulse
                float flash = sin(_Time.y * _FlashSpeed) * 0.5 + 0.5;
                flash = lerp(_FlashMin, _FlashMax, flash);
                o.flash = flash;
                
                // Apply flash to outline width
                float outlineWidth = _OutlineWidth * (1.0 + flash * 0.3);
                float3 outlinePos = v.vertex.xyz + v.normal * outlineWidth;
                o.pos = UnityObjectToClipPos(float4(outlinePos, 1.0));
                
                return o;
            }
            
            float4 frag(v2f i) : SV_TARGET 
            {
                // Outline can also pulse slightly
                float4 outline = _OutlineColor;
                outline.rgb *= (1.0 + i.flash * 0.1);
                return outline;
            }
            ENDCG
        }
        
        // Main Pass
        Pass 
        {
            Tags { "LightMode"="ForwardBase" }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            
            struct appdata 
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f 
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float flash : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            sampler2D _EmissionMap;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _EmissionColor;
            float4 _FlashColor;
            float _FlashSpeed;
            float _FlashMin;
            float _FlashMax;
            float _FlashRiseTime;
            float _FlashFallTime;
            float _FlashDelay;
            
            // Custom flash function with control over rise/fall times
            float CalculateFlash(float time, float speed, float minVal, float maxVal, float riseTime, float fallTime, float delay)
            {
                // Normalized time in cycle
                float cycleTime = fmod(time * speed, 1.0 + delay);
                
                if (cycleTime <= 1.0) 
                {
                    // Flash period (0 to 1)
                    if (cycleTime < riseTime) 
                    {
                        // Rise phase
                        return lerp(minVal, maxVal, cycleTime / riseTime);
                    }
                    else if (cycleTime < (1.0 - fallTime)) 
                    {
                        // Peak phase
                        return maxVal;
                    }
                    else 
                    {
                        // Fall phase
                        float fallStart = 1.0 - fallTime;
                        float fallProgress = (cycleTime - fallStart) / fallTime;
                        return lerp(maxVal, minVal, fallProgress);
                    }
                }
                else 
                {
                    // Delay period
                    return minVal;
                }
            }
            
            v2f vert(appdata v) 
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                
                // Calculate flash with custom timing
                o.flash = CalculateFlash(
                    _Time.y, 
                    _FlashSpeed, 
                    _FlashMin, 
                    _FlashMax, 
                    _FlashRiseTime, 
                    _FlashFallTime, 
                    _FlashDelay
                );
                
                return o;
            }
            
            float4 frag(v2f i) : SV_TARGET 
            {
                // Get base color
                float4 texColor = tex2D(_MainTex, i.uv) * _Color;
                
                // Get emission
                float3 emissionTex = tex2D(_EmissionMap, i.uv).rgb;
                float3 baseEmission = emissionTex * _EmissionColor.rgb;
                
                // Apply lighting to base color only
                float3 worldNormal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ndotl = max(0, dot(worldNormal, lightDir));
                float3 lighting = ndotl * _LightColor0.rgb + unity_AmbientSky.rgb;
                
                float3 litColor = texColor.rgb * lighting;
                
                // Add flash emission
                float3 flashEmission = _FlashColor.rgb * i.flash;
                float3 finalEmission = baseEmission + flashEmission;
                
                // Combine
                float3 finalColor = litColor + finalEmission;
                
                return float4(finalColor, texColor.a);
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}