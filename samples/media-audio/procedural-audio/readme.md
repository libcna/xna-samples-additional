# Procedural Audio

DynamicSoundEffectInstance consumes generated mono PCM16 sine, square and sawtooth blocks. BufferNeeded maintains a bounded queue and a continuous phase.

## Run and controls

```sh
python3 tools/run.py --sample procedural-audio
python3 tools/run.py --headless --sample procedural-audio --frames 150 --verify --capture .artifacts/procedural-audio.png
```

Run these commands from the repository root. Space switches waveform; Up/Down change frequency. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Waveform checks exercise phase and shape; FNA must consume and refill blocks while the voice remains playing.

## Status and boundaries

The defining checks and a bounded run on FNA have passed. See [verification](../../../docs/verification.md) for environment and limitations.
