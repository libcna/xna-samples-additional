# Cascaded Shadow Maps

Three camera-depth intervals get separate directional-light depth maps. A fragment chooses its cascade and evaluates 3x3 PCF shadow comparisons.

## Run and controls

```sh
python3 tools/run.py --sample cascaded-shadow-maps
python3 tools/run.py --headless --sample cascaded-shadow-maps --frames 150 --verify --capture .artifacts/cascaded-shadow-maps.png
```

Run these commands from the repository root. Space colors cascades; Up/Down adjust shadow bias. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Each map must contain geometry; fitted light transforms must be distinct.

## Status and boundaries

The C# source builds, but shader bytecode has not yet been compiled in the current environment. This sample has **not** passed its defining FNA runtime checks.
