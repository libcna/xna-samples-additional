using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace AdditionalSamples;

[SampleInfo("media-library", "Music Playlist and Visualizer - FNA Portable Portion")]
public sealed class MediaLibrarySample : Sample
{
    private readonly List<Song> songs = new();
    private readonly VisualizationData visualization = new();
    private int selected;
    private string nativeStatus = "";
    private bool visualizationAvailable;
    public override string Controls => "SPACE NEXT SONG  P PAUSE/RESUME  UP/DOWN VOLUME  R RESET";
    public override void Load(Context c)
    {
        base.Load(c);
        // Report the native boundary explicitly; never present local files as a system MediaLibrary.
        try { var library = new MediaLibrary(); nativeStatus = $"SYSTEM LIBRARY SONGS {library.Songs.Count}"; }
        catch (NotImplementedException) { nativeStatus = "SYSTEM MEDIALIBRARY UNAVAILABLE IN FNA"; }
        foreach (int hz in new[] { 220, 440 })
            songs.Add(Song.FromUri($"Generated {hz} Hz", new Uri(Path.Combine(AppContext.BaseDirectory, "assets", $"tone-{hz}.wav"))));
        MediaPlayer.IsRepeating = true; MediaPlayer.IsVisualizationEnabled = true;
        visualizationAvailable = MediaPlayer.IsVisualizationEnabled;
        MediaPlayer.Volume = .3f; MediaPlayer.Play(songs[0]);
    }
    public override void Update(float dt)
    {
        Time += dt;
        if (C.Pressed(Keys.Space)) { selected = (selected + 1) % songs.Count; MediaPlayer.Play(songs[selected]); }
        if (C.Pressed(Keys.P)) { if (MediaPlayer.State == MediaState.Playing) MediaPlayer.Pause(); else MediaPlayer.Resume(); }
        if (C.Pressed(Keys.Up)) MediaPlayer.Volume = Math.Min(1, MediaPlayer.Volume + .1f);
        if (C.Pressed(Keys.Down)) MediaPlayer.Volume = Math.Max(0, MediaPlayer.Volume - .1f);
        MediaPlayer.GetVisualizationData(visualization);
    }
    public override void Draw()
    {
        U.Begin(); U.Text(nativeStatus, 60, 100, Color.Gold);
        U.Text("LOCAL GENERATED PLAYLIST", 60, 145, Color.White, 3);
        for (int i = 0; i < songs.Count; i++) U.Text(songs[i].Name, 70, 210 + i * 40, i == selected ? Color.Cyan : Color.Gray);
        if (!visualizationAvailable) U.Text("FAUDIO VISUALIZATION API UNAVAILABLE", 60, 380, Color.Gold);
        for (int i = 0; visualizationAvailable && i < 256; i++)
        {
            int h = (int)(Math.Clamp(visualization.Frequencies[i], 0, 1) * 200);
            U.Box(new Rectangle(65 + i * 3, 545 - h, 2, h + 1), Color.Cyan);
        }
        U.End(); Status = $"{MediaPlayer.State} - QUEUE ACTIVE SONG {MediaPlayer.Queue.ActiveSong?.Name} - NATIVE LIBRARY REMAINS BLOCKED";
    }
    public override void Verify()
    {
        Require(MediaPlayer.State == MediaState.Playing && MediaPlayer.Queue.ActiveSong != null, "Local music playback or queue failed.");
        if (visualizationAvailable) Require(visualization.Frequencies.Any(f => f > 0), "No real visualization data was produced.");
        else Console.WriteLine("LIMITATION FAudio visualization is unavailable; no fabricated spectrum is displayed.");
    }
    public override void Dispose() { MediaPlayer.Stop(); MediaPlayer.IsVisualizationEnabled = false; foreach (var song in songs) song.Dispose(); }
}
