using Microsoft.Xna.Framework;

namespace AdditionalSamples;

public readonly record struct NavRect(float Left, float Top, float Right, float Bottom)
{
    public Vector2 Center => new((Left + Right) / 2, (Top + Bottom) / 2);
    public bool Contains(Vector2 p) => p.X >= Left && p.X <= Right && p.Y >= Top && p.Y <= Bottom;
    public NavRect Expand(float r) => new(Left - r, Top - r, Right + r, Bottom + r);
}
public readonly record struct Portal(int To, Vector2 A, Vector2 B);

public sealed class NavigationMesh
{
    public readonly NavRect Bounds;
    public readonly NavRect[] Obstacles;
    public readonly List<NavRect> Cells = new();
    public readonly List<List<Portal>> Adjacency = new();
    public NavigationMesh(NavRect bounds, IEnumerable<NavRect> obstacles, float clearance)
    {
        Bounds = bounds.Expand(-clearance); Obstacles = obstacles.Select(o => o.Expand(clearance)).ToArray();
        // Vertical decomposition yields convex free-space polygons of unequal size.
        var xs = Obstacles.SelectMany(o => new[] { o.Left, o.Right }).Append(Bounds.Left).Append(Bounds.Right)
            .Where(x => x >= Bounds.Left && x <= Bounds.Right).Distinct().Order().ToArray();
        for (int x = 0; x + 1 < xs.Length; x++)
        {
            float middle = (xs[x] + xs[x + 1]) / 2;
            float cursor = Bounds.Top;
            foreach (var obstacle in Obstacles.Where(o => middle > o.Left && middle < o.Right).OrderBy(o => o.Top))
            {
                float top = Math.Clamp(obstacle.Top, Bounds.Top, Bounds.Bottom), bottom = Math.Clamp(obstacle.Bottom, Bounds.Top, Bounds.Bottom);
                if (top > cursor) Cells.Add(new(xs[x], cursor, xs[x + 1], top));
                cursor = Math.Max(cursor, bottom);
            }
            if (cursor < Bounds.Bottom) Cells.Add(new(xs[x], cursor, xs[x + 1], Bounds.Bottom));
        }
        for (int i = 0; i < Cells.Count; i++) Adjacency.Add(new());
        for (int a = 0; a < Cells.Count; a++) for (int b = a + 1; b < Cells.Count; b++)
            {
                NavRect ca = Cells[a], cb = Cells[b];
                if (MathF.Abs(ca.Right - cb.Left) > .001f && MathF.Abs(cb.Right - ca.Left) > .001f) continue;
                float top = Math.Max(ca.Top, cb.Top), bottom = Math.Min(ca.Bottom, cb.Bottom);
                if (bottom - top <= .001f) continue;
                float px = MathF.Abs(ca.Right - cb.Left) < .001f ? ca.Right : ca.Left;
                Vector2 p = new(px, top), q = new(px, bottom);
                Adjacency[a].Add(new(b, p, q)); Adjacency[b].Add(new(a, p, q));
            }
    }
    public int Locate(Vector2 p) => !Bounds.Contains(p) || Obstacles.Any(o => o.Contains(p)) ? -1 : Cells.FindIndex(c => c.Contains(p));
    public bool Visible(Vector2 start, Vector2 end)
    {
        if (Locate(start) < 0 || Locate(end) < 0) return false;
        foreach (var o in Obstacles)
        {
            float first = 0, last = 1; Vector2 delta = end - start;
            bool Clip(float p, float q)
            {
                if (MathF.Abs(p) < 1e-7f) return q >= 0;
                float r = q / p;
                if (p < 0) { if (r > last) return false; first = Math.Max(first, r); }
                else { if (r < first) return false; last = Math.Min(last, r); }
                return true;
            }
            if (Clip(-delta.X, start.X - o.Left) && Clip(delta.X, o.Right - start.X) &&
                Clip(-delta.Y, start.Y - o.Top) && Clip(delta.Y, o.Bottom - start.Y)) return false;
        }
        return true;
    }
    public List<Vector2> Path(Vector2 start, Vector2 end)
    {
        int from = Locate(start), to = Locate(end);
        if (from < 0 || to < 0) return new();
        var open = new PriorityQueue<int, float>(); var cost = new Dictionary<int, float> { [from] = 0 };
        var previous = new Dictionary<int, (int From, Portal Edge)>(); open.Enqueue(from, 0);
        while (open.TryDequeue(out int cell, out _))
        {
            if (cell == to) break;
            foreach (var portal in Adjacency[cell])
            {
                float candidate = cost[cell] + Vector2.Distance(Cells[cell].Center, Cells[portal.To].Center);
                if (cost.TryGetValue(portal.To, out float old) && old <= candidate) continue;
                cost[portal.To] = candidate; previous[portal.To] = (cell, portal);
                open.Enqueue(portal.To, candidate + Vector2.Distance(Cells[portal.To].Center, Cells[to].Center));
            }
        }
        if (!cost.ContainsKey(to)) return new();
        var route = new List<Vector2> { end };
        for (int cell = to; cell != from; cell = previous[cell].From)
        {
            var portal = previous[cell].Edge; route.Add((portal.A + portal.B) / 2);
        }
        route.Add(start); route.Reverse();
        // Exact segment tests remove unnecessary corners without cutting through obstacles.
        var smooth = new List<Vector2> { start }; int index = 0;
        while (index < route.Count - 1)
        {
            int next = route.Count - 1;
            while (next > index + 1 && !Visible(route[index], route[next])) next--;
            smooth.Add(route[next]); index = next;
        }
        return smooth;
    }
}
