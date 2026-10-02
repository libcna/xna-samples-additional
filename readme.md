# XNA Samples Additional

Small educational samples covering gaps in the adjacent Microsoft XNA sample
collection. New code, documentation, and generated assets are in English and
licensed under MIT. The original samples are not modified or relicensed.

The desktop runner targets .NET 8 and **FNA**, with XNA-shaped rendering and
input APIs. Each example has its own directory under `samples/`; a shared
browser provides a window, procedural geometry, a tiny bitmap font, keyboard
controls, and automated screenshot capture.

## Roadmap

See [plan.md](plan.md) for all 22 proposed samples, groups, dependencies,
acceptance criteria, and platform restrictions. Implementation and verification
are tracked separately: compiling a sample does not establish that its effect
works correctly.

Current checkpoint: **14 samples passed real FNA runs**, including two partial
topics; **53 algorithm checks pass**. Seven custom-shader samples build as C#
but await compiled effects. Xbox Live profiles/friends are platform blocked.

## Set up and run

Requirements: .NET 8 SDK, an FNA build, matching FNA3D/FAudio/SDL native
libraries, and a graphics driver. The verified Linux configuration uses SDL3's
offscreen driver and Mesa software rendering. Custom effects use legacy
Direct3D effect bytecode; `.fx` sources are included, and `.fxb` outputs are
kept alongside them once a working compiler is available.

```sh
python3 tools/setup.py --fna-root /path/to/FNA
python3 tools/build_assets.py --media-only
dotnet build src/SampleBrowser -c Release
python3 tools/run.py --list
python3 tools/run.py --sample scissor-clipping
python3 tools/verify.py
```

To build the complete solution in this restricted workspace, use
`dotnet build XnaSamplesAdditional.sln -c Release -m:1 -p:RestoreDisableParallel=true`.
The single-worker solution build passes; the default parallel solution build
returns an unexplained failure here. The project build used by verification
passes normally.

Generated media and streaming banks are already included. `--media-only`
regenerates them using FFmpeg and the original narrow bank builder. To enable
the seven shader examples, also run `build_assets.py --fxc /path/to/fxc.exe`
in an environment with working legacy effect compilation.

`setup.py` copies an existing FNA assembly and native libraries into ignored
`.deps/`. It never modifies the supplied FNA checkout. See
[docs/dependencies.md](docs/dependencies.md) for configuration and reproducible
dependency records, and [docs/verification.md](docs/verification.md) for test
commands and the latest actual results.

Browser controls: **Left/Right** change examples, **Space** changes the active
example's mode, **Up/Down** change its parameter, **R** resets it, **Esc** exits.
Individual readmes document extra controls. `--frames N --capture file.png`
provides a bounded run suitable for automation; `--verify` runs sample-specific
checks before reporting success. Failures return a nonzero exit status.
For automation, add `--headless --frames 150`; SDL dummy audio exercises the
audio APIs without establishing audible quality. The browser renders to a
preserved 960x640 target so captures include the entire scene and controls.

The verification script returns **1** for a failed check, **2** for remaining
asset/platform coverage gaps, and **0** only for a complete check set. Use
`--allow-incomplete` to accept a passing runnable subset while retaining its
explicit gaps in `.artifacts/verification.json`.

## Platform limits

FNA does not implement the original device-wide `MediaLibrary` collections or
Xbox Live profiles/friends. The plan preserves these topics and distinguishes
portable functionality from native service coverage. No mock service is
presented as working Xbox Live. Video uses FNA's Theora decoder and a small
locally generated clip; loading a loose video uses an explicitly documented
FNA extension. XACT uses original generated streaming banks; its matching XAP
project is included, with Microsoft authoring validation still pending.

## License

[MIT](LICENSE) applies to this repository's new work. FNA and its native
dependencies retain their own licenses. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
