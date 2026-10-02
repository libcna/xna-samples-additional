using System.Buffers.Binary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace AdditionalSamples;

[SampleInfo("procedural-audio", "Procedural Audio - PCM Waveform Synthesis")]
public sealed class ProceduralAudioSample : Sample
{
    private DynamicSoundEffectInstance voice = null!;
    private double phase;
    private int submitted;
    public override void Load(Context c)
    {
        base.Load(c);
        voice = new DynamicSoundEffectInstance(48000, AudioChannels.Mono);
        voice.BufferNeeded += Fill;
        Fill(this, EventArgs.Empty);
        voice.Play();
    }
    public static float Wave(double cycle, int kind)
    {
        double fraction = cycle - Math.Floor(cycle);
        return (kind % 3) switch
        {
            0 => (float)Math.Sin(fraction * Math.PI * 2),
            1 => fraction < .5 ? .7f : -.7f,
            _ => (float)(fraction * 2 - 1)
        };
    }
    private void Fill(object? sender, EventArgs args)
    {
        while (voice.PendingBufferCount < 3)
        {
            var buffer = new byte[2048 * 2];
            for (int i = 0; i < 2048; i++)
            {
                short sample = (short)(Wave(phase, Mode) * 6000);
                BinaryPrimitives.WriteInt16LittleEndian(buffer.AsSpan(i * 2, 2), sample);
                phase += 440 * Parameter / 48000;
            }
            voice.SubmitBuffer(buffer); submitted++;
        }
    }
    public override void Draw()
    {
        U.Begin(); U.Text(new[] { "SINE", "SQUARE", "SAWTOOTH" }[Mode % 3], 70, 100, Color.White, 4);
        U.Line(new(60, 330), new(900, 330), Color.SlateGray);
        for (int i = 0; i < 800; i++)
            U.Line(new(70 + i, 330 - Wave(i / 100.0, Mode) * 120),
                new(71 + i, 330 - Wave((i + 1) / 100.0, Mode) * 120), Color.Cyan);
        U.Text("BUFFERNEEDED REFILLS THREE PCM BLOCKS", 70, 510, Color.White); U.End();
        Status = $"FREQUENCY {440 * Parameter:F0} HZ  SUBMITTED {submitted}  PENDING {voice.PendingBufferCount}";
    }
    public override void Verify()
    {
        Require(voice.State == SoundState.Playing && submitted > 3, "Dynamic PCM voice did not refill consumed buffers.");
        Require(voice.PendingBufferCount <= 3, "Audio queue grew without a bound.");
    }
    public override void Dispose() { voice.BufferNeeded -= Fill; voice.Stop(); voice.Dispose(); }
}
