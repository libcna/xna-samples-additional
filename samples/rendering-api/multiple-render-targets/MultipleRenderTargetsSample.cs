using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdditionalSamples;

[SampleInfo("multiple-render-targets", "Multiple Render Targets - One Geometry Pass")]
public sealed class MultipleRenderTargetsSample : Sample
{
    private GeometryBuffers buffers = null!; private Effect scene = null!, post = null!; private RenderTarget2D depthView = null!;
    public override void Load(Context c) { base.Load(c); buffers = new(G); scene = Effect("Scene"); post = Effect("Post"); depthView = Target(); }
    public override void Draw()
    {
        buffers.Render(C, scene, Time); buffers.Bind(post); G.SetRenderTarget(depthView); Fullscreen.Draw(G, post, "ShowDepth"); G.SetRenderTarget(C.Screen);
        Panel(buffers.Color, new(20, 100, 450, 260), "ALBEDO - COLOR0"); Panel(buffers.Normal, new(490, 100, 450, 260), "NORMAL - COLOR1");
        Panel(depthView, new(255, 375, 450, 190), "LINEAR DEPTH - COLOR2"); Status = "THREE ATTACHMENTS PRODUCED BY EACH GEOMETRY DRAW";
    }
    public override void Verify() => buffers.Verify();
    public override void Dispose() { buffers.Dispose(); scene.Dispose(); post.Dispose(); depthView.Dispose(); }
}
