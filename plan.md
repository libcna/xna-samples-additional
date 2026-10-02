# Implementation plan

## Scope and status

This repository adds all 22 topics proposed during the sample-collection audit.
Existing examples such as replay, quadtree collision, basic network prediction,
skinning, and bloom are not recreated. All new material is English.

**Planned** means designed but not implemented. **Implemented** means source
exists. **Verified** requires a Release build, defining behavior checks, and a
real FNA run. **Partial** identifies a useful implementation that does not yet
cover every acceptance criterion. **Platform blocked** preserves a topic whose
original API is unavailable in FNA; it is never counted as completed coverage.

## Group A: Rendering API foundations

| ID / sample | Demonstration | Acceptance criteria | Dependencies |
|---|---|---|---|
| A01 `scissor-clipping` | A scrolling list inside nested rectangular panels. | Intersect child and parent clips; verify pixels outside a clip stay unchanged; restore the previous rasterizer and scissor state. | Shared browser |
| A02 `stencil-mask` | A circular window into a colored scene and an object outline. | Write a stencil mask, test it in a separate pass, expose mask versus result modes, and verify inside/outside pixels. | Depth24Stencil8 target |
| A03 `multiple-render-targets` | Color, normal, and depth attachments displayed side by side. | Produce distinct attachments in one draw using multiple shader outputs; verify readbacks independently. Restoring a prior target list alone does not count. | HiDef, effect compiler |
| A04 `dynamic-cubemap` | A reflective object surrounded by moving colored objects. | Render all six cube faces with correct camera directions; compare static/frozen and live updates; show diagnostic faces. | HiDef, shared 3D geometry |
| A05 `volume-texture` | A procedural density volume with adjustable slices and a volume view. | Create and sample a real Texture3D; demonstrate interpolation along its third axis; show slices and accumulated density. | HiDef, custom shader |

## Group B: Advanced rendering techniques

| ID / sample | Demonstration | Acceptance criteria | Dependencies |
|---|---|---|---|
| B01 `deferred-lighting` | A scene lit by many moving point lights. | Separate geometry and lighting passes; reconstruct lighting from color/normal/depth buffers; show buffers and vary light count. | A03 |
| B02 `hdr-tone-mapping` | Bright lights in a dark environment with exposure adaptation. | Retain values above 1 in a floating-point render target; expose manual/automatic exposure, tone mapping, and a reference LDR view. Bloom alone does not count. | HiDef, float target |
| B03 `ambient-occlusion` | Corner and contact darkening around boxes and recesses. | Sample depth and normals in screen space; expose radius and an AO-only view; apply an edge-aware blur. | A03 |
| B04 `cascaded-shadow-maps` | Directional shadows over a landscape at different distances. | Partition camera depth; render multiple light-space maps; select cascades per fragment; expose bias, filtering, and colored cascade diagnostics. | Shared terrain and shader |
| B05 `water-rendering` | A pool with objects above and below an animated surface. | Separate clipped reflection/refraction passes; Fresnel composition and animated normals; show intermediate targets. | RenderTarget2D, custom shader |
| B06 `terrain-lod` | A large terrain composed of independently detailed tiles. | Distance-dependent mesh density, crack-free shared boundaries, frustum culling, wireframe, and triangle counts. | Procedural terrain |

## Group C: Media, audio, and platform services

| ID / sample | Demonstration | Acceptance criteria | FNA boundary |
|---|---|---|---|
| C01 `video-texture` | A short video in a 2D panel and on a 3D screen. | Real VideoPlayer playback and GetTexture; pause/resume, looping, volume, and aspect ratio. | Loose Theora loading uses Video.FromUriEXT; document the XNA Content.Load alternative. |
| C02 `media-library` | Browse songs/albums, manage a queue, and visualize music. | Original MediaLibrary collections plus MediaPlayer playback/queue and visualization on supported platforms. | FNA collections throw NotImplementedException; this FAudio build's visualization is unimplemented. Local playlist playback is verified, while native library and visualization coverage remain blocked. |
| C03 `gamer-profile-friends` | Signed-in profile, gamer picture, and friends. | Async requests, sign-in/out transitions, cancellation and failure UI against a real provider. | FNA has no operational Xbox Live provider. Keep the original-XNA design and requirements; do not substitute mock data for service coverage. |
| C04 `procedural-audio` | A small waveform synthesizer. | Generate PCM, refill DynamicSoundEffectInstance from BufferNeeded, change waveform/frequency, count submitted buffers and release the voice. | Dummy audio may exercise submission, but does not establish audible quality. |
| C05 `xact-streaming` | Streaming music, cue controls, and category mixing. | Include an original XACT project, load a real streaming wave bank, expose cue variables/categories and transition layers. | Original native banks can be generated by the narrow fixture builder; matching XAP authoring validation still requires XactBld3. |

