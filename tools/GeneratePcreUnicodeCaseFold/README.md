# Regenerate PCRE simple-case data

This maintenance-only C#13/net10 tool emits the Unicode17 C/S mappings used by
existing PCRE scalar equivalence. It has no project/package references and is
not a runtime dependency. Full-string F and optional Turkic T mappings are
excluded; unsupported extra modes remain unsupported.

```sh
curl --fail --location --output /tmp/CaseFolding.txt \
  https://raw.githubusercontent.com/PCRE2Project/pcre2/7978954dbd2efc6f2196869290553cf1871b4ce6/maint/Unicode.tables/CaseFolding.txt
dotnet run --project tools/GeneratePcreUnicodeCaseFold -- \
  /tmp/CaseFolding.txt src/Pcre/PcreUtf8CaseFold.Data.cs
dotnet run --project tools/GeneratePcreUnicodeCaseFold -- --verify \
  /tmp/CaseFolding.txt src/Pcre/PcreUtf8CaseFold.Data.cs
```

Input SHA256:
`ff8d8fefbf123574205085d6714c36149eb946d717a0c585c27f0f4ef58c4183`.
Equivalent upstream input: https://www.unicode.org/Public/17.0.0/ucd/CaseFolding.txt .
The tool validates source bytes, scalar bounds, duplicate/identity mappings,
idempotence and ASCII fast-path agreement. It emits BOM-free UTF8/LF on every
host. Of 1,512 mappings, 26 ASCII pairs use the existing fast path; 1,486 packed
ulong entries require 11,888 static data bytes. Other scalars fold to themselves.

Linux PR/main CI verifies byte-for-byte regeneration. Tests independently compare
complete semantic equivalence classes over all 1,112,064 valid scalars with
PCRE2 10.48, in unrestricted and restricted modes, plus live JIT matching paths.
Canonical key choice is not the native oracle. Unicode permission terms ship
in `THIRD-PARTY-NOTICES.txt` with the package. Category data uses the adjacent
`GeneratePcreUnicodeCategories` tool and is unchanged here.
