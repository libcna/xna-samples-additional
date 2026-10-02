namespace AdditionalSamples;

public sealed class FixedStepModel
{
    public const double Step = 1.0 / 60;
    public double Position { get; private set; }
    public double PreviousPosition { get; private set; }
    public double Accumulator { get; private set; }
    public double DroppedTime { get; private set; }
    public int LastUpdates { get; private set; }
    public double Alpha => Accumulator / Step;
    public double Interpolated => PreviousPosition + (Position - PreviousPosition) * Alpha;
    public void Advance(double elapsed, int maxUpdates = 8)
    {
        if (!double.IsFinite(elapsed) || elapsed < 0 || maxUpdates < 1) throw new ArgumentOutOfRangeException(nameof(elapsed));
        Accumulator += elapsed;
        LastUpdates = 0;
        while (Accumulator + 1e-12 >= Step && LastUpdates < maxUpdates)
        {
            PreviousPosition = Position; Position += 90 * Step;
            Accumulator -= Step; LastUpdates++;
        }
        if (Accumulator >= Step)
        {
            double remainder = Accumulator % Step;
            DroppedTime += Accumulator - remainder; Accumulator = remainder;
        }
        Accumulator = Math.Max(0, Accumulator);
    }
}
