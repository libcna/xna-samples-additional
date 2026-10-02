# HDR Tone Mapping

A HalfVector4 target preserves bright values above one. Manual and automatic exposure feed Reinhard/filmic tone mapping, with an LDR clipping reference.

## Run and controls

```sh
python3 tools/run.py --sample hdr-tone-mapping
python3 tools/run.py --headless --sample hdr-tone-mapping --frames 150 --verify --capture .artifacts/hdr-tone-mapping.png
```

Run these commands from the repository root. Space cycles manual Reinhard, filmic, LDR clipping and automatic exposure; Up/Down adjust exposure. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Readback proves HDR values exceed one; changing exposure must affect the tone-mapped image.

## Status and boundaries

The C# source builds, but shader bytecode has not yet been compiled in the current environment. This sample has **not** passed its defining FNA runtime checks.
