# Implementation handoff

Recorded on 2026-10-02. This document describes the last verified implementation
checkpoint and the remaining work; it does not claim that all 22 topics are
complete.

## Objective and repository

The user requested additional samples for gaps in the existing XNA collection,
organized into groups and progressively implemented and tested on FNA. All new
code, documentation, sample UI, and commit messages must be English.

- Repository: `/rv/tmp/XNAGameStudio/Samples/xna-samples-additional`.
- Branch: `main`.
- Implementation checkpoint: `565f3e7` (`feat: add additional XNA sample suite
  and FNA verification`). This handoff is a subsequent documentation change.
- Independent Git repository with an MIT license, README, ignore rules,
  contribution guidance, dependency records, and a shared sample browser.
- The original sample collection and the FNA checkout were not modified.

See [plan.md](plan.md) for all 22 specifications and acceptance criteria,
[NEXT.md](NEXT.md) for the short work queue, and
[docs/coverage-audit.md](docs/coverage-audit.md) for the original coverage audit.

## Current status

All 22 topics have English documentation. Source implementations exist for 21;
the gamer profile/friends topic has a design and provider requirements only.

| Status | Count | Meaning at this checkpoint |
|---|---:|---|
| Verified | 12 | Defining checks and a real FNA run passed. |
| Partial | 2 | Portable media-library and XACT runtime portions passed; some acceptance criteria remain open. |
| Implemented | 7 | C# source builds, but missing effect bytecode prevents shader/FNA verification. |
| Platform blocked | 1 | Gamer profile/friends requires an operational service provider. |

The browser, generated original media, persistence, streaming, input rebinding,
timing, terrain LOD, physics, and navigation implementations are in place.
The runnable set also includes clipping, stencil masking, dynamic cubemaps,
video playback, procedural audio, local music playback, and XACT streaming.

Last recorded verification on 2026-10-02:

- Release build: zero warnings and zero errors, with warnings treated as errors.
- Pure algorithm suite: 53 passing checks.
- Real FNA runs: 14 passed, including the two Partial topics.
- Whole-suite summary: 16 passed (build + algorithms + 14 runs), zero failed,
  eight blocked entries. Partial coverage is reported separately.
- Runs used 150 frames per sample, or 270 frames for XACT. Representative PNGs
  were inspected, including clipping, cubemaps, terrain, video, physics, and
  navigation.

Logs, captures, runtime hashes, and `verification.json` are in ignored
`.artifacts/` and `.deps/`. A fresh clone will not contain these local artifacts.
See [docs/verification.md](docs/verification.md) for evidence and limitations.
This documentation update did not rerun the application or tests.

## Active blockers

### Effect compilation

The original `assets/Scene.fx`, `Post.fx`, `Volume.fx`, and `Water.fx` sources
exist, but their `.fxb` output is missing. This blocks these seven samples:

| Sample | Required bytecode |
|---|---|
| `multiple-render-targets` | `Scene.fxb`, `Post.fxb` |
| `volume-texture` | `Volume.fxb` |
| `deferred-lighting` | `Scene.fxb`, `Post.fxb` |
| `hdr-tone-mapping` | `Post.fxb` |
| `ambient-occlusion` | `Scene.fxb`, `Post.fxb` |
| `cascaded-shadow-maps` | `Scene.fxb` |
| `water-rendering` | `Scene.fxb`, `Water.fxb` |

A successful C# build does not validate the HLSL or these rendering techniques.
Compiler errors, runtime incompatibilities, and visual defects may still need
fixes after the bytecode becomes available.

The Microsoft DirectX June 2010 SDK is available locally at
`/rv/tmp/samples/_tools/directx-sdk-june-2010/DXSDK_Jun10.exe`. Extracted compiler
files also exist there and under ignored `.deps/directx/`. However:

- The 32-bit Wine launcher is stopped by restricted syscalls.
- The 64-bit Wine path reports `sock_init: ERROR in sock_check_pollhup()` and
  attempts to create its server directory under read-only `/run/user/1000/wine/`.
- Local `WINEPREFIX` and `XDG_RUNTIME_DIR` settings did not redirect that path.
- An unchanged upstream Wine 10 server was built locally in ignored `.deps/`,
  but using it did not remove the installed client's runtime-path dependency.
- A native HLSL-to-FX compiler is not installed. Terminal network access is
  restricted, and some compiler build prerequisites are missing.

Do not repeat the same Wine prefix/server attempts without new evidence. No
system files or sandbox protections were changed. `tools/build_assets.py`
supports `--fxc` and an experimental `--vkd3d` route; the latter has not been
tested here and requires a compiler with HLSL-to-FX support. See
[docs/dependencies.md](docs/dependencies.md).

