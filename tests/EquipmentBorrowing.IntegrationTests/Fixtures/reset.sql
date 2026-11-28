-- Executed only against a newly generated lab6_<GUID> database.
-- @d is a typed date parameter supplied by the test fixture, not workstation time.
TRUNCATE TABLE "Borrowings", "Equipment", "Students" RESTART IDENTITY;
INSERT INTO "Students" ("Id", "StudentNumber", "Name", "Contact", "IsAllowedToBorrow") VALUES
(1,'ST-100','Ada',NULL,true), (2,'ST-200','Ben',NULL,false),
(3,'ST-300','Cara',NULL,true), (4,'ST-400','Dani',NULL,true);
INSERT INTO "Equipment" ("Id", "Identifier", "Name", "Description", "IsAvailable") VALUES
(1,'EQ-001','Camera',NULL,false), (2,'EQ-002','Laptop',NULL,true),
(3,'EQ-003','Projector',NULL,true), (4,'EQ-004','Tablet',NULL,false),
(5,'EQ-005','Microphone',NULL,false), (6,'EQ-006','Tripod',NULL,true),
(7,'EQ-007','Speaker',NULL,true);
INSERT INTO "Borrowings" ("StudentId","EquipmentId","DateBorrowed","ExpectedReturnDate","ActualReturnDate","Status") VALUES
(1,1,@d - 9,@d - 1,NULL,'Active'),
(1,3,@d - 8,@d + 2,@d - 2,'Returned'),
(3,4,@d - 7,@d + 2,NULL,'Active'),
(3,5,@d - 7,@d + 2,NULL,'Active');
SELECT setval(pg_get_serial_sequence('"Students"','Id'),100,false);
SELECT setval(pg_get_serial_sequence('"Equipment"','Id'),100,false);
