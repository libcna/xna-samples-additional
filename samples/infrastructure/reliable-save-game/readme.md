# Reliable Save Game

A versioned JSON envelope includes payload integrity and migration. A flushed temporary file atomically replaces the primary; a valid backup recovers corruption.

## Run and controls

```sh
python3 tools/run.py --sample reliable-save-game
python3 tools/run.py --headless --sample reliable-save-game --frames 150 --verify --capture .artifacts/reliable-save-game.png
```

Run these commands from the repository root. S saves; L loads; C corrupts the primary; F injects pre-replace failure; V writes a legacy save; Up/Down change coins. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Roundtrip, interruption preservation, checksum validation, migration and backup recovery are tested in isolated temporary storage.

## Status and boundaries

The defining checks and a bounded run on FNA have passed. See [verification](../../../docs/verification.md) for environment and limitations.
