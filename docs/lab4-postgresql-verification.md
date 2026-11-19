# Lab 4 PostgreSQL migration verification

Codex ran the initial migration on a fresh, disposable PostgreSQL 18 database. A separate PostgreSQL client then read the migration history and counted the records.

| Check | Expected | Observed |
| --- | --- | --- |
| Migration history | Initial PostgreSQL migration recorded | `20261118125230_InitialPostgreSql` recorded |
| Seed students | 3 | 3 |
| Seed equipment | 4 | 4 |
| Initial borrowings | 0 | 0 |

Close and reopen the app during the checkout and return verification. Those workflows are checked separately from initial migration.
