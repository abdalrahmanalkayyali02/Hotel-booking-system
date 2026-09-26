# Reference seeds

The offline seed set contains 250 countries, 5,308 states, 152,970 cities, 6 regions,
22 subregions, English and Arabic languages, and five roles. Generation, validation and
the loader perform no database writes. Only the explicit `--seed-import` command writes
to MySQL. The loader returns the existing model types.

## Generate, validate, and test

Run from the API project directory:

```sh
dotnet run -- --seed-generate Db/Seeds
dotnet run -- --seed-validate Db/Seeds
dotnet run --project Tests/Seeds/Seeds.Tests.csproj
dotnet run --project Tests/Seeds/Seeds.Tests.csproj -- Db/Seeds
```

Generation writes the persistent mapping, staged data, and coverage report even when
translations are incomplete. Generation and validation return **1** for incomplete or invalid
data, **2** for file/format/command errors, and **0** only for complete validation.
Generation requires an explicit seed directory. Validation defaults to the packaged
`Db/Seeds` directory beside the application, independent of the working directory:

```sh
dotnet /path/to/publish/HBS.API.dll --seed-validate
```

`await new SeedDataLoader().LoadAsync()` validates before returning `SeedData` and throws
`SeedValidationException` if any issue remains. `AuditAsync()` exposes staging data and
issues for review only; it does not approve an incomplete dataset for database import.
Neither method generates IDs or writes files. Cancellation tokens are supported.

## Explicit MySQL import

From the repository root, after reviewing the staged data and creating the schema:

```sh
dotnet run --project ./HBS.API/HBS.API.csproj -- --seed-import
```

This command **writes to MySQL**. It does not create a schema, run migrations, regenerate
seeds, or retrieve translations. By default it reads the packaged `Db/Seeds` directory
beside the application. Like the other seed commands, it accepts an optional second
argument naming a seed directory. It uses the application's default configuration
providers/environment and `ConnectionStrings:DefaultConnection`, with Pomelo
`UseMySql` and `ServerVersion.AutoDetect`, as in `Program.cs`.

`SeedDataLoader.LoadAsync()` completes validation **before** context creation, server
version discovery or transaction opening. The importer then starts one serializable
EF Core transaction. Before any insert it checks **every** target table, including
collections with zero staged rows. Tables must exist and use InnoDB so rollback and
serializable empty-table checks apply. If any target contains rows, import refuses and
names that target. There is no merge, upsert, replacement, deletion or automatic retry.
Tables outside the seed collections are neither checked for emptiness nor written.

Insertion order:

1. Languages → Roles → Regions → SubRegions → Countries → States → Cities
2. RoleTranslations → CountriesTranslations → StatesTranslations → CitiesTranslations
   → RegionsTranslations → SubRegionsTranslations

Each collection is split into batches of at most **1,000 rows**. Each batch uses
`AddRange`, one `SaveChangesAsync`, then `ChangeTracker.Clear()`. Parents are already
saved within the same transaction before child batches are attached; only staged FK
scalars and UUIDs are used. The loader still holds the full seed artifact in memory,
but EF tracks at most one batch instead of all 475,685 rows.

The current dataset requires **485 SaveChanges calls**: 153 for Cities, 306 for
CitiesTranslations, 6 for States, 11 for StatesTranslations, and 9 for the remaining
collections. EF may issue multiple SQL commands per SaveChanges. No batch commits
independently. Commit happens once after all collections succeed. Query/insert/save
or commit failures trigger rollback and a nonzero exit code; rollback failure is
reported explicitly without claiming the database state is known. Context/transaction
resources are disposed. Concurrent writes may cause a deadlock/refusal; stop application
writes while performing this initial import.

Successful output (counts follow the process culture) starts with:

```text
Seed import committed: 475,685 rows inserted.
  Languages: 2
  Roles: 5
  Regions: 6
  SubRegions: 22
  Countries: 250
  States: 5,308
  Cities: 152,970
  RoleTranslations: 10
  CountriesTranslations: 500
  StatesTranslations: 10,616
  CitiesTranslations: 305,940
  RegionsTranslations: 12
  SubRegionsTranslations: 44
```

