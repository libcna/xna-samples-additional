# Third-party notices

The new samples are MIT licensed. Runtime dependencies are supplied separately
in ignored `.deps/` and remain under their original licenses:

| Dependency | License | Upstream |
|---|---|---|
| FNA | Microsoft Public License | https://github.com/FNA-XNA/FNA |
| FNA3D | zlib | https://github.com/FNA-XNA/FNA3D |
| FAudio | zlib | https://github.com/FNA-XNA/FAudio |
| SDL | zlib | https://libsdl.org/ |
| Theorafile | zlib, with bundled codec notices | https://github.com/FNA-XNA/Theorafile |

The Microsoft DirectX SDK's `fxc.exe` and `XactBld3.exe` are optional external
authoring tools. They are not committed or redistributed with this repository.
FFmpeg is an optional external tool used to generate the original demonstration
video. Generated waveforms, video test patterns, geometry, and bitmap glyphs
originate in this repository. Dependency hashes and upstream revision IDs are
recorded by `tools/setup.py` in `.deps/manifest.json`.

An ignored Wine 10 source/server build was used while investigating effect
compilation. It remains under Wine's LGPL notices and is not part of the new
MIT sample code. [Wine source](https://github.com/wine-mirror/wine).
The optional vkd3d compiler likewise retains its upstream LGPL license.
