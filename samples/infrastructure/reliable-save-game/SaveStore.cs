using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AdditionalSamples;

public sealed record SavedGame(int Coins, float PlayerX);
public sealed record SaveEnvelope(int Version, string Payload, string Sha256);

public sealed class SaveStore
{
    public string DirectoryPath { get; }
    public string Primary => Path.Combine(DirectoryPath, "save.json");
    public string Backup => Path.Combine(DirectoryPath, "save.backup.json");
    public string LastSource { get; private set; } = "None";

    public SaveStore(string directory) => DirectoryPath = Path.GetFullPath(directory);

    public static SaveEnvelope Encode(SavedGame state) => Envelope(2, JsonSerializer.Serialize(state));
    public static SaveEnvelope Envelope(int version, string payload) =>
        new(version, payload, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))));

    public static SavedGame Decode(string text)
    {
        var envelope = JsonSerializer.Deserialize<SaveEnvelope>(text) ?? throw new InvalidDataException("Empty save.");
        if (envelope.Payload == null || string.IsNullOrEmpty(envelope.Sha256)) throw new InvalidDataException("Incomplete save envelope.");
        if (Envelope(envelope.Version, envelope.Payload).Sha256 != envelope.Sha256)
            throw new InvalidDataException("Save payload checksum mismatch.");
        SavedGame game;
        if (envelope.Version == 1)
        {
            using var legacy = JsonDocument.Parse(envelope.Payload);
            if (legacy.RootElement.ValueKind != JsonValueKind.Object ||
                !legacy.RootElement.TryGetProperty("Gold", out var gold) || gold.ValueKind != JsonValueKind.Number || !gold.TryGetInt32(out int coins) ||
                !legacy.RootElement.TryGetProperty("X", out var x) || x.ValueKind != JsonValueKind.Number || !x.TryGetSingle(out float position))
                throw new InvalidDataException("Incomplete legacy save.");
            game = new SavedGame(coins, position);
        }
        else if (envelope.Version == 2)
            game = JsonSerializer.Deserialize<SavedGame>(envelope.Payload) ?? throw new InvalidDataException("Empty state.");
        else throw new InvalidDataException($"Unsupported save version {envelope.Version}.");
        if (game.Coins < 0 || !float.IsFinite(game.PlayerX)) throw new InvalidDataException("Invalid state values.");
        return game;
    }

    public void Save(SavedGame game, bool simulateFailureBeforeReplace = false)
    {
        Decode(JsonSerializer.Serialize(Encode(game)));
        Directory.CreateDirectory(DirectoryPath);
        string temporary = Path.Combine(DirectoryPath, Guid.NewGuid().ToString("N") + ".tmp");
        string backupTemporary = temporary + ".backup";
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Encode(game)));
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
            }
            if (simulateFailureBeforeReplace) throw new IOException("Simulated interruption before replacement.");
            if (File.Exists(Primary))
            {
                // Preserve only a valid previous state, so a corrupt primary cannot replace a good backup.
                try
                {
                    string previous = File.ReadAllText(Primary); Decode(previous);
                    using (var stream = new FileStream(backupTemporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    { stream.Write(Encoding.UTF8.GetBytes(previous)); stream.Flush(flushToDisk: true); }
                    File.Move(backupTemporary, Backup, overwrite: true);
                }
                catch (Exception e) when (e is InvalidDataException or JsonException) { }
            }
            File.Move(temporary, Primary, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); if (File.Exists(backupTemporary)) File.Delete(backupTemporary); }
    }

    public SavedGame Load()
    {
        Exception? error = null;
        foreach (string path in new[] { Primary, Backup })
        {
            try
            {
                var state = Decode(File.ReadAllText(path));
                LastSource = path == Primary ? "Primary" : "Backup";
                return state;
            }
            catch (Exception e) when (e is IOException or JsonException or InvalidDataException)
            { error = e; }
        }
        throw new InvalidDataException("No valid save or backup is available.", error);
    }
}
