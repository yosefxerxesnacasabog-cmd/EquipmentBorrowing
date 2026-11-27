-- Lab 5 acceptance dataset. Run only in the dedicated test database.
-- Re-running this script replaces all student, equipment and borrowing test rows.
BEGIN;
DO $$
BEGIN
    IF current_database() <> 'equipment_borrowing_lab5_test' THEN
        RAISE EXCEPTION 'Select equipment_borrowing_lab5_test before running this script.';
    END IF;
END $$;

TRUNCATE TABLE "Borrowings", "Equipment", "Students" RESTART IDENTITY;

INSERT INTO "Students" ("Id", "StudentNumber", "Name", "Contact", "IsAllowedToBorrow")
VALUES
    (1, 'ST-100', 'Ada Cruz', NULL, true),
    (2, 'ST-200', 'Ben Diaz', NULL, false),
    (3, 'ST-300', 'Cara Lim', NULL, true),
    (4, 'ST-400', 'Dani Ong', NULL, true);

INSERT INTO "Equipment" ("Id", "Identifier", "Name", "Description", "IsAvailable")
VALUES
    (1, 'EQ-001', 'Camera', NULL, false),
    (2, 'EQ-002', 'Laptop', NULL, true),
    (3, 'EQ-003', 'Projector', NULL, true),
    (4, 'EQ-004', 'Tablet', NULL, false),
    (5, 'EQ-005', 'Microphone', NULL, false),
    (6, 'EQ-006', 'Tripod', NULL, true),
    (7, 'EQ-007', 'Speaker', NULL, true);

-- Dates use Asia/Manila and the handout's offsets from the observed local date.
WITH clock AS (SELECT (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Manila')::date AS today)
INSERT INTO "Borrowings"
    ("StudentId", "EquipmentId", "DateBorrowed", "ExpectedReturnDate", "ActualReturnDate", "Status")
SELECT 1, 1, today - 9, today - 1, NULL, 'Active' FROM clock
UNION ALL SELECT 1, 3, today - 8, today + 2, today - 2, 'Returned' FROM clock
UNION ALL SELECT 3, 4, today - 7, today + 2, NULL, 'Active' FROM clock
UNION ALL SELECT 3, 5, today - 7, today + 2, NULL, 'Active' FROM clock;

SELECT setval(pg_get_serial_sequence('"Students"', 'Id'), 100, false);
SELECT setval(pg_get_serial_sequence('"Equipment"', 'Id'), 100, false);
COMMIT;

SELECT current_database() AS database,
       (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Manila')::date AS observed_local_date;
SELECT "StudentNumber", "Name", "IsAllowedToBorrow" FROM "Students" ORDER BY "Id";
SELECT "Identifier", "Name", "IsAvailable" FROM "Equipment" ORDER BY "Id";
SELECT s."StudentNumber", e."Identifier", b."Status",
       b."DateBorrowed", b."ExpectedReturnDate", b."ActualReturnDate"
FROM "Borrowings" b
JOIN "Students" s ON s."Id" = b."StudentId"
JOIN "Equipment" e ON e."Id" = b."EquipmentId"
ORDER BY b."Id";
