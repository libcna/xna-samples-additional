using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdditionalSamples;

public static class Fullscreen
{
    private static readonly VertexPositionTexture[] Quad = {
        new(new(-1,1,0),new(0,0)),new(new(1,1,0),new(1,0)),new(new(1,-1,0),new(1,1)),
        new(new(-1,1,0),new(0,0)),new(new(1,-1,0),new(1,1)),new(new(-1,-1,0),new(0,1)) };
    public static void Draw(GraphicsDevice device, Effect effect, string technique)
    {
        device.BlendState = BlendState.Opaque; device.DepthStencilState = DepthStencilState.None;
        device.RasterizerState = RasterizerState.CullNone;
        effect.CurrentTechnique = effect.Techniques[technique];
        foreach (var pass in effect.CurrentTechnique.Passes)
        { pass.Apply(); device.DrawUserPrimitives(PrimitiveType.TriangleList, Quad, 0, 2); }
    }
}

public sealed class GeometryBuffers : IDisposable
{
    public RenderTarget2D Color { get; }
    public RenderTarget2D Normal { get; }
    public RenderTarget2D Depth { get; }
    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }
    public GeometryBuffers(GraphicsDevice device)
    {
        Color = new(device, 480, 280, false, SurfaceFormat.Color, DepthFormat.Depth24);
        Normal = new(device, 480, 280, false, SurfaceFormat.Color, DepthFormat.None);
        Depth = new(device, 480, 280, false, SurfaceFormat.Single, DepthFormat.None);
    }
    public void Render(Context c, Effect effect, float time)
    {
        View = SceneRenderer.View(.6f); Projection = SceneRenderer.Projection(480f / 280);
        c.Device.SetRenderTargets(Color, Normal, Depth);
        c.Device.Clear(Microsoft.Xna.Framework.Color.Black);
        c.Device.RasterizerState = RasterizerState.CullNone;
        effect.CurrentTechnique = effect.Techniques["GBuffer"];
        c.Scene.DrawScene(View, Projection, time, effect);
        c.Device.SetRenderTarget(c.Screen);
    }
    public void Bind(Effect effect)
    {
        effect.Parameters["ColorMap"].SetValue(Color); effect.Parameters["NormalMap"].SetValue(Normal);
        effect.Parameters["DepthMap"].SetValue(Depth);
        effect.Parameters["InverseProjection"].SetValue(Matrix.Invert(Projection));
        effect.Parameters["InverseView"].SetValue(Matrix.Invert(View));
    }
    public void Verify()
    {
        var color = new Color[480 * 280]; var normal = new Color[color.Length]; var depth = new float[color.Length];
        Color.GetData(color); Normal.GetData(normal); Depth.GetData(depth);
        if (!depth.Any(d => d > 0 && d < 1) || !normal.Any(n => n.B > 0) || color.SequenceEqual(normal))
            throw new InvalidOperationException("MRT did not produce independent color, normal and depth outputs.");
    }
    public void Dispose() { Color.Dispose(); Normal.Dispose(); Depth.Dispose(); }
}
