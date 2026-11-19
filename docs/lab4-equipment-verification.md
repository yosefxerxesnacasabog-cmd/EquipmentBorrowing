# Lab 4 equipment management verification

The build succeeded and the initial PostgreSQL migration applied on the partner's PC. Windows Application Control blocked the desktop executable there.

On a second PC, the desktop app registered `EQ-DESKTOP-04` as available, saved its name as `Desktop Check Camera Updated`, and rejected a duplicate identifier with different casing. After restarting the app, the catalog still showed the updated name. A fresh PostgreSQL query confirmed the renamed row was still available. A separate application-service check edited an item with an active loan and confirmed that its ID, loan relationship, and unavailable state were preserved.
