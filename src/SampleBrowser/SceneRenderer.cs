using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdditionalSamples;

public sealed class SceneRenderer : IDisposable
{
    private readonly GraphicsDevice device;
    private readonly BasicEffect basic;
    private readonly VertexPositionNormalTexture[] cube;
    private readonly VertexPositionNormalTexture[] sphere;
    public SceneRenderer(GraphicsDevice device)
    {
        this.device = device; basic = new BasicEffect(device); basic.EnableDefaultLighting();
        cube = MakeCube(); sphere = MakeSphere();
    }
    public static Matrix View(float angle = .6f) => Matrix.CreateLookAt(new Vector3(MathF.Sin(angle) * 11, 7, MathF.Cos(angle) * 11), new Vector3(0, .5f, 0), Vector3.Up);
    public static Matrix Projection(float aspect = 1.7f) => Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect, .1f, 40);
    public void DrawScene(Matrix view, Matrix projection, float time, Effect? effect = null)
    {
        DrawBox(Matrix.CreateScale(14, .2f, 14) * Matrix.CreateTranslation(0, -.5f, 0), new Color(100, 117, 139), view, projection, effect);
        for (int i = 0; i < 7; i++)
        {
            float angle = i * MathHelper.TwoPi / 7;
            Matrix world = Matrix.CreateScale(1, 1 + (i % 3) * .7f, 1) * Matrix.CreateRotationY(time * .3f + angle) *
                Matrix.CreateTranslation(MathF.Sin(angle) * 3, .4f + (i % 3) * .35f, MathF.Cos(angle) * 3);
            DrawBox(world, new Color(.3f + (i % 3) * .23f, .35f + (i % 2) * .3f, .8f - i * .07f), view, projection, effect);
        }
    }
    public void DrawBox(Matrix world, Color color, Matrix view, Matrix projection, Effect? effect = null) => DrawMesh(cube, world, color, view, projection, effect);
    public void DrawSphere(Matrix world, Color color, Matrix view, Matrix projection, Effect effect) => DrawMesh(sphere, world, color, view, projection, effect);
    public void DrawMesh(VertexPositionNormalTexture[] mesh, Matrix world, Color color, Matrix view, Matrix projection, Effect? effect = null)
    {
        device.BlendState = BlendState.Opaque; device.DepthStencilState = DepthStencilState.Default;
        if (effect == null)
        {
            basic.World = world; basic.View = view; basic.Projection = projection; basic.DiffuseColor = color.ToVector3();
            effect = basic;
        }
        else if (effect is EnvironmentMapEffect environment)
        {
            environment.World = world; environment.View = view; environment.Projection = projection;
        }
        else
        {
            effect.Parameters["World"]?.SetValue(world); effect.Parameters["View"]?.SetValue(view);
            effect.Parameters["Projection"]?.SetValue(projection); effect.Parameters["Albedo"]?.SetValue(color.ToVector3());
        }
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply(); device.DrawUserPrimitives(PrimitiveType.TriangleList, mesh, 0, mesh.Length / 3);
        }
    }
    public static VertexPositionNormalTexture[] MakeCube()
    {
        var list = new List<VertexPositionNormalTexture>();
        Vector3[] normals = { Vector3.Right, Vector3.Left, Vector3.Up, Vector3.Down, Vector3.Backward, Vector3.Forward };
        foreach (var normal in normals)
        {
            Vector3 tangent = MathF.Abs(normal.Y) > .5f ? Vector3.Right : Vector3.Up;
            Vector3 bitangent = Vector3.Cross(normal, tangent);
            Vector3[] corners = { normal - tangent - bitangent, normal + tangent - bitangent,
                normal + tangent + bitangent, normal - tangent + bitangent };
            Vector2[] uv = { new(0, 1), new(0, 0), new(1, 0), new(1, 1) };
            foreach (int i in new[] { 0, 1, 2, 0, 2, 3 }) list.Add(new(corners[i] * .5f, normal, uv[i]));
        }
        return list.ToArray();
    }
    private static VertexPositionNormalTexture[] MakeSphere()
    {
        var vertices = new List<VertexPositionNormalTexture>();
        VertexPositionNormalTexture At(int x, int y)
        {
            float u = x / 24f, v = y / 12f;
            var n = new Vector3(MathF.Sin(v * MathHelper.Pi) * MathF.Cos(u * MathHelper.TwoPi), MathF.Cos(v * MathHelper.Pi), MathF.Sin(v * MathHelper.Pi) * MathF.Sin(u * MathHelper.TwoPi));
            return new(n, n, new Vector2(u, v));
        }
        for (int y = 0; y < 12; y++) for (int x = 0; x < 24; x++)
                vertices.AddRange(new[] { At(x, y), At(x + 1, y), At(x + 1, y + 1), At(x, y), At(x + 1, y + 1), At(x, y + 1) });
        return vertices.ToArray();
    }
    public void Dispose() => basic.Dispose();
}
