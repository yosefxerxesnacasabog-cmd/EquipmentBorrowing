# Lab 5 consolidated acceptance results

Initial manual execution: November 26, 2026, Jazz Dwight W. Salac's PC.
Completion and retest: November 27, 2026, Yosef Xerxes Nacasabog's PC,
Windows, .NET SDK 10.0.401/runtime 10.0.12, PostgreSQL 18.
The final run uses a dedicated local cluster on port 55432 and disposable
`equipment_borrowing_lab5_test`; no shared or submitted data was reset.

The reference's November 10 examples are evaluated using November 27 as today.
Seed offsets are unchanged: Camera overdue yesterday, returned Projector two
days ago, Tablet/Microphone active and due in two days. New issues are due
November 29. IDs are resolved from student numbers/equipment codes via the API.

## Execution and evidence

Run [the acceptance script](../scripts/lab5/acceptance.py) after applying the
existing migration. [Tool instructions](../scripts/lab5/README.md) describe the
disposable reset and fault setup. JSON captures contain method, route, request,
expected/observed status and response. Every mutation, including each rejection,
has before/after row snapshots from separate `psql` connections covering Students,
Equipment and Borrowings. Sequence gaps are not partial application writes.

| Test | Expected | Observed on completion run | Result / evidence |
| --- | --- | --- | --- |
| T01 Independent API reads | Correct lists/details/relations, empty history, missing/route 404, method 405; accurate OpenAPI; no writes | 4 students, 7 items, 4 loans, 4 available items. Ada active/returned, Dani empty. All expected statuses and 16 OpenAPI operations. Served JSON equals checked-in contract; actual response fields/types/required properties/date formats validated against schemas throughout the run. | Passed — [T01](evidence/lab5/T01.json) |
| T02 Student management | Create 201+Location, edits/eligibility 200, blank 400, normalized duplicate/restricted issue 409; history/restart intact | ST-500 created once, eligible with null contact; ID 100 retained on edit. Restrict Ada, reject issue, return Camera, permit Ada, valid Laptop issue and repeated permit gave expected statuses. Fresh rows and restart readback retained edits/history. | Passed — [T02](evidence/lab5/T02.json) |
| T03 Equipment management | Create available item 201+Location, edit 200; blank/protected fields 400; duplicate 409; restart | EQ-008 created/edited under the same ID. Camera descriptive edit retained unavailable state and borrowing IDs. Rejections left application rows unchanged. Edits survived API restart. | Passed — [T03](evidence/lab5/T03.json) |
| T04 Single checkout/return | 201 then 200 returned; invalid date 400, missing 404, restricted/unavailable/repeat return 409; restart | Dani/Laptop created one active loan and unavailable item; same loan returned with November 27 actual date and item available. Bad date syntax/today, missing student/item, restricted/unavailable and repeat return rejected without changes. Returned loan persisted across restart. | Passed — [T04](evidence/lab5/T04.json) |
| T05 Batch and rollback | Two-item 201; exactly-three boundaries succeed; over-limit 409; malformed 400, missing 404; injected fault 500 with no partial writes | Dani E2/E6 persisted two loans. Reset Cara+E2 single and Ada+E2/E6 batch each reached exactly three. Cara+two, duplicates, one item, unavailable item, restricted student, missing item and invalid date each left reset snapshot unchanged. Fault during second INSERT returned safe 500 and rolled back all loan/availability changes. | Passed — [T05](evidence/lab5/T05.json) |
| T06 Histories/queries | Correct subjects, empty/missing semantics, partial mixed-case search, each status, both date sorts/directions with stable ties, combined/reset/no-match | Ada/Dani and Camera/Projector histories matched seed. Name/number/equipment searches, All/Active/Returned, four orders, combined query, clearing parameters, no match and invalid-status 400 matched expected membership/order. Fresh database unchanged. | Passed — [T06](evidence/lab5/T06.json) |
| T07 Desktop HTTP | Independent API; responsive UI registration/edit/issue/return/history; external update appears after Refresh; no desktop DB access | API reads succeeded before desktop launch. Desktop ran without a database connection variable. UI-500/UI-EQ registered/edited, checkout/return succeeded, returned history showed November 27. Independent PUT changed student name; Refresh displayed it. Adapter harness passed 14 checks. | Passed — [UI/database](evidence/lab5/T07.json), [walkthrough below](#desktop-walkthrough), [client checks](evidence/lab5/client-check.txt) |
| T08 Failures/recovery | No HTTP response when API stopped; safe received 500 with invalid DB; distinct UI messages; recovery/readback; no blind retry | Real stopped API produced connection refusal. Invalid disposable connection returned 500 for read/write; actual DB unchanged. Restored API read prior committed loan. UI showed connection/database messages and recovered. Unknown-outcome retry gate passed deterministic handler simulation. | Passed — [wire/database](evidence/lab5/T08.json), [client checks](evidence/lab5/client-check.txt), [UI below](#failure-feedback-and-retest) |

## Atomic failure point

T05 installs a temporary BEFORE INSERT trigger only in the test DB. First insert
proceeds. On the second, the trigger verifies the first loan is visible inside
the transaction, increments a nontransactional sequence again, then raises.
Sequence value 3 proves the first insert was observed before failure. A fresh
connection sees the exact original application rows and availability. Fault
objects are removed in `finally`. This is a known pre-commit failure, not a real
lost commit acknowledgement. Client handling of the latter is simulated;
server `committing` classification is inspected in `PostgresOperations`.

## Desktop walkthrough

UI tests restored the controlled dataset after console checks. Only API URL was
supplied to the desktop; its database connection variable was removed. The API
retained its own connection. Screenshots supplement HTTP and fresh DB captures.

1. Equipment reflects seed availability. [Capture](evidence/lab5/T07-catalog.png).
2. Students: register UI-500 / UI Student; edit to UI Student Edited.
   [Capture](evidence/lab5/T07-student.png).
3. Manage Equipment: register UI-EQ / UI Item; edit to UI Item Edited.
   [Capture](evidence/lab5/T07-equipment.png).
4. Borrow Equipment: choose those records, due November 29. Active list shows
   saved relationship. [Capture](evidence/lab5/T07-active.png).
5. Return selected loan. It leaves active list; History & Search shows Returned
   and November 27 actual date. [Capture](evidence/lab5/T07-history.png).
6. Independent PUT changes name to UI Student via Independent HTTP. Refresh
   shows it in list and editor. [Capture](evidence/lab5/T07-independent-refresh.png).

Fresh PostgreSQL state and GET responses after checkout, return, independent
update and API recovery are in T07.json. Await-based ViewModels/HttpClient and
absence of desktop persistence dependencies support the UI/code walkthrough.

## Failure feedback and retest

First outage demo found old `Changes saved successfully.` text below a new load
error. Diagnosis: manual Refresh reused previous action feedback. Correction:
the four mutation ViewModels clear feedback on explicit refresh; internal reloads
preserve the current mutation result, including unknown-outcome warnings.
Rebuilt solution and all seven original regression tests passed again.

Corrected-build retest: after a successful save, explicit Refresh during outage
clears old success text and shows `Cannot reach the API. Start the API and check
its address, then retry.` Reachable API with invalid DB instead shows `The server
could not load records from the database. Try again later.` Restoring configuration
and Refresh reloads persisted data. Captures: [connection](evidence/lab5/T08-api-unreachable.png),
[server error](evidence/lab5/T08-server-error.png), [recovery](evidence/lab5/T08-recovery.png).
Detailed local server logs are ignored by Git; safe wire responses are retained.

## Earlier issues, corrections and retests

- Jd's November 26 progress is preserved in `a071d8e`. Camera variable initially
  unset: resolving its ID via API corrected the route and GET returned 200.
- Repeated creation returned 409 while PowerShell retained old `$response` 201.
  GET ST-500 and fresh PostgreSQL confirmed one saved ID 100. Completion script
  records each response independently; no stale variable supplies evidence.
- Empty-database migration history lookup logged an error, then migration applied
  successfully. Subsequent schema/read checks passed.
- Jd's desktop executable was blocked by Windows Application Control previously.
  No policy was bypassed. Actual UI demo was performed on Xerxes's PC; this does
  not claim Jd ran it on his PC.
- Initial completion UI launcher failed on an empty-log readiness check. Correcting
  the local launcher enabled the walkthrough; production data/source was unaffected.

All eight supplied cases were executed, including corrected UI feedback.
Original regression suite: 7 passed. HTTP adapter harness: 14 checks passed; this
is separate from actual UI evidence. A real lost commit acknowledgement was not
manufactured: a test handler checks retry gating, and staff must inspect persisted
history before deciding to retry after an unknown outcome.
