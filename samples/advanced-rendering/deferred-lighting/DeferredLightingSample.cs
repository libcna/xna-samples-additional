using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdditionalSamples;

[SampleInfo("deferred-lighting", "Deferred Lighting - G-Buffer and Point Lights")]
public sealed class DeferredLightingSample : Sample
{
    private GeometryBuffers buffers = null!;
    private Effect geometry = null!, lighting = null!;
    private RenderTarget2D target = null!;
    private readonly Vector4[] lights = new Vector4[32];
    public override void Load(Context c)
    {
        base.Load(c); buffers = new(G); geometry = Effect("Scene"); lighting = Effect("Post"); target = Target();
    }
    public override void Draw()
    {
        buffers.Render(C, geometry, Time); buffers.Bind(lighting);
        int count = Math.Clamp((int)(Parameter * 8), 1, 32);
        for (int i = 0; i < 32; i++)
        {
            float angle = i * MathHelper.TwoPi / count + Time * .3f;
            lights[i] = new Vector4(MathF.Sin(angle) * 4, 1.7f + MathF.Sin(i + Time), MathF.Cos(angle) * 4, 6);
        }
        lighting.Parameters["Lights"].SetValue(lights); lighting.Parameters["LightCount"].SetValue(count);
        G.SetRenderTarget(target); Fullscreen.Draw(G, lighting, "DeferredLighting"); G.SetRenderTarget(C.Screen);
        Panel(Mode % 3 == 1 ? buffers.Color : Mode % 3 == 2 ? buffers.Normal : target,
            new(80, 90, 800, 460), new[] { "DEFERRED POINT LIGHTING", "ALBEDO", "NORMALS" }[Mode % 3]);
        Status = $"LIGHTS {count}/32 - GEOMETRY PASS THEN SCREEN-SPACE LIGHTING";
    }
    public override void Verify()
    {
        buffers.Verify(); var first = Pixels(target);
        float old = Parameter; Parameter = .2f; Draw();
        Require(!first.SequenceEqual(Pixels(target)), "Changing light count had no effect on deferred lighting.");
        Parameter = old; Draw();
    }
    public override void Dispose() { buffers.Dispose(); geometry.Dispose(); lighting.Dispose(); target.Dispose(); }
}
