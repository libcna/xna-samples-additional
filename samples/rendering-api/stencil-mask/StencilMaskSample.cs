using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdditionalSamples;

[SampleInfo("stencil-mask", "Stencil Mask - Window and Outline")]
public sealed class StencilMaskSample : Sample
{
    private RenderTarget2D target = null!;
    private BasicEffect effect = null!;
    private readonly DepthStencilState write = new() { DepthBufferEnable = false, StencilEnable = true, StencilFunction = CompareFunction.Always, StencilPass = StencilOperation.Replace, ReferenceStencil = 1 };
    private readonly DepthStencilState inside = new() { DepthBufferEnable = false, StencilEnable = true, StencilFunction = CompareFunction.Equal, StencilPass = StencilOperation.Keep, ReferenceStencil = 1 };
    private readonly DepthStencilState outside = new() { DepthBufferEnable = false, StencilEnable = true, StencilFunction = CompareFunction.NotEqual, StencilPass = StencilOperation.Keep, ReferenceStencil = 1 };
    private readonly BlendState noColor = new() { ColorWriteChannels = ColorWriteChannels.None };
    public override void Load(Context c)
    {
        base.Load(c); target = Target(320, 280, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        effect = new(G) { VertexColorEnabled = true, Projection = Matrix.CreateOrthographicOffCenter(0, 320, 280, 0, 0, 1) };
    }
    private void Circle(float radius, Color color)
    {
        var v = new VertexPositionColor[96 * 3];
        for (int i = 0; i < 96; i++)
        {
            float a = i * MathHelper.TwoPi / 96, b = (i + 1) * MathHelper.TwoPi / 96;
            v[i * 3] = new(new(160, 140, 0), color); v[i * 3 + 1] = new(new(160 + MathF.Cos(a) * radius, 140 + MathF.Sin(a) * radius, 0), color);
            v[i * 3 + 2] = new(new(160 + MathF.Cos(b) * radius, 140 + MathF.Sin(b) * radius, 0), color);
        }
        effect.CurrentTechnique.Passes[0].Apply(); G.DrawUserPrimitives(PrimitiveType.TriangleList, v, 0, 96);
    }
    public override void Draw()
    {
        G.SetRenderTarget(target); G.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, new Color(24, 34, 48), 1, 0);
        G.RasterizerState = RasterizerState.CullNone; G.DepthStencilState = write; G.BlendState = noColor; Circle(88, Color.White);
        if (Mode % 3 == 2)
        {
            G.DepthStencilState = outside; G.BlendState = BlendState.Opaque; Circle(102, Color.Cyan);
            G.DepthStencilState = inside; Circle(88, new Color(55, 88, 120));
        }
        else
        {
            U.Begin(depth: Mode % 3 == 0 ? inside : outside);
            for (int i = 0; i < 8; i++) U.Box(new Rectangle(i * 40, 0, 40, 280), i % 2 == 0 ? Color.Coral : Color.CornflowerBlue);
            U.End();
        }
        G.SetRenderTarget(C.Screen); G.DepthStencilState = DepthStencilState.None; G.BlendState = BlendState.Opaque;
        Panel(target, new(215, 80, 530, 464), Mode % 3 == 2 ? "STENCIL OUTLINE" : Mode % 3 == 0 ? "EQUAL MASK" : "INVERTED MASK");
        Status = "COLOR WRITES OFF IN MASK PASS - DEPTH24STENCIL8";
    }
    public override void Verify()
    {
        int old = Mode; Mode = 0; Draw(); var p = Pixels(target);
        Require(p[140 * 320 + 160] != new Color(24, 34, 48) && p[0] == new Color(24, 34, 48), "Stencil mask failed inside/outside test.");
        Mode = 1; Draw(); p = Pixels(target); Require(p[0] != new Color(24, 34, 48) && p[140 * 320 + 160] == new Color(24, 34, 48), "Inverted stencil test failed.");
        Mode = old; Draw();
    }
    public override void Dispose() { target.Dispose(); effect.Dispose(); write.Dispose(); inside.Dispose(); outside.Dispose(); noColor.Dispose(); }
}
