Shader "Custom/WorldPositionTextureOffset"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _TextureScale("Texture Scale", Float) = 1.0
    }

        SubShader
        {
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float _TextureScale;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    
                    // Offset UVs by the translation and ensure they wrap by using fmod
                    o.uv = (mul(unity_ObjectToWorld, v.vertex).xy * _TextureScale); // Adjust the 0.1 scaling factor as needed
                
                    return o;
                }

                half4 frag(v2f i) : SV_Target
                {
                    return tex2D(_MainTex, i.uv);
                }
                ENDCG
            }
        }
}
