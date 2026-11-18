# Lab 4 catalog, checkout, and return verification

Codex ran these application-service checks against a disposable PostgreSQL database and used fresh connections to verify the persisted results.

| Check | Observed result |
| --- | --- |
| Catalog and available equipment | Four seeded items were listed. After checkout, the checked-out item was excluded from the available list. |
| Single checkout | An eligible student received one active loan. The active list showed the student, item, and expected return date. A fresh database read showed the item was unavailable. |
| Request rejection | An ineligible student, missing equipment, and an expected return date of today were rejected. |
| Return | The same loan was preserved as returned, with an actual return date. A fresh connection showed the item was available. A repeat return was rejected. |
| Database constraints | PostgreSQL rejected a second active loan for the same item and a loan referencing a nonexistent student. |

The desktop restart walkthrough and database disconnect demonstration are separate checks.
