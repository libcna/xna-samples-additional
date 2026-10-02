using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AdditionalSamples;

[SampleInfo("reliable-save-game", "Reliable Save Game - Recovery and Migration")]
public sealed class ReliableSaveGameSample : Sample
{
    private SaveStore store = null!;
    private SavedGame state = new(3, 120);
    private string message = "S SAVE / L LOAD / C CORRUPT / F INTERRUPT / V LEGACY";
    public override string Controls => "S SAVE  L LOAD  C CORRUPT  F INTERRUPT  V LEGACY  UP/DOWN COINS";

    public override void Load(Context context)
    {
        base.Load(context);
        store = new SaveStore(Path.Combine(Environment.CurrentDirectory, ".artifacts", "userdata", "save-sample"));
    }
    public override void Update(float dt)
    {
        base.Update(dt);
        if (C.Pressed(Keys.Up)) state = state with { Coins = state.Coins + 1 };
        if (C.Pressed(Keys.Down)) state = state with { Coins = Math.Max(0, state.Coins - 1) };
        state = state with { PlayerX = 300 + MathF.Sin(Time) * 150 };
        try
        {
            if (C.Pressed(Keys.S)) { store.Save(state); message = "SAVED VERSION 2 WITH SHA256"; }
            if (C.Pressed(Keys.L)) { state = store.Load(); message = "LOADED FROM " + store.LastSource; }
            if (C.Pressed(Keys.C)) { Directory.CreateDirectory(store.DirectoryPath); File.WriteAllText(store.Primary, "broken"); message = "PRIMARY CORRUPTED - L TRIES BACKUP"; }
            if (C.Pressed(Keys.F)) store.Save(state, simulateFailureBeforeReplace: true);
            if (C.Pressed(Keys.V))
            {
                Directory.CreateDirectory(store.DirectoryPath);
                File.WriteAllText(store.Primary, System.Text.Json.JsonSerializer.Serialize(SaveStore.Envelope(1, "{\"Gold\":7,\"X\":200}")));
                message = "LEGACY VERSION 1 WRITTEN - L MIGRATES IT";
            }
        }
        catch (Exception e) when (e is IOException or InvalidDataException) { message = e.Message; }
    }
    public override void Draw()
    {
        U.Begin();
        U.Text($"COINS {state.Coins}", 70, 110, Color.Gold, 4);
        U.Box(new Rectangle(70, 250, 800, 8), Color.SlateGray);
        U.Box(new Rectangle((int)state.PlayerX, 200, 40, 50), Color.CornflowerBlue);
        U.Text("TEMP WRITE + FLUSH + ATOMIC RENAME", 70, 320, Color.White);
        U.Text("VALID BACKUP / CHECKSUM / VERSION MIGRATION", 70, 350, Color.White);
        U.Text(message, 70, 410, Color.Cyan);
        U.End();
        Status = "STATE FILES ARE ISOLATED UNDER .ARTIFACTS/USERDATA";
    }
    public override void Verify()
    {
        var test = new SaveStore(Path.Combine(Environment.CurrentDirectory, ".artifacts", "verify-save", Guid.NewGuid().ToString("N")));
        try
        {
            test.Save(new(3, 120)); test.Save(new(8, 240));
            File.WriteAllText(test.Primary, "corrupt");
            Require(test.Load().Coins == 3 && test.LastSource == "Backup", "Backup recovery failed.");
            try { test.Save(new(20, 0), true); } catch (IOException) { }
            Require(test.Load().Coins == 3, "Interrupted save lost the valid backup.");
        }
        finally { if (Directory.Exists(test.DirectoryPath)) Directory.Delete(test.DirectoryPath, true); }
    }
}