## Group D: Game infrastructure

| ID / sample | Demonstration | Acceptance criteria | Dependencies |
|---|---|---|---|
| D01 `reliable-save-game` | Save/load a small game and simulate failures. | Versioned format, migration, payload integrity, atomic replacement, backup fallback, and errors without losing the last valid save. | Isolated sample storage |
| D02 `content-streaming` | Move between rooms while their content loads and unloads. | Background CPU preparation, bounded main-thread GPU work, cancellation of stale requests, bounded residency, and ownership/disposal counters. | Shared browser |
| D03 `input-rebinding` | Rebind actions to keyboard/gamepad controls. | Capture at runtime, detect conflicts, restore defaults, persist/reload, and retain a way to exit the binding UI. | D01 persistence principles |
| D04 `game-loop-timing` | Compare variable step, fixed step, and interpolated rendering. | Adjustable frame intervals and stalls; fixed update accumulation with a catch-up limit; reproducible simulation distance and interpolation. | Pure timing model |

## Group E: Game algorithms

| ID / sample | Demonstration | Acceptance criteria | Dependencies |
|---|---|---|---|
| E01 `rigid-body-physics` | Falling boxes, contact friction, impulses, and a joint. | Collision detection and iterative impulse response, rotation, stacking, friction, and a constraint; expose contact diagnostics. | Fixed timestep, pure physics model |
| E02 `navigation-behavior` | Agents patrolling, chasing, and searching around obstacles. | A navigation mesh, adjacency search, a traversable path, and a visible behavior tree with changing decisions. A tile-grid A* alone does not cover navigation meshes. | Pure navigation and behavior models |

## Delivery order

1. Repository, dependency records, browser, generated assets, bounded run mode.
2. Group A, verifying actual FNA rendering before expanding the shared renderer.
3. Group D and pure algorithm checks; Group C portable media and audio.
4. Group B, building on verified render targets, plus Group E.
5. Final whole-suite runs, screenshot review, documentation, and commits.

Each sample gets a directory, focused source, an English readme with controls,
and defining-behavior checks. Shared code must not hide the technique being
taught. Status updates must cite actual runs and explain limitations.

## Progress

- Repository initialized on `main`; MIT license and supporting files created.
- All 22 topics have grouped specifications and individual English readmes.
- 21 sample implementations exist, plus the provider-blocked design.
- 14 sample runs passed on FNA, including two partial topics. There are 12
  Verified topics, two Partial topics, seven Implemented topics awaiting
  effect compilation, and one Platform blocked topic.
- Release build: zero warnings/errors. Algorithm suite: 53 passing checks.
  Whole-suite verification: 16 passed (build, algorithms, 14 runs), zero failed,
  eight blocked. See [verification evidence](docs/verification.md).

| Sample | Current status |
|---|---|
| `scissor-clipping` | Verified on FNA |
| `stencil-mask` | Verified on FNA |
| `multiple-render-targets` | Implemented; effect compilation blocked |
| `dynamic-cubemap` | Verified on FNA |
| `volume-texture` | Implemented; effect compilation blocked |
| `deferred-lighting` | Implemented; effect compilation blocked |
| `hdr-tone-mapping` | Implemented; effect compilation blocked |
| `ambient-occlusion` | Implemented; effect compilation blocked |
| `cascaded-shadow-maps` | Implemented; effect compilation blocked |
| `water-rendering` | Implemented; effect compilation blocked |
| `terrain-lod` | Verified on FNA |
| `video-texture` | Verified on FNA |
| `media-library` | Partial; portable FNA run passed |
| `gamer-profile-friends` | Platform blocked |
| `procedural-audio` | Verified on FNA |
| `xact-streaming` | Partial; runtime FNA run passed |
| `reliable-save-game` | Verified on FNA |
| `content-streaming` | Verified on FNA |
| `input-rebinding` | Verified on FNA |
| `game-loop-timing` | Verified on FNA |
| `rigid-body-physics` | Verified on FNA |
| `navigation-behavior` | Verified on FNA |

The current Wine environment cannot compile the original effect sources;
seven entries have not passed shader or FNA behavior checks. Obtaining a
working compiler and validating their output is required before they become
Verified. [NEXT.md](NEXT.md) records the exact outstanding steps. This
checkpoint does not mark the complete 22-topic objective as achieved.
