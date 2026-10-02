using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AdditionalSamples;

[SampleInfo("navigation-behavior", "Navigation Mesh and Behavior Tree")]
public sealed class NavigationBehaviorSample : Sample
{
    private NavigationMesh mesh = null!;
    private NavAgent agent = null!;
    private readonly NavRect[] obstacles = { new(310, 190, 405, 425), new(500, 310, 590, 485) };
    public override string Controls => "SPACE HIDE/SHOW TARGET  CLICK LEFT AREA TO MOVE TARGET  R RESET";
    public override void Load(Context c)
    {
        base.Load(c); mesh = new(new(50, 125, 660, 550), obstacles, 12);
        agent = new(mesh, new(110, 210), new[] { new Vector2(110, 180), new(610, 180), new(440, 500), new(110, 500) });
        agent.Target = new(230, 230);
    }
    public override void Update(float dt)
    {
        base.Update(dt); var mouse = Mouse.GetState(); Vector2 position = new(mouse.X, mouse.Y);
        if (mouse.LeftButton == ButtonState.Pressed && mesh.Locate(position) >= 0) agent.Target = position;
        agent.TargetEnabled = Mode % 2 == 0; agent.Update(dt);
    }
    private void Rectangle(NavRect r, Color color)
    {
        Vector2[] points = { new(r.Left, r.Top), new(r.Right, r.Top), new(r.Right, r.Bottom), new(r.Left, r.Bottom) };
        for (int i = 0; i < 4; i++) U.Line(points[i], points[(i + 1) % 4], color);
    }
    public override void Draw()
    {
        U.Begin();
        foreach (var c in mesh.Cells) Rectangle(c, new Color(48, 66, 90));
        foreach (var o in obstacles) U.Box(new((int)o.Left, (int)o.Top, (int)(o.Right - o.Left), (int)(o.Bottom - o.Top)), Color.SlateGray);
        foreach (var edges in mesh.Adjacency) foreach (var p in edges) U.Line(p.A, p.B, Color.DarkCyan, 3);
        var previous = agent.Position; foreach (var p in agent.Route) { U.Line(previous, p, Color.Gold); previous = p; }
        U.Box(new((int)agent.Position.X - 8, (int)agent.Position.Y - 8, 16, 16), Color.Cyan);
        if (agent.TargetEnabled) U.Box(new((int)agent.Target.X - 6, (int)agent.Target.Y - 6, 12, 12), Color.OrangeRed);
        if (agent.CanSee) U.Line(agent.Position, agent.Target, Color.OrangeRed);
        U.Text("REACTIVE BEHAVIOR TREE", 685, 140, Color.White);
        string[] nodes = { "Priority selector", "Chase sequence", "Target visible?", "Chase", "Search sequence", "Last sighting remembered?", "Search", "Patrol" };
        for (int i = 0; i < nodes.Length; i++) U.Text(nodes[i], 685, 195 + i * 36, agent.Trace.Contains(nodes[i]) ? Color.Cyan : Color.Gray, 1);
        U.Text("STATE " + agent.State, 685, 515, Color.Gold); U.End();
        Status = $"{mesh.Cells.Count} CONVEX POLYGONS - PORTAL A* + SEGMENT PRUNING - MEMORY {agent.Memory:F1} S";
    }
    public override void Verify()
    {
        var path = mesh.Path(new(100, 300), new(630, 460));
        Require(path.Count >= 3, "Navigation did not route around obstacles.");
        for (int i = 1; i < path.Count; i++) Require(mesh.Visible(path[i - 1], path[i]), "Navigation route intersects an obstacle.");
        Require(mesh.Locate(agent.Position) >= 0, "Agent left the navigation mesh.");
        Require(agent.Trace.Contains("Priority selector"), "Behavior tree was not evaluated.");
    }
}
