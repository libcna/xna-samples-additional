# Deferred Lighting

A geometry pass writes albedo, normals and depth. A separate fullscreen shader reconstructs world position and sums moving point lights.

## Run and controls

```sh
python3 tools/run.py --sample deferred-lighting
python3 tools/run.py --headless --sample deferred-lighting --frames 150 --verify --capture .artifacts/deferred-lighting.png
```

Run these commands from the repository root. Space switches lighting/albedo/normals; Up/Down change light count. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

G-buffer attachments are checked and a different light count must change the rendered image.

## Status and boundaries

The C# source builds, but shader bytecode has not yet been compiled in the current environment. This sample has **not** passed its defining FNA runtime checks.
