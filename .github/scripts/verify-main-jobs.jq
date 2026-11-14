[
  "Linux x64", "Linux ARM64", "Windows x64", "Windows ARM64", "macOS ARM64",
  "macOS x64 net10.0", "macOS x64"
] as $expected
| (.total_count == ($expected | length))
  and (([.jobs[].name] | sort) == ($expected | sort))
  and all(.jobs[]; .status == "completed" and .conclusion == "success")
