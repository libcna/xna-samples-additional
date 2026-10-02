#!/usr/bin/env python3
"""Build, check pure algorithms, and verify every currently runnable FNA sample."""
import argparse
import datetime
import json
from pathlib import Path
import subprocess
import sys
from run import ROOT, APP, catalog, environment

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--interactive', action='store_true', help='Use real display/audio instead of SDL offscreen/dummy.')
parser.add_argument('--allow-incomplete', action='store_true', help='Exit zero if runnable checks pass despite recorded coverage gaps.')
args = parser.parse_args()
artifacts = ROOT / '.artifacts'
artifacts.mkdir(exist_ok=True)
results = {'timestamp_utc': datetime.datetime.now(datetime.timezone.utc).isoformat(),
           'headless': not args.interactive, 'dependency_manifest': json.loads((ROOT / '.deps/manifest.json').read_text()),
           'checks': [], 'coverage_limits': ['gamer-profile-friends: FNA has no Xbox Live provider',
              'media-library: device collections and current FAudio visualization unavailable',
              'xact-streaming: Microsoft XAP authoring validation pending', 'Dummy audio does not establish audible quality.']}


def execute(label, command, timeout):
    try:
        result = subprocess.run(command, cwd=ROOT, env=environment(not args.interactive),
                                capture_output=True, text=True, timeout=timeout)
        log = result.stdout + result.stderr
        (artifacts / (label + '.log')).write_text(log)
        results['checks'].append({'id': label, 'status': 'passed' if result.returncode == 0 else 'failed', 'exit_code': result.returncode})
        print(('PASS ' if result.returncode == 0 else 'FAIL ') + label, flush=True)
        if result.returncode:
            print(log[-3000:], flush=True)
        return result.returncode == 0
    except subprocess.TimeoutExpired as error:
        (artifacts / (label + '.log')).write_text(str(error))
        results['checks'].append({'id': label, 'status': 'failed', 'reason': 'timeout'})
        print('FAIL ' + label + ' (timeout)', flush=True)
        return False


if execute('release-build', ['dotnet', 'build', 'tests/SampleTests', '-c', 'Release', '-p:NuGetAudit=false'], 60):
    execute('algorithm-tests', ['dotnet', str(ROOT / 'tests/SampleTests/bin/Release/net8.0/SampleTests.dll')], 30)
    for entry in catalog():
        missing = [name for name in entry['requires'] if not (ROOT / 'assets' / name).is_file()]
        if entry['status'] == 'platform-blocked' or missing:
            reason = 'platform unavailable' if not missing else 'missing assets: ' + ', '.join(missing)
            results['checks'].append({'id': entry['id'], 'status': 'blocked', 'reason': reason})
            print('BLOCKED ' + entry['id'] + ': ' + reason, flush=True)
            continue
        frames = '270' if entry['id'] == 'xact-streaming' else '150'
        execute(entry['id'], ['dotnet', str(APP), '--sample', entry['id'], '--frames', frames, '--verify',
                              '--capture', str(artifacts / (entry['id'] + '.png'))], 30)
summary = {state: sum(check['status'] == state for check in results['checks']) for state in ('passed', 'failed', 'blocked')}
results['summary'] = summary
(artifacts / 'verification.json').write_text(json.dumps(results, indent=2) + '\n')
print(json.dumps(summary), flush=True)
if summary['failed']:
    sys.exit(1)
sys.exit(0 if args.allow_incomplete or not (summary['blocked'] or results['coverage_limits']) else 2)
