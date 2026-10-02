# Video Texture

A real VideoPlayer decodes an original generated Theora clip. GetTexture supplies a 2D panel and a perspective 3D screen.

## Run and controls

```sh
python3 tools/run.py --sample video-texture
python3 tools/run.py --headless --sample video-texture --frames 150 --verify --capture .artifacts/video-texture.png
```

Run these commands from the repository root. Space pauses/resumes; L toggles looping; Up/Down adjust volume. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Decoded frames must be varied, playback must advance, and pause/resume states must change.

## Status and boundaries

The defining checks and a bounded run on FNA have passed. See [verification](../../../docs/verification.md) for environment and limitations.
