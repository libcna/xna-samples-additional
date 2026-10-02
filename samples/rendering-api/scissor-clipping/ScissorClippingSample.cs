using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdditionalSamples;

[SampleInfo("scissor-clipping", "Scissor Clipping - Nested Panels")]
public sealed class ScissorClippingSample : Sample
{
    private RenderTarget2D target = null!;
    private readonly RasterizerState clipping = new() { CullMode = CullMode.None, ScissorTestEnable = true };
    public override void Load(Context c) { base.Load(c); target = Target(480, 280); }
    public override void Draw()
    {
        var oldRaster = G.RasterizerState; var oldClip = G.ScissorRectangle;
        G.SetRenderTarget(target); G.Clear(new Color(25, 34, 48));
        try
        {
            U.Begin(clipping);
            Rectangle outer = new(30, 25, 420, 230), child = new(70, 55, 330, 180);
            G.ScissorRectangle = outer;
            U.Box(new Rectangle(0, 0, 480, 280), new Color(49, 68, 94));
            G.ScissorRectangle = Rectangle.Intersect(outer, child);
            for (int i = 0; i < 12; i++)
            {
                int y = 55 + i * 40 - (Mode % 2 == 0 ? (int)(Time * 25) % 200 : 0);
                U.Box(new Rectangle(55, y, 370, 34), i % 2 == 0 ? new Color(35, 118, 170) : new Color(34, 85, 127));
                U.Text($"ITEM {i + 1}", 85, y + 9, Color.White);
            }
            U.End();
        }
        finally { G.RasterizerState = oldRaster; G.ScissorRectangle = oldClip; G.SetRenderTarget(C.Screen); }
        Panel(target, new(80, 90, 800, 460), "PARENT CLIP / CHILD INTERSECTION");
        Status = "OUTSIDE PIXELS PRESERVED - SPACE PAUSES SCROLLING";
    }
    public override void Verify()
    {
        var p = Pixels(target);
        Require(p[0] == new Color(25, 34, 48) && p[279 * 480 + 479] == p[0], "Scissor leaked beyond the parent clip.");
        Require(p[60 * 480 + 35] == new Color(49, 68, 94), "Nested clipping overwrote the parent panel.");
        Require(p[100 * 480 + 100] != p[0], "Clipped panel rendered no content.");
    }
    public override void Dispose() { target.Dispose(); clipping.Dispose(); }
}