Success returns **0**. Import validation, connection, refusal and transaction failures
return **1**; malformed command usage returns **2**. A second successful-import attempt
will refuse because the seed tables are no longer empty.

Database-free import tests, including a read-only audit of the real staged dataset:

```sh
dotnet run --project ./HBS.API/Tests/Seeds/Seeds.Tests.csproj -- --import-audit ./HBS.API/Db/Seeds
```

Tests use a fake transactional database, generate fixtures only in a temporary directory,
and do not connect to MySQL or regenerate the real staged files. The MySQL adapter is
compiled and inspected; these tests do not substitute for a live MySQL integration test.

## UUIDv7 and model mapping

Every generated primary key uses `Guid.CreateVersion7()`. `id-map.json.gz` persists
source-key-to-GUID assignments, including IDs reserved for missing Arabic translations.
Never delete or replace this mapping: doing so would change reference identities.
Generation refuses to recreate a missing map when staged data already exists.

Keys use `country/111`, `state/<source-id>`, `city/<source-id>`, `region/<source-id>`,
`subregion/<CSV-id>`, `language/en`, `language/ar`, and `role/<code>`. Translation keys use
`<kind>-translation/<source-id>/<language-code>`. Consumers can resolve role/language
IDs from this map; language codes and role codes are not added to the database models.

| Role code | English | Arabic |
| --- | --- | --- |
| super_admin | Super Admin | مدير النظام |
| hotel_admin | Hotel Admin | مدير الفندق |
| receptionist | Receptionist | موظف الاستقبال |
| customer | Customer | عميل |
| guest | Guest | مستخدم زائر |

`Staging/seed-data.json.gz` contains UTF-8 JSON collections matching the C# models,
including bilingual role descriptions. Compression keeps the worldwide assets small.
`Regions.RegionId` is preserved as the model's primary-key property. Region CSV IDs
resolve through the existing `region/<CSV-id>` mappings; no existing IDs are replaced.
Subregions resolve `region_id` against those same region mappings. Unknown parents,
duplicate source IDs, blank names and names longer than 50 characters are rejected.
Users, OTPs, permissions and role-permission grants are not seeded.

`regions.csv` has `id,name,wikiDataId`; `subregions.csv` has
`id,name,region_id,wikiDataId`. Both supply English names only. Their exact `name`
values produce English translations. Arabic values are supplied by the deterministic
`Sources/arabic-corrections.json` sidecar: 12 `RegionsTranslations` and 44
`SubRegionsTranslations` rows in total. All five geography levels require both
`language/en` and `language/ar`. Normal generation never resolves `wikiDataId` online.

`SeedData` includes `SubRegions`, `RegionsTranslations` and `SubRegionsTranslations`.
The generic compressed serializer and loader include these collections automatically.
The importer inserts parent entities before their translations, with Languages first.
JSON property order itself does not perform database inserts. The strict loader rejects
incomplete Arabic coverage before the importer creates a database context.

## Translation coverage and corrections

All 250 countries, 5,308 states, 152,970 cities, 6 regions, 22 subregions, and five
roles now have English and Arabic rows. The Arabic enrichment added 7,420 translations:
90 states, 7,302 cities, 6 regions and 22 subregions. It did not replace accepted names.

`coverage-report.json` must report `Complete: true` and zero issues. Validation checks
both languages at every geography level, persistent UUIDv7 identities, parent/language
uniqueness, parent geography, nonblank names, Arabic script, and configured name limits
(50 characters for countries/states/regions/subregions, 100 for cities). Names are never
truncated to satisfy those limits. Latin-only or Cyrillic-only Arabic labels fail.

`Sources/arabic-corrections.json` is the accepted, offline enrichment input. The existing
Cameroon correction is unchanged. Each additional value is either an identity-checked
Wikidata Arabic label or a generated Arabic rendering of the existing English name.
`Sources/arabic-enrichment-report.json` records the breakdown, preservation checks and
source limitations. Generated values are explicitly identified; they are not claimed
to be sourced Arabic labels or independently linguistically certified. They use
country-aware phonetic transliteration, conventional geography names and translation
of administrative descriptors; samples and identified ambiguities were reviewed by
Codex. Independent linguistic review remains useful before public presentation.

