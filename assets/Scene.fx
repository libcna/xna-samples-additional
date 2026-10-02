float4x4 World, View, Projection;
float3 Albedo;
float4 ClipPlane = float4(0,1,0,0);
float ClipEnabled = 0;
float4x4 LightVP0, LightVP1, LightVP2;
float2 CascadeSplits = float2(8,16);
float ShadowBias = 0.003;
float ShowCascades = 0;
texture Shadow0, Shadow1, Shadow2;
sampler S0 = sampler_state { Texture = <Shadow0>; MinFilter=POINT; MagFilter=POINT; AddressU=CLAMP; AddressV=CLAMP; };
sampler S1 = sampler_state { Texture = <Shadow1>; MinFilter=POINT; MagFilter=POINT; AddressU=CLAMP; AddressV=CLAMP; };
sampler S2 = sampler_state { Texture = <Shadow2>; MinFilter=POINT; MagFilter=POINT; AddressU=CLAMP; AddressV=CLAMP; };
struct Input { float4 p:POSITION0; float3 n:NORMAL0; float2 uv:TEXCOORD0; };
struct Output { float4 p:POSITION0; float3 world:TEXCOORD0; float3 n:TEXCOORD1; float depth:TEXCOORD2; float4 light0:TEXCOORD3; float4 light1:TEXCOORD4; float4 light2:TEXCOORD5; };
Output VS(Input input) {
    Output o;
    float4 w=mul(input.p,World), v=mul(w,View);
    o.p=mul(v,Projection); o.world=w.xyz;
    o.n=normalize(mul(input.n,(float3x3)World)); o.depth=-v.z;
    o.light0=mul(w,LightVP0); o.light1=mul(w,LightVP1); o.light2=mul(w,LightVP2);
    return o;
}
struct Buffers { float4 color:COLOR0; float4 normal:COLOR1; float4 depth:COLOR2; };
Buffers Geometry(Output i) {
    Buffers o; o.color=float4(Albedo,1); o.normal=float4(normalize(i.n)*.5+.5,1);
    o.depth=float4(i.depth/40,0,0,1); return o;
}
float4 Lit(Output i):COLOR0 {
    clip(ClipEnabled ? dot(float4(i.world,1),ClipPlane) : 1);
    float light=.22+.78*saturate(dot(normalize(i.n),normalize(float3(-.4,1,.3))));
    return float4(Albedo*light,1);
}
float4 Depth(Output i):COLOR0 { return float4(i.light0.z/i.light0.w,0,0,1); }
float CompareShadow(sampler s,float4 p) {
    float3 q=p.xyz/p.w; float2 uv=q.xy*float2(.5,-.5)+.5;
    if(any(uv<0)||any(uv>1)||q.z<0||q.z>1) return 1;
    float sum=0;
    for(int y=-1;y<=1;y++) for(int x=-1;x<=1;x++)
        sum+=(q.z-ShadowBias<=tex2D(s,uv+float2(x,y)/512).r)?1:0;
    return sum/9;
}
float4 Shadowed(Output i):COLOR0 {
    int c=i.depth<CascadeSplits.x?0:(i.depth<CascadeSplits.y?1:2);
    float shadow=c==0?CompareShadow(S0,i.light0):(c==1?CompareShadow(S1,i.light1):CompareShadow(S2,i.light2));
    float3 tint=ShowCascades?(c==0?float3(1,.3,.3):(c==1?float3(.3,1,.3):float3(.3,.4,1))):float3(1,1,1);
    float l=.2+.8*saturate(dot(normalize(i.n),normalize(float3(-.4,1,.3))))*shadow;
    return float4(Albedo*l*tint,1);
}
technique GBuffer { pass P { VertexShader=compile vs_3_0 VS(); PixelShader=compile ps_3_0 Geometry(); } }
technique Lighting { pass P { VertexShader=compile vs_3_0 VS(); PixelShader=compile ps_3_0 Lit(); } }
technique ShadowDepth { pass P { VertexShader=compile vs_3_0 VS(); PixelShader=compile ps_3_0 Depth(); } }
technique Cascades { pass P { VertexShader=compile vs_3_0 VS(); PixelShader=compile ps_3_0 Shadowed(); } }
