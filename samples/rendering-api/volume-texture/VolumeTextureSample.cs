using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdditionalSamples;

[SampleInfo("volume-texture", "Volume Texture - Slices and Accumulation")]
public sealed class VolumeTextureSample : Sample
{
    private Texture3D volume = null!; private Effect shader = null!; private RenderTarget2D target = null!;
    public override void Load(Context c)
    {
        base.Load(c); volume = new(G, 32, 32, 32, false, SurfaceFormat.Color); var data = new Color[32 * 32 * 32];
        for (int z = 0; z < 32; z++) for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
                {
                    var p = new Vector3(x / 31f, y / 31f, z / 31f) - new Vector3(.5f);
                    float density = Math.Clamp((.48f - p.Length()) * 5 * (.65f + .35f * MathF.Sin(x * .6f + MathF.Cos(y * .5f) + z * .4f)), 0, 1);
                    data[(z * 32 + y) * 32 + x] = new Color(.3f + density * .7f, .45f + density * .5f, .85f, density);
                }
        volume.SetData(data); shader = Effect("Volume"); shader.Parameters["Volume"].SetValue(volume); target = Target(); Parameter = .5f;
    }
    public override void Draw()
    {
        shader.Parameters["Slice"].SetValue(Math.Clamp(Parameter, 0, 1)); shader.Parameters["Mode"].SetValue((float)(Mode % 2)); shader.Parameters["Angle"].SetValue(Time * .3f);
        G.SetRenderTarget(target); Fullscreen.Draw(G, shader, "VolumeView"); G.SetRenderTarget(C.Screen);
        Panel(target, new(100, 90, 760, 450), Mode % 2 == 0 ? "TRILINEAR VOLUME SLICE" : "48 SAMPLES THROUGH THE VOLUME"); Status = $"SLICE {Parameter:F2} - REAL TEXTURE3D 32 X 32 X 32";
    }
    public override void Verify()
    {
        var p = Pixels(target); Require(p.Distinct().Count() > 30, "Volume shader produced a uniform image.");
        float old = Parameter; Parameter = .15f; Draw(); var a = Pixels(target); Parameter = .5f; Draw(); Require(!a.SequenceEqual(Pixels(target)), "Third volume coordinate had no effect."); Parameter = old; Draw();
    }
    public override void Dispose() { volume.Dispose(); shader.Dispose(); target.Dispose(); }
}
