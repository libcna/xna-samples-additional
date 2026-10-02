using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AdditionalSamples;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var entries = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.IsAbstract && typeof(Sample).IsAssignableFrom(t))
                .Select(t => (Type: t, Info: t.GetCustomAttribute<SampleInfoAttribute>()!))
                .Where(t => t.Info != null).OrderBy(t => t.Info.Id).ToArray();
            if (args.Contains("--list"))
            {
                foreach (var entry in entries) Console.WriteLine($"{entry.Info.Id,-26} {entry.Info.Title}");
                Console.WriteLine("gamer-profile-friends      Platform blocked: original Xbox Live provider required");
                return 0;
            }
            string Value(string flag, string fallback)
            {
                int i = Array.IndexOf(args, flag);
                return i < 0 ? fallback : i + 1 < args.Length ? args[i + 1] : throw new ArgumentException($"Missing value for {flag}");
            }
            string id = Value("--sample", "scissor-clipping");
            int index = Array.FindIndex(entries, e => e.Info.Id == id);
            if (index < 0) throw new ArgumentException($"Unknown or unsupported sample '{id}'. Use --list.");
            int limit = int.Parse(Value("--frames", "0"));
            if (limit < 0 || (args.Contains("--verify") || args.Contains("--capture")) && limit == 0)
                throw new ArgumentException("Use a positive --frames limit for verification or capture.");
            using var game = new BrowserGame(entries.Select(e => e.Type).ToArray(), index,
                limit, Value("--capture", ""), args.Contains("--verify"));
            game.Run();
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class SampleInfoAttribute(string id, string title) : Attribute
{
    public string Id { get; } = id;
    public string Title { get; } = title;
}

public abstract class Sample : IDisposable
{
    protected Context C = null!;
    protected GraphicsDevice G => C.Device;
    protected Canvas U => C.Canvas;
    protected SceneRenderer Scene => C.Scene;
    public int Mode { get; protected set; }
    public float Parameter { get; protected set; } = 1;
    public float Time { get; protected set; }
    public string Status { get; protected set; } = "";
    public virtual string Controls => "SPACE MODE  UP/DOWN PARAMETER  R RESET";
    public virtual bool CapturingInput => false;
    public virtual void Load(Context context) => C = context;
    public virtual void Update(float dt)
    {
        Time += dt;
        if (C.Pressed(Keys.Space)) Mode++;
        if (C.Pressed(Keys.Up)) Parameter = Math.Min(10, Parameter + .1f);
        if (C.Pressed(Keys.Down)) Parameter = Math.Max(.1f, Parameter - .1f);
    }
    public abstract void Draw();
    public virtual void Verify() { }
    public virtual void Dispose() { }
    protected Effect Effect(string filename) => new(G, File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "assets", filename + ".fxb")));
    protected RenderTarget2D Target(int width = 480, int height = 280,
        SurfaceFormat format = SurfaceFormat.Color, DepthFormat depth = DepthFormat.None) =>
        new(G, width, height, false, format, depth);
    protected static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    protected static Color[] Pixels(Texture2D texture)
    {
        var data = new Color[texture.Width * texture.Height];
        texture.GetData(data);
        return data;
    }
    protected void Panel(Texture2D texture, Rectangle bounds, string label)
    {
        G.ScissorRectangle = G.Viewport.Bounds;
        U.Begin(); U.Image(texture, bounds); U.Text(label, bounds.X + 8, bounds.Y + 8, Color.White); U.End();
    }
}

public sealed class Context(GraphicsDevice device, Canvas canvas, SceneRenderer scene, RenderTarget2D screen, bool verifying)
{
    public GraphicsDevice Device { get; } = device;
    public Canvas Canvas { get; } = canvas;
    public SceneRenderer Scene { get; } = scene;
    public bool Verifying { get; } = verifying;
    public RenderTarget2D Screen { get; } = screen;
    public KeyboardState Current { get; internal set; }
    public KeyboardState Previous { get; internal set; }
    public bool Pressed(Keys key) => Current.IsKeyDown(key) && Previous.IsKeyUp(key);
}

