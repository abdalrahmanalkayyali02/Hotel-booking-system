"""Normalize the pinned CSC exports without generating IDs or inventing translations.

Usage: python3 Db/Seeds/prepare-source.py world.json.gz states.json cities.json.gz
       python3 Db/Seeds/prepare-source.py --regions-only
Download URLs and input checksums are recorded in Sources/manifest.json.
"""
import csv
import gzip
import hashlib
import json
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parent
BASE = 'https://github.com/dr5hn/countries-states-cities-database'
VERSION = 'v3.2-export.7'


def read(path):
    opener = gzip.open if path.suffix == '.gz' else open
    with opener(path, 'rt', encoding='utf-8') as stream:
        return json.load(stream)


def region_sources():
    """Read the checked-in English CSV labels verbatim; wikiDataId is metadata only."""
    result = {}
    for kind, columns in [('regions', ['id', 'name', 'wikiDataId']),
                          ('subregions', ['id', 'name', 'region_id', 'wikiDataId'])]:
        with (ROOT / (kind + '.csv')).open(encoding='utf-8', newline='') as stream:
            reader = csv.DictReader(stream)
            if reader.fieldnames != columns:
                raise ValueError(f'{kind}: unexpected CSV columns {reader.fieldnames}')
            rows = list(reader)
        found = set()
        normalized = []
        for row in rows:
            source_id, name = row['id'], row['name']
            if None in row or any(value is None for value in row.values()):
                raise ValueError(f'{kind}: malformed CSV row')
            if not source_id.isdecimal() or source_id in found:
                raise ValueError(f'{kind}: invalid or duplicate source ID {source_id!r}')
            if not name.strip() or len(name) > 50:
                raise ValueError(f'{kind}/{source_id}: name must contain 1-50 characters')
            found.add(source_id)
            item = dict(SourceId=source_id, Name=name)
            if kind == 'subregions':
                if row['region_id'] not in {r['SourceId'] for r in result['Regions']}:
                    raise ValueError(f'subregions/{source_id}: unknown region {row["region_id"]!r}')
                item['RegionSourceId'] = row['region_id']
            normalized.append(item)
        result['Regions' if kind == 'regions' else 'SubRegions'] = sorted(normalized, key=lambda r: int(r['SourceId']))
    return result


def write_snapshot(data, manifest):
    target = ROOT / 'Sources'
    target.mkdir(exist_ok=True)
    raw = json.dumps(data, ensure_ascii=False, separators=(',', ':')).encode('utf-8')
    snapshot = gzip.compress(raw, mtime=0)
    manifest.update(SnapshotSha256=hashlib.sha256(snapshot).hexdigest(), Counts={k:len(v) for k,v in data.items()})
    (target / 'geography.json.gz').write_bytes(snapshot)
    (target / 'manifest.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print(manifest['Counts'])


def refresh_regions():
    """Extend the existing verified snapshot without redownloading or replacing other source rows."""
    manifest = read(ROOT / 'Sources/manifest.json')
    path = ROOT / 'Sources/geography.json.gz'
    if hashlib.sha256(path.read_bytes()).hexdigest() != manifest['SnapshotSha256']:
        raise ValueError('Geography source checksum mismatch')
    for name, checksum in manifest['BaselineSha256'].items():
        if hashlib.sha256((ROOT / name).read_bytes()).hexdigest() != checksum:
            raise ValueError(f'Baseline changed: {name}; reconcile the source before refreshing')
    data = read(path)
    regions = region_sources()
    if {r['SourceId']:r['Name'] for r in data['Regions']} != {r['SourceId']:r['Name'] for r in regions['Regions']}:
        raise ValueError('Existing region identities/names differ from regions.csv; reconcile explicitly')
    data.update(regions)
    write_snapshot(data, manifest)


def main():
    if sys.argv[1:] == ['--regions-only']:
        refresh_regions()
        return
    paths = [Path(p) for p in sys.argv[1:]]
    if len(paths) != 3:
        raise SystemExit(__doc__)
    countries, states, cities = map(read, paths)
    local = {}
    for kind in ('countries', 'states', 'regions'):
        with (ROOT / (kind + '.csv')).open(encoding='utf-8', newline='') as stream:
            local[kind] = {int(r['id']): r for r in csv.DictReader(stream)}
    for kind, rows in [('countries', countries), ('states', states)]:
        assert len({r['id'] for r in rows}) == len(rows), f'Duplicate {kind} IDs'
        assert set(local[kind]) == {r['id'] for r in rows}, f'{kind} coverage differs'
        for row in rows:
            old = local[kind][row['id']]
            assert old['name'] == row['name'], f'{kind}/{row["id"]}: name differs'
            if kind == 'states':
                assert int(old['country_id']) == row['country_id'], 'State country differs'
    def normalized(kind, rows):
        result = []
        for row in sorted(rows, key=lambda r: r['id']):
            translations = row.get('translations')
            ar = translations.get('ar') if isinstance(translations, dict) else None
            country = row['id'] if kind == 'countries' else row['country_id']
            region = local['countries'][country]['region_id']
            if kind == 'cities':
                assert row['state_id'] in local['states'], 'Orphan city state'
                assert int(local['states'][row['state_id']]['country_id']) == country, 'City country differs'
            result.append(dict(SourceId=str(row['id']), Name=row['name'], ArabicName=ar,
                CountrySourceId=str(country), RegionSourceId=region or None,
                StateSourceId=str(row['state_id']) if kind == 'cities' else None))
        return result
    data = {kind.title(): normalized(kind, rows) for kind, rows in
            [('countries', countries), ('states', states), ('cities', cities)]}
    data.update(region_sources())
    urls = [f'{BASE}/releases/download/{VERSION}/json-countries%2Bstates%2Bcities.json.gz',
            f'https://raw.githubusercontent.com/dr5hn/countries-states-cities-database/{VERSION}/json/states.json',
            f'{BASE}/releases/download/{VERSION}/json-cities.json.gz']
    manifest = dict(Source=BASE, Version=VERSION, License='ODbL-1.0',
        TranslationPolicy='Explicit ar labels from the pinned source; no native-name or English fallback. Source labels are not independent linguistic certification.',
        Inputs=[dict(Url=url, Sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p,url in zip(paths, urls)],
        BaselineSha256={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(ROOT.glob('*.csv'))})
    write_snapshot(data, manifest)


if __name__ == '__main__':
    main()
