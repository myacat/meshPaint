Shader "Unlit/Parallax"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HighMap ("Normal Map", 2D) = "white" {}
        _Scale("Scale" , Range(-0.3,0.3)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 worldPos: TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _HighMap;
            half _Scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = v.vertex;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 ViewDir = normalize( ObjSpaceViewDir(i.worldPos));
                float h = normalize( UnpackNormal(tex2D( _HighMap ,i.uv ))).z;
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv - float2( ViewDir.x , ViewDir.y ) * ( h * _Scale ));
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
