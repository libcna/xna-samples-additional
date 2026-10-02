using Microsoft.Xna.Framework;

namespace AdditionalSamples;

public enum BehaviorStatus { Failure, Success, Running }
public abstract class BehaviorNode(string name)
{
    public readonly string Name = name;
    public BehaviorStatus LastStatus;
    public BehaviorStatus Tick(NavAgent agent, float dt)
    {
        agent.Trace.Add(Name); return LastStatus = Evaluate(agent, dt);
    }
    protected abstract BehaviorStatus Evaluate(NavAgent agent, float dt);
}
public sealed class SelectorNode(string name, params BehaviorNode[] children) : BehaviorNode(name)
{
    protected override BehaviorStatus Evaluate(NavAgent agent, float dt)
    {
        foreach (var child in children) { var status = child.Tick(agent, dt); if (status != BehaviorStatus.Failure) return status; }
        return BehaviorStatus.Failure;
    }
}
public sealed class SequenceNode(string name, params BehaviorNode[] children) : BehaviorNode(name)
{
    protected override BehaviorStatus Evaluate(NavAgent agent, float dt)
    {
        foreach (var child in children) { var status = child.Tick(agent, dt); if (status != BehaviorStatus.Success) return status; }
        return BehaviorStatus.Success;
    }
}
public sealed class ConditionNode(string name, Func<NavAgent, bool> condition) : BehaviorNode(name)
{
    protected override BehaviorStatus Evaluate(NavAgent agent, float dt) => condition(agent) ? BehaviorStatus.Success : BehaviorStatus.Failure;
}
public sealed class ActionNode(string name, Func<NavAgent, float, BehaviorStatus> action) : BehaviorNode(name)
{
    protected override BehaviorStatus Evaluate(NavAgent agent, float dt) => action(agent, dt);
}

public sealed class NavAgent
{
    private readonly NavigationMesh mesh;
    private readonly Vector2[] patrol;
    private int patrolIndex;
    private Vector2 goal = new(float.NaN);
    private float replan;
    public Vector2 Position, Target, LastSeen;
    public bool TargetEnabled = true, CanSee;
    public float Memory;
    public string State = "Patrol";
    public List<Vector2> Route = new();
    public readonly List<string> Trace = new();
    public readonly BehaviorNode Tree;
    public NavAgent(NavigationMesh mesh, Vector2 position, Vector2[] patrol)
    {
        this.mesh = mesh; Position = position; this.patrol = patrol;
        Tree = new SelectorNode("Priority selector",
            new SequenceNode("Chase sequence", new ConditionNode("Target visible?", a => a.CanSee),
                new ActionNode("Chase", (a, dt) => { a.State = "Chase"; a.Move(a.Target, dt, 105); return BehaviorStatus.Running; })),
            new SequenceNode("Search sequence", new ConditionNode("Last sighting remembered?", a => a.Memory > 0),
                new ActionNode("Search", (a, dt) => { a.State = "Search"; a.Move(a.LastSeen, dt, 75); return BehaviorStatus.Running; })),
            new ActionNode("Patrol", (a, dt) =>
            {
                a.State = "Patrol"; if (a.Move(patrol[patrolIndex], dt, 65)) patrolIndex = (patrolIndex + 1) % patrol.Length;
                return BehaviorStatus.Running;
            }));
    }
    public void Update(float dt)
    {
        CanSee = TargetEnabled && Vector2.Distance(Position, Target) < 280 && mesh.Visible(Position, Target);
        if (CanSee) { LastSeen = Target; Memory = 3; } else Memory = Math.Max(0, Memory - dt);
        replan -= dt; Trace.Clear(); Tree.Tick(this, dt);
    }
    private bool Move(Vector2 destination, float dt, float speed)
    {
        if (!float.IsFinite(goal.X) || Vector2.DistanceSquared(goal, destination) > 225 || replan <= 0)
        { goal = destination; Route = mesh.Path(Position, destination); replan = .3f; }
        float remaining = dt * speed;
        while (Route.Count > 0 && remaining > 0)
        {
            Vector2 delta = Route[0] - Position; float distance = delta.Length();
            if (distance <= remaining) { Position = Route[0]; Route.RemoveAt(0); remaining -= distance; }
            else { Position += delta / distance * remaining; remaining = 0; }
        }
        return Vector2.Distance(Position, destination) < 3;
    }
}
