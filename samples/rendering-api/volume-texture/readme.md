# Volume Texture

A real Texture3D stores a generated density field. A shader shows interpolated slices or accumulates 48 samples through the volume.

## Run and controls

```sh
python3 tools/run.py --sample volume-texture
python3 tools/run.py --headless --sample volume-texture --frames 150 --verify --capture .artifacts/volume-texture.png
```

Run these commands from the repository root. Space toggles slice/volume modes; Up/Down change the slice position. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Changing slice depth must change output pixels.

## Status and boundaries

The C# source builds, but shader bytecode has not yet been compiled in the current environment. This sample has **not** passed its defining FNA runtime checks.
