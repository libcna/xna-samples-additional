# Navigation and Behavior

Vertical decomposition makes convex free-space polygons around inflated obstacles. Portal adjacency supports A*, then exact segment tests prune corners. A reactive behavior tree chooses chase, remembered search or patrol.

## Run and controls

```sh
python3 tools/run.py --sample navigation-behavior
python3 tools/run.py --headless --sample navigation-behavior --frames 150 --verify --capture .artifacts/navigation-behavior.png
```

Run these commands from the repository root. Space hides/shows the target; clicking free space moves the target. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Routes must preserve obstacle clearance; disconnected regions reject paths; behavior transitions and memory expiry are tested.

## Status and boundaries

The defining checks and a bounded run on FNA have passed. See [verification](../../../docs/verification.md) for environment and limitations.
