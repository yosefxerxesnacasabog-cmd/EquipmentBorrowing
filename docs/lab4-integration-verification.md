# Lab 4 integration verification

Verification used disposable PostgreSQL 18 databases and the Avalonia desktop app. Test data used sample students, sample equipment, and a test-only item.

| Area | Observed result |
| --- | --- |
| Migration and constraints | The initial migration applied to an empty database. A fresh client read three students, four equipment items, and zero borrowings. PostgreSQL rejected a second active loan for the same item and an invalid student foreign key. |
| Single checkout and return | The active list displayed the borrower and item. Return preserved the same borrowing row, set the actual return date, and made the item available again. A repeat return was rejected. |
| Student management | Registration rejected blank required fields and normalized duplicates. Editing retained the student identity and existing loan. Restriction blocked a new loan while allowing return. Restoring eligibility allowed checkout again. |
| Equipment management | The desktop app registered and renamed an item and rejected a duplicate identifier. The rename survived restart. Separate application-service checks confirmed an edit preserved the loan relationship and availability. |
| Coordinated checkout | The desktop app rejected a one-item request in multi-item mode, then checked out two items in one request. Both active loans and unavailable flags survived restart. A forced second-insert failure rolled back the entire request. Competing requests preserved the active-loan limit and prevented two active loans for the same item. |
| History and search | The desktop app displayed active and returned loans for the selected student and equipment. An item with no history showed an empty state. A partial equipment-name search returned the expected row. Service checks covered the three searchable fields, status filters, both date sorts in both directions, tie ordering, cleared criteria, and connection failure. |
| Build and tests | The solution built and all seven existing unit tests passed. |

## Repeat the desktop readback

1. Follow the Lab 4 setup in the repository README to migrate an empty test database and launch the app.
2. Register `EQ-DESKTOP-04`, rename it, and try the identifier again with different case. Expect one item with the updated name.
3. Check out `EQ-001` and `EQ-002` to `STU-001` in one request with a future return date.
4. Close and reopen the app. Confirm both active loans in **Borrowings** and the edited equipment in the catalog.
5. Open a fresh PostgreSQL client connection and run this query.

```sql
SELECT s."StudentNumber", e."Identifier", b."Status", e."IsAvailable"
FROM "Borrowings" b
JOIN "Equipment" e ON e."Id" = b."EquipmentId"
JOIN "Students" s ON s."Id" = b."StudentId"
WHERE s."StudentNumber" = 'STU-001'
  AND e."Identifier" IN ('EQ-001', 'EQ-002')
ORDER BY e."Identifier";
```

Expect two `Active` rows, each with `IsAvailable` set to false.
