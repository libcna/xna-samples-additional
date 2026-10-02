# Dynamic Cubemap

Six camera passes capture moving geometry in a RenderTargetCube. EnvironmentMapEffect reflects it on a sphere; six diagnostic faces appear below.

## Run and controls

```sh
python3 tools/run.py --sample dynamic-cubemap
python3 tools/run.py --headless --sample dynamic-cubemap --frames 150 --verify --capture .artifacts/dynamic-cubemap.png
```

Run these commands from the repository root. Space freezes/resumes cube updates while the scene continues moving. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Opposing cube faces differ and the reflected scene contains detailed output.

## Status and boundaries

The defining checks and a bounded run on FNA have passed. See [verification](../../../docs/verification.md) for environment and limitations.
