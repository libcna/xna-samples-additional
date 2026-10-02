using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace AdditionalSamples;

[SampleInfo("xact-streaming", "XACT - Streaming Wave Bank and Layered Cues")]
public sealed class XactStreamingSample : Sample
{
    private AudioEngine engine = null!;
    private WaveBank waves = null!;
    private SoundBank sounds = null!;
    private AudioCategory category;
    private Cue baseCue = null!, layer = null!;
    private float intensity, targetIntensity, volume = .3f;
    public override string Controls => "SPACE TRANSITION LAYER  P PAUSE  UP/DOWN CATEGORY VOLUME  R RESET";
    public override void Load(Context c)
    {
        base.Load(c); string path = Path.Combine(AppContext.BaseDirectory, "assets");
        engine = new(Path.Combine(path, "streaming.xgs"));
        waves = new(engine, Path.Combine(path, "Streaming.xwb"), 0, 16);
        sounds = new(engine, Path.Combine(path, "streaming.xsb"));
        category = engine.GetCategory("Music"); category.SetVolume(volume);
        baseCue = sounds.GetCue("Base"); layer = sounds.GetCue("Layer");
        layer.SetVariable("Intensity", 0); baseCue.Play(); layer.Play();
    }
    public override void Update(float dt)
    {
        Time += dt;
        if (C.Pressed(Keys.Space)) targetIntensity = 1 - targetIntensity;
        if (C.Pressed(Keys.P)) { if (baseCue.IsPaused) { baseCue.Resume(); layer.Resume(); } else { baseCue.Pause(); layer.Pause(); } }
        if (C.Pressed(Keys.Up)) volume = Math.Min(1, volume + .1f);
        if (C.Pressed(Keys.Down)) volume = Math.Max(0, volume - .1f);
        intensity += Math.Clamp(targetIntensity - intensity, -dt, dt);
        layer.SetVariable("Intensity", intensity); category.SetVolume(volume); engine.Update();
    }
    public override void Draw()
    {
        U.Begin(); U.Text("REAL STREAMING WAVEBANK / ORIGINAL PCM", 65, 110, Color.White, 3);
        U.Text("BASE 220 HZ - LOOPING CUE", 70, 225, Color.Cyan);
        U.Text("LAYER 440 HZ - INTENSITY VOLUME RPC", 70, 335, Color.Gold);
        U.Box(new(70, 380, 800, 25), Color.SlateGray); U.Box(new(70, 380, (int)(800 * intensity), 25), Color.Gold);
        U.Text("ONE SECOND TRANSITION / CATEGORY MIXING", 70, 470, Color.White); U.End();
        Status = $"BASE PLAYING {baseCue.IsPlaying} LAYER PLAYING {layer.IsPlaying} INTENSITY {intensity:F2} MUSIC VOLUME {volume:F2}";
    }
    public override void Verify()
    {
        Console.WriteLine($"XACT base={baseCue.IsPlaying} layer={layer.IsPlaying} bankInUse={waves.IsInUse}");
        Require(baseCue.IsPlaying && layer.IsPlaying, "Streaming XACT cues did not start.");
        // FAudio's current entryRefs array is never incremented, so WaveBank.IsInUse
        // cannot prove playback. The streaming constructor rejects non-streaming flags.
        var bank = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "assets", "Streaming.xwb"));
        Require((BitConverter.ToUInt32(bank, 52) & 1) != 0, "Wave bank does not declare streaming content.");
        layer.SetVariable("Intensity", .75f); engine.Update();
        Console.WriteLine($"XACT Intensity={layer.GetVariable("Intensity")}");
        Require(Math.Abs(layer.GetVariable("Intensity") - .75f) < .001f, "Cue variable failed.");
        baseCue.Pause(); Require(baseCue.IsPaused, "Cue pause failed."); baseCue.Resume();
        layer.SetVariable("Intensity", intensity);
    }
    public override void Dispose()
    {
        baseCue?.Stop(AudioStopOptions.Immediate); layer?.Stop(AudioStopOptions.Immediate);
        baseCue?.Dispose(); layer?.Dispose(); sounds?.Dispose(); waves?.Dispose(); engine?.Dispose();
    }
}
