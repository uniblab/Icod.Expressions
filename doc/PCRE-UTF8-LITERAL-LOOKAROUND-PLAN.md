# UTF-8 fixed literal lookaround plan

1. Add internal tests for Grep's UTF-mode lookbehind, UTF-8 literal and `\xFF` lookaround, all four polarities/directions, invalid-subject fragment barriers, captures, start offsets, diagnostics, work and cancellation. Push a test-only draft PR and confirm a Staging red failure for the missing behavior.
2. Allow the existing literal assertion parser in UTF mode. Keep raw UTF-8 literal bytes and encode escaped scalar values as UTF-8. Limit matcher assertion comparisons to the current fragment, leaving byte mode unchanged.
3. Update the master feature matrix and verify all six Staging jobs, net8/net9/net10, frozen GNU tests and package smoke. Request independent review and fix material findings before marking the PR ready.
