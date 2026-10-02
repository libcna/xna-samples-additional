using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AdditionalSamples;

[SampleInfo("input-rebinding", "Input Rebinding - Actions and Conflict Detection")]
public sealed class InputRebindingSample : Sample
{
    private readonly BindingMap bindings = new();
    private int selected;
    private bool capturing;
    public override bool CapturingInput => capturing;
    private string message = "ENTER TO REBIND SELECTED ACTION";
    private string path = null!;
    private GamePadState previousPad;
    public override string Controls => "UP/DOWN SELECT  ENTER CAPTURE  SPACE DEFAULTS  S SAVE  L LOAD  ESC EXIT";
    public override void Load(Context c)
    {
        base.Load(c); path = Path.Combine(Environment.CurrentDirectory, ".artifacts", "userdata", "bindings.json");
    }
    public override void Update(float dt)
    {
        Time += dt;
        var pad = GamePad.GetState(PlayerIndex.One);
        string action = bindings.Actions.Keys.ElementAt(selected);
        if (capturing)
        {
            if (C.Pressed(Keys.Escape)) { capturing = false; message = "CAPTURE CANCELED"; previousPad = pad; return; }
            Keys key = C.Current.GetPressedKeys().FirstOrDefault(k => k != Keys.Enter && k != Keys.Escape && !C.Previous.IsKeyDown(k));
            Buttons[] candidates = { Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.LeftShoulder, Buttons.RightShoulder, Buttons.DPadLeft, Buttons.DPadRight };
            Buttons? button = candidates.Where(b => pad.IsButtonDown(b) && !previousPad.IsButtonDown(b)).Cast<Buttons?>().FirstOrDefault();
            if (key != Keys.None || button != null)
            {
                var old = bindings.Actions[action];
                string? conflict = bindings.Bind(action, new(key != Keys.None ? key : old.Key, button ?? old.Button));
                message = conflict == null ? "BINDING UPDATED" : "CONFLICT WITH " + conflict;
                capturing = false;
            }
        }
        else
        {
            if (C.Pressed(Keys.Up)) selected = (selected + 3) % 4;
            if (C.Pressed(Keys.Down)) selected = (selected + 1) % 4;
            if (C.Pressed(Keys.Enter)) { capturing = true; message = "PRESS A NEW KEY OR GAMEPAD BUTTON"; }
            if (C.Pressed(Keys.Space)) { bindings.Reset(); message = "DEFAULTS RESTORED"; }
            try
            {
                if (C.Pressed(Keys.S)) { bindings.Save(path); message = "BINDINGS SAVED"; }
                if (C.Pressed(Keys.L)) { bindings.Load(path); message = "BINDINGS LOADED"; }
            }
            catch (Exception e) when (e is IOException or InvalidDataException or System.Text.Json.JsonException) { message = e.Message; }
        }
        previousPad = pad;
    }
    public override void Draw()
    {
        U.Begin(); int i = 0;
        foreach (var action in bindings.Actions)
        {
            int y = 110 + i * 85;
            U.Box(new Rectangle(70, y, 820, 65), i == selected ? new Color(35, 91, 136) : new Color(32, 42, 60));
            bool active = C.Current.IsKeyDown(action.Value.Key) || action.Value.Button is Buttons b && GamePad.GetState(PlayerIndex.One).IsButtonDown(b);
            U.Text($"{action.Key}  {action.Value.Key} / {action.Value.Button}", 90, y + 20, active ? Color.Gold : Color.White);
            i++;
        }
        U.Text(message, 70, 500, capturing ? Color.Gold : Color.Cyan); U.End();
        Status = "CONFLICTS ARE REJECTED - ESCAPE REMAINS AVAILABLE";
    }
    public override void Verify()
    {
        var map = new BindingMap();
        Require(map.Bind("Jump", new(Keys.A)) == "MoveLeft", "Duplicate input was accepted.");
        Require(map.Bind("Jump", new(Keys.K)) == null && map.Actions["Jump"].Key == Keys.K, "Valid binding was rejected.");
    }
}
