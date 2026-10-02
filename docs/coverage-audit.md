# Baseline sample-collection audit

The initial read-only inventory, before creating this repository, found 153
package directories and 2,689 C#/VB/HLSL/C++ source files, excluding build
output. C#/VB duplicates, archived versions and asset packages are not distinct
technique demonstrations. The original neighboring files were not modified.

The collection already offers broad coverage of ordinary sprite/3D rendering,
stock/custom effects, animation and skinning, particles, cameras, basic input
and phone sensors, collision, elementary AI, content processing, audio,
networking, game-state/UI patterns, localization and profiling. Replay in the
racing kit, quadtree collision in RobotGame and voice-related networking do
not need another nominal sample just to increase the count.

The 22 proposals address techniques that the static audit did not establish
as focused demonstrations. The audit combined source inventory, API/technique
searches and inspection of relevant examples; a keyword occurrence or asset
name alone was not treated as coverage. Absence in this static audit is not
a guarantee that no incidental related code exists in a large archive.

| Group | Proposed additions |
|---|---|
| Rendering API foundations | Nested scissor clipping, stencil masks, true one-pass MRT outputs, dynamic cube-map capture, Texture3D density sampling |
| Advanced rendering | Deferred point lights, floating-point HDR/exposure, SSAO with bilateral blur, cascaded directional shadows, reflection/refraction water, stitched terrain LOD |
| Media/audio/services | Video textures, native library/playlist/visualization, profile/friends provider behavior, procedural PCM refill, streaming XACT layers |
| Infrastructure | Failure-resistant versioned saves, asynchronous content residency, persisted action rebinding, fixed/variable/interpolated timing |
| Game algorithms | Rotating rigid bodies with impulse/friction/joints, convex navigation mesh with behavior-tree decisions |

[plan.md](../plan.md) gives the visual design, API boundary, dependencies and
acceptance criteria for each proposal. [Verification](verification.md) separates
actual FNA results from missing compiler output and unsupported native services.
