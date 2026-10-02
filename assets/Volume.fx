texture Volume;
sampler Density=sampler_state {Texture=<Volume>;MinFilter=LINEAR;MagFilter=LINEAR;AddressU=CLAMP;AddressV=CLAMP;AddressW=CLAMP;};
float Slice=.5, Mode=0, Angle=0;
struct Input {float4 p:POSITION0;float2 uv:TEXCOORD0;};
struct Output {float4 p:POSITION0;float2 uv:TEXCOORD0;};
Output VS(Input i) {Output o;o.p=i.p;o.uv=i.uv;return o;}
float4 PS(Output i):COLOR0 {
    if(Mode<.5) return tex3D(Density,float3(i.uv,Slice));
    float4 sum=0;
    for(int j=0;j<48;j++) {
        float3 p=float3(i.uv,(j+.5)/48.0)-.5;
        p.xz=float2(p.x*cos(Angle)-p.z*sin(Angle),p.x*sin(Angle)+p.z*cos(Angle));
        float4 d=tex3D(Density,p+.5);float a=d.a*.08;
        sum.rgb+=(1-sum.a)*a*d.rgb;sum.a+=(1-sum.a)*a;
    }
    return float4(sum.rgb,1);
}
technique VolumeView {pass P {VertexShader=compile vs_3_0 VS();PixelShader=compile ps_3_0 PS();}}
