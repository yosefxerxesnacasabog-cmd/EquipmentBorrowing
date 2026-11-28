# Lab 6 execution and diagnostic record

## Phase 1 - infrastructure verification

Execution crossed midnight November 28-29, 2026 (Asia/Manila). Environment:
Windows, .NET SDK 10.0.401/runtime 10.0.12, PostgreSQL 18.6, xUnit 2.9.3,
ASP.NET Core MVC Testing 10.0.12. The disposable local PostgreSQL cluster used
port 55432; test databases were unique per Scenario. No application database
was reset. Fixed local date D = 2026-11-10 at UTC+08.

| Actual implementation | Expected | Observed | Result |
| --- | --- | --- | --- |
| `FixtureVerificationTests.Migrated_fixture_exposes_deterministic_related_records_through_real_pipeline` | Migrations/fixture present; actual HTTP 200, seven items, four students/four loans, related Camera/Ada and exact seed offsets; no row changes | All enabled assertions satisfied | Passed |
| `FixtureVerificationTests.Independent_scenarios_do_not_share_rows_and_reset_restores_the_same_fixture` | Two independent databases; POST in first affects no rows in second; reset restores original first snapshot | All enabled assertions satisfied | Passed |
| `FixtureVerificationTests.Checkout_validation_and_persisted_dates_use_injected_clock_instead_of_machine_date` | Reject due D without writes; accept due D+2 even though machine date differs; checkout/return and fresh PostgreSQL dates equal D | 400 then 201/200; asserted dates/retained returned row through independent connections | Passed |
| Original `BorrowEquipmentServiceTests` (seven methods) | Existing legacy service checks remain green | Seven passed, zero failed | Passed within their in-memory scope |
| Generated database cleanup | No `lab6_<GUID>` databases remain after fixture disposal | Fresh PostgreSQL catalog count: zero | Passed |

Evidence: [sanitized results](evidence/lab6/phase1-results.json), generated from
the actual TRX execution results. Raw TRX stays locally in ignored `TestResults/`
because it contains machine paths. The baseline, commands and fixture scope are
documented in [baseline/plan](lab6-baseline-and-plan.md) and [README](../README.md).

## Diagnostics and limitations

- Initial compilation reported two xUnit analyzer warnings about filtering before
  `Assert.Single`. Corrected to its predicate overload; the execution rebuilt the
  test project and all enabled assertions passed.
- Tool communication was interrupted while waiting for test output. Inspection
  of saved TRX showed both projects had completed successfully (3 + 7 tests).
  Results were recovered from those files; a separate fresh PostgreSQL catalog
  check confirmed cleanup rather than assuming the interrupted tool meant a test
  pass or failure.
- Initial migration-history query on a new database logs an absent-table error
  before EF creates the schema. The migrations and subsequent fixture/HTTP
  assertions completed successfully; this is not classified as an app defect.
- No discovered application defect is claimed in this phase. Clock injection is
  a testability refactor: production defaults remain system-local calendar dates.
- These are fixture checks, not the completed 22 TC6 objectives. TC6-E08 actual
  process restart, TC6-E01-E08 failure scenarios and MA6-01/02 remain Not Run.
  In-process WebApplicationFactory startup is not evidence of process restart.

## Remaining execution record

| Required objectives | Status | Planned phase |
| --- | --- | --- |
| TC6-U01/U02/U12-U16 | Not Run as mapped TC6 scenarios | 2 |
| TC6-U03-U09 | Not Run as mapped TC6 scenarios | 3 |
| TC6-E01-E08 | Not Run | 4 |
| MA6-01/02 | Not Run | 5 |

Later phases must add the actual test methods, expected/observed results, defect
references, correction/retest and evidence links for every required scenario.
