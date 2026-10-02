# Game Loop Timing

Three lanes compare variable integration, fixed updates and interpolation. A simulated render rate and injected stalls expose catch-up limits and dropped time.

## Run and controls

```sh
python3 tools/run.py --sample game-loop-timing
python3 tools/run.py --headless --sample game-loop-timing --frames 150 --verify --capture .artifacts/game-loop-timing.png
```

Run these commands from the repository root. Space injects a stall; Up/Down adjust simulated frame rate. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Ten-second simulations at five render rates preserve distance; interpolation stays bounded and catch-up cannot spiral.

## Status and boundaries

The defining checks and a bounded run on FNA have passed. See [verification](../../../docs/verification.md) for environment and limitations.
