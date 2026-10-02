using Microsoft.Xna.Framework;

namespace AdditionalSamples;

public sealed class RigidBody(Vector2 position, Vector2 halfSize, float mass)
{
    public Vector2 Position = position, Velocity;
    public float Angle, AngularVelocity;
    public readonly Vector2 HalfSize = halfSize;
    public readonly float InverseMass = mass > 0 ? 1 / mass : 0;
    public readonly float InverseInertia = mass > 0 ? 3 / (mass * halfSize.LengthSquared()) : 0;
    public float Friction = .6f, Restitution = .05f;
    public Vector2 Rotate(Vector2 p) => new(p.X * MathF.Cos(Angle) - p.Y * MathF.Sin(Angle), p.X * MathF.Sin(Angle) + p.Y * MathF.Cos(Angle));
    public Vector2[] Corners() => new[] { Position + Rotate(-HalfSize), Position + Rotate(new(HalfSize.X, -HalfSize.Y)), Position + Rotate(HalfSize), Position + Rotate(new(-HalfSize.X, HalfSize.Y)) };
    public void Impulse(Vector2 impulse, Vector2 offset) { Velocity += impulse * InverseMass; AngularVelocity += PhysicsWorld.Cross(offset, impulse) * InverseInertia; }
}

public sealed class Contact(RigidBody a, RigidBody b, Vector2 normal, float penetration, Vector2 point)
{
    public readonly RigidBody A = a, B = b;
    public readonly Vector2 Normal = normal, Point = point;
    public readonly float Penetration = penetration;
    internal float NormalImpulse, TangentImpulse, Bounce;
}

public sealed class PinJoint(RigidBody body, Vector2 localAnchor, Vector2 worldAnchor)
{
    public readonly RigidBody Body = body;
    public readonly Vector2 LocalAnchor = localAnchor, Anchor = worldAnchor;
    public Vector2 Position => Body.Position + Body.Rotate(LocalAnchor);
    internal void Solve(float dt)
    {
        Vector2 r = Body.Rotate(LocalAnchor), error = Position - Anchor;
        Vector2 velocity = Body.Velocity + PhysicsWorld.Cross(Body.AngularVelocity, r);
        float m = Body.InverseMass, i = Body.InverseInertia;
        float a = m + r.Y * r.Y * i, b = -r.X * r.Y * i, d = m + r.X * r.X * i, determinant = a * d - b * b;
        if (determinant < 1e-10f) return;
        Vector2 rhs = -(velocity + error * (.15f / dt));
        Body.Impulse(new((d * rhs.X - b * rhs.Y) / determinant, (a * rhs.Y - b * rhs.X) / determinant), r);
    }
}

