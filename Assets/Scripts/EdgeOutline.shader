Shader "Custom/AdvancedEmissionFlashURP"
{
    Properties
    {
        // Main properties
        [MainTexture] _MainTex ("Albedo", 2D) = "white" {}
        [MainColor] _Color ("Color", Color) = (1,1,1,1)
        
        // Emission properties
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
        
        // Surface properties
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2 // Back
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        
        // Outline Pass
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            Cull Front
            ZWrite On
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float flash : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                float4 _OutlineColor;
                float _FlashSpeed;
                float _FlashMin;
                float _FlashMax;
            CBUFFER_END
            
            float CalculateSimpleFlash(float time, float speed, float minVal, float maxVal)
            {
                float flash = sin(time * speed) * 0.5 + 0.5;
                return lerp(minVal, maxVal, flash);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                // Calculate flash pulse
                float flash = CalculateSimpleFlash(_Time.y, _FlashSpeed, _FlashMin, _FlashMax);
                output.flash = flash;
                
                // Apply flash to outline width
                float outlineWidth = _OutlineWidth * (1.0 + flash * 0.3);
                float3 outlinePos = input.positionOS.xyz + input.normalOS * outlineWidth;
                output.positionCS = TransformObjectToHClip(outlinePos);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Outline can also pulse slightly
                half4 outline = _OutlineColor;
                outline.rgb *= (1.0 + input.flash * 0.1);
                return outline;
            }
            ENDHLSL
        }
        
        // Main Forward Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Cull [_Cull]
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // URP keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            
            // Unity keywords
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 texcoord : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float flash : TEXCOORD3;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 4);
                float3 viewDirWS : TEXCOORD5;
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                float4 shadowCoord : TEXCOORD6;
                #endif
                half fogFactor : TEXCOORD7;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Textures
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _EmissionColor;
                half4 _FlashColor;
                float _FlashSpeed;
                float _FlashMin;
                float _FlashMax;
                float _FlashRiseTime;
                float _FlashFallTime;
                float _FlashDelay;
                half _Smoothness;
                half _Metallic;
            CBUFFER_END
            
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
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                // Calculate flash with custom timing
                output.flash = CalculateFlash(
                    _Time.y, 
                    _FlashSpeed, 
                    _FlashMin, 
                    _FlashMax, 
                    _FlashRiseTime, 
                    _FlashFallTime, 
                    _FlashDelay
                );
                
                // Transform position and normal
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
                
                // Setup shadow coordinate if needed
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                output.shadowCoord = GetShadowCoord(vertexInput);
                #endif
                
                // Fog
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                
                // Lightmap and SH
                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Sample textures
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                half3 emissionTex = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb;
                half3 baseEmission = emissionTex * _EmissionColor.rgb;
                
                // Add flash emission
                half3 flashEmission = _FlashColor.rgb * input.flash;
                half3 finalEmission = baseEmission + flashEmission;
                
                // Get normal
                half3 normalWS = normalize(input.normalWS);
                
                // Get shadow coordinate
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                float4 shadowCoord = input.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                #else
                float4 shadowCoord = float4(0, 0, 0, 0);
                #endif
                
                // Main light
                Light mainLight = GetMainLight(shadowCoord, input.positionWS, half4(1, 1, 1, 1));
                half3 mainLightColor = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                
                // Lighting calculation
                half3 ambient = SampleSH(normalWS);
                
                // Basic Lambert lighting
                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = mainLightColor * ndotl * baseColor.rgb;
                
                // Simple specular (Blinn-Phong)
                half3 viewDir = normalize(input.viewDirWS);
                half3 halfDir = normalize(mainLight.direction + viewDir);
                half ndoth = saturate(dot(normalWS, halfDir));
                half specular = pow(ndoth, _Smoothness * 128.0) * _Metallic;
                half3 specularColor = mainLightColor * specular;
                
                // Combine lighting
                half3 litColor = (ambient + diffuse + specularColor) * baseColor.rgb;
                
                // Add emission
                half3 finalColor = litColor + finalEmission;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, baseColor.a);
            }
            ENDHLSL
        }
        
        // Shadow Caster Pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            float3 _LightDirection;
            float3 _LightPosition;
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                float _FlashSpeed;
                float _FlashMin;
                float _FlashMax;
            CBUFFER_END
            
            float CalculateSimpleFlash(float time, float speed, float minVal, float maxVal)
            {
                float flash = sin(time * speed) * 0.5 + 0.5;
                return lerp(minVal, maxVal, flash);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                
                // Calculate flash for outline offset
                float flash = CalculateSimpleFlash(_Time.y, _FlashSpeed, _FlashMin, _FlashMax);
                float outlineWidth = _OutlineWidth * (1.0 + flash * 0.3);
                
                // Apply outline offset
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz + input.normalOS * outlineWidth);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif
                
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                
                output.uv = input.texcoord;
                output.positionCS = positionCS;
                return output;
            }
            
            half4 frag(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
        
        // Depth Only Pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            ColorMask 0
            ZWrite On
            ZTest LEqual
            Cull [_Cull]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                float _FlashSpeed;
                float _FlashMin;
                float _FlashMax;
            CBUFFER_END
            
            float CalculateSimpleFlash(float time, float speed, float minVal, float maxVal)
            {
                float flash = sin(time * speed) * 0.5 + 0.5;
                return lerp(minVal, maxVal, flash);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                // Calculate flash for outline offset
                float flash = CalculateSimpleFlash(_Time.y, _FlashSpeed, _FlashMin, _FlashMax);
                float outlineWidth = _OutlineWidth * (1.0 + flash * 0.3);
                
                // Apply outline offset
                float3 positionOS = input.positionOS.xyz + input.normalOS * outlineWidth;
                output.positionCS = TransformObjectToHClip(positionOS);
                output.uv = input.texcoord;
                return output;
            }
            
            half4 frag(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Simple Lit"
}