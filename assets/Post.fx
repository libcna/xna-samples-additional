texture ColorMap, NormalMap, DepthMap, OtherMap;
sampler Colors=sampler_state {Texture=<ColorMap>;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};
sampler Normals=sampler_state {Texture=<NormalMap>;MinFilter=POINT;MagFilter=POINT;AddressU=CLAMP;AddressV=CLAMP;};
sampler Depths=sampler_state {Texture=<DepthMap>;MinFilter=POINT;MagFilter=POINT;AddressU=CLAMP;AddressV=CLAMP;};
sampler Other=sampler_state {Texture=<OtherMap>;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;};
float Time=0, Exposure=1, Radius=1, Mode=0;
float2 PixelSize=float2(1.0/480,1.0/280);
float4x4 InverseProjection, InverseView;
float3 Eye;
float4 Lights[32];
int LightCount=8;
struct Input {float4 p:POSITION0;float2 uv:TEXCOORD0;};
struct Output {float4 p:POSITION0;float2 uv:TEXCOORD0;};
Output VS(Input i) {Output o;o.p=i.p;o.uv=i.uv;return o;}
float3 WorldPosition(float2 uv,float depth) {
    float4 ray=mul(float4(uv*float2(2,-2)+float2(-1,1),1,1),InverseProjection);
    ray.xyz/=ray.w;
    float3 view=ray.xyz*(-depth*40/ray.z);
    return mul(float4(view,1),InverseView).xyz;
}
float4 Copy(Output i):COLOR0 {return tex2D(Colors,i.uv);}
float4 DepthView(Output i):COLOR0 {float d=tex2D(Depths,i.uv).r;return float4(d,d,d,1);}
float4 Deferred(Output i):COLOR0 {
    float d=tex2D(Depths,i.uv).r;
    if(d<=0) return float4(.04,.055,.085,1);
    float3 n=normalize(tex2D(Normals,i.uv).xyz*2-1), p=WorldPosition(i.uv,d);
    float3 light=.12;
    for(int j=0;j<32;j++) {
        if(j>=LightCount) break;
        float3 delta=Lights[j].xyz-p;
        float atten=saturate(1-length(delta)/Lights[j].w);
        float3 color=float3(.5+.5*sin(j*2),.5+.5*sin(j*2+2),.5+.5*sin(j*2+4));
        light+=color*atten*atten*max(dot(n,normalize(delta)),0)*3;
    }
    return float4(tex2D(Colors,i.uv).rgb*light,1);
}
float4 AO(Output i):COLOR0 {
    float d=tex2D(Depths,i.uv).r;
    if(d<=0) return 1;
    float3 p=WorldPosition(i.uv,d),n=normalize(tex2D(Normals,i.uv).xyz*2-1);
    float occlusion=0;
    for(int j=0;j<16;j++) {
        float a=j*2.39996, r=Radius*(3+j)*.8;
        float2 uv=i.uv+float2(cos(a),sin(a))*PixelSize*r;
        float sd=tex2D(Depths,uv).r;
        float3 v=WorldPosition(uv,sd)-p;
        float dist=length(v);
        if(sd>0&&dist>.02) occlusion+=saturate(dot(n,normalize(v))-.1)*saturate(1-dist/(Radius*2));
    }
    float ao=saturate(1-occlusion*.22);return float4(ao,ao,ao,1);
}
float4 BlurAO(Output i):COLOR0 {
    float d=tex2D(Depths,i.uv).r, total=0, weights=0;
    for(int y=-2;y<=2;y++) for(int x=-2;x<=2;x++) {
        float2 uv=i.uv+float2(x,y)*PixelSize;
        float w=exp(-abs(tex2D(Depths,uv).r-d)*800)*exp(-(x*x+y*y)/4.0);
        total+=tex2D(Other,uv).r*w;weights+=w;
    }
    float ao=total/max(weights,.001);
    if(Mode>0) return float4(ao,ao,ao,1);
    float3 n=normalize(tex2D(Normals,i.uv).xyz*2-1);
    float light=.3+.7*saturate(dot(n,normalize(float3(-.4,1,.3))));
    return float4(tex2D(Colors,i.uv).rgb*light*ao,1);
}
float4 HDRScene(Output i):COLOR0 {
    float3 base=float3(.04,.07,.12);
    for(int j=0;j<5;j++) {
        float2 p=float2(.18+j*.16,.48+sin(j+Time)*.1);
        float dist=length(i.uv-p);
        float bright=(j+1)*4/(1+dist*dist*500);
        base+=bright*float3(.6+.4*sin(j),.6+.4*cos(j),.7);
    }
    return float4(base,1);
}
float4 ToneMap(Output i):COLOR0 {
    float3 value=tex2D(Colors,i.uv).rgb*Exposure;
    if(Mode<.5) value=value/(1+value);
    else if(Mode<1.5) value=saturate((value*(2.51*value+.03))/(value*(2.43*value+.59)+.14));
    else value=saturate(value);
    return float4(pow(value,1/2.2),1);
}
float4 Water(Output i):COLOR0 {
    float2 ripple=float2(sin(i.uv.y*70+Time*2),cos(i.uv.x*65-Time*1.4))*.012;
    float3 reflection=tex2D(Colors,float2(i.uv.x,1-i.uv.y)+ripple).rgb;
    float3 refraction=tex2D(Other,i.uv-ripple).rgb*float3(.6,.85,1);
    float fresnel=.08+.7*pow(1-i.uv.y,3);
    return float4(lerp(refraction,reflection,fresnel),1);
}
technique CopyImage {pass P {VertexShader=compile vs_3_0 VS();PixelShader=compile ps_3_0 Copy();}}
technique ShowDepth {pass P {VertexShader=compile vs_3_0 VS();PixelShader=compile ps_3_0 DepthView();}}
technique DeferredLighting {pass P {VertexShader=compile vs_3_0 VS();PixelShader=compile ps_3_0 Deferred();}}
technique AmbientOcclusion {pass P {VertexShader=compile vs_3_0 VS();PixelShader=compile ps_3_0 AO();}}
technique BlurOcclusion {pass P {VertexShader=compile vs_3_0 VS();PixelShader=compile ps_3_0 BlurAO();}}
technique Radiance {pass P {VertexShader=compile vs_3_0 VS();PixelShader=compile ps_3_0 HDRScene();}}
technique ToneMapping {pass P {VertexShader=compile vs_3_0 VS();PixelShader=compile ps_3_0 ToneMap();}}
technique WaterSurface {pass P {VertexShader=compile vs_3_0 VS();PixelShader=compile ps_3_0 Water();}}
