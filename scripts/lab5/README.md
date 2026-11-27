# Lab 5 acceptance tools

Use only a disposable database named `equipment_borrowing_lab5_test`.
`reset.sql` replaces its application rows; it never targets another database.
The database must already have the checked-in migration applied.

`acceptance.py` uses Python 3's standard library, `psql` on PATH (or `PSQL`),
and the already built .NET 10 API. It starts/stops its own API on port 5185.
PostgreSQL must be running. Configure the same database through both
`EQUIPMENT_BORROWING_CONNECTION` and PostgreSQL's `PGHOST`, `PGPORT`,
`PGUSER`, `PGDATABASE` and, when needed, `PGPASSWORD`. Keep values local.

```powershell
dotnet build EquipmentBorrowing.slnx
python scripts/lab5/acceptance.py
```

Captured requests, statuses, response JSON and independent PostgreSQL snapshots
are written to `docs/evidence/lab5/T01.json` through `T08.json` (T07 has a
separate UI record). Every mutation records fresh connections before and after.
Rejected mutations must leave all application rows unchanged. Sequence gaps
after a rolled-back INSERT are normal and are not application rows.

The T05 test creates and removes a temporary trigger and sequence **only in this
database**. The first borrowing insert must occur; the second raises a controlled
exception. A nontransactional sequence probe proves the fault followed the first
insert. The same snapshots must survive the resulting 500 unchanged.

Dates follow the handout offsets relative to the local date of execution;
the original November 10 example date is not hardcoded into future checkouts.
API IDs are resolved from institution/equipment codes after every reset.

For the desktop walkthrough, use `python scripts/lab5/acceptance.py --serve`.
It resets the disposable data and serves the API until interrupted. Start the
desktop in another terminal using the API URL from the main README. This is an
interactive test, recorded separately; a console test cannot establish UI behavior.
