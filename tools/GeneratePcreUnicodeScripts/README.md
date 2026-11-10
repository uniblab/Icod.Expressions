# Regenerate PCRE Unicode scripts

Maintenance-only C#13/net10; no project/package references or runtime dependency.
Use the exact Unicode17 inputs bundled with PCRE2 10.48:

```sh
for unicode_script_source in Scripts.txt ScriptExtensions.txt PropertyValueAliases.txt; do
  curl --fail --location --output "/tmp/$unicode_script_source" \
    "https://raw.githubusercontent.com/PCRE2Project/pcre2/7978954dbd2efc6f2196869290553cf1871b4ce6/maint/Unicode.tables/$unicode_script_source"
done
dotnet run --project tools/GeneratePcreUnicodeScripts -- \
  /tmp/Scripts.txt /tmp/ScriptExtensions.txt /tmp/PropertyValueAliases.txt \
  src/Pcre/PcreUnicodeScripts.Data.cs
dotnet run --project tools/GeneratePcreUnicodeScripts -- --verify \
  /tmp/Scripts.txt /tmp/ScriptExtensions.txt /tmp/PropertyValueAliases.txt \
  src/Pcre/PcreUnicodeScripts.Data.cs
```

The three input hashes, in command order:

- `9f5e50d3abaee7d6ce09480f325c706f485ae3240912527e651954d2d6b035bf`
- `ec2107e58825a1586acee8e0911ce18260394ac8b87e535ca325f1ccbeb06bc6`
- `64e9a5f76f7a1e8b5a47d6a1f9a26522a251208f5276bdfa1559dac7cf2e827a`

Upstream equivalents: https://www.unicode.org/Public/17.0.0/ucd/ .
The tool rejects other source bytes, overlaps and alias collisions. It sorts
175 scripts by ordinal ISO short code; Hrkt is not a native PCRE2 script and is
excluded. The 344 normalized aliases include legacy Qaac/Qaai. Scripts not
explicitly assigned in Scripts.txt are Unknown. Native scx membership is the
union of primary script and extension membership, including Common/Inherited.

Before emission the tool verifies an independent PCRE2 UCD fingerprint over
all 1,112,064 valid scalars. Records contain one primary-ID byte followed by
22 membership bytes, one low-order bit per script in the sorted order.
The SHA256 is
`f6e14157525f8aa69f6bd1564ec306717bcd6992b54280b92a105904f1868b82`.
Native research uses UCD_SCRIPT and UCD_SCRIPTX from the pinned source; native
PT_SC-only scripts have primary membership only. Source parsing and native
records agree at every valid scalar.

Output is BOM-free UTF8/LF. Primary/extended tables contain 1,717/2,104 packed
inclusive intervals, with 21 bits each for start/end and ushort offsets per
script. Native boundary fixtures independently exercise 3,508 points through
JIT and managed matching. Linux CI verifies byte-for-byte regeneration. Unicode
permission terms ship in `THIRD-PARTY-NOTICES.txt`. Binary properties, script-run
groups and broader optimizer/public/consumer qualification are separate gates.
