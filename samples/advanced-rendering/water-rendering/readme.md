# Water Rendering

Mirrored and regular camera passes clip geometry on opposite sides of a pool. Animated normals and Fresnel weighting combine reflection and refraction.

## Run and controls

```sh
python3 tools/run.py --sample water-rendering
python3 tools/run.py --headless --sample water-rendering --frames 150 --verify --capture .artifacts/water-rendering.png
```

Run these commands from the repository root. Space toggles intermediate-target diagnostics; Up/Down adjust wave strength. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Reflection and refraction readbacks must differ and contain geometry.

## Status and boundaries

The C# source builds, but shader bytecode has not yet been compiled in the current environment. This sample has **not** passed its defining FNA runtime checks.
