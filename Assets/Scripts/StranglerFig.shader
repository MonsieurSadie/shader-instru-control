Shader "ImageEffects/StranglerFig"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _iChannel0 ("iChannel0", 2D) = "white" {}
        _iChannel1 ("iChannel1", 2D) = "white" {}
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
            sampler2D _iChannel0;
            sampler2D _iChannel1;

            // fixed4 frag (v2f i) : SV_Target
            // {
            //     fixed4 col = tex2D(_MainTex, i.uv);
            //     // just invert the colors
            //     col.rgb = 1 - col.rgb;
            //     return col;
            // }

            //###############################################
            // Created by inigo quilez - 2020
            // License Creative Commons Attribution-NonCommercial-ShareAlike 3.0

            // Other "Iterations" shaders:
            //
            // "trigonometric"   : https://www.shadertoy.com/view/Mdl3RH
            // "trigonometric 2" : https://www.shadertoy.com/view/Wss3zB
            // "circles"         : https://www.shadertoy.com/view/MdVGWR
            // "coral"           : https://www.shadertoy.com/view/4sXGDN
            // "guts"            : https://www.shadertoy.com/view/MssGW4
            // "inversion"       : https://www.shadertoy.com/view/XdXGDS
            // "inversion 2"     : https://www.shadertoy.com/view/4t3SzN
            // "shiny"           : https://www.shadertoy.com/view/MslXz8
            // "worms"           : https://www.shadertoy.com/view/ldl3W4
            // "stripes"         : https://www.shadertoy.com/view/wlsfRn

            #define AA 2
            #define SCREEN_WIDTH 640
            #define SCREEN_HEIGHT 480
            float2 iResolution = float2(SCREEN_WIDTH, SCREEN_HEIGHT);

            float hash( in float n )
            {
                return frac(sin(n)*43758.5453);
            }
            float noise( in float2 p )
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f*f*(3.0-2.0*f);
                float n = i.x + i.y*57.0;
                return lerp(lerp( hash(n+ 0.0), hash(n+ 1.0),f.x),
                          lerp( hash(n+57.0), hash(n+58.0),f.x),f.y);
            }

            float2 map( in float2 p, in float time )
            {
                for( int i=0; i<4; i++ )
                {
                  float a = noise(p*1.5)*6.2831 + time;
                p += 0.1*float2( cos(a), sin(a) );
                }
                return p;
            }

            float height( in float2 p, in float2 q )
            {
                float h = dot(p-q,p-q);
                h += 0.005*tex2D(_iChannel0,0.75*(p+q)).x;
                return h;
            }

            // void mainImage( out float4 fragColor, in float2 fragCoord )
            fixed4 frag (v2f i) : SV_Target
            {
                float time = 0.25*_Time.y;
                
                float3 tot = float3(0,0,0);
              #if AA>1
                for( int m=0; m<AA; m++ )
                for( int n=0; n<AA; n++ )
                {
                    float2 o = float2(float(m),float(n)) / float(AA) - 0.5;
                    float2 p = 2.0*(i.uv+o);//(2.0*(i.uv+o)-iResolution.xy)/iResolution.y;
                #else    
                    float2 p = 2.0*(i.uv); //(2.0*i.uv-iResolution.xy)/iResolution.y;
                #endif

                    // deformation
                    float2 q = map(p,time);

                    // color
                    float w = 10.0*q.x;
                    float u = floor(w);
                    float f = frac(w);
                    float3  col = float3(0.7,0.55,0.5) + 0.3*sin(3.0*u+float3(0.0,1.5,2.0));
                    
                    // filtered drop-shadow
                    float sha = smoothstep(0.0,0.5,f)-smoothstep(1.0-fwidth(w),1.0,f);
                    
                    // normal
                    float2  eps = float2(2.0/SCREEN_WIDTH,0.0);
                float l2c = height(q,p);
                    float l2x = height(map(p+eps.xy,time),p) - l2c;
                    float l2y = height(map(p+eps.yx,time),p) - l2c;
                    float3  nor = normalize( float3( l2x, eps.x, l2y ) );
                        
                    // lighting
                    col *= 0.3+0.7*sha;
                    col *= 0.8+0.2*float3(1.0,0.9,0.3)*dot(nor,float3(0.7,0.3,0.7));
                    col += 0.3*pow(nor.y,8.0)*sha;
                    col *= 7.5*l2c;

                    tot += col;
              #if AA>1
                }
                tot /= float(AA*AA);
              #endif

              return fixed4( tot, 1.0 );
            }


            ENDCG
        }
    }
}
