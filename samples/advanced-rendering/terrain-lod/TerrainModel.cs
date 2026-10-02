using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdditionalSamples;

public static class TerrainModel
{
    public const int Tiles = 6, Cells = 16;
    public const float TileSize = 8;
    public static float Height(float x, float z) => 1.6f * MathF.Sin(x * .16f) * MathF.Cos(z * .19f) + .6f * MathF.Sin((x + z) * .4f);
    public static Vector3 Origin(int x, int z) => new((x - Tiles / 2) * TileSize, 0, (z - Tiles / 2) * TileSize);
    public static int Level(float distance) => distance < 12 ? 0 : distance < 22 ? 1 : distance < 35 ? 2 : 3;
    public static float BoundaryHeight(float x, float z, bool alongX, int level)
    {
        float spacing = TileSize / Cells * (1 << level);
        float value = alongX ? x : z;
        float a = MathF.Floor(value / spacing) * spacing, t = (value - a) / spacing;
        return alongX ? MathHelper.Lerp(Height(a, z), Height(a + spacing, z), t) : MathHelper.Lerp(Height(x, a), Height(x, a + spacing), t);
    }
    public static VertexPositionNormalTexture[] Mesh(int tx, int tz, int[,] levels)
    {
        int own = levels[tx, tz], step = 1 << own, n = Cells / step;
        Vector3 origin = Origin(tx, tz);
        Vector3 At(int x, int z)
        {
            float px = origin.X + x * step * TileSize / Cells, pz = origin.Z + z * step * TileSize / Cells;
            float y = Height(px, pz);
            // Fine edge vertices lie on the neighboring coarse edge, including its intermediate points.
            if (x == 0 && tx > 0) y = BoundaryHeight(px, pz, false, Math.Max(own, levels[tx - 1, tz]));
            if (x == n && tx + 1 < Tiles) y = BoundaryHeight(px, pz, false, Math.Max(own, levels[tx + 1, tz]));
            if (z == 0 && tz > 0) y = BoundaryHeight(px, pz, true, Math.Max(own, levels[tx, tz - 1]));
            if (z == n && tz + 1 < Tiles) y = BoundaryHeight(px, pz, true, Math.Max(own, levels[tx, tz + 1]));
            return new(px, y, pz);
        }
        var vertices = new List<VertexPositionNormalTexture>(n * n * 6);
        void Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
            if (normal.Y < 0) normal = -normal;
            foreach (var p in new[] { a, b, c }) vertices.Add(new(p, normal, new(p.X / TileSize, p.Z / TileSize)));
        }
        for (int z = 0; z < n; z++) for (int x = 0; x < n; x++)
            {
                var a = At(x, z); var b = At(x + 1, z); var c = At(x + 1, z + 1); var d = At(x, z + 1);
                Triangle(a, d, c); Triangle(a, c, b);
            }
        return vertices.ToArray();
    }
}
