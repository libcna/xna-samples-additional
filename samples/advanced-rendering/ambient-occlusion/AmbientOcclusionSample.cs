using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdditionalSamples;

[SampleInfo("ambient-occlusion", "Ambient Occlusion - Depth Sampling and Bilateral Blur")]
public sealed class AmbientOcclusionSample : Sample
{
    private GeometryBuffers buffers = null!;
    private Effect geometry = null!, post = null!;
    private RenderTarget2D raw = null!, result = null!;
    public override void Load(Context c)
    {
        base.Load(c); buffers = new(G); geometry = Effect("Scene"); post = Effect("Post"); raw = Target(); result = Target();
    }
    public override void Draw()
    {
        buffers.Render(C, geometry, Time); buffers.Bind(post); post.Parameters["Radius"].SetValue(Parameter);
        G.SetRenderTarget(raw); Fullscreen.Draw(G, post, "AmbientOcclusion"); G.SetRenderTarget(C.Screen);
        post.Parameters["OtherMap"].SetValue(raw); post.Parameters["Mode"].SetValue(Mode % 3 == 1 ? 1f : 0f);
        G.SetRenderTarget(result); Fullscreen.Draw(G, post, "BlurOcclusion"); G.SetRenderTarget(C.Screen);
        Panel(Mode % 3 == 2 ? raw : result, new(80, 90, 800, 460), new[] { "COMPOSITED OCCLUSION", "BILATERAL AO", "RAW AO" }[Mode % 3]);
        Status = $"RADIUS {Parameter:F1} - 16 DEPTH SAMPLES / EDGE-AWARE 5X5 FILTER";
    }
    public override void Verify()
    {
        buffers.Verify(); var image = Pixels(raw);
        Require(image.Any(p => p.R < 250) && image.Any(p => p.R == 255), "AO did not distinguish occluded and open regions.");
        Require(Pixels(result).Distinct().Count() > 30, "Occlusion composition is uniform.");
    }
    public override void Dispose() { buffers.Dispose(); geometry.Dispose(); post.Dispose(); raw.Dispose(); result.Dispose(); }
}
