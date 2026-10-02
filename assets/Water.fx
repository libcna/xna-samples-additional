float4x4 World,View,Projection;
float3 Albedo,Eye;
float Time;
texture ReflectionMap,RefractionMap;
sampler Reflections=sampler_state {Texture=<ReflectionMap>;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};
sampler Refractions=sampler_state {Texture=<RefractionMap>;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};
struct Input {float4 p:POSITION0;float3 n:NORMAL0;float2 uv:TEXCOORD0;};
struct Output {float4 p:POSITION0;float3 world:TEXCOORD0;float4 projected:TEXCOORD1;};
Output VS(Input i) {Output o;float4 w=mul(i.p,World);o.world=w.xyz;o.p=mul(mul(w,View),Projection);o.projected=o.p;return o;}
float4 PS(Output i):COLOR0 {
    float2 uv=i.projected.xy/i.projected.w*float2(.5,-.5)+.5;
    float2 waves=float2(sin(i.world.x*3+Time*1.8),cos(i.world.z*2.8-Time*1.3));
    float3 n=normalize(float3(waves.x*.12,1,waves.y*.12));
    float fresnel=.04+.96*pow(1-saturate(dot(n,normalize(Eye-i.world))),5);
    float3 reflected=tex2D(Reflections,uv+waves*.009).rgb;
    float3 refracted=tex2D(Refractions,uv-waves*.009).rgb*float3(.6,.82,.95);
    return float4(lerp(refracted,reflected,fresnel),1);
}
technique Water {pass P {VertexShader=compile vs_3_0 VS();PixelShader=compile ps_3_0 PS();}}
