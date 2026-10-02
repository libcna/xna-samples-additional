# Terrain LOD

Convex terrain tiles choose four mesh densities by distance. Fine boundary vertices interpolate the neighboring coarse edge. Frustum culling and a wireframe view expose the result.

## Run and controls

```sh
python3 tools/run.py --sample terrain-lod
python3 tools/run.py --headless --sample terrain-lod --frames 150 --verify --capture .artifacts/terrain-lod.png
```

Run these commands from the repository root. Space toggles wireframe; Up/Down adjust camera distance. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Pure tests check triangle counts and coincident seams; FNA verifies visible terrain and a reduced triangle count.

## Status and boundaries

The defining checks and a bounded run on FNA have passed. See [verification](../../../docs/verification.md) for environment and limitations.
