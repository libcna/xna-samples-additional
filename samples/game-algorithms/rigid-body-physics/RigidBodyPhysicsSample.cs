using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AdditionalSamples;

[SampleInfo("rigid-body-physics", "Rigid Body Physics - SAT, Impulses and a Pin Joint")]
public sealed class RigidBodyPhysicsSample : Sample
{
    private readonly PhysicsWorld world = new();
    private float accumulator;
    public override string Controls => "SPACE SPAWN BOX  UP/DOWN FRICTION  C CONTACTS  R RESET";
    public override void Load(Context c)
    {
        base.Load(c); world.Bodies.Add(new(new(480, 542), new(440, 15), 0));
        for (int i = 0; i < 5; i++) world.Bodies.Add(new(new(320, 501 - i * 40), new(20), 1));
        var pendulum = new RigidBody(new(700, 255), new(12, 70), 2) { Angle = .65f };
        world.Bodies.Add(pendulum); world.Joints.Add(new(pendulum, new(0, -60), pendulum.Position + pendulum.Rotate(new(0, -60))));
    }
    public override void Update(float dt)
    {
        Time += dt;
        if (C.Pressed(Keys.Space)) world.Bodies.Add(new(new(400 + world.Bodies.Count % 5 * 30, 100), new(18, 24), 1) { Angle = .3f, Velocity = new(90, 0) });
        if (C.Pressed(Keys.C)) Mode++;
        if (C.Pressed(Keys.Up)) Parameter = Math.Min(2, Parameter + .1f);
        if (C.Pressed(Keys.Down)) Parameter = Math.Max(0, Parameter - .1f);
        foreach (var body in world.Bodies) body.Friction = Parameter * .6f;
        accumulator += Math.Min(dt, .1f);
        while (accumulator >= 1f / 120) { world.Step(1f / 120); accumulator -= 1f / 120; }
        if (world.Bodies.Count > 40) world.Bodies.RemoveAt(6);
    }
    public override void Draw()
    {
        U.Begin();
        foreach (var b in world.Bodies)
        {
            var vertices = b.Corners(); Color color = b.InverseMass == 0 ? Color.SlateGray : Color.CornflowerBlue;
            for (int i = 0; i < 4; i++) U.Line(vertices[i], vertices[(i + 1) % 4], color, 3);
            U.Line(b.Position, b.Position + b.Rotate(new(12, 0)), Color.White);
        }
        foreach (var j in world.Joints) { U.Box(new((int)j.Anchor.X - 4, (int)j.Anchor.Y - 4, 8, 8), Color.Gold); U.Line(j.Anchor, j.Position, Color.Gold); }
        if (Mode % 2 == 0) foreach (var c in world.Contacts) { U.Box(new((int)c.Point.X - 2, (int)c.Point.Y - 2, 4, 4), Color.Red); U.Line(c.Point, c.Point + c.Normal * 18, Color.Red); }
        U.Text("16 SOLVER ITERATIONS / 120 HZ / TWO-POINT MANIFOLDS", 60, 90, Color.White); U.End();
        Status = $"BODIES {world.Bodies.Count} CONTACTS {world.Contacts.Count} FRICTION {Parameter * .6f:F2} JOINT ERROR {Vector2.Distance(world.Joints[0].Position, world.Joints[0].Anchor):F2}";
    }
    public override void Verify()
    {
        Require(world.Bodies.All(b => float.IsFinite(b.Position.X) && float.IsFinite(b.Angle)), "Physics produced non-finite motion.");
        Require(world.Contacts.Count > 0, "Falling stack generated no contacts.");
        Require(Vector2.Distance(world.Joints[0].Position, world.Joints[0].Anchor) < 4, "Pin joint diverged.");
    }
}
