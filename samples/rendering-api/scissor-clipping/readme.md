# Scissor Clipping

Nested scrolling panels intersect parent and child scissor rectangles. Draw state is restored after the clipping pass.

## Run and controls

```sh
python3 tools/run.py --sample scissor-clipping
python3 tools/run.py --headless --sample scissor-clipping --frames 150 --verify --capture .artifacts/scissor-clipping.png
```

Run these commands from the repository root. Space pauses scrolling; Up/Down change the shared parameter. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Pixel readbacks check the untouched outer corners and the nested parent region.

## Status and boundaries

The defining checks and a bounded run on FNA have passed. See [verification](../../../docs/verification.md) for environment and limitations.
