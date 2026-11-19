# Lab 4 multi-item checkout verification

The solution built after the multi-item checkout changes. Windows Application Control prevented a desktop walkthrough on the partner's PC.

On a second PC, the desktop app rejected a one-item request in multi-item mode. Checking out `EQ-001` and `EQ-002` to `STU-001` succeeded in one request. After restart, both loans appeared in Active Borrowings. A fresh PostgreSQL query confirmed that both loans were active and both items were unavailable.

A separate PostgreSQL test forced a constraint error during the second borrowing insert. A database probe confirmed the first row was visible inside the transaction before the failure. A fresh connection showed zero loans for that request and both items still available. Additional checks covered duplicate selections, missing and unavailable items, restricted students, invalid dates, the three-item limit, and competing checkout requests.
