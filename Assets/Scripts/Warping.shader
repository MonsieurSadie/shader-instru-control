Shader "ImageEffects/Warping"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;


            // Created by inigo quilez - iq/2013
            // License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.

            // See here for a tutorial on how to make this:
            //
            // http://www.iquilezles.org/www/articles/warp/warp.htm

            //====================================================================

            const float2x2 m = float2x2( 0.80,  0.60, -0.60,  0.80 );

            float noise( in float2 p )
            {
              return sin(p.x)*sin(p.y);
            }

            float fbm4( float2 p )
            {
                float f = 0.0;
                f += 0.5000*noise( p ); p = mul(p,m)*2.02;
                f += 0.2500*noise( p ); p = mul(p,m)*2.03;
                f += 0.1250*noise( p ); p = mul(p,m)*2.01;
                f += 0.0625*noise( p );
                return f/0.9375;
            }

            float fbm6( float2 p )
            {
                float f = 0.0;
                f += 0.500000*(0.5+0.5*noise( p )); p = mul(p,m)*2.02;
                f += 0.250000*(0.5+0.5*noise( p )); p = mul(p,m)*2.03;
                f += 0.125000*(0.5+0.5*noise( p )); p = mul(p,m)*2.01;
                f += 0.062500*(0.5+0.5*noise( p )); p = mul(p,m)*2.04;
                f += 0.031250*(0.5+0.5*noise( p )); p = mul(p,m)*2.01;
                f += 0.015625*(0.5+0.5*noise( p ));
                return f/0.96875;
            }

            float2 fbm4_2( float2 p )
            {
                return float2(fbm4(p), fbm4(p+float2(7.8, 7.8)));
            }

            float2 fbm6_2( float2 p )
            {
                return float2(fbm6(p+float2(16.8, 16.8)), fbm6(p+float2(11.5, 11.5)));
            }

            //====================================================================

            float func( float2 q, out float4 ron )
            {
              q += 0.03*sin( float2(0.27,0.23)*_Time.y + length(q)*float2(4.1,4.3));

              float2 o = fbm4_2( 0.9*q );

              o += 0.04*sin( float2(0.12,0.14)*_Time.y + length(o));

              float2 n = fbm6_2( 3.0*o );

              ron = float4( o, n );

              float f = 0.5 + 0.5*fbm4( 1.8*q + 6.0*n );

              return lerp( f, f*f*f*3.5, f*abs(n.x) );
            }

            // void mainImage( out float4 fragColor, in float2 fragCoord )
            fixed4 frag (v2f i) : SV_Target
            {
              // float2 p = (2.0*fragCoord-_ScreenParams.xy)/_ScreenParams.y;
              // float e = 2.0/_ScreenParams.y;
              float2 p = (2.0*i.uv);
              float e = 2.0;

              float4 on = float4(0,0,0,0);
              float f = func(p, on);

              float3 col = float3(0,0,0);
              col = lerp( float3(0.2,0.1,0.4), float3(0.3,0.05,0.05), f );
              col = lerp( col, float3(0.9,0.9,0.9), dot(on.zw,on.zw) );
              col = lerp( col, float3(0.4,0.3,0.3), 0.2 + 0.5*on.y*on.y );
              col = lerp( col, float3(0.0,0.2,0.4), 0.5*smoothstep(1.2,1.3,abs(on.z)+abs(on.w)) );
              col = clamp( col*f*2.0, 0.0, 1.0 );
                
            #if 0
                // gpu derivatives - bad quality, but fast
              float3 nor = normalize( float3( ddx(f)*_ScreenParams.x, 6.0, ddy(f)*_ScreenParams.y ) );
            #else    
                // manual derivatives - better quality, but slower
              float4 kk;
              float3 nor = normalize( float3( func(p+float2(e,0.0),kk)-f, 
                                            2.0*e,
                                            func(p+float2(0.0,e),kk)-f ) );
            #endif

              float3 lig = normalize( float3( 0.9, 0.2, -0.4 ) );
              float dif = clamp( 0.3+0.7*dot( nor, lig ), 0.0, 1.0 );
              float3 lin = float3(0.70,0.90,0.95)*(nor.y*0.5+0.5) + float3(0.15,0.10,0.05)*dif;
              col *= 1.2*lin;
              col = 1.0 - col;
              col = 1.1*col*col;
              
              return float4( col.xyz, 1.0 );
            }

            ENDCG
        }
    }
}