public sealed class BrowserGame : Game
{
    private readonly GraphicsDeviceManager graphics;
    private readonly Type[] types;
    private readonly int frameLimit;
    private readonly string capture;
    private readonly bool verify;
    private int index, frames;
    private Sample sample = null!;
    private Canvas canvas = null!;
    private SceneRenderer scene = null!;
    private RenderTarget2D screen = null!;
    private Context context = null!;
    public BrowserGame(Type[] types, int index, int frameLimit, string capture, bool verify)
    {
        this.types = types; this.index = index; this.frameLimit = frameLimit; this.capture = capture; this.verify = verify;
        graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 960,
            PreferredBackBufferHeight = 640,
            GraphicsProfile = GraphicsProfile.HiDef,
            SynchronizeWithVerticalRetrace = false
        };
        IsMouseVisible = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60);
    }
    protected override void LoadContent()
    {
        Console.WriteLine($"BACKBUFFER {GraphicsDevice.PresentationParameters.BackBufferWidth}x{GraphicsDevice.PresentationParameters.BackBufferHeight} CLIENT {Window.ClientBounds}");
        canvas = new Canvas(GraphicsDevice); scene = new SceneRenderer(GraphicsDevice);
        screen = new RenderTarget2D(GraphicsDevice, 960, 640, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
        context = new Context(GraphicsDevice, canvas, scene, screen, verify);
        Select();
    }
    private void Select()
    {
        sample?.Dispose();
        string id = types[index].GetCustomAttribute<SampleInfoAttribute>()!.Id;
        using var catalog = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "assets", "catalog.json")));
        var entry = catalog.RootElement.EnumerateArray().First(e => e.GetProperty("id").GetString() == id);
        var missing = entry.GetProperty("requires").EnumerateArray().Select(e => e.GetString()!)
            .Where(f => !File.Exists(Path.Combine(AppContext.BaseDirectory, "assets", f))).ToArray();
        sample = missing.Length > 0 ? new UnavailableSample(string.Join(", ", missing)) : (Sample)Activator.CreateInstance(types[index])!;
        sample.Load(context);
        var info = types[index].GetCustomAttribute<SampleInfoAttribute>()!;
        Window.Title = $"XNA Samples Additional - {info.Title}";
        Console.WriteLine($"RUN {info.Id} on FNA {typeof(Game).Assembly.GetName().Version}");
    }
    protected override void Update(GameTime gameTime)
    {
        context.Previous = context.Current; context.Current = Keyboard.GetState();
        if (!sample.CapturingInput)
        {
            if (context.Pressed(Keys.Escape)) Exit();
            if (context.Pressed(Keys.Left)) { index = (index + types.Length - 1) % types.Length; Select(); }
            if (context.Pressed(Keys.Right)) { index = (index + 1) % types.Length; Select(); }
            if (context.Pressed(Keys.R)) Select();
        }
        sample.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        base.Update(gameTime);
    }
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(screen);
        GraphicsDevice.Viewport = new Viewport(0, 0, 960, 640);
        GraphicsDevice.ScissorRectangle = GraphicsDevice.Viewport.Bounds;
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        GraphicsDevice.Clear(new Color(17, 22, 34));
        sample.Draw();
        bool finalFrame = frames + 1 >= frameLimit && frameLimit > 0;
        if (finalFrame && verify) sample.Verify();
        GraphicsDevice.ScissorRectangle = GraphicsDevice.Viewport.Bounds;
        canvas.Begin();
        canvas.Box(new Rectangle(0, 0, 960, 48), new Color(9, 13, 23));
        canvas.Text(types[index].GetCustomAttribute<SampleInfoAttribute>()!.Title, 18, 12, Color.White, 2);
        canvas.Box(new Rectangle(0, 584, 960, 56), new Color(9, 13, 23));
        canvas.Text(sample.Controls, 16, 594, new Color(133, 201, 250));
        canvas.Text(sample.Status.Length > 77 ? sample.Status[..74] + "..." : sample.Status, 16, 616, Color.White);
        canvas.End();
        if (++frames >= frameLimit && frameLimit > 0)
        {
            if (capture.Length > 0)
            {
                string path = Path.GetFullPath(capture);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                GraphicsDevice.SetRenderTarget(null);
                var data = new Color[960 * 640]; screen.GetData(data);
                using var texture = new Texture2D(GraphicsDevice, 960, 640); texture.SetData(data);
                using var file = File.Create(path); texture.SaveAsPng(file, 960, 640);
            }
            Console.WriteLine($"{(verify ? "PASS" : "DONE")} {types[index].GetCustomAttribute<SampleInfoAttribute>()!.Id} frames={frames}");
            Exit();
        }
        GraphicsDevice.SetRenderTarget(null);
        canvas.Begin(); canvas.Image(screen, new Rectangle(0, 0, 960, 640)); canvas.End();
        base.Draw(gameTime);
    }
    protected override void UnloadContent()
    {
        GraphicsDevice.SetRenderTarget(null);
        sample?.Dispose(); canvas?.Dispose(); scene?.Dispose(); screen?.Dispose();
    }
}

public sealed class UnavailableSample(string missing) : Sample
{
    public override void Draw()
    {
        U.Begin(); U.Text("ASSET BUILD REQUIRED", 60, 150, Color.Gold, 3);
        U.Text("MISSING: " + missing, 60, 220, Color.White);
        U.Text("SEE DOCS/DEPENDENCIES.MD AND TOOLS/BUILD_ASSETS.PY", 60, 300, Color.White); U.End();
        Status = "SOURCE IMPLEMENTED - RUNTIME CHECKS REMAIN PENDING";
    }
    public override void Verify() => throw new FileNotFoundException("Cannot verify this sample: missing " + missing);
}
