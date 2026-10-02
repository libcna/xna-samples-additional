using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace AdditionalSamples;

[SampleInfo("video-texture", "Video Texture - Playback Inside a 3D Scene")]
public sealed class VideoTextureSample : Sample
{
    private Video video = null!;
    private VideoPlayer player = null!;
    private BasicEffect screen = null!;
    private RenderTarget2D target = null!;
    private Texture2D? frame;
    private readonly VertexPositionNormalTexture[] quad = {
        new(new(-2,2,0),Vector3.Backward,new(0,0)), new(new(2,2,0),Vector3.Backward,new(1,0)), new(new(2,-.4f,0),Vector3.Backward,new(1,1)),
        new(new(-2,2,0),Vector3.Backward,new(0,0)), new(new(2,-.4f,0),Vector3.Backward,new(1,1)), new(new(-2,-.4f,0),Vector3.Backward,new(0,1)) };
    public override string Controls => "SPACE PAUSE/RESUME  UP/DOWN VOLUME  L TOGGLE LOOP  R RESET";
    public override void Load(Context c)
    {
        base.Load(c);
        // FNA extension for loose files. Original XNA loads Video through Content.Load<Video>.
        video = Video.FromUriEXT(new Uri(Path.Combine(AppContext.BaseDirectory, "assets", "test-pattern.ogv")), G);
        player = new VideoPlayer { IsLooped = true, Volume = .4f }; player.Play(video);
        screen = new BasicEffect(G) { TextureEnabled = true };
        target = Target(640, 400, depth: DepthFormat.Depth24);
    }
    public override void Update(float dt)
    {
        Time += dt;
        if (C.Pressed(Keys.Space)) { if (player.State == MediaState.Playing) player.Pause(); else if (player.State == MediaState.Paused) player.Resume(); else player.Play(video); }
        if (C.Pressed(Keys.Up)) player.Volume = Math.Min(1, player.Volume + .1f);
        if (C.Pressed(Keys.Down)) player.Volume = Math.Max(0, player.Volume - .1f);
        if (C.Pressed(Keys.L)) player.IsLooped = !player.IsLooped;
    }
    public override void Draw()
    {
        if (player.State != MediaState.Stopped) frame = player.GetTexture();
        G.SetRenderTarget(target); G.Clear(new Color(22, 30, 46)); G.RasterizerState = RasterizerState.CullNone;
        var view = Matrix.CreateLookAt(new Vector3(MathF.Sin(Time * .3f) * 3, 2, 7), new Vector3(0, .7f, 0), Vector3.Up);
        var projection = SceneRenderer.Projection(1.6f);
        Scene.DrawBox(Matrix.CreateScale(5, .2f, 3) * Matrix.CreateTranslation(0, -.7f, 0), Color.SlateGray, view, projection);
        if (frame != null)
        {
            screen.World = Matrix.Identity; screen.View = view; screen.Projection = projection; screen.Texture = frame;
            screen.CurrentTechnique.Passes[0].Apply(); G.DrawUserPrimitives(PrimitiveType.TriangleList, quad, 0, 2);
        }
        G.SetRenderTarget(C.Screen); Panel(target, new(250, 90, 660, 430), "VIDEO ON A 3D SCREEN");
        if (frame != null) Panel(frame, new(30, 180, 200, 120), "2D VIEW");
        Status = $"STATE {player.State}  POSITION {player.PlayPosition.TotalSeconds:F2}S  LOOP {player.IsLooped}";
    }
    public override void Verify()
    {
        Require(frame != null && Pixels(frame).Distinct().Count() > 40, "VideoPlayer did not decode a real image.");
        Require(player.PlayPosition.TotalSeconds > .1, "Video playback did not advance.");
        player.Pause(); Require(player.State == MediaState.Paused, "Pause failed.");
        player.Resume(); Require(player.State == MediaState.Playing, "Resume failed.");
    }
    public override void Dispose() { player.Stop(); player.Dispose(); screen.Dispose(); target.Dispose(); }
}