public sealed class PhysicsWorld
{
    public readonly List<RigidBody> Bodies = new();
    public readonly List<PinJoint> Joints = new();
    public readonly List<Contact> Contacts = new();
    public Vector2 Gravity = new(0, 500);
    public static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
    public static Vector2 Cross(float a, Vector2 b) => new(-a * b.Y, a * b.X);
    public static List<Contact> Collide(RigidBody a, RigidBody b)
    {
        var av = a.Corners(); var bv = b.Corners();
        float penetration = float.MaxValue; Vector2 normal = Vector2.Zero;
        foreach (Vector2 axis in new[] { a.Rotate(Vector2.UnitX), a.Rotate(Vector2.UnitY), b.Rotate(Vector2.UnitX), b.Rotate(Vector2.UnitY) })
        {
            var ap = av.Select(v => Vector2.Dot(v, axis)).ToArray(); var bp = bv.Select(v => Vector2.Dot(v, axis)).ToArray();
            float overlap = Math.Min(ap.Max(), bp.Max()) - Math.Max(ap.Min(), bp.Min());
            if (overlap <= 0) return new();
            if (overlap < penetration) { penetration = overlap; normal = Vector2.Dot(b.Position - a.Position, axis) < 0 ? -axis : axis; }
        }
        // Clip the convex rectangle against the four half-planes of the other rectangle.
        var polygon = av.ToList();
        for (int edge = 0; edge < 4 && polygon.Count > 0; edge++)
        {
            Vector2 p = bv[edge], delta = bv[(edge + 1) % 4] - p;
            var clipped = new List<Vector2>(); Vector2 previous = polygon[^1]; float pd = Cross(delta, previous - p);
            foreach (var current in polygon)
            {
                float cd = Cross(delta, current - p);
                if ((cd >= 0) != (pd >= 0)) clipped.Add(previous + (current - previous) * (pd / (pd - cd)));
                if (cd >= 0) clipped.Add(current);
                previous = current; pd = cd;
            }
            polygon = clipped;
        }
        if (polygon.Count == 0) return new();
        Vector2 tangent = new(-normal.Y, normal.X);
        float plane = (av.Max(v => Vector2.Dot(v, normal)) + bv.Min(v => Vector2.Dot(v, normal))) * .5f;
        Vector2 OnPlane(Vector2 v) => v + normal * (plane - Vector2.Dot(v, normal));
        Vector2 left = OnPlane(polygon.MinBy(v => Vector2.Dot(v, tangent)));
        Vector2 right = OnPlane(polygon.MaxBy(v => Vector2.Dot(v, tangent)));
        var result = new List<Contact> { new(a, b, normal, penetration, left) };
        if (Vector2.DistanceSquared(left, right) > .01f) result.Add(new(a, b, normal, penetration, right));
        return result;
    }
    public void Step(float dt)
    {
        if (!(dt > 0) || !float.IsFinite(dt)) throw new ArgumentOutOfRangeException(nameof(dt));
        foreach (var body in Bodies.Where(b => b.InverseMass > 0))
        {
            body.Velocity += Gravity * dt; body.Position += body.Velocity * dt; body.Angle += body.AngularVelocity * dt;
        }
        Contacts.Clear();
        for (int a = 0; a < Bodies.Count; a++) for (int b = a + 1; b < Bodies.Count; b++)
                if (Bodies[a].InverseMass + Bodies[b].InverseMass > 0) Contacts.AddRange(Collide(Bodies[a], Bodies[b]));
        foreach (var c in Contacts)
        {
            Vector2 relative = VelocityAt(c.B, c.Point) - VelocityAt(c.A, c.Point);
            float closing = Vector2.Dot(relative, c.Normal);
            c.Bounce = closing < -20 ? -closing * Math.Min(c.A.Restitution, c.B.Restitution) : 0;
        }
        for (int iteration = 0; iteration < 16; iteration++)
        {
            foreach (var c in Contacts) Resolve(c, dt);
            foreach (var joint in Joints) joint.Solve(dt);
        }
        foreach (var c in Contacts)
        {
            float inverseMass = c.A.InverseMass + c.B.InverseMass;
            Vector2 correction = c.Normal * (Math.Max(c.Penetration - .5f, 0) * .15f / inverseMass);
            c.A.Position -= correction * c.A.InverseMass; c.B.Position += correction * c.B.InverseMass;
        }
    }
    private static Vector2 VelocityAt(RigidBody b, Vector2 point) => b.Velocity + Cross(b.AngularVelocity, point - b.Position);
    private static float EffectiveMass(Contact c, Vector2 axis)
    {
        float ra = Cross(c.Point - c.A.Position, axis), rb = Cross(c.Point - c.B.Position, axis);
        return c.A.InverseMass + c.B.InverseMass + ra * ra * c.A.InverseInertia + rb * rb * c.B.InverseInertia;
    }
    private static void Apply(Contact c, Vector2 impulse)
    {
        c.A.Impulse(-impulse, c.Point - c.A.Position); c.B.Impulse(impulse, c.Point - c.B.Position);
    }
    private static void Resolve(Contact c, float dt)
    {
        float normalSpeed = Vector2.Dot(VelocityAt(c.B, c.Point) - VelocityAt(c.A, c.Point), c.Normal);
        float bias = .12f / dt * Math.Max(c.Penetration - .5f, 0);
        float old = c.NormalImpulse;
        c.NormalImpulse = Math.Max(0, old + (bias + c.Bounce - normalSpeed) / EffectiveMass(c, c.Normal));
        Apply(c, c.Normal * (c.NormalImpulse - old));
        Vector2 tangent = new(-c.Normal.Y, c.Normal.X);
        float tangentSpeed = Vector2.Dot(VelocityAt(c.B, c.Point) - VelocityAt(c.A, c.Point), tangent);
        float limit = MathF.Sqrt(c.A.Friction * c.B.Friction) * c.NormalImpulse;
        old = c.TangentImpulse; c.TangentImpulse = Math.Clamp(old - tangentSpeed / EffectiveMass(c, tangent), -limit, limit);
        Apply(c, tangent * (c.TangentImpulse - old));
    }
}
