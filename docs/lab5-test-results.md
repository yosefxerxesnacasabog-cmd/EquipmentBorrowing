\# Lab 5 Acceptance Test Results



\## T01 — Independent API reads



Date: November 26, 2026

Database: equipment\_borrowing\_lab5\_test

Environment: PostgreSQL and API running; desktop closed.



Expected: Correct related records, empty arrays for empty histories,

404 for missing resources and routes, 405 for an unsupported method,

and no changes to the controlled dataset.



Observed: Student and equipment lists matched the dataset. Ada's

history contained an active Camera loan and a returned Projector loan.

Dani's history returned 200 with \[]. Camera details, Camera history,

Projector history, borrowing detail, and active borrowings returned 200.

Missing-resource and unknown-route requests returned safe JSON errors

with 404. DELETE equipment returned 405.



OpenAPI listed API-01 through API-16 with methods, routes and status

codes. Detailed schema comparison remains pending.



Fresh PostgreSQL read: 4 students, 7 equipment items, 4 borrowings.



Correction and retest: The Camera variable was initially unset.

After resolving its ID from the equipment list, both Camera requests

returned the expected 200 responses.



Result: Executed read and status checks passed. Full OpenAPI schema

comparison remains pending.




## T02 — Student management

Date: November 26, 2026

Observed: ST-500 was created as eligible with null contact, then
updated under the same ID, 100. A fresh PostgreSQL read confirmed
the changed name and contact. Blank-name registration returned 400.
A normalized duplicate student number returned 409.

After the eligibility sequence, PostgreSQL showed Ada eligible,
her Camera loan returned, and a new active Laptop loan. Both Ada's
history and Eli's edited details returned 200 after API restart.

Result: Registration, editing, rejection and restart readback passed.
The HTTP status captures for the eligibility sequence still need
to be included in the evidence.

## Remaining tests

T03-T08: Not run yet.
