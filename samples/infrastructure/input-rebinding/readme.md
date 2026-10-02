# Input Rebinding

An action map captures keyboard/gamepad bindings, rejects conflicts, resets defaults and atomically persists JSON.

## Run and controls

```sh
python3 tools/run.py --sample input-rebinding
python3 tools/run.py --headless --sample input-rebinding --frames 150 --verify --capture .artifacts/input-rebinding.png
```

Run these commands from the repository root. Up/Down select an action; Enter starts capture; Esc cancels capture; S/L save/load; Space restores defaults. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Conflict rejection preserves the old binding, reserved exit keys are protected and malformed persisted maps are rejected.

## Status and boundaries

The defining checks and a bounded run on FNA have passed. See [verification](../../../docs/verification.md) for environment and limitations.
