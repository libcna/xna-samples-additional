# Stencil Mask

A circular stencil write pass controls normal/inverted scene masks and an outline pass.

## Run and controls

```sh
python3 tools/run.py --sample stencil-mask
python3 tools/run.py --headless --sample stencil-mask --frames 150 --verify --capture .artifacts/stencil-mask.png
```

Run these commands from the repository root. Space cycles normal mask, inverse mask and outline. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Inside/outside pixels are compared after separate stencil write and test passes.

## Status and boundaries

The defining checks and a bounded run on FNA have passed. See [verification](../../../docs/verification.md) for environment and limitations.
