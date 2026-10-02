using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdditionalSamples;

[SampleInfo("terrain-lod", "Terrain - Distance LOD and Shared Edges")]
public sealed class TerrainLodSample : Sample
{
    private RenderTarget2D target = null!;
    private readonly RasterizerState wire = new() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
    private readonly int[,] levels = new int[TerrainModel.Tiles, TerrainModel.Tiles];
    private int triangles, visible;
    public override string Controls => "SPACE WIREFRAME  UP/DOWN CAMERA DISTANCE  R RESET";
    public override void Load(Context c) { base.Load(c); target = Target(840, 490, depth: DepthFormat.Depth24); }
    public override void Draw()
    {
        var eye = new Vector3(MathF.Sin(Time * .1f) * 22 * Parameter, 13 * Parameter, MathF.Cos(Time * .1f) * 22 * Parameter);
        var view = Matrix.CreateLookAt(eye, Vector3.Zero, Vector3.Up);
        var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 840f / 490, .1f, 150);
        var frustum = new BoundingFrustum(view * projection);
        for (int z = 0; z < TerrainModel.Tiles; z++) for (int x = 0; x < TerrainModel.Tiles; x++)
                levels[x, z] = TerrainModel.Level(Vector3.Distance(eye, TerrainModel.Origin(x, z) + new Vector3(4, 0, 4)));
        G.SetRenderTarget(target); G.Clear(new Color(25, 35, 54));
        G.RasterizerState = Mode % 2 == 0 ? RasterizerState.CullNone : wire;
        triangles = visible = 0;
        Color[] colors = { Color.LightGreen, Color.CadetBlue, Color.SteelBlue, Color.SlateGray };
        for (int z = 0; z < TerrainModel.Tiles; z++) for (int x = 0; x < TerrainModel.Tiles; x++)
            {
                Vector3 origin = TerrainModel.Origin(x, z);
                var bounds = new BoundingBox(origin + new Vector3(0, -3, 0), origin + new Vector3(8, 3, 8));
                if (frustum.Contains(bounds) == ContainmentType.Disjoint) continue;
                var mesh = TerrainModel.Mesh(x, z, levels); visible++; triangles += mesh.Length / 3;
                Scene.DrawMesh(mesh, Matrix.Identity, colors[levels[x, z]], view, projection);
            }
        G.SetRenderTarget(C.Screen); Panel(target, new(60, 65, 840, 490), "GREEN FINE - BLUE COARSE - CULLED TILES OMITTED");
        Status = $"VISIBLE {visible}/36 TILES  TRIANGLES {triangles} / FULL DETAIL {36 * 512}";
    }
    public override void Verify()
    {
        Require(visible > 0 && triangles < 36 * 512, "Distance LOD did not reduce the terrain mesh.");
        Require(Pixels(target).Distinct().Count() > 50, "Terrain was not rendered.");
    }
    public override void Dispose() { target.Dispose(); wire.Dispose(); }
}
