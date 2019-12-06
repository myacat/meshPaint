/**************************************************************************************************
2019-12-06 17:21:03
@yvanliao
Cloth simulation Shader
***************************************************************************************************/

Shader "LGame/Effect/MyaClothSimulation" 
{
    Properties 
    {
        [Enum(UnityEngine.Rendering.BlendMode)]             _SrcBlend ("__SrcFactor", Float) = 5.0
        [Enum(UnityEngine.Rendering.BlendMode)]             _DstBlend ("__DstFactor", Float) = 10.0
        [KeywordEnum(Heightmap, NormalMap , doubleMap)]     _Mode("Rendering Mode", Float) = 0.0
        
        _Color ("Tint", Color) = (1,1,1,1)
        _MainTex ("Sprite Texture", 2D) = "white" {}
        
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1.0
        
        _HeightMap ("Height Texture", 2D) = "white" {}
        _HeightScale("Height _Scale" , float) = 1.0
        _Smoothness("Smoothness" , float) = 1
        
        _WaveMask ("WaveMask", 2D) = "white" {}
        _WaveIntensity ("Wave Intensity", Range(-0.1, 0.1)) = 0.01    
        
        
        [hdr]_LightColor("Light Color" , Color) = (1,1,1,1)
        [hdr]_AmbientCol("Ambient Color" , Color) = (0,0,0,0)
        _LightDir("Light Direction" , Vector) = (1,1,-1,0)

        _ScreenAtten("Screen Atten" , float) = 1

        [PerRendererData] _StencilComp ("Stencil Comparison", Float) = 8
        [PerRendererData] _Stencil ("Stencil ID", Float) = 0
        [PerRendererData] _StencilOp ("Stencil Operation", Float) = 0
        [PerRendererData] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [PerRendererData] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [PerRendererData] _ColorMask ("Color Mask", Float) = 15
    }
    SubShader {
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend [_SrcBlend] [_DstBlend]
        ColorMask [_ColorMask]

        Pass {
            Name "Default"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _MODE_HEIGHTMAP _MODE_NORMALMAP _MODE_DOUBLEMAP 

            #include "UnityCG.cginc"
            #pragma target 3.0


            fixed4      _Color;

            sampler2D   _MainTex;
            float4      _MainTex_ST;

            sampler2D   _BumpMap;
            half4       _BumpMap_ST;
            half        _BumpScale;

            sampler2D   _WaveMask ;
            float4      _WaveMask_ST;
            half        _WaveIntensity;
            fixed4      _LightColor;
            half4       _LightDir;
            fixed4      _AmbientCol;
            half        _ScreenAtten;

            sampler2D   _HeightMap;
            float4      _HeightMap_ST;
            float4      _HeightMap_TexelSize;
            float       _Smoothness;
            float       _HeightScale;

            struct a2v 
            {
                float4 vertex   : POSITION;
                fixed4 color    : COLOR;
                float4 tangent  : TANGENT;
                float3 normal   : NORMAL;
                float2 texcoord : TEXCOORD0;
            };
            struct v2f 
            {
                fixed4 color                : COLOR;
                float4 pos                  : SV_POSITION;
                float2 uv                   : TEXCOORD0;
                float4 uvMask               : TEXCOORD1;
                half4 screenPos             : TEXCOORD2;
                half3 viewDir               : TEXCOORD3;
                half3 lightDir              : TEXCOORD4;
            };


            v2f vert (a2v v ) 
            {
                v2f o = (v2f)0;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);
                o.color = v.color * _Color;

                TANGENT_SPACE_ROTATION; 
                o.viewDir = mul (rotation, ObjSpaceViewDir(v.vertex));
                o.lightDir =  mul (rotation, _LightDir.xyz);

                o.uv.xy =  TRANSFORM_TEX(v.texcoord, _MainTex) ;
                o.uvMask.xy = v.texcoord * _BumpMap_ST.xy   + frac(_Time.y * _BumpMap_ST.zw);
                o.uvMask.zw = v.texcoord * _HeightMap_ST.xy + frac(_Time.y * _HeightMap_ST.zw);

                return o;
            }



            fixed4 frag(v2f i) : SV_Target 
            {

                half3 viewDir = normalize(i.viewDir);
                half3 lightDir = normalize(i.lightDir);
                half3 normalDir;
                half height;
                half2 uvOffset;
                #if _MODE_HEIGHTMAP
                    float2 du = float2(_HeightMap_TexelSize.x * _Smoothness, 0);
                    float u1 = tex2D(_HeightMap, i.uvMask.zw - du);
                    float u2 = tex2D(_HeightMap, i.uvMask.zw + du);
                    float3 tu = float3(1, 0, (u2 - u1) * _HeightScale);
            
                    float2 dv = float2(0, _HeightMap_TexelSize.y * _Smoothness);
                    float v1 = tex2D(_HeightMap, i.uvMask.zw - dv);
                    float v2 = tex2D(_HeightMap, i.uvMask.zw + dv);
                    float3 tv = float3(0, 1, (v2 - v1) * _HeightScale);

                    height = tex2D(_HeightMap, i.uvMask.zw); 
                    uvOffset = viewDir.xy/viewDir.z *height* _WaveIntensity ;
                    normalDir = normalize(-cross(tv, tu)); //这里加不加负号可以放到高度图的a通道来决定
                #elif _MODE_NORMALMAP
                    height = tex2D(_BumpMap, i.uvMask.xy).a; 
                    uvOffset = viewDir.xy/viewDir.z *height* _WaveIntensity ;
                    half3 normalTex = UnpackNormal(tex2D(_BumpMap , i.uvMask.xy + uvOffset));
                    normalTex.xy *= _BumpScale ;
                    normalDir=normalize(normalTex);
                #elif _MODE_DOUBLEMAP
                    height = tex2D(_HeightMap, i.uvMask.zw); 
                    uvOffset = viewDir.xy/viewDir.z *height* _WaveIntensity ;
                    half3 normalTex = UnpackNormal(tex2D(_BumpMap , i.uvMask.xy + uvOffset));
                    normalTex.xy *= _BumpScale ;
                    normalDir=normalize(normalTex);
                #else
                   normalDir = float3(0,0,1);
                   height = 0;
                #endif

                half4 texCol = tex2D(_MainTex, i.uv + uvOffset) * i.color;

                half atten = saturate(_ScreenAtten * (1 - i.screenPos.y/i.screenPos.w));

                half Mask = tex2D(_WaveMask , i.uv);

                half3 halfDir = normalize( viewDir + lightDir);
                half spec = pow(saturate(dot(halfDir, normalDir)) ,32) ;
                half diff =  saturate(dot(normalDir, lightDir));
                

				half3 col = texCol * lerp( _AmbientCol.rgb ,_LightColor.rgb  , diff ) * atten + _LightColor.rgb * spec * atten; 
				half alpha = texCol.a;

				//col =  texCol.rgb  * nl;
                return fixed4(col ,alpha);
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
    CustomEditor"FlagWaveShaderGUI"
}