### Windows VM availability

The user explicitly reported that a Windows 7 VM in VirtualBox cannot currently
be provided: another AI agent is using an Android emulator, and VirtualBox
cannot run concurrently in the user's current setup. Treat this as an
availability constraint for this workspace. Do not launch VirtualBox, stop the
Android emulator, or interfere with the other agent's work.

The next compiler step should use a working native compiler or another
already-available Windows machine/toolchain, with the resulting four `.fxb`
files brought back here. A VM is not a prerequisite for continuing via those
alternatives. A Windows VM alone would not supply an Xbox Live service provider.

### Platform APIs and authoring validation

- `media-library`: FNA's native library collections throw
  `NotImplementedException`; this FAudio build's visualization API is a stub.
  Local playlist playback and queue behavior passed, but library discovery and
  real visualization remain uncovered. Do not substitute invented spectra.
- `gamer-profile-friends`: FNA has no operational Xbox Live provider. Real
  sign-in/profile/friends behavior cannot be verified in this runtime; mock data
  would not satisfy its acceptance criteria.
- `xact-streaming`: real generated XGS/XSB/streaming XWB banks passed runtime
  checks, including cue variables, a volume RPC, categories, and transitions.
  The matching original `assets/streaming.xap` still needs independent validation
  with Microsoft's `XactBld3`. The fixture builder is not a general XAP compiler.

## Remaining work, in order

1. Obtain a working effect compiler without relying on the unavailable
   VirtualBox VM. Compile all four original effects to the `fx_2_0` effect
   format and place their `.fxb` files in `assets/`.
2. Rebuild and run all seven graphics samples with their defining checks and
   captures. Inspect every mode and intermediate buffer, then fix any shader,
   rendering, or verification findings. Keep their status Implemented until
   actual checks and FNA runs pass.
3. Rerun the whole suite after implementation changes and review the resulting
   report and PNGs.
4. Validate `streaming.xap` with `XactBld3`; compare the authored output's
   runtime behavior with the generated fixture.
5. Check audible procedural/XACT audio, physical controller capture/rebinding,
   and visible-window controls. Dummy audio verifies state and submission,
   not sound quality. The current video fixture is silent.
6. Complete native media-library/visualization and gamer-service criteria only
   when an operational supporting runtime/provider becomes available. Otherwise
   preserve their explicit Partial/Platform blocked status.
7. Update `plan.md`, `assets/catalog.json`, `docs/verification.md`, `NEXT.md`,
   and this handoff from actual evidence, then commit verified milestones.

The earlier rough estimate was 6–12 hours for the remaining graphics work
after obtaining a working compiler. It is not a calendar completion promise;
compiler acquisition and platform-service availability remain unresolved.

## Resume commands and environment

Run from the repository root:

```sh
python3 tools/setup.py --fna-root /rv/tmp/FNA
dotnet build XnaSamplesAdditional.sln -c Release -m:1 -p:RestoreDisableParallel=true -p:NuGetAudit=false
python3 tools/run.py --list
python3 tools/run.py --headless --sample dynamic-cubemap --frames 150 --verify --capture .artifacts/dynamic-cubemap.png
python3 tools/verify.py
```

Use `python3 tools/build_assets.py --media-only` if the original media needs
regeneration. Once a suitable native compiler is available, the experimental
route is `python3 tools/build_assets.py --vkd3d /path/to/vkd3d-compiler`.
Successful asset compilation must be followed by FNA verification.

The tested runtime is .NET SDK 8.0.424, FNA 26.8.0.0 at revision
`1358793096d556388be798d24dc3728b2abb10ff`, and FNA3D/FAudio 0.26.08.
The headless runner sets SDL offscreen/dummy drivers, OpenGL, and software Mesa
rendering. This worked with Mesa 25.0.7 / llvmpipe. X11/Xvfb did not work in
this restricted environment. Do not force `FNA_PLATFORM_BACKEND=SDL2`: the
native graphics library is linked against SDL3.

Default parallel solution builds fail here without a useful diagnostic; the
serial command above passed. Dependencies and build output stay under ignored
local directories. Do not modify `/rv/tmp/FNA` or the original samples.

`tools/verify.py` returns 2 when coverage is incomplete, 1 for failed checks,
and 0 for complete success. Its explicit `--allow-incomplete` option permits a
runnable-subset success code while still recording gaps; it does not establish
complete coverage. The current default exit code 2 is expected.
