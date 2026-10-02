# Rigid Body Physics

Oriented rectangle SAT and convex clipping build two-point manifolds. Iterative accumulated impulses resolve rotation, restitution and Coulomb friction. A pin joint constrains a pendulum.

## Run and controls

```sh
python3 tools/run.py --sample rigid-body-physics
python3 tools/run.py --headless --sample rigid-body-physics --frames 150 --verify --capture .artifacts/rigid-body-physics.png
```

Run these commands from the repository root. Space spawns a box; Up/Down adjust friction; C toggles contact markers. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Tests cover separated/contacting boxes, torque, settling, friction and joint stability; FNA checks finite motion and active contacts.

## Status and boundaries

The defining checks and a bounded run on FNA have passed. See [verification](../../../docs/verification.md) for environment and limitations.
