#!/usr/bin/env python3
"""Prepare local FNA dependencies without modifying their source checkout."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--fna-root', type=Path)
parser.add_argument('--fna-dll', type=Path)
parser.add_argument('--native-dir', type=Path)
args = parser.parse_args()
deps = ROOT / '.deps'
native = deps / 'native'
native.mkdir(parents=True, exist_ok=True)
fna = args.fna_root.resolve() if args.fna_root else None
assembly = args.fna_dll
if assembly is None and fna:
    assembly = next((p for p in [fna / 'bin/Release/net8.0/FNA.dll',
                                fna / 'bin/Debug/net8.0/FNA.dll'] if p.is_file()), None)
if assembly is None or not assembly.is_file():
    parser.error('Supply --fna-dll or an FNA root containing a net8.0 build.')
shutil.copy2(assembly, deps / 'FNA.dll')
if args.native_dir:
    for p in args.native_dir.iterdir():
        if p.is_file() and ('.so' in p.name or p.suffix in ('.dll', '.dylib')):
            shutil.copy2(p, native / p.name)
if fna and not args.native_dir:
    for name in ('FNA3D', 'FAudio'):
        paths = sorted((fna / 'lib' / name / 'build').glob(f'lib{name}.so.*'))
        if not paths:
            parser.error(f'Missing {name}; supply --native-dir from fnalibs.')
        shutil.copy2(paths[-1], native / f'lib{name}.so')
    tf = fna / 'lib/Theorafile'
    # Compile unchanged bundled sources with generic CPU code. Output is local.
    sources = [tf / 'theorafile.c']
    for codec in ('ogg', 'vorbis', 'theora'):
        sources.extend(sorted((tf / 'lib' / codec).glob('*.c')))
    command = ['gcc', '-O2', '-fPIC', '-shared', '-o', str(native / 'libtheorafile.so')]
    command += [str(p) for p in sources]
    command += [f'-I{p}' for p in [tf, tf / 'lib', tf / 'lib/ogg',
                                  tf / 'lib/vorbis', tf / 'lib/theora']]
    command += ['-lm']
    subprocess.run(command, check=True)
files = [deps / 'FNA.dll', *sorted(native.iterdir())]
manifest = {'fna_source': str(fna) if fna else str(assembly), 'files': {
    str(p.relative_to(deps)): hashlib.sha256(p.read_bytes()).hexdigest() for p in files}}
if fna:
    result = subprocess.run(['git', '-C', str(fna), 'rev-parse', 'HEAD'],
                            capture_output=True, text=True)
    manifest['fna_revision'] = result.stdout.strip() if result.returncode == 0 else 'unknown'
(deps / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
print(f'Prepared FNA and {len(files)-1} native libraries in {deps}')
