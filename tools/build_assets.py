#!/usr/bin/env python3
"""Generate original media and compile XNA effect sources."""
import argparse
import math
import os
from pathlib import Path
import struct
import subprocess
import wave

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--fxc', type=Path)
parser.add_argument('--vkd3d', type=Path, help='Native vkd3d-compiler with HLSL -> fx_2_0 support (experimental).')
parser.add_argument('--media-only', action='store_true')
parser.add_argument('--xact', type=Path)
parser.add_argument('--wine', default='wine', help='Wine launcher (use wine64 for a 64-bit compiler).')
args = parser.parse_args()
assets = ROOT / 'assets'
assets.mkdir(exist_ok=True)
env = os.environ.copy()
env['WINEPREFIX'] = str(ROOT / '.deps/wine')
env['XDG_RUNTIME_DIR'] = str(ROOT / '.deps/runtime')
Path(env['XDG_RUNTIME_DIR']).mkdir(parents=True, exist_ok=True, mode=0o700)
env['WINEDEBUG'] = '-all'
env['WINEDLLOVERRIDES'] = 'mscoree,mshtml='
if args.wine.endswith('wine64'):
    env['WINEARCH'] = 'win64'
    env['WINELOADER'] = args.wine
    env['WINESERVER'] = str(Path(args.wine).with_name('wineserver64'))
prefix = [args.wine] if os.name != 'nt' else []
if args.fxc and args.vkd3d:
    parser.error('Select one effect compiler.')
if not args.media_only and args.fxc is None and args.vkd3d is None:
    parser.error('Supply --fxc, --vkd3d, or use --media-only.')
for source in ([] if args.media_only else sorted(assets.glob('*.fx'))):
    output=source.with_suffix('.fxb')
    command=([str(args.vkd3d.resolve()), '-x', 'hlsl', '-b', 'fx', '-p', 'fx_2_0', '-o', str(output), str(source)]
             if args.vkd3d else prefix + [str(args.fxc.resolve()), '/nologo', '/T', 'fx_2_0', '/Fo', str(output), str(source)])
    subprocess.run(command, env=env, check=True, timeout=90)
for frequency in (220, 440):
    with wave.open(str(assets / f'tone-{frequency}.wav'), 'wb') as output:
        output.setparams((1, 2, 22050, 0, 'NONE', 'not compressed'))
        pcm = [int(math.sin(2 * math.pi * frequency * i / 22050) * 10000)
               for i in range(22050 * 3)]
        output.writeframes(struct.pack(f'<{len(pcm)}h', *pcm))
subprocess.run(['ffmpeg', '-hide_banner', '-loglevel', 'error', '-y',
                '-f', 'lavfi', '-i', 'testsrc2=size=160x96:rate=24',
                '-t', '3', '-c:v', 'libtheora', '-q:v', '5', '-an',
                str(assets / 'test-pattern.ogv')], check=True, timeout=30)
from build_xact_fixture import build
build()
if args.xact:
    subprocess.run(prefix + [str(args.xact.resolve()), '/nologo', str(assets / 'streaming.xap')],
                   cwd=assets, env=env, check=True, timeout=90)
print('Assets generated successfully.')
