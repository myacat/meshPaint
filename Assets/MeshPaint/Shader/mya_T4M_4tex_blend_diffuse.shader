/*
@mya
2017-02-15 12:09:36

支持4张贴图进行基于Splatting算法的地表混合

利用贴图的A通道影响混合的强度，实现更自然的过渡

*/


Shader"Mya/texBlend/mya_4tex_blend_diffuce" 
{
    Properties {
    	_Splat0 ("Layer 1(RGBA)", 2D) = "white" {}
    	_Splat1 ("Layer 2(RGBA)", 2D) = "white" {}
    	_Splat2 ("Layer 3(RGBA)", 2D) = "white" {}
    	_Splat3 ("Layer 4(RGBA)", 2D) = "white" {}
        _Tiling3("_Tiling4 x/y", Vector)=(1,1,0,0)
    	_Control ("Control (RGBA)", 2D) = "white" {}
        _Weight("Blend Weight" , Range(0.001,1)) = 0.2
        
    }
                    
    SubShader {
    	Tags {
    	   "SplatCount" = "4"
    	   "RenderType" = "Opaque"
           
    	}
    CGPROGRAM
    #pragma surface surf BlinnPhong
    #pragma exclude_renderers xbox360 ps3
    #pragma target 2.0

    struct Input 
    {
    	float2 uv_Control : TEXCOORD0;
    	float2 uv_Splat0 : TEXCOORD1;
    	float2 uv_Splat1 : TEXCOORD2;
    	float2 uv_Splat2 : TEXCOORD3;
    	//float2 uv_Splat3 : TEXCOORD4;
    };
     
    sampler2D _Control;
    sampler2D _Splat0,_Splat1,_Splat2,_Splat3;

    float4 _Tiling3;
    float _Weight;

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
    	half4 splat_control = tex2D (_Control, IN.uv_Control).rgba;
    		
    	half4 lay1 = tex2D (_Splat0, IN.uv_Splat0);
    	half4 lay2 = tex2D (_Splat1, IN.uv_Splat1);
    	half4 lay3 = tex2D (_Splat2, IN.uv_Splat2);

        //SM2.0超过4张贴图的UV会出BUG，所以用control来算出第四张贴图的UV
    	half4 lay4 = tex2D (_Splat3, IN.uv_Control*_Tiling3.xy);


        //纯色测试代码
        //lay1.rgb = fixed3(1,0,0);
        //lay2.rgb = fixed3(0,1,0);
        //lay3.rgb = fixed3(0,0,1);
        //lay4.rgb = fixed3(0,0,0);

        half4 blend = Blend(lay1.a,lay2.a,lay3.a,lay4.a,splat_control);
    	o.Alpha = 0.0;
        o.Albedo.rgb = blend.r * lay1 + blend.g * lay2 + blend.b * lay3 + blend.a * lay4;//混合


    }
    ENDCG 
    }
    FallBack "Diffuse"
}