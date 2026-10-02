# Ambient Occlusion

Depth and normals drive a 16-sample screen-space occlusion pass followed by a depth-aware bilateral filter.

## Run and controls

```sh
python3 tools/run.py --sample ambient-occlusion
python3 tools/run.py --headless --sample ambient-occlusion --frames 150 --verify --capture .artifacts/ambient-occlusion.png
```

Run these commands from the repository root. Space switches composition, filtered AO and raw AO; Up/Down adjust radius. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Raw AO must contain both open and occluded regions, and composition must be nonuniform.

## Status and boundaries

The C# source builds, but shader bytecode has not yet been compiled in the current environment. This sample has **not** passed its defining FNA runtime checks.
