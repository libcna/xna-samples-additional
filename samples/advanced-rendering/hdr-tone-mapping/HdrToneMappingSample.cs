using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace AdditionalSamples;

[SampleInfo("hdr-tone-mapping", "HDR Tone Mapping - Exposure and Adaptation")]
public sealed class HdrToneMappingSample : Sample
{
    private Effect shader = null!;
    private RenderTarget2D hdr = null!, output = null!;
    private float exposure = 1, targetExposure = 1;
    private int frames;
    public override void Load(Context c) { base.Load(c); shader = Effect("Post"); hdr = Target(format: SurfaceFormat.HalfVector4); output = Target(); }
    public override void Update(float dt)
    {
        base.Update(dt); exposure = MathHelper.Lerp(exposure, targetExposure, 1 - MathF.Exp(-dt * 2));
    }
    public override void Draw()
    {
        shader.Parameters["Time"].SetValue(Time); G.SetRenderTarget(hdr); Fullscreen.Draw(G, shader, "Radiance"); G.SetRenderTarget(C.Screen);
        if (++frames % 15 == 0)
        {
            // Educational readback; a production renderer would reduce luminance on the GPU.
            var pixels = new HalfVector4[hdr.Width * hdr.Height]; hdr.GetData(pixels);
            double logLuminance = pixels.Average(p => { var v = p.ToVector4(); return Math.Log(.0001 + v.X * .2126 + v.Y * .7152 + v.Z * .0722); });
            targetExposure = .5f / (float)Math.Exp(logLuminance);
        }
        shader.Parameters["ColorMap"].SetValue(hdr);
        shader.Parameters["Exposure"].SetValue(Mode % 4 == 3 ? exposure : Parameter);
        shader.Parameters["Mode"].SetValue((float)(Mode % 4 == 3 ? 0 : Mode % 4));
        G.SetRenderTarget(output); Fullscreen.Draw(G, shader, "ToneMapping"); G.SetRenderTarget(C.Screen);
        Panel(output, new(80, 90, 800, 460), new[] { "REINHARD", "FILMIC CURVE", "LDR CLIPPING REFERENCE", "AUTOMATIC EXPOSURE" }[Mode % 4]);
        Status = $"EXPOSURE {(Mode % 4 == 3 ? exposure : Parameter):F2} - FLOATING-POINT RADIANCE RETAINS VALUES ABOVE 1";
    }
    public override void Verify()
    {
        var values = new HalfVector4[hdr.Width * hdr.Height]; hdr.GetData(values);
        Require(values.Any(v => v.ToVector4().X > 1), "HDR target lost values above 1.");
        var old = Parameter; Parameter = .2f; Draw(); var dim = Pixels(output); Parameter = 2; Draw();
        Require(!dim.SequenceEqual(Pixels(output)), "Exposure had no effect."); Parameter = old; Draw();
    }
    public override void Dispose() { shader.Dispose(); hdr.Dispose(); output.Dispose(); }
}