For a reference-backed correction:

```json
{
  "city/<source-id>": {
    "Name": "اسم عربي موثق",
    "SourceUrl": "https://source.example/page-supporting-this-name",
    "ReviewedBy": "Reviewer and identity checks",
    "SourceType": "reference",
    "Verification": "Source ID and verified country/state"
  }
}
```

The default `SourceType` is `reference` for backward compatibility. `wikidata` entries
link to the retrieved entity revision and describe English identity, geography type,
country and administrative ancestry checks. An identifier resolving is insufficient:
wrong identities, unconfirmed hierarchy, unsuitable labels and scope changes fall back
to rendering the CSV name. In particular, Polar stays polar regions, and Australia and
New Zealand does not become the broader Australasia. Middle Africa is rendered as
وسط إفريقيا, not the Central African Republic.

Generated entries use `SourceType: "generated"`, a required `GenerationMethod`, and
explicit generation provenance in `ReviewedBy`. Their `SourceUrl` documents the pinned
**English input**, not an external Arabic authority. The sidecar accepts `country/`,
`state/`, `city/`, `region/` and `subregion/` source keys. Unknown keys, missing provenance,
and corrections that replace usable source Arabic names are rejected. The script
check establishes script coverage, not linguistic accuracy.

Future enrichment must build a missing-pair worklist first and append only those
corrections. Never rewrite existing accepted corrections or English/source names.
External retrieval is an explicit enrichment activity, not part of `prepare-source.py`,
seed generation, validation or loading. Checked-in accepted corrections make normal
regeneration fully offline. Keep the original CSVs and normalized snapshot unchanged
when adding corrections.

The Arabic completion reuses all 7,392 reserved state/city Arabic UUIDs and appends only
6 `region-translation/<id>/ar` and 22 `subregion-translation/<id>/ar` UUIDv7 mappings.
The persistent map has 475,685 entries. Repeated generation must preserve both the map
and staged data byte-for-byte.

## Source provenance and regeneration

Geography data: [Countries States Cities Database](https://github.com/dr5hn/countries-states-cities-database),
release `v3.2-export.7`, under [ODbL-1.0](https://github.com/dr5hn/countries-states-cities-database/blob/v3.2-export.7/LICENSE).
Retain attribution and the upstream database license when redistributing derived data.
Wikidata labels: [Wikidata structured data](https://www.wikidata.org/wiki/Wikidata:Licensing),
CC0; each accepted label includes its entity revision URL in the corrections sidecar.
Unicode country correction: [CLDR Arabic territory names](https://github.com/unicode-org/cldr-json/blob/main/cldr-json/cldr-localenames-full/main/ar/territories.json),
under the [Unicode license](https://www.unicode.org/license.txt).

`Sources/manifest.json` records the pinned input URLs and SHA-256 checksums, local CSV
checksums, normalized snapshot checksum, and counts. `Sources/geography.json.gz`
preserves source IDs, hierarchy and Arabic labels. Runtime loading needs no network.

To reproduce the normalized snapshot, download the three URLs in the manifest and run:

```sh
python3 Db/Seeds/prepare-source.py world.json.gz states.json cities.json.gz
dotnet run -- --seed-generate Db/Seeds
```

To extend the existing verified snapshot from the checked-in region/subregion CSVs
without downloading or changing country/state/city source rows:

```sh
python3 Db/Seeds/prepare-source.py --regions-only
dotnet run -- --seed-generate Db/Seeds
dotnet run -- --seed-validate Db/Seeds
```

This checks the existing snapshot and CSV checksums and refuses changed region
identities/names. It updates the snapshot checksum/counts in the manifest, preserving
upstream provenance. Generation appends only missing identity keys to the persistent map.
The first extension adds 50 UUIDv7 mappings: 22 subregions, 6 English region translations,
and 22 English subregion translations. Repeated generation reuses all assignments.

The normalizer checks country/state identity and names against the original CSVs and
city foreign keys before writing. The loader verifies the normalized snapshot and
baseline CSV checksums. Upstream changes require explicit reconciliation and an updated
manifest; existing identity mappings must be retained.
