# Lab 4 student management verification

With ChatGPT assistance, the student application-service checks were run against disposable PostgreSQL and persisted records were verified with fresh contexts.

| Check | Observed result |
| --- | --- |
| Registration | Registration trimmed the number, name, and contact, and the saved student was eligible. Blank required fields and a duplicate number with different casing and spaces were rejected. |
| Update | The new student number and name persisted and the optional contact cleared. An edit using another student's number was rejected. The existing loan kept the same student relationship. |
| Eligibility | Restriction blocked a new checkout, kept the active loan, and allowed its return. Repeating the restriction produced feedback. Restoring eligibility allowed another checkout. |
| Readback | A fresh database context found both loans linked to the same student. |

The desktop walkthrough is recorded separately once performed.
