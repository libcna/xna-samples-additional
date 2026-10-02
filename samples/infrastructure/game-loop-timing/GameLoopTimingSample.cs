using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AdditionalSamples;

[SampleInfo("game-loop-timing", "Game Loop Timing - Fixed Step and Interpolation")]
public sealed class GameLoopTimingSample : Sample
{
    private readonly FixedStepModel fixedStep = new();
    private double variable, budget;
    private bool stalled;
    public override string Controls => "UP/DOWN SIMULATED FPS  SPACE ADD 350MS STALL  R RESET";
    public override void Update(float dt)
    {
        Time += dt;
        if (C.Pressed(Keys.Up)) Parameter = Math.Min(4, Parameter + .25f);
        if (C.Pressed(Keys.Down)) Parameter = Math.Max(.25f, Parameter - .25f);
        budget += dt;
        double interval = 1 / (30 * Parameter);
        if (C.Pressed(Keys.Space)) { budget += .35; stalled = true; }
        if (budget >= interval)
        {
            variable += 90 * budget; fixedStep.Advance(budget); budget = 0;
        }
    }
    public override void Draw()
    {
        U.Begin();
        double[] positions = { variable, fixedStep.Position, fixedStep.Interpolated };
        string[] labels = { "VARIABLE STEP", "FIXED STEP", "FIXED + INTERPOLATION" };
        for (int i = 0; i < 3; i++)
        {
            int y = 120 + i * 135;
            U.Text(labels[i], 65, y, Color.White);
            U.Box(new Rectangle(65, y + 55, 820, 3), Color.SlateGray);
            U.Box(new Rectangle(65 + (int)(positions[i] % 780), y + 25, 30, 30), i == 2 ? Color.Cyan : Color.Coral);
        }
        U.End();
        Status = $"FPS {30 * Parameter:F0}  UPDATES {fixedStep.LastUpdates}  DROPPED {fixedStep.DroppedTime:F3}S  STALL {stalled}";
    }
    public override void Verify()
    {
        var model = new FixedStepModel();
        for (int i = 0; i < 30; i++) model.Advance(1.0 / 30);
        Require(Math.Abs(model.Position - 90) < 1e-6, "Fixed updates depend on render frequency.");
        model.Advance(.5); Require(model.LastUpdates == 8 && model.DroppedTime > .3, "Catch-up limit failed.");
        Require(model.Alpha >= 0 && model.Alpha < 1, "Interpolation alpha escaped its range.");
    }
}
