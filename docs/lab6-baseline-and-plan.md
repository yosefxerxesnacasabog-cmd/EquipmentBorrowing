# Lab 6 baseline and phased verification

## Authentic starting point

Lab 6 starts from the published `lab5-submitted` tag at
`bd9b44b06d430892ddc04833202f875be3d5a602` (`Add partner names and BSIT-3Cx
section to README`). Before creating branch `lab6`, `git ls-remote` confirmed
that the remote master and tag both pointed to that SHA. Existing commits,
migrations, source and Lab 5 evidence are preserved. No historical tag is moved
for Lab 6. The final Lab 6 tag will be `lab6-submitted` only after completion.

## Six planned commits

| Phase | Work | Status |
| --- | --- | --- |
| 1 | xUnit HTTP/PostgreSQL test infrastructure, fixed clock and isolated fixtures | Complete: 3 fixture checks and 7 legacy tests passed; [execution](lab6-execution-record.md) |
| 2 | TC6-U01/U02/U12–U16: catalog, availability, registration, editing, eligibility | Not run |
| 3 | TC6-U03–U09: checkout, return, histories, queries | Not run |
| 4 | TC6-E01–E08: edge/failure/constraints/rollback/transport/persistence/restart | Not run |
| 5 | MA6-01/02, defect corrections and affected/broader regression | Not run |
| 6 | Completed UC/BR/API/TC6/test/results/evidence matrices, final README and tag | Pending |

These are meaningful development groups, not a required commit count. A defect
may justify a separate fix. The 22 objectives need enabled identifiable xUnit
assertions; neither this plan nor old Lab 5 green output proves their completion.

## Test infrastructure and scope

`EquipmentBorrowing.IntegrationTests` hosts the real API entry point through
`WebApplicationFactory<Program>`. Routing, binding, middleware and response
serialization execute through ASP.NET Core TestServer. This is an in-process
HTTP pipeline, not a listening TCP server and not an actual API process restart.
The latter is a separate requirement of TC6-E08 and must have its own evidence.

Every `Scenario` creates a uniquely named `lab6_<GUID>` PostgreSQL database,
applies the existing EF migration and inserts the S1–S4/E1–E7 fixture. Each
scenario/subcase starts fresh; no process-global connection variable is changed
by the tests. The test host replaces the context factory with one targeting
only that scenario. Snapshot reads open a new non-pooled PostgreSQL connection.
Reset uses one transaction. Cleanup may drop only its generated database name;
an arbitrary name or the administrator's configured database is never a target.
If setup fails, tests fail clearly rather than skipping or using SQLite/InMemory.
If a test process is forcibly terminated, its `lab6_<GUID>` DB may need manual
cleanup after confirming it is an unused test DB.

Fixed local date **D = 2026-11-10**, at UTC+08, is independent of Windows time.
Fixture dates use D-9/D-1, D-8/D+2/D-2 and D-7/D+2 as specified. Test host injects
a fixed TimeProvider into LendingService and EfBorrowingRepository; production
defaults to TimeProvider.System and still uses workstation local calendar dates.
No public clock-setting route or production environment date override is added.

The original seven legacy unit tests are retained. They exercise older in-memory
service behavior and do not prove PostgreSQL, routing or transactional behavior.
The three Phase 1 infrastructure tests verify migrations/seed relationships,
cross-scenario isolation/reset and fixed-date validation/checkout/return using
fresh persisted state. Full TC6 coverage will be mapped in subsequent phases.
