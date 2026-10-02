# Verification

## Required checks

1. Release build of the browser and tests with warnings treated as errors.
2. Algorithm tests covering save corruption and migration, input conflicts,
   fixed-step accumulation, terrain seams, physics contacts, and navigation.
3. FNA runs with sample-specific assertions. A render-target readback verifies
   stencil/scissor behavior, independent MRT attachments, and other defining
   outputs rather than just testing that the process stayed alive.
4. PNG captures and visual inspection of representative examples.
5. Honest reporting of external asset or platform requirements.

`tools/verify.py` runs the automated checks, captures outputs in ignored
`.artifacts/`, and writes a machine-readable summary. Its default Linux run
uses SDL3 offscreen/dummy drivers and Mesa software rendering. Add
`--interactive` for real display/audio. It does not require NuGet test packages.

## Latest results

Recorded on 2026-10-02 using .NET SDK 8.0.424 and FNA 26.8.0.0, with OpenGL
4.5 compatibility on Mesa 25.0.7 / llvmpipe (LLVM 19.1.7).

```sh
python3 tools/verify.py
```

- Release build: zero warnings and zero errors, warnings treated as errors.
  The solution also passes with `-m:1 -p:RestoreDisableParallel=true`; default
  parallel solution builds fail without a diagnostic in this restricted host.
- Pure algorithms: **53 checks passed**, including malformed/corrupt saves,
  migration and backup preservation, invalid/conflicting input bindings,
  timing across five frame rates, waveform shapes, terrain seams, contact
  friction and joint settling, navigation clearance and behavior transitions.
- **14 FNA samples passed**: scissor-clipping, stencil-mask, dynamic-cubemap,
  terrain-lod, video-texture, media-library (portable portion), procedural-audio,
  xact-streaming (runtime portion), reliable-save-game, content-streaming,
  input-rebinding, game-loop-timing, rigid-body-physics, navigation-behavior.
- FNA runs capture PNGs after 150 frames; XACT runs 270 frames to exceed its
  three-second source wave duration. Representative captures were inspected,
  including clipping, reflection, terrain, video, physics and navigation.
- Seven C# implementations could not run because Scene/Post/Volume/Water
  effect bytecode is missing: MRT, volume texture, deferred lighting, HDR,
  AO, cascaded shadows and water. Shader behavior is **unverified**.
- Gamer profile/friends has no FNA implementation because its provider is absent.

The result is **16 passing checks** (build + algorithms + 14 runs), **zero
failures**, **eight blocked entries**, and explicitly recorded partial coverage.
Exit code **2** is intentional: it prevents this checkpoint from appearing to
be a complete 22-topic success. Per-run logs, screenshots, dependency hashes
and the machine-readable report are in ignored `.artifacts/`.

## Practical limits

MediaLibrary collections are unsupported. The installed FAudio visualization
API is a stub; the sample reports this and does not draw invented spectra.
The XACT runtime path uses real banks, native cue variables and a volume RPC;
its matching Microsoft XAP authoring project has not been compiler-validated.
FAudio's WaveBank.IsInUse bookkeeping is incomplete in this build, so that flag
is diagnostic rather than a playback assertion. Dummy audio proves state and
buffer activity, not audible quality. Physical controller interaction and
visible-window manual controls remain manual checks.

The physics solver is educational: oriented rectangles and a pin constraint,
without continuous collision detection or a production sleeping/warm-start
system. Terrain edge stitching preserves geometry, but LOD changes do not
geomorph. Navigation uses convex rectangles and exact segment pruning rather
than a general polygon triangulator/funnel implementation.
