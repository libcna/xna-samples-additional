using System.Text.Json;
using Microsoft.Xna.Framework.Input;

namespace AdditionalSamples;

public sealed record InputBinding(Keys Key, Buttons? Button = null);
public sealed class BindingMap
{
    private static readonly Keys[] Reserved = { Keys.Escape, Keys.Left, Keys.Right, Keys.R, Keys.Enter, Keys.Space, Keys.S, Keys.L, Keys.Up, Keys.Down };
    private static bool Invalid(InputBinding? b) => b == null || Reserved.Contains(b.Key) ||
        b.Key == Keys.None && b.Button == null || !Enum.IsDefined(b.Key) || b.Button is Buttons button && !Enum.IsDefined(button);
    public Dictionary<string, InputBinding> Actions { get; private set; } = Defaults();
    private static Dictionary<string, InputBinding> Defaults() => new()
    {
        ["MoveLeft"] = new(Keys.A, Buttons.DPadLeft),
        ["MoveRight"] = new(Keys.D, Buttons.DPadRight),
        ["Jump"] = new(Keys.J, Buttons.A),
        ["Interact"] = new(Keys.E, Buttons.X)
    };
    public string? Bind(string action, InputBinding binding)
    {
        if (!Actions.ContainsKey(action)) throw new ArgumentException("Unknown action.");
        if (Invalid(binding)) return "Reserved or invalid binding";
        foreach (var existing in Actions)
            if (existing.Key != action && (binding.Key != Keys.None && existing.Value.Key == binding.Key ||
                binding.Button != null && existing.Value.Button == binding.Button)) return existing.Key;
        Actions[action] = binding; return null;
    }
    public void Reset() => Actions = Defaults();
    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        string temp = path + ".tmp"; File.WriteAllText(temp, JsonSerializer.Serialize(Actions)); File.Move(temp, path, true);
    }
    public void Load(string path)
    {
        var loaded = JsonSerializer.Deserialize<Dictionary<string, InputBinding>>(File.ReadAllText(path)) ?? throw new InvalidDataException("Empty bindings.");
        if (!loaded.Keys.Order().SequenceEqual(Defaults().Keys.Order())) throw new InvalidDataException("Action set changed.");
        if (loaded.Values.Any(Invalid))
            throw new InvalidDataException("Invalid binding.");
        if (loaded.Values.Where(b => b.Key != Keys.None).GroupBy(b => b.Key).Any(g => g.Count() > 1) ||
            loaded.Values.Where(b => b.Button != null).GroupBy(b => b.Button).Any(g => g.Count() > 1))
            throw new InvalidDataException("Conflicting bindings.");
        Actions = loaded;
    }
}
