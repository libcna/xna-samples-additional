# Remaining work

1. Obtain a working `fx_2_0` effect compiler. The installed Wine environment is
   blocked by restricted syscalls/read-only runtime paths. No protections were
   disabled. Prefer a native compiler with HLSL/FX support, or compile the four
   original `.fx` files on a supported Windows toolchain.
2. Build Scene.fxb, Post.fxb, Volume.fxb and Water.fxb, rebuild the browser,
   and run MRT, volume, deferred lighting, HDR, AO, cascaded shadows and water
   with defining checks. Fix compiler/runtime findings and inspect every mode.
   These seven entries must remain Implemented until that work passes.
3. Validate streaming.xap with XactBld3 and compare its native runtime behavior
   with the original generated fixture. Listen to procedural/XACT audio and
   exercise a physical controller and visible-window controls.
4. Native MediaLibrary/visualization and Xbox Live profile/friends coverage
   require an operational provider/runtime. Do not replace service evidence
   with mock data or mark these portions complete on the current FNA build.
5. Update plan.md, catalog.json and docs/verification.md only from actual runs.

The runnable checkpoint is reproduced with `python3 tools/verify.py`.
An exit status of 2 currently means the runnable checks passed and coverage
remains incomplete; status 1 means a check failed.
