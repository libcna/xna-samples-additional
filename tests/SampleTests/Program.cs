using System.Text.Json;
using AdditionalSamples;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

int passed = 0;
void Check(bool condition, string description)
{
    if (!condition) throw new InvalidOperationException(description);
    Console.WriteLine("PASS " + description); passed++;
}
void Reject<T>(Action action, string description) where T : Exception
{
    try { action(); } catch (T) { Check(true, description); return; }
    throw new InvalidOperationException(description + " did not reject invalid input.");
}

var directory = Path.Combine(Path.GetTempPath(), "xna-additional-tests-" + Guid.NewGuid().ToString("N"));
try
{
    var store = new SaveStore(directory);
    store.Save(new(3, 120)); store.Save(new(8, 240));
    Check(store.Load() == new SavedGame(8, 240), "save roundtrip");
    Reject<IOException>(() => store.Save(new(20, 50), true), "interruption before atomic replace");
    Check(store.Load().Coins == 8, "interrupted save preserves primary");
    File.WriteAllText(store.Primary, "corrupt");
    Check(store.Load().Coins == 3 && store.LastSource == "Backup", "corrupt primary recovers backup");
    store.Save(new(10, 500)); File.WriteAllText(store.Primary, "corrupt");
    Check(store.Load().Coins == 3, "corrupt primary never overwrites valid backup");
    var legacy = SaveStore.Envelope(1, "{\"Gold\":7,\"X\":200}");
    Check(SaveStore.Decode(JsonSerializer.Serialize(legacy)) == new SavedGame(7, 200), "version 1 migration");
    var envelope = SaveStore.Encode(new(2, 4));
    Reject<InvalidDataException>(() => SaveStore.Decode(JsonSerializer.Serialize(envelope with { Payload = "{}" })), "payload checksum");
    Reject<InvalidDataException>(() => SaveStore.Decode(JsonSerializer.Serialize(SaveStore.Envelope(99, "{}"))), "unknown version");
    Reject<InvalidDataException>(() => SaveStore.Decode(JsonSerializer.Serialize(SaveStore.Encode(new(-1, 0)))), "invalid state values");
    Reject<InvalidDataException>(() => SaveStore.Decode("{\"Version\":2,\"Sha256\":\"bad\"}"), "incomplete save envelope rejected");
    Reject<InvalidDataException>(() => SaveStore.Decode(JsonSerializer.Serialize(SaveStore.Envelope(1, "{}"))), "incomplete legacy save rejected");
    File.WriteAllText(store.Primary, "{\"Version\":2,\"Sha256\":\"bad\"}");
    Check(store.Load().Coins == 3, "incomplete primary still recovers backup");

    var bindings = new BindingMap();
    Check(bindings.Bind("Jump", new(Keys.A)) == "MoveLeft", "binding conflict detection");
    Check(bindings.Actions["Jump"].Key == Keys.J, "conflict leaves old binding intact");
    Check(bindings.Bind("Jump", new(Keys.Escape)) != null, "escape remains reserved");
    Check(bindings.Bind("Jump", new(Keys.R)) != null, "browser shortcuts remain reserved");
    Check(bindings.Bind("Jump", new(Keys.K, (Buttons)123)) != null, "undefined gamepad binding rejected");
    Check(bindings.Bind("Jump", new(Keys.K, Buttons.A)) == null, "valid rebinding");
    string bindingPath = Path.Combine(directory, "bindings.json"); bindings.Save(bindingPath);
    var loaded = new BindingMap(); loaded.Load(bindingPath);
    Check(loaded.Actions["Jump"].Key == Keys.K, "binding persistence");
    File.WriteAllText(bindingPath, "{\"Jump\":{\"Key\":65}}");
    Reject<InvalidDataException>(() => loaded.Load(bindingPath), "incomplete binding map");
    File.WriteAllText(bindingPath, "{\"MoveLeft\":null,\"MoveRight\":null,\"Jump\":null,\"Interact\":null}");
    Reject<InvalidDataException>(() => loaded.Load(bindingPath), "null binding entries rejected");

    foreach (int fps in new[] { 15, 30, 60, 120, 144 })
    {
        var timing = new FixedStepModel();
        for (int i = 0; i < fps * 10; i++) timing.Advance(1.0 / fps);
        Check(Math.Abs(timing.Position - 900) < 1e-6, $"fixed timestep at {fps} FPS");
        Check(timing.Alpha >= 0 && timing.Alpha < 1, $"bounded interpolation at {fps} FPS");
    }
    var stalled = new FixedStepModel(); stalled.Advance(.5);
    Check(stalled.LastUpdates == 8 && stalled.DroppedTime > .3, "bounded catch-up after stall");
    Reject<ArgumentOutOfRangeException>(() => stalled.Advance(-1), "negative elapsed time");
    Check(Math.Abs(ProceduralAudioSample.Wave(.25, 0) - 1) < .001, "sine phase");
    Check(ProceduralAudioSample.Wave(.25, 1) > 0 && ProceduralAudioSample.Wave(.75, 1) < 0, "square wave halves");
    Check(ProceduralAudioSample.Wave(.1, 2) < ProceduralAudioSample.Wave(.9, 2), "sawtooth progression");
    var levels = new int[TerrainModel.Tiles, TerrainModel.Tiles]; levels[2, 1] = 3;
    var fine = TerrainModel.Mesh(1, 1, levels); var coarse = TerrainModel.Mesh(2, 1, levels);
    float seamX = TerrainModel.Origin(2, 1).X;
    Check(fine.Length == 16 * 16 * 6 && coarse.Length == 2 * 2 * 6, "terrain LOD changes mesh density");
    var edge = fine.Where(v => Math.Abs(v.Position.X - seamX) < .001f).Select(v => v.Position).ToArray();
    Check(edge.All(p => Math.Abs(p.Y - TerrainModel.BoundaryHeight(p.X, p.Z, false, 3)) < .001f), "fine terrain edge follows coarse segments");
    Check(coarse.Where(v => Math.Abs(v.Position.X - seamX) < .001f).All(v => edge.Any(p => Vector3.Distance(p, v.Position) < .001f)), "terrain seam endpoints coincide");

    var a = new RigidBody(new(0, 0), new(2), 1); var b = new RigidBody(new(3, 0), new(2), 1);
    Check(PhysicsWorld.Collide(a, b).Count == 2, "two-point box contact manifold");
    b.Position = new(7, 0); Check(PhysicsWorld.Collide(a, b).Count == 0, "separated boxes have no contact");
    a.Impulse(new(0, 10), new(2, 0)); Check(a.AngularVelocity > 0, "off-center impulse produces rotation");
    var physics = new PhysicsWorld(); var ground = new RigidBody(new(0, 50), new(100, 5), 0);
    var box = new RigidBody(new(0, 0), new(5), 1); physics.Bodies.AddRange(new[] { ground, box });
    for (int i = 0; i < 1200; i++) physics.Step(1f / 120);
    Check(Math.Abs(box.Position.Y - 40) < 2 && Math.Abs(box.Velocity.Y) < 5, "box settles on a static floor");
    box.Position = new(0, 39.8f); box.Velocity = new(80, 0);
    for (int i = 0; i < 240; i++) physics.Step(1f / 120);
    Check(Math.Abs(box.Velocity.X) < 5, "contact friction stops sliding");
    var jointWorld = new PhysicsWorld(); var hanging = new RigidBody(new(0, 15), new(2, 10), 1) { Angle = .7f };
    jointWorld.Bodies.Add(hanging); var joint = new PinJoint(hanging, new(0, -10), hanging.Position + hanging.Rotate(new(0, -10))); jointWorld.Joints.Add(joint);
    for (int i = 0; i < 1200; i++) jointWorld.Step(1f / 120);
    Check(Vector2.Distance(joint.Position, joint.Anchor) < .5f, "pin joint keeps its anchor");

    var nav = new NavigationMesh(new(0, 0, 100, 100), new[] { new NavRect(40, 20, 60, 80) }, 2);
    var route = nav.Path(new(10, 50), new(90, 50));
    Check(nav.Cells.Count == 4 && route.Count >= 3, "convex navigation mesh routes around obstacle");
    Check(Enumerable.Range(1, route.Count - 1).All(i => nav.Visible(route[i - 1], route[i])), "navigation segments preserve clearance");
    Check(nav.Path(new(50, 50), new(90, 50)).Count == 0, "obstructed navigation start rejected");
    var disconnected = new NavigationMesh(new(0, 0, 100, 100), new[] { new NavRect(40, 0, 60, 100) }, 2);
    Check(disconnected.Path(new(10, 50), new(90, 50)).Count == 0, "disconnected navigation islands rejected");
    var agent = new NavAgent(nav, new(10, 10), new[] { new Vector2(10, 90), new(90, 10) }) { TargetEnabled = false, Target = new(20, 10) };
    agent.Update(.01f); Check(agent.State == "Patrol", "behavior tree patrol without target");
    agent.TargetEnabled = true; agent.Update(.01f); Check(agent.State == "Chase", "behavior tree chooses visible target");
    agent.TargetEnabled = false; agent.Update(.01f); Check(agent.State == "Search", "behavior tree searches last sighting");
    for (int i = 0; i < 200; i++) agent.Update(.02f);
    Check(agent.State == "Patrol" && nav.Locate(agent.Position) >= 0, "behavior memory expires and agent stays on mesh");
    Console.WriteLine($"{passed} checks passed.");
    return 0;
}
catch (Exception error) { Console.Error.WriteLine(error); return 1; }
finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
