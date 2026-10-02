# Content Streaming

Background tasks prepare room pixels. Main-thread uploads are bounded to one per update. Canceled generations are discarded and residency keeps only the current room and neighbors.

## Run and controls

```sh
python3 tools/run.py --sample content-streaming
python3 tools/run.py --headless --sample content-streaming --frames 150 --verify --capture .artifacts/content-streaming.png
```

Run these commands from the repository root. Space moves forward; Up moves backward. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Defined checks wait for CPU tasks, count GPU uploads/disposals and prove bounded residency after room changes.

## Status and boundaries

The defining checks and a bounded run on FNA have passed. See [verification](../../../docs/verification.md) for environment and limitations.
