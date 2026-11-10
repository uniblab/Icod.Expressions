# Regenerate PCRE Unicode categories

This maintenance-only C#13/net10 tool emits the managed category data used by
internal PCRE matching and capture-name grammar. It has no project/package
references and is not a production runtime dependency.

Download the exact Unicode17 data bundled with the pinned PCRE2 source:

```sh
curl --fail --location --output /tmp/DerivedGeneralCategory.txt \
  https://raw.githubusercontent.com/PCRE2Project/pcre2/7978954dbd2efc6f2196869290553cf1871b4ce6/maint/Unicode.tables/DerivedGeneralCategory.txt
dotnet run --project tools/GeneratePcreUnicodeCategories -- \
  /tmp/DerivedGeneralCategory.txt src/Pcre/PcreUnicodeCategories.Data.cs
dotnet run --project tools/GeneratePcreUnicodeCategories -- --verify \
  /tmp/DerivedGeneralCategory.txt src/Pcre/PcreUnicodeCategories.Data.cs
```

The source SHA256 is
`d62e5bab70ca74f099343f71224fa051cb1fdd61a1ab45c0488c44cfc0b6102e`.
The equivalent upstream input is
https://www.unicode.org/Public/17.0.0/ucd/extracted/DerivedGeneralCategory.txt .
The tool rejects other bytes, overlapping ranges and incomplete coverage.
Output is BOM-free UTF8 with LF on every host. It contains128 ASCII category
bytes and4,116 range starts, each packed with its five-bit .NET category value:
16,592 data bytes. Source surrogates are retained; the UTF decoder never admits
them as scalars. Unicode permission terms ship in `THIRD-PARTY-NOTICES.txt`.

Linux CI verifies byte-for-byte regeneration. Tests independently pin all
1,112,064 valid scalar categories using a native PCRE2 10.48 fingerprint, plus
live JIT category/capture/name comparisons. Case-fold mappings, script and
binary properties, and grapheme data are outside this category-only table.
