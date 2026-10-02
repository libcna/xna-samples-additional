# Asset provenance

All demonstration assets here originate in this repository. There are no copied
Microsoft sample models, textures, music or video clips.

`Scene.fx`, `Post.fx`, `Volume.fx` and `Water.fx` are original effect sources.
Their compiled `.fxb` outputs are currently missing. Use the asset build tool
with a working legacy FX compiler, then run the defining FNA checks.

`tone-220.wav` and `tone-440.wav` contain generated three-second sine tones.
`test-pattern.ogv` is a three-second, silent FFmpeg test pattern, 160x96 at 24 FPS,
encoded as Theora. The video sample's volume control exercises VideoPlayer's
property; this silent fixture does not provide an audible volume demonstration.

`streaming-project.json` describes an original two-cue project. The narrow
`tools/build_xact_fixture.py` writer emits its genuine XACT-format `.xgs`, `.xsb`
and streaming `.xwb` files. `streaming.xap` is the matching authoring design,
with independent Microsoft compiler validation still pending. The builder
requires the reserved NumCueInstances variable in slot zero before Intensity.

Regenerate media and banks with `python3 tools/build_assets.py --media-only`.
The glyph atlas and all 3D geometry are generated in the runner at runtime.
