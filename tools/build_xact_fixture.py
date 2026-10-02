#!/usr/bin/env python3
"""Build the original two-cue PCM fixture in the XACT 3 bank format.

This is a narrow fixture builder, not an XactBld3 replacement. Its layout follows
FAudio's FACT.h and FACT_internal.c format readers. It emits genuine streaming
XWB, XGS settings and XSB sounds consumed by AudioEngine/WaveBank/SoundBank.
The matching XAP remains available for independent Microsoft authoring validation.
"""
import json
from pathlib import Path
import struct
import wave

ASSETS = Path(__file__).resolve().parents[1] / 'assets'


def pack(fmt, *values):
    return struct.pack('<' + fmt, *values)


def name(value, size=64):
    data = value.encode('ascii') + b'\0'
    if len(data) > size:
        raise ValueError('Fixture name is too long.')
    return data.ljust(size, b'\0')


def build():
    project = json.loads((ASSETS / 'streaming-project.json').read_text())
    cues = project['cues']
    if len(cues) != 2:
        raise ValueError('This fixture builder supports exactly two PCM cues.')
    # XGS: one category, reserved NumCueInstances at slot zero, Intensity, one RPC.
    category_offset = 77
    variable_offset = category_offset + 10
    category_index = variable_offset + 26
    variable_index = category_index + 6
    category_names = variable_index + 12
    variable_names = category_names + len(project['category']) + 1
    intensity_name = variable_names + len('NumCueInstances') + 1
    rpc_offset = intensity_name + len(project['variable']) + 1
    xgs = b'XGSF' + pack('HHHQB7H', 46, 42, 0, 0, 3, 1, 2, 0, 0, 1, 0, 0)
    xgs += pack('11I', category_offset, variable_offset, 0, category_index, 0, variable_index, category_names, variable_names, rpc_offset, 0, 0)
    assert len(xgs) == category_offset
    xgs += pack('BHHBHBB', 255, 0, 0, 0, 65535, 180, 1)
    xgs += pack('Bfff', 15, 0, 0, 255) + pack('Bfff', 5, 0, 0, 1)
    xgs += pack('IH', category_names, 255) + pack('IH', variable_names, 255) + pack('IH', intensity_name, 255)
    xgs += project['category'].encode() + b'\0NumCueInstances\0' + project['variable'].encode() + b'\0'
    xgs += pack('HBH', 1, 2, 0) + pack('ffB', 0, -9600, 0) + pack('ffB', 1, 0, 0)
    (ASSETS / 'streaming.xgs').write_bytes(xgs)

    # Streaming PCM wave bank: bank data, 24-byte entries and 2048-aligned audio.
    bank_data = pack('II', 1, 2) + name(project['bank']) + pack('4IQ', 24, 0, 2048, 0, 0)
    metadata_offset = 52 + len(bank_data)
    data_offset = 2048
    pcm = bytearray(); metadata = bytearray()
    for cue in cues:
        with wave.open(str(ASSETS / cue['wave']), 'rb') as input_file:
            if (input_file.getnchannels(), input_file.getsampwidth()) != (1, 2):
                raise ValueError('Expected mono PCM16 source.')
            samples, rate = input_file.getnframes(), input_file.getframerate()
            data = input_file.readframes(samples)
        offset = len(pcm)
        mini_format = (1 << 2) | (rate << 5) | (2 << 23) | (1 << 31)
        metadata += pack('6I', samples << 4, mini_format, offset, len(data), 0, samples)
        pcm += data
        pcm += bytes((-len(pcm)) % 2048)
    xwb = b'WBND' + pack('II', 46, 44)
    for offset, size in [(52, len(bank_data)), (metadata_offset, len(metadata)), (0, 0), (0, 0), (data_offset, len(pcm))]:
        xwb += pack('II', offset, size)
    xwb += bank_data + metadata
    xwb += bytes(data_offset - len(xwb)) + pcm
    (ASSETS / 'Streaming.xwb').write_bytes(xwb)

    # Complex sounds hold a single infinitely looping play-wave event.
    sound_offset = 138 + 64
    sounds = bytearray(); sound_codes = []
    for index, cue in enumerate(cues):
        code = sound_offset + len(sounds); sound_codes.append(code)
        rpc = pack('HB I', 7, 1, rpc_offset) if cue['rpc'] else b''
        event_offset = code + 10 + len(rpc) + 9
        event = pack('B I H B B H B B H H', 1, 1, 0, 255, 0, index, 0, 255, 0, 0)
        size = 10 + len(rpc) + 9 + len(event)
        sounds += pack('BHBhBHB', 3 if cue['rpc'] else 1, 0, 180, 0, 128, size, 1)
        sounds += rpc + pack('BIHH', 180, event_offset, 0, 0) + event
    cue_offset = sound_offset + len(sounds)
    cue_names = cue_offset + 10
    names_data = b''.join(c['name'].encode() + b'\0' for c in cues)
    cue_indices = cue_names + len(names_data)
    xsb = b'SDBK' + pack('HHHQBHHHHBHHH', 46, 43, 0, 0, 1, 2, 0, 0, 2, 1, 2, len(names_data), 0)
    xsb += pack('10i', cue_offset, -1, cue_names, -1, -1, -1, 138, -1, cue_indices, sound_offset)
    xsb += name('AdditionalAudio')
    assert len(xsb) == 138
    xsb += name(project['bank']) + sounds
    xsb += b''.join(pack('BI', 4, code) for code in sound_codes) + names_data
    offset = cue_names
    for cue in cues:
        xsb += pack('IH', offset, 255); offset += len(cue['name']) + 1
    (ASSETS / 'streaming.xsb').write_bytes(xsb)
    print('Generated original streaming XGS/XSB/XWB fixture.')


if __name__ == '__main__':
    build()
