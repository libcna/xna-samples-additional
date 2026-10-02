# Contributing

Keep each example focused on the topic named in its directory. Put reusable
window, geometry, and diagnostic code in `src/SampleBrowser`; keep the technique
in the sample itself. All source comments, documentation, UI text, and commit
messages must be English.

Prefer generated geometry and assets with recorded provenance. Do not copy
code or assets from the neighboring Microsoft samples without their license
and attribution. Changes here are independent of contributions to FNA.

Build the Release configuration, run the algorithm tests, and run affected
graphics examples on FNA with `--verify` and a bounded frame count. Inspect
screenshots when a rendering change is visible. Update `plan.md` and
`docs/verification.md` with evidence, including unsupported features.

Do not mark a topic complete merely because a demonstration window opens.
Verify its defining API or algorithm, state restoration, and resource cleanup.
