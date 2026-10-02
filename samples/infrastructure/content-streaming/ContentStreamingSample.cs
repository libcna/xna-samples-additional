using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AdditionalSamples;

[SampleInfo("content-streaming", "Content Streaming - Room Residency and Cancellation")]
public sealed class ContentStreamingSample : Sample
{
    private sealed record PreparedRoom(int Id, Color[] Pixels);
    private readonly Dictionary<int, Texture2D> resident = new();
    private readonly Queue<PreparedRoom> pendingUploads = new();
    private CancellationTokenSource? cancellation;
    private Task<PreparedRoom[]>? preparation;
    private int currentRoom, created, disposed, cancelled, generation;
    private int[] desired = Array.Empty<int>();
    public override string Controls => "SPACE NEXT ROOM  UP PREVIOUS ROOM  R RESET";
    public override void Load(Context c) { base.Load(c); Request(0); }

    private void Request(int room)
    {
        currentRoom = Math.Clamp(room, 0, 12); generation++;
        if (preparation is { IsCompleted: false }) cancelled++;
        cancellation?.Cancel(); cancellation?.Dispose(); cancellation = new();
        pendingUploads.Clear(); desired = Enumerable.Range(Math.Max(0, currentRoom - 1), Math.Min(12, currentRoom + 1) - Math.Max(0, currentRoom - 1) + 1).ToArray();
        foreach (int stale in resident.Keys.Except(desired).ToArray()) { resident[stale].Dispose(); resident.Remove(stale); disposed++; }
        int[] missing = desired.Except(resident.Keys).ToArray();
        CancellationToken token = cancellation.Token;
        preparation = Task.Run(() => missing.Select(id =>
        {
            token.ThrowIfCancellationRequested(); var pixels = new Color[128 * 128];
            for (int y = 0; y < 128; y++)
            {
                token.ThrowIfCancellationRequested();
                for (int x = 0; x < 128; x++)
                    pixels[y * 128 + x] = new Color((x * 2 + id * 31) % 255, (y * 2 + id * 53) % 255, (x ^ y ^ id * 17) % 255);
            }
            return new PreparedRoom(id, pixels);
        }).ToArray(), token);
    }
    public override void Update(float dt)
    {
        Time += dt;
        if (C.Pressed(Keys.Space)) Request(currentRoom + 1);
        if (C.Pressed(Keys.Up)) Request(currentRoom - 1);
        if (preparation is { IsCompleted: true })
        {
            if (preparation.IsFaulted) throw preparation.Exception!;
            if (!preparation.IsCanceled) foreach (var room in preparation.Result) pendingUploads.Enqueue(room);
            preparation = null;
        }
        // Bound expensive GPU work to one upload per Update, on the game thread.
        if (pendingUploads.TryDequeue(out var data) && desired.Contains(data.Id))
        {
            var texture = new Texture2D(G, 128, 128); texture.SetData(data.Pixels);
            resident.Add(data.Id, texture); created++;
        }
    }
    public override void Draw()
    {
        U.Begin(); U.Text($"CURRENT ROOM {currentRoom}", 60, 85, Color.White, 3);
        for (int i = 0; i < desired.Length; i++)
        {
            int id = desired[i]; Rectangle bounds = new(60 + i * 290, 160, 260, 260);
            if (resident.TryGetValue(id, out var texture)) U.Image(texture, bounds);
            else U.Box(bounds, Color.DarkSlateGray);
            U.Text($"ROOM {id}", bounds.X + 8, bounds.Bottom + 15, Color.White);
        }
        U.Text("CPU PREPARATION ON WORKER / GPU UPLOAD ON GAME THREAD", 60, 490, Color.Cyan); U.End();
        Status = $"RESIDENT {resident.Count}/3  CREATED {created}  DISPOSED {disposed}  CANCELLED {cancelled}  GENERATION {generation}";
    }
    public override void Verify()
    {
        int original = currentRoom;
        preparation?.GetAwaiter().GetResult(); Update(0); Update(0); Update(0);
        Require(resident.Count > 0 && resident.Count <= 3, "Room residency was not bounded.");
        Request(6); preparation!.GetAwaiter().GetResult(); Update(0); Update(0); Update(0);
        Require(resident.Keys.All(desired.Contains) && created - disposed == resident.Count, "Streaming ownership counters disagree.");
        Request(1); Request(12); preparation!.GetAwaiter().GetResult(); Update(0); Update(0); Update(0);
        Require(resident.Keys.All(id => id >= 11), "A superseded room request uploaded stale content.");
        Request(original); preparation!.GetAwaiter().GetResult(); Update(0); Update(0); Update(0);
        G.Clear(new Color(17, 22, 34)); Draw();
    }
    public override void Dispose()
    {
        cancellation?.Cancel(); cancellation?.Dispose();
        foreach (var texture in resident.Values) texture.Dispose(); resident.Clear();
    }
}
