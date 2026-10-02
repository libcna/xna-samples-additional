using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdditionalSamples;

[SampleInfo("dynamic-cubemap", "Dynamic Cubemap - Live Reflections")]
public sealed class DynamicCubeMapSample : Sample
{
    private RenderTargetCube cube = null!; private EnvironmentMapEffect reflection = null!; private RenderTarget2D target = null!;
    private readonly Vector3[] directions = { Vector3.Right, Vector3.Left, Vector3.Up, Vector3.Down, Vector3.Backward, Vector3.Forward };
    private bool captured;
    private Texture2D white = null!;
    public override void Load(Context c)
    {
        base.Load(c); cube = new(G, 128, false, SurfaceFormat.Color, DepthFormat.Depth24);
        white = new(G, 1, 1); white.SetData(new[] { Color.White });
        reflection = new(G) { Texture = white, EnvironmentMap = cube, EnvironmentMapAmount = 1, FresnelFactor = 0, DiffuseColor = new(.08f), EnvironmentMapSpecular = Vector3.Zero };
        reflection.EnableDefaultLighting(); target = Target(800, 460, depth: DepthFormat.Depth24);
    }
    public override void Draw()
    {
        if (!captured || Mode % 2 == 0)
        {
            for (int i = 0; i < 6; i++)
            {
                G.SetRenderTarget(cube, (CubeMapFace)i); G.Clear(new Color(38 + i * 7, 53, 77)); G.RasterizerState = RasterizerState.CullNone;
                Vector3 up = i == 2 ? Vector3.Forward : i == 3 ? Vector3.Backward : Vector3.Up;
                Scene.DrawScene(Matrix.CreateLookAt(new(0, 1.5f, 0), new Vector3(0, 1.5f, 0) + directions[i], up), Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1, .1f, 40), Time);
            }
            captured = true;
        }
        G.SetRenderTarget(target); G.Clear(new Color(22, 30, 46)); G.RasterizerState = RasterizerState.CullNone;
        Matrix view = SceneRenderer.View(Time * .08f), projection = SceneRenderer.Projection(800f / 460);
        Scene.DrawScene(view, projection, Time);
        Scene.DrawSphere(Matrix.CreateScale(1.7f) * Matrix.CreateTranslation(0, 1.5f, 0), Color.White, view, projection, reflection);
        G.SetRenderTarget(C.Screen); Panel(target, new(80, 80, 800, 460), Mode % 2 == 0 ? "SIX FACES UPDATED EVERY FRAME" : "FROZEN REFLECTION / LIVE SCENE");
        U.Begin(); for (int i = 0; i < 6; i++)
        { var pixels = new Color[128 * 128]; cube.GetData((CubeMapFace)i, pixels); using var face = new Texture2D(G, 128, 128); face.SetData(pixels); U.Image(face, new Rectangle(80 + i * 134, 470, 120, 68)); }
        U.End();
        Status = "RENDERTARGETCUBE + ENVIRONMENTMAPEFFECT";
    }
    public override void Verify()
    {
        var a = new Color[128 * 128]; var b = new Color[a.Length]; cube.GetData(CubeMapFace.PositiveX, a); cube.GetData(CubeMapFace.NegativeX, b);
        Require(!a.SequenceEqual(b), "Cubemap faces are identical."); Require(Pixels(target).Distinct().Count() > 100, "Reflective scene did not render.");
    }
    public override void Dispose() { cube.Dispose(); reflection.Dispose(); target.Dispose(); white.Dispose(); }
}
