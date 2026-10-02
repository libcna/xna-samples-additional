using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdditionalSamples;

[SampleInfo("water-rendering", "Water Rendering - Reflection, Refraction and Fresnel")]
public sealed class WaterRenderingSample : Sample
{
    private Effect sceneShader = null!, water = null!;
    private RenderTarget2D reflection = null!, refraction = null!, result = null!;
    private readonly VertexPositionNormalTexture[] plane = {
        new(new(-6,0,-6),Vector3.Up,new(0,0)),new(new(6,0,-6),Vector3.Up,new(1,0)),new(new(6,0,6),Vector3.Up,new(1,1)),
        new(new(-6,0,-6),Vector3.Up,new(0,0)),new(new(6,0,6),Vector3.Up,new(1,1)),new(new(-6,0,6),Vector3.Up,new(0,1)) };
    public override void Load(Context c)
    {
        base.Load(c); sceneShader = Effect("Scene"); water = Effect("Water");
        reflection = Target(depth: DepthFormat.Depth24); refraction = Target(depth: DepthFormat.Depth24); result = Target(depth: DepthFormat.Depth24);
    }
    private void ScenePass(Matrix view, Matrix projection)
    {
        Scene.DrawScene(view, projection, Time, sceneShader);
        Scene.DrawBox(Matrix.CreateScale(4, .2f, 4) * Matrix.CreateTranslation(0, -2, 0), Color.DarkCyan, view, projection, sceneShader);
        Scene.DrawBox(Matrix.CreateScale(1, 1, 1) * Matrix.CreateTranslation(1, -1, 1), Color.Gold, view, projection, sceneShader);
    }
    public override void Draw()
    {
        Vector3 eye = new(8, 6, 9), mirrored = new(eye.X, -eye.Y, eye.Z);
        Matrix view = Matrix.CreateLookAt(eye, Vector3.Zero, Vector3.Up), projection = SceneRenderer.Projection(480f / 280);
        sceneShader.CurrentTechnique = sceneShader.Techniques["Lighting"];
        sceneShader.Parameters["ClipEnabled"].SetValue(true); sceneShader.Parameters["ClipPlane"].SetValue(new Vector4(0, 1, 0, 0));
        G.SetRenderTarget(reflection); G.Clear(new Color(49, 87, 133)); G.RasterizerState = RasterizerState.CullNone;
        ScenePass(Matrix.CreateLookAt(mirrored, Vector3.Zero, Vector3.Down), projection);
        sceneShader.Parameters["ClipPlane"].SetValue(new Vector4(0, -1, 0, 0));
        G.SetRenderTarget(refraction); G.Clear(new Color(9, 48, 60)); ScenePass(view, projection);
        sceneShader.Parameters["ClipEnabled"].SetValue(false);
        G.SetRenderTarget(result); G.Clear(new Color(49, 87, 133)); ScenePass(view, projection);
        water.Parameters["ReflectionMap"].SetValue(reflection); water.Parameters["RefractionMap"].SetValue(refraction);
        water.Parameters["Eye"].SetValue(eye); water.Parameters["Time"].SetValue(Time * Parameter);
        Scene.DrawMesh(plane, Matrix.Identity, Color.White, view, projection, water);
        G.SetRenderTarget(C.Screen); Panel(Mode % 3 == 1 ? reflection : Mode % 3 == 2 ? refraction : result,
            new(80, 90, 800, 460), new[] { "WATER SURFACE", "ABOVE-WATER REFLECTION", "BELOW-WATER REFRACTION" }[Mode % 3]);
        Status = "TWO CLIPPED SCENE PASSES / PROJECTED SAMPLING / ANIMATED NORMALS";
    }
    public override void Verify()
    {
        Require(!Pixels(reflection).SequenceEqual(Pixels(refraction)), "Reflection and refraction are not independent.");
        Require(Pixels(result).Distinct().Count() > 60, "Water scene did not render.");
    }
    public override void Dispose() { sceneShader.Dispose(); water.Dispose(); reflection.Dispose(); refraction.Dispose(); result.Dispose(); }
}
