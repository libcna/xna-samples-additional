using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdditionalSamples;

[SampleInfo("cascaded-shadow-maps", "Cascaded Shadow Maps - Three Camera Depth Ranges")]
public sealed class CascadedShadowMapsSample : Sample
{
    private Effect effect = null!;
    private readonly RenderTarget2D[] shadows = new RenderTarget2D[3];
    private readonly Matrix[] lightMatrices = new Matrix[3];
    private RenderTarget2D target = null!;
    public override void Load(Context c)
    {
        base.Load(c); effect = Effect("Scene"); target = Target(depth: DepthFormat.Depth24);
        for (int i = 0; i < 3; i++) shadows[i] = Target(512, 512, SurfaceFormat.Single, DepthFormat.Depth24);
    }
    public override void Draw()
    {
        Matrix view = SceneRenderer.View(.6f + MathF.Sin(Time * .1f) * .2f), projection = SceneRenderer.Projection(480f / 280);
        var corners = new BoundingFrustum(view * projection).GetCorners();
        float[] splits = { .1f, 8, 16, 40 };
        for (int c = 0; c < 3; c++)
        {
            var section = new Vector3[8]; Vector3 center = Vector3.Zero;
            for (int i = 0; i < 4; i++)
            {
                section[i] = Vector3.Lerp(corners[i], corners[i + 4], (splits[c] - .1f) / 39.9f);
                section[i + 4] = Vector3.Lerp(corners[i], corners[i + 4], (splits[c + 1] - .1f) / 39.9f);
            }
            foreach (var corner in section) center += corner / 8;
            var lightView = Matrix.CreateLookAt(center + Vector3.Normalize(new Vector3(-.4f, 1, .3f)) * 50, center, Vector3.Up);
            var box = BoundingBox.CreateFromPoints(section.Select(v => Vector3.Transform(v, lightView)));
            float width = box.Max.X - box.Min.X + 2, height = box.Max.Y - box.Min.Y + 2;
            float x = MathF.Floor((box.Min.X + box.Max.X) * .5f / (width / 512)) * (width / 512);
            float y = MathF.Floor((box.Min.Y + box.Max.Y) * .5f / (height / 512)) * (height / 512);
            var lightProjection = Matrix.CreateOrthographicOffCenter(x - width / 2, x + width / 2, y - height / 2, y + height / 2,
                Math.Max(.1f, -box.Max.Z - 10), -box.Min.Z + 10);
            lightMatrices[c] = lightView * lightProjection;
            effect.Parameters["LightVP0"].SetValue(lightMatrices[c]); effect.CurrentTechnique = effect.Techniques["ShadowDepth"];
            G.SetRenderTarget(shadows[c]); G.Clear(Color.White); G.RasterizerState = RasterizerState.CullNone;
            Scene.DrawScene(lightView, lightProjection, Time, effect);
        }
        for (int c = 0; c < 3; c++) { effect.Parameters[$"LightVP{c}"].SetValue(lightMatrices[c]); effect.Parameters[$"Shadow{c}"].SetValue(shadows[c]); }
        effect.Parameters["ShowCascades"].SetValue(Mode % 2 == 1);
        effect.Parameters["ShadowBias"].SetValue(Parameter * .0015f); effect.CurrentTechnique = effect.Techniques["Cascades"];
        G.SetRenderTarget(target); G.Clear(new Color(22, 30, 46)); Scene.DrawScene(view, projection, Time, effect);
        G.SetRenderTarget(C.Screen); Panel(target, new(80, 90, 800, 460), Mode % 2 == 1 ? "CASCADE COLORS" : "THREE FILTERED SHADOW RANGES");
        Status = $"SPLITS 8 / 16 / 40 - 512 X 512 EACH - BIAS {Parameter * .0015f:F4}";
    }
    public override void Verify()
    {
        foreach (var shadow in shadows)
        {
            var pixels = new float[512 * 512]; shadow.GetData(pixels);
            Require(pixels.Any(p => p < .99f && p >= 0), "A cascade contains no rendered shadow depths.");
        }
        Require(lightMatrices.Distinct().Count() == 3, "Cascade transforms are identical.");
    }
    public override void Dispose() { effect.Dispose(); target.Dispose(); foreach (var shadow in shadows) shadow.Dispose(); }
}
