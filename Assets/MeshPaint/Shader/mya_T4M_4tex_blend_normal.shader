/*
@mya
2017-02-15 12:09:36

支持4张贴图进行基于Splatting算法的地表混合

利用贴图的A通道影响混合的强度，实现更自然的过渡

2017-04-05 16:39:01
增加法线贴图的支持
增加高光支持

*/

Shader"Mya/texBlend/mya_4tex_blend_normal" 
{
    Properties 
    {
        _LightCol("Light Color", Color) = (1,1,1,1)
        _Gloss("_Gloss" , Range(0,1)) = 0.8
    	_Splat0 ("Layer 1(RGBA)", 2D) = "white" {}
        _BumpSplat0 ("Layer 1 Normal(Bump)", 2D) = "Bump" {}
    	_Splat1 ("Layer 2(RGBA)", 2D) = "white" {}
        _BumpSplat1 ("Layer 2 Normal(Bump)", 2D) = "Bump" {}
    	_Splat2 ("Layer 3(RGBA)", 2D) = "white" {}
        _BumpSplat2 ("Layer 3 Normal(Bump)", 2D) = "Bump" {}
        _Splat3 ("Layer 4(RGBA)", 2D) = "white" {}
        _BumpSplat3 ("Layer 4 Normal(Bump)", 2D) = "Bump" {}
    	_Control ("Control (RGBA)", 2D) = "white" {}
        _Weight("Blend Weight" , Range(0.001,1)) = 0.2      
    }
                    
    SubShader {
    	Tags {
    	   "SplatCount" = "4"
    	   "RenderType" = "Opaque"
           
    	}
    CGPROGRAM
    #pragma surface surf BlendModel
    #pragma exclude_renderers xbox360 ps3
    #pragma target 3.0

         
    sampler2D _Control;
    sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
    sampler2D _BumpSplat0,_BumpSplat1,_BumpSplat2,_BumpSplat3;

    half _Weight;
    half4 _LightDir;
    fixed4 _LightCol;
    half _Gloss;


    struct Input 
    {
    	half2 uv_Control : TEXCOORD0;
    	half2 uv_Splat0 : TEXCOORD1;
    	half2 uv_Splat1 : TEXCOORD2;
    	half2 uv_Splat2 : TEXCOORD3;
    	half2 uv_Splat3 : TEXCOORD4;
    };


    inline half4  LightingBlendModel(SurfaceOutput s, half3 lightDir,half3 viewDir, half atten)  
    {
            half4 col;  
  
            half diffuseF = max(0,dot(s.Normal,lightDir));  
            
            _Gloss = pow(4096,_Gloss);
            half specF;  
            half3 H = normalize(lightDir+viewDir);  
            half specBase = max(0,dot(s.Normal,H));  
            specF = pow(specBase,_Gloss);  
  
            col.rgb = s.Albedo * _LightColor0 * diffuseF *atten + _LightCol*specF;   
            col.a = s.Alpha;  
            return col;  

    }


    inline half4 Blend(half depth1 ,half depth2,half depth3,half depth4 , fixed4 control) 
    {
        half4 blend ;
        
        blend.r =depth1 * control.r;
        blend.g =depth2 * control.g;
        blend.b =depth3 * control.b;
        blend.a =depth4 * control.a;
        
        half ma = max(blend.r, max(blend.g, max(blend.b, blend.a)));
        blend = max(blend - ma +_Weight , 0) * control;
        return blend/(blend.r + blend.g + blend.b + blend.a);
    }

    void surf (Input IN, inout SurfaceOutput o) {
    	fixed4 splat_control = tex2D (_Control, IN.uv_Control).rgba;	
        //纹理贴图
    	fixed4 lay1 = tex2D (_Splat0, IN.uv_Splat0);
    	fixed4 lay2 = tex2D (_Splat1, IN.uv_Splat1);
    	fixed4 lay3 = tex2D (_Splat2, IN.uv_Splat2);
    	fixed4 lay4 = tex2D (_Splat3, IN.uv_Splat3);

        //纹理贴图
        fixed3 nor1 = UnpackNormal(tex2D (_BumpSplat0, IN.uv_Splat0));
    	fixed3 nor2 = UnpackNormal(tex2D (_BumpSplat1, IN.uv_Splat1));
    	fixed3 nor3 = UnpackNormal(tex2D (_BumpSplat2, IN.uv_Splat2));
    	fixed3 nor4 = UnpackNormal(tex2D (_BumpSplat3, IN.uv_Splat3));

        //纯色测试代码
        //lay1.rgb = fixed3(1,0,0);
        //lay2.rgb = fixed3(0,1,0);
        //lay3.rgb = fixed3(0,0,1);
        //lay4.rgb = fixed3(0,0,0);

        //计算混合
        half4 blend = Blend(lay1.a,lay2.a,lay3.a,lay4.a,splat_control);

    	o.Alpha = 0.0;
        o.Albedo.rgb = lay1 *blend.r  + lay2 * blend.g + lay3 * blend.b + lay4 * blend.a;//混合
        o.Normal = nor1 * blend.r  + nor2 * blend.g  + nor3 * blend.b  + nor4 * blend.a;//法线混合


    }
    ENDCG 
    }
    FallBack "Specular"
}