# Media Library Portable Portion

Local generated songs demonstrate MediaPlayer playback and queue control. The sample reports unavailable system MediaLibrary collections and unavailable FAudio visualization instead of inventing data.

## Run and controls

```sh
python3 tools/run.py --sample media-library
python3 tools/run.py --headless --sample media-library --frames 150 --verify --capture .artifacts/media-library.png
```

Run these commands from the repository root. Space selects the next song; P pauses/resumes; Up/Down adjust volume. Left/Right switch samples; R reloads the active sample; Esc exits the browser.

## Defining checks

Native playback/queue state is checked. Visualization is checked only if the runtime enables the real API.

## Status and boundaries

**Partial coverage:** FNA system-library collections throw NotImplementedException. The installed FAudio visualization entry points are unimplemented and return no data. The local playlist is runnable; native library/visualization coverage remains blocked.
