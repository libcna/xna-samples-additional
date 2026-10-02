#!/usr/bin/env python3
"""Run a selected FNA sample, with optional bounded headless verification."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
APP = ROOT / 'src/SampleBrowser/bin/Release/net8.0/SampleBrowser.dll'


def environment(headless=False, software=False):
    env = os.environ.copy()
    if headless:
        env['SDL_VIDEODRIVER'] = 'offscreen'
        env['SDL_AUDIODRIVER'] = 'dummy'
    if headless or software:
        env['LIBGL_ALWAYS_SOFTWARE'] = '1'
        env['FNA3D_FORCE_DRIVER'] = 'OpenGL'
    return env


def catalog():
    return json.loads((ROOT / 'assets/catalog.json').read_text())


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--sample', default='scissor-clipping')
    parser.add_argument('--list', action='store_true')
    parser.add_argument('--headless', action='store_true')
    parser.add_argument('--software', action='store_true')
    parser.add_argument('--frames', type=int, default=0)
    parser.add_argument('--verify', action='store_true')
    parser.add_argument('--capture', type=Path)
    args = parser.parse_args()
    entries = catalog()
    if args.list:
        for e in entries:
            missing = [name for name in e['requires'] if not (ROOT / 'assets' / name).is_file()]
            state = 'missing assets: ' + ', '.join(missing) if missing else e['status']
            print(f"{e['id']:26} {state}")
        return 0
    sample = next((e for e in entries if e['id'] == args.sample), None)
    if sample is None:
        parser.error('Unknown sample. Use --list.')
    if sample['status'] == 'platform-blocked':
        print('Platform blocked: FNA has no Xbox Live profile/friends provider.', file=sys.stderr)
        return 2
    missing = [name for name in sample['requires'] if not (ROOT / 'assets' / name).is_file()]
    if missing:
        print('Asset build required: ' + ', '.join(missing) + '. See docs/dependencies.md.', file=sys.stderr)
        return 2
    if args.frames < 0 or (args.verify or args.headless or args.capture) and args.frames == 0:
        parser.error('Use a positive --frames limit for headless, verification or capture runs.')
    if not APP.is_file():
        print('Build first: dotnet build src/SampleBrowser -c Release', file=sys.stderr)
        return 2
    command = ['dotnet', str(APP), '--sample', args.sample, '--frames', str(args.frames)]
    if args.verify:
        command += ['--verify']
    if args.capture:
        command += ['--capture', str(args.capture)]
    return subprocess.run(command, cwd=ROOT, env=environment(args.headless, args.software)).returncode


if __name__ == '__main__':
    sys.exit(main())
