-- 1. Basic Retrieval
-- Retrieve all equipment.

SELECT *
FROM Equipment;


-- 2. Filtering
-- Retrieve only currently available equipment.

SELECT *
FROM Equipment
WHERE IsAvailable = 1;


-- 3. Join
-- Retrieve active borrowings with student and equipment information.

SELECT
    Students.Name AS Student,
    Equipment.Name AS Equipment,
    Borrowings.DateBorrowed AS Borrowed,
    Borrowings.ExpectedReturnDate AS Due
FROM Borrowings
INNER JOIN Students
    ON Borrowings.StudentId = Students.Id
INNER JOIN Equipment
    ON Borrowings.EquipmentId = Equipment.Id
WHERE Borrowings.Status = 'Active';


-- 4. Aggregate
-- Count the number of active borrowings.

SELECT COUNT(*) AS ActiveBorrowings
FROM Borrowings
WHERE Status = 'Active';


-- 5. Update
-- Mark an equipment item as available.

UPDATE Equipment
SET IsAvailable = 1
WHERE Id = 1;