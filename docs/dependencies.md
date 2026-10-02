# Dependencies

Use FNA's [.NET 8 project](https://github.com/FNA-XNA/FNA/blob/master/FNA.Core.csproj)
and matching native dependencies. FNA's official
[setup instructions](https://fna-xna.github.io/docs/1%3A-Setting-Up-FNA/)
describe obtaining source and fnalibs. This repository does not patch FNA.

`tools/setup.py --fna-root PATH` uses `PATH/bin/Debug/net8.0/FNA.dll` or
`PATH/bin/Release/net8.0/FNA.dll`. `--fna-dll PATH` selects a prebuilt assembly.
`--native-dir PATH` supplies a directory of native libraries. Linux library
builds in the FNA checkout can also be detected automatically. The system SDL
library is used. If needed, Theorafile is built into `.deps/native/` from the
unchanged source supplied with FNA; all build output stays in this repository.

Effect compilation requires Microsoft's legacy `fxc.exe` (`fx_2_0` effect
profile). On Linux, `build_assets.py` runs it with Wine using an isolated prefix
under `.deps/`. Set `--fxc PATH` and optionally `--xact PATH` for `XactBld3.exe`.
Only our own shaders and XACT source are passed to these tools. Regeneration of
the short Theora clip requires `ffmpeg` with the libtheora encoder.

A native `--vkd3d /path/to/vkd3d-compiler` route is also provided for a compiler
with HLSL-to-FX support. This route is experimental and has not been exercised
here. Older vkd3d packages only support DXBC translation and are insufficient.
See the [vkd3d 2.1 release notes](https://list.winehq.org/hyperkitty/list/wine-announce%40list.winehq.org/2026/8/)
for current FX support; compiling a file still requires its subsequent FNA run.

The XACT fixture does not depend on the proprietary compiler at runtime:
`tools/build_xact_fixture.py` generates the original two-cue settings, sound bank
and streaming PCM wave bank from `assets/streaming-project.json`. It deliberately
supports this fixture only. The binary layout follows FAudio's format readers;
it is not a general XAP compiler. `assets/streaming.xap` describes the equivalent
authoring project and remains pending independent XactBld3 validation.

## Environment observed in this workspace

FNA revision `1358793096d556388be798d24dc3728b2abb10ff`, assembly 26.8.0.0;
.NET SDK 8.0.424; FNA3D/FAudio native builds 0.26.08. The native graphics build
links SDL3, so forcing FNA's SDL2 backend is incompatible. The system SDL3
library is supplied separately. `setup.py` records hashes of copied runtime
files in `.deps/manifest.json`.

`SDL_VIDEODRIVER=offscreen`, `SDL_AUDIODRIVER=dummy`,
`FNA3D_FORCE_DRIVER=OpenGL` and `LIBGL_ALWAYS_SOFTWARE=1` produced actual FNA
runs on Mesa llvmpipe. X11/Xvfb was not usable in this restricted environment.

Effect compilation remains blocked here: the 32-bit Wine launcher is stopped
by restricted syscalls, while 64-bit Wine attempts to create a server directory
under read-only `/run/user/1000`. Local Wine prefixes do not redirect that path.
A locally built, unchanged upstream Wine 10 server did not remove the client
dependency on that directory. No system files or sandbox protections were
changed. A native compiler is not installed, and terminal network access is
restricted, so obtaining it is an outstanding dependency step.

Dependencies are not committed. Do not interpret FNA compatibility as proof
that legacy Microsoft network services remain available.
