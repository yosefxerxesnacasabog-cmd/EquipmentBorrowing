# Campus Equipment Borrowing System

## Laboratory Activity 1

**ITSD 81 -- Desktop Application Development**

**Prepared by:**

Yosef Xerxes S. Nacasabog\
Jazz Dwight W. Salac

**August 2026**

------------------------------------------------------------------------

# Part A -- Requirements and Use Case Analysis

## A. Actors

### Actor: Student

**What the actor expects the system to do:**

The student expects the system to allow them to request available
equipment, borrow equipment when the requirements are satisfied, and
return borrowed equipment.

------------------------------------------------------------------------

## B. Use Cases

### Use Case 1 -- Borrow Equipment

  -----------------------------------------------------------------------
  Item                                Description
  ----------------------------------- -----------------------------------
  **Use Case**                        **Borrow Equipment**

  **Primary Actor**                   Student

  **Preconditions**                   The student is currently allowed to
                                      borrow equipment; the requested
                                      equipment exists; the equipment is
                                      available; and the student has not
                                      reached the maximum number of
                                      active borrowings.

  **Main Action**                     The student requests to borrow a
                                      piece of equipment. The system
                                      verifies the student's eligibility,
                                      checks that the equipment exists
                                      and is available, checks the
                                      student's active borrowing limit,
                                      and records the borrowing if all
                                      conditions are satisfied.

  **Expected Result**                 The borrowing is successfully
                                      recorded with the student,
                                      equipment, date borrowed, expected
                                      return date, and borrowing status.
                                      The equipment becomes unavailable
                                      for another borrowing.

  **Possible Failure**                The student is not allowed to
                                      borrow; the equipment does not
                                      exist; the equipment is
                                      unavailable; or the student has
                                      reached the maximum number of
                                      active borrowings.
  -----------------------------------------------------------------------

### Use Case 2 -- Return Equipment

  -----------------------------------------------------------------------
  Item                                Description
  ----------------------------------- -----------------------------------
  **Use Case**                        **Return Equipment**

  **Primary Actor**                   Student

  **Preconditions**                   The student has an existing active
                                      borrowing for the equipment being
                                      returned.

  **Main Action**                     The student returns the borrowed
                                      equipment. The system identifies
                                      the active borrowing, marks the
                                      borrowing as returned, and changes
                                      the equipment's status to
                                      available.

  **Expected Result**                 The borrowing is marked as
                                      **Returned**, and the equipment
                                      becomes available for another
                                      student to borrow.

  **Possible Failure**                The student does not have an active
                                      borrowing for the equipment being
                                      returned, or the borrowing record
                                      cannot be found.
  -----------------------------------------------------------------------

### Use Case 3 -- Find Available Equipment

  -----------------------------------------------------------------------
  Item                                Description
  ----------------------------------- -----------------------------------
  **Use Case**                        **Find Available Equipment**

  **Primary Actor**                   Student

  **Preconditions**                   Equipment records are available in
                                      the system.

  **Main Action**                     The student requests available
                                      equipment. The system checks the
                                      equipment records and identifies
                                      equipment that is currently
                                      available for borrowing.

  **Expected Result**                 The system provides the student
                                      with the available equipment that
                                      can potentially be borrowed.

  **Possible Failure**                No equipment is available, or the
                                      requested equipment does not exist.
  -----------------------------------------------------------------------

------------------------------------------------------------------------

## C. Identify Domain Concepts

  ---------------------------------------------------------------------------
  Domain Concept        Information It    Rules / State     Should NOT Be
                        Contains                            Responsible For
  --------------------- ----------------- ----------------- -----------------
  **Student**           Student ID, name, Whether the       Database
                        borrowing         student is        operations, UI,
                        eligibility       allowed to borrow repository
                                                            operations

  **Equipment**         Equipment ID,     Whether the       Database
                        name,             equipment is      operations, UI,
                        availability      currently         borrowing
                                          available         workflow

  **Borrowing**         Student,          Active or         Database
                        equipment, date   Returned          operations, UI,
                        borrowed,                           finding unrelated
                        expected return                     equipment
                        date, status                        

  **BorrowingStatus**   Active, Returned  Represents the    Database access,
                                          current state of  UI, application
                                          a borrowing       workflow
  ---------------------------------------------------------------------------

------------------------------------------------------------------------

# Part B -- Architecture Explanation

## 1. Solution Structure

### Domain

The **Domain** project contains the important concepts and states of the
Campus Equipment Borrowing System.

Examples include:

-   `Student`
-   `Equipment`
-   `Borrowing`
-   `BorrowingStatus`

The Domain represents the problem itself and does not handle database
operations or user-interface code.

### Application

The **Application** project contains the operations performed by the
system.

The main implemented application service is:

``` text
BorrowEquipmentService
```

It coordinates the domain objects and repository interfaces needed to
process a borrowing request.

### Infrastructure

The **Infrastructure** project contains the technical implementations of
the repository interfaces.

The current implementation uses in-memory repositories:

-   `InMemoryStudentRepository`
-   `InMemoryEquipmentRepository`
-   `InMemoryBorrowingRepository`

No database is used for this laboratory activity.

### Tests

The **Tests** project contains the automated xUnit tests used to verify
the borrowing service and its different outcomes.

### Console Demonstration

The project also contains `BorrowingSystemDemo`, which provides a simple
console demonstration of the implemented borrowing use case.

The console program only demonstrates the application flow. The actual
business logic remains in the Application project.

------------------------------------------------------------------------

## 2. Dependency Direction

The solution separates the responsibilities between the projects.

``` text
BorrowingSystemDemo
        |
        v
   Application
        |
        v
      Domain

Infrastructure
        |
        v
Application Interfaces
```

The Application layer depends on repository abstractions instead of
directly depending on the repository implementations.

The Infrastructure layer implements those repository abstractions.

This allows the storage implementation to be changed later without
changing the main application service.

------------------------------------------------------------------------

## 3. Use Case Mapping

The implemented use case is **Borrow Equipment**.

  -----------------------------------------------------------------------
  Item                                Implementation
  ----------------------------------- -----------------------------------
  **Actor**                           Student

  **Use Case**                        Borrow Equipment

  **Application Service**             `BorrowEquipmentService`

  **Domain Objects Used**             `Student`, `Equipment`,
                                      `Borrowing`, `BorrowingStatus`

  **Repository Interfaces Used**      `IStudentRepository`,
                                      `IEquipmentRepository`,
                                      `IBorrowingRepository`

  **Infrastructure Implementations    `InMemoryStudentRepository`,
  Used**                              `InMemoryEquipmentRepository`,
                                      `InMemoryBorrowingRepository`
  -----------------------------------------------------------------------

------------------------------------------------------------------------

## 4. Reflection

### 1. Why should the application service depend on a repository interface instead of directly depending on a database implementation?

The application service should depend on a repository interface because
it should focus on the business operation instead of how the data is
stored. This also makes the system easier to change and test.

### 2. Which parts of your current solution could remain unchanged if SQLite were added later?

The Domain models, Application service, and repository interfaces could
remain mostly unchanged. A new SQLite repository implementation could be
added in the Infrastructure project.

### 3. Which project would eventually contain Avalonia Views?

Avalonia Views would eventually be placed in a separate presentation or
desktop UI project. Avalonia is not required for this laboratory
activity.

### 4. Should an Avalonia button directly execute database queries? Why or why not?

No. An Avalonia button should call an application service instead of
directly executing database queries. This keeps the user interface,
business logic, and data access responsibilities separated.

### 5. What part of your implementation represents the actual business operation requested by the actor?

`BorrowEquipmentService.BorrowEquipmentAsync()` represents the main
business operation because it validates the borrowing request and
creates the borrowing record when the required conditions are satisfied.

------------------------------------------------------------------------

# Application Flow Demonstration

The implemented **Borrow Equipment** use case is demonstrated through a
simple console program.

## Successful Case

``` text
[TEST 1] Successful Borrowing Case
Student ID: 1 requests Equipment ID: 1

Result: SUCCESS
Borrowing record created successfully.
```

This demonstrates a successful request where the student is allowed to
borrow and the requested equipment is available.

## Failure Case

``` text
[TEST 2] Student Not Allowed Case
Student ID: 3 requests Equipment ID: 1

Result: FAILED
Student is not allowed to borrow equipment.
```

This demonstrates that the application rejects a borrowing request when
the student is not allowed to borrow.

------------------------------------------------------------------------

# Testing

The project uses xUnit for automated testing.

Current test result:

``` text
Test summary: total: 7, failed: 0, succeeded: 7, skipped: 0
```

All seven automated tests passed successfully.

------------------------------------------------------------------------

# Git Development

The project was developed using multiple Git commits.

The development history includes commits for the solution structure,
domain models, repository abstractions, services, in-memory
repositories, and automated tests.

------------------------------------------------------------------------

# Current Status

The solution builds successfully and all automated tests currently pass.

The project demonstrates:

-   C# and .NET
-   Separation of responsibilities
-   Domain models
-   Repository abstractions
-   In-memory repository implementations
-   Application service
-   Manual dependency injection
-   Automated testing
-   Successful borrowing demonstration
-   Failure case demonstration
-   Git-based development

The current solution now includes a graphical Avalonia desktop interface
for Equipment, Borrow Equipment, Active Borrowings, and Return
Equipment. The original Activity 1 domain, application, repository,
infrastructure, and testing work remains part of the solution.

------------------------------------------------------------------------

# Laboratory Activity 2 -- Avalonia UI and MVVM

## Desktop Project

A separate Avalonia desktop project named `EquipmentBorrowing.Desktop`
was added to the existing solution.

The Desktop project is responsible for the graphical user interface,
presentation state, user input, navigation, data binding, commands, and
user feedback.

The Desktop project references the Application and Infrastructure
projects. The Domain and Application projects do not depend on Avalonia.

The Desktop project uses:

-   Avalonia UI
-   XAML
-   MVVM
-   CommunityToolkit.Mvvm
-   Microsoft.Extensions.DependencyInjection
-   Existing Application services
-   Existing repository abstractions and in-memory repository
    implementations

### Main Desktop Components

``` text
EquipmentBorrowing.Desktop
├── ViewModels
│   ├── MainWindowViewModel.cs
│   ├── EquipmentViewModel.cs
│   ├── BorrowingsViewModel.cs
│   └── BorrowEquipmentViewModel.cs
├── Views
│   ├── EquipmentView.axaml
│   ├── BorrowingsView.axaml
│   └── BorrowEquipmentView.axaml
├── App.axaml
├── App.axaml.cs
├── MainWindow.axaml
└── MainWindow.axaml.cs
```

## Updated Architecture

The Activity 2 desktop interface extends the Activity 1 architecture
instead of replacing it.

``` text
User
  |
  v
Avalonia View
  |
  v
ViewModel
  |
  v
Application Service
  |
  v
Repository Interface
  |
  v
Infrastructure Repository
  |
  v
In-Memory Data
```

The main responsibilities are separated as follows:

  -----------------------------------------------------------------------
  Layer                               Responsibility
  ----------------------------------- -----------------------------------
  **View**                            XAML layout, controls, bindings,
                                      and presentation

  **ViewModel**                       Presentation state, selected
                                      values, observable collections,
                                      commands, and feedback messages

  **Application**                     Application operations and business
                                      workflows

  **Domain**                          Core entities and domain state

  **Application Interfaces**          Repository abstractions

  **Infrastructure**                  In-memory repository
                                      implementations

  **Desktop DI**                      Composition of repositories,
                                      services, ViewModels, and the main
                                      window
  -----------------------------------------------------------------------

The Desktop project does not recreate the borrowing rules. It calls the
existing `BorrowEquipmentService` and `ReturnEquipmentService`.

## Dependency Injection

Dependency injection is configured in `App.axaml.cs`, which serves as
the Desktop composition point.

The existing repositories are registered as singletons:

``` text
IStudentRepository  -> InMemoryStudentRepository
IEquipmentRepository -> InMemoryEquipmentRepository
IBorrowingRepository -> InMemoryBorrowingRepository
```

The application services are also registered:

``` text
BorrowEquipmentService
ReturnEquipmentService
```

The ViewModels receive their dependencies through constructors rather
than creating services or repositories themselves.

Using singleton repository instances also allows the in-memory state to
remain available while navigating between the Equipment, Borrow
Equipment, and Active Borrowings views.

## Borrow Equipment Flow

The graphical borrowing flow follows this sequence:

``` text
User selects student
        |
        v
User selects equipment
        |
        v
User selects expected return date
        |
        v
User clicks "Borrow Equipment"
        |
        v
BorrowEquipmentViewModel
        |
        v
BorrowEquipmentService
        |
        v
Student / Equipment / Borrowing Repositories
        |
        v
Borrowing created
Equipment marked unavailable
        |
        v
ViewModel refreshes the displayed data
        |
        v
Success or failure message is shown
```

The ViewModel performs presentation-level checks such as missing
selections and an invalid expected return date. The application service
continues to enforce the borrowing rules.

The tested failure cases include:

-   No student selected
-   No equipment selected
-   No expected return date
-   Expected return date is not in the future
-   Student is not allowed to borrow
-   Equipment is unavailable

## Active Borrowings

The **Active Borrowings** view displays the current active borrowing
records.

It shows information including:

-   Equipment name
-   Equipment ID
-   Student name
-   Date borrowed
-   Expected return date

The selected borrowing can be returned through the **Return Selected
Equipment** command.

## Return Equipment Flow

The graphical return flow follows this sequence:

``` text
User opens Active Borrowings
        |
        v
User selects an active borrowing
        |
        v
User clicks "Return Selected Equipment"
        |
        v
BorrowingsViewModel
        |
        v
ReturnEquipmentService
        |
        v
IBorrowingRepository
        |
        v
Active borrowing located
        |
        v
Borrowing marked as Returned
Equipment marked as Available
        |
        v
Active Borrowings refreshed
        |
        v
Success or failure message is shown
```

The ViewModel does not directly modify the borrowing or equipment state.
The return operation is handled by `ReturnEquipmentService`.

## Equipment Interface

The Equipment view uses data binding to display the equipment collection
provided by `EquipmentViewModel`.

Each equipment item displays:

-   Equipment ID
-   Equipment name
-   Availability

The displayed availability changes when equipment is borrowed or
returned.

## Navigation and Refresh

The main window provides navigation between:

-   Equipment
-   Borrowings
-   Borrow Equipment

`MainWindowViewModel` controls the current view and exposes commands for
navigation.

The affected information is refreshed after successful borrowing and
returning so that the interface reflects the current in-memory state.

## Shared Styles and Resources

Shared UI styles are defined in `App.axaml` instead of repeating the
same presentation settings throughout every view.

The shared resources include styles for:

-   Buttons
-   Headings
-   Form labels
-   Feedback messages

This keeps the interface consistent and reduces duplicated styling in
individual views.

## Architectural Reflection

Adding Avalonia did not require rewriting the Activity 1 business logic.
The Desktop project was added as a presentation layer on top of the
existing Domain, Application, repository abstractions, and
Infrastructure implementations.

The main lesson from the extension is that the user interface should
coordinate with application services rather than contain the business
rules itself. A user action travels through a View, ViewModel command,
application service, and repository abstraction before the result is
reflected back in the interface.

This separation also makes the application easier to test and allows the
presentation technology to remain independent from the core business
logic.

## Activity 2 Current Status

The Avalonia desktop application is working and has been tested for the
required operations:

-   Equipment display
-   Borrow Equipment
-   Active Borrowings
-   Return Equipment
-   Validation and business-rule feedback
-   Navigation between views
-   State refresh after borrowing and returning

The complete solution builds successfully with `dotnet build`.

------------------------------------------------------------------------

------------------------------------------------------------------------
# Laboratory Activity 3 -- From In-Memory Data to Persistent Storage

## 1. Relational Database Design

The Equipment Borrowing System was extended from in-memory storage to a relational SQLite database. The database stores information about students, equipment, and borrowing transactions.

### Database Diagram

The relational database design is represented by:

```text
Students 1 -------- * Borrowings * -------- 1 Equipment
```

The database diagram is available in:

```text
docs/database-diagram.png
```

### Tables

#### Students

| Column            | Description                                                  |
| ----------------- | ------------------------------------------------------------ |
| Id                | Primary key identifying the student                          |
| Name              | Student's name                                               |
| IsAllowedToBorrow | Indicates whether the student is allowed to borrow equipment |

#### Equipment

| Column      | Description                                            |
| ----------- | ------------------------------------------------------ |
| Id          | Primary key identifying the equipment                  |
| Name        | Name of the equipment                                  |
| IsAvailable | Indicates whether the equipment is currently available |

#### Borrowings

| Column             | Description                                                     |
| ------------------ | --------------------------------------------------------------- |
| Id                 | Primary key identifying the borrowing transaction               |
| StudentId          | Foreign key referencing `Students.Id`                           |
| EquipmentId        | Foreign key referencing `Equipment.Id`                          |
| DateBorrowed       | Date and time when the equipment was borrowed                   |
| ExpectedReturnDate | Expected return date                                            |
| Status             | Current status of the borrowing, such as `Active` or `Returned` |

### Keys

* `Students.Id` is the primary key of the `Students` table.
* `Equipment.Id` is the primary key of the `Equipment` table.
* `Borrowings.Id` is the primary key of the `Borrowings` table.
* `Borrowings.StudentId` is a foreign key referencing `Students.Id`.
* `Borrowings.EquipmentId` is a foreign key referencing `Equipment.Id`.

### Relationships

The database uses the following relationships:

```text
Students 1 -------- * Borrowings * -------- 1 Equipment
```

* One student can have many borrowing records.
* One equipment item can appear in many borrowing records over time.
* Each borrowing belongs to one student and one equipment item.

### Important Constraints

The database and EF Core configurations enforce important constraints:

* Primary keys uniquely identify records.
* Student and equipment names are required.
* Student and equipment names have maximum lengths.
* Equipment names are configured as unique values.
* `StudentId` and `EquipmentId` are required foreign keys.
* Foreign key relationships maintain referential integrity.
* The borrowing `Status` enum is converted for database storage.
* Equipment availability is represented by the `IsAvailable` property.
* A student must be allowed to borrow equipment before a borrowing can be created.

---

## 2. SQLite and EF Core

SQLite was introduced as the persistent database provider for the Equipment Borrowing System. Instead of storing records only in memory, the application now stores its data in an SQLite database file:

```text
equipmentborrowings.db
```

Entity Framework Core was added to provide an object-relational mapping layer between the C# application and the SQLite database.

EF Core allows the application to work with C# entities and LINQ queries while translating database operations into SQL statements that SQLite can execute.

The SQLite and EF Core implementation is located in the Infrastructure project.

The application registers the `EquipmentBorrowingDbContext` through dependency injection and configures it to use SQLite.

The resulting architecture is:

```text
Avalonia View
      ↓
ViewModel
      ↓
Application Service
      ↓
Repository Interface
      ↓
EF Core Repository
      ↓
EquipmentBorrowingDbContext
      ↓
SQLite Database
```

This allows the application to use persistent storage without placing database-specific code inside the UI.

---

## 3. DbContext

`EquipmentBorrowingDbContext` is the main Entity Framework Core database context for the application.

Its responsibilities include:

* Providing access to database tables through `DbSet` properties.
* Configuring the database model.
* Applying entity configurations.
* Managing relationships between entities.
* Tracking changes to entities.
* Saving changes to the SQLite database.
* Providing the EF Core entry point for LINQ queries.

The context contains the following `DbSet` properties:

```text
Students
Equipment
Borrowings
```

The entity configurations define the primary keys, required properties, maximum lengths, unique constraints, foreign keys, relationships, indexes, and enum conversion.

The `DbContext` is located in the Infrastructure layer so that the application and presentation layers do not need to directly depend on SQLite.

---

## 4. Repository Transition

In the previous laboratory activities, repository interfaces were connected to in-memory repository implementations.

The original structure was:

```text
Repository Interface
        ↓
In-Memory Repository
```

For Laboratory Activity 3, the repository implementations were replaced with EF Core repositories:

```text
Repository Interface
        ↓
EF Core Repository
        ↓
SQLite
```

The Infrastructure project now provides EF Core repository implementations such as:

```text
EfEquipmentRepository
EfStudentRepository
EfBorrowingRepository
```

The application services continue to depend on repository interfaces instead of directly depending on SQLite or EF Core.

This allowed the persistence implementation to change while keeping the application services and UI architecture largely unchanged.

---

## 5. Migration Process

EF Core migrations were used to create and update the SQLite database schema.

The migration was created using the EF Core command:

```powershell
dotnet ef migrations add SeedInitialData --project .\src\EquipmentBorrowing.Infrastructure --startup-project .\src\EquipmentBorrowing.Desktop
```

The resulting migration is:

```text
20260927074226_SeedInitialData
```

The database was updated using:

```powershell
dotnet ef database update --project .\src\EquipmentBorrowing.Infrastructure --startup-project .\src\EquipmentBorrowing.Desktop
```

The migration created the required database tables, relationships, constraints, and initial seed data.

The application also applies pending migrations when it starts by using:

```text
Database.Migrate()
```

The database contains the following tables:

```text
Students
Equipment
Borrowings
__EFMigrationsHistory
```

The `__EFMigrationsHistory` table records the EF Core migrations that have already been applied to the database.

---

## 6. Generated SQL

Three meaningful LINQ queries were implemented and tested against the SQLite database.

### Query 1: Retrieve Available Equipment

LINQ:

```csharp
var availableEquipment = await dbContext.Equipment
    .AsNoTracking()
    .Where(equipment => equipment.IsAvailable)
    .ToListAsync();
```

This query retrieves equipment that is currently available for borrowing.

Generated SQL:

```sql
SELECT "e"."Id", "e"."IsAvailable", "e"."Name"
FROM "Equipment" AS "e"
WHERE "e"."IsAvailable" = 1;
```

The LINQ `Where` condition is translated into the SQL `WHERE` clause. SQLite uses `1` to represent a true Boolean value.

### Query 2: Retrieve Active Borrowings with Student and Equipment

LINQ:

```csharp
var activeBorrowings = await dbContext.Borrowings
    .Include(borrowing => borrowing.Student)
    .Include(borrowing => borrowing.Equipment)
    .AsNoTracking()
    .Where(borrowing => borrowing.Status == BorrowingStatus.Active)
    .ToListAsync();
```

Generated SQL:

```sql
SELECT "b"."Id",
       "b"."DateBorrowed",
       "b"."EquipmentId",
       "b"."ExpectedReturnDate",
       "b"."Status",
       "b"."StudentId",
       "s"."Id",
       "s"."IsAllowedToBorrow",
       "s"."Name",
       "e"."Id",
       "e"."IsAvailable",
       "e"."Name"
FROM "Borrowings" AS "b"
INNER JOIN "Students" AS "s"
    ON "b"."StudentId" = "s"."Id"
INNER JOIN "Equipment" AS "e"
    ON "b"."EquipmentId" = "e"."Id"
WHERE "b"."Status" = 'Active';
```

The `Include` operations cause EF Core to retrieve the related student and equipment information using SQL joins.

### Query 3: Count Active Borrowings by Student

LINQ:

```csharp
var activeBorrowingCount = await dbContext.Borrowings
    .CountAsync(borrowing =>
        borrowing.StudentId == studentId &&
        borrowing.Status == BorrowingStatus.Active);
```

Generated SQL:

```sql
SELECT COUNT(*)
FROM "Borrowings" AS "b"
WHERE "b"."StudentId" = @studentId
  AND "b"."Status" = 'Active';
```

This query provides a meaningful summary by counting the number of active borrowing records belonging to a particular student.

The generated SQL was inspected during testing to verify that EF Core was translating the LINQ queries into SQL and executing them against SQLite.

---

## 7. Persistence Demonstration

The pair verified that borrowing information survives after the application is closed and restarted.

The persistence test was performed as follows:

1. The application was started.
2. An available Laptop was borrowed by a student.
3. The Laptop became unavailable.
4. The borrowing record appeared in the application.
5. The application was closed.
6. The application was started again.
7. The borrowing record was still present.
8. The Laptop remained unavailable.
9. The Laptop was returned.
10. The equipment became available again.
11. The application was closed and restarted.
12. The returned state remained stored.

The SQLite database was also opened using DB Browser for SQLite. The `Students`, `Equipment`, and `Borrowings` tables were inspected to confirm that the records were actually stored in the database.

This demonstrated that the application was no longer dependent on temporary in-memory data and could maintain its information between application sessions.

---

## 8. Architectural Reflection

### 1. Why did the application not need to be completely rewritten when SQLite was introduced?

The application did not need to be completely rewritten because it already used repository interfaces and application services. The services depended on abstractions instead of directly depending on the in-memory storage implementation.

The Infrastructure layer could therefore replace the in-memory repository implementations with EF Core repository implementations while keeping the existing application and presentation layers.

### 2. Why should the ViewModel not use DbContext directly?

The ViewModel is responsible for presentation and user interaction, while database access belongs to the Infrastructure layer.

If the ViewModel used `DbContext` directly, it would become tightly coupled to EF Core and SQLite. Keeping the database operations behind services and repository interfaces preserves separation of concerns.

The intended flow remains:

```text
View
 ↓
ViewModel
 ↓
Application Service
 ↓
Repository Interface
 ↓
EF Core Repository
 ↓
DbContext
 ↓
SQLite
```

### 3. What responsibility does the repository implementation now perform?

The repository implementation is responsible for communicating with the database through EF Core.

It performs operations such as:

* Retrieving students.
* Retrieving equipment.
* Finding available equipment.
* Creating borrowing records.
* Updating equipment availability.
* Returning equipment.
* Saving changes to the database.

It hides the database-specific implementation from the application services.

### 4. What is the purpose of an EF Core migration?

An EF Core migration records changes to the application's database model and provides instructions for creating or updating the database schema.

It allows the database structure to evolve together with the application's entity models in a controlled and repeatable way.

### 5. Why are foreign keys important in the borrowing database?

Foreign keys connect borrowing records to valid students and equipment.

For example:

```text
Borrowings.StudentId  → Students.Id
Borrowings.EquipmentId → Equipment.Id
```

They help maintain referential integrity and prevent borrowing records from referencing students or equipment that do not exist.

### 6. Why can a read-only query benefit from `AsNoTracking()`?

`AsNoTracking()` tells EF Core that the retrieved entities do not need to be tracked for later modification.

This can reduce tracking overhead for read-only operations because EF Core does not need to maintain those entities in its change tracker.

For example:

```csharp
dbContext.Equipment
    .AsNoTracking()
    .Where(equipment => equipment.IsAvailable)
```

For operations where an entity will be modified and saved, tracking can still be useful.

### 7. What would happen to the rest of the application if the SQLite implementation were replaced later by another database provider?

Most of the application could remain unchanged because the application services depend on repository interfaces rather than directly depending on SQLite.

The main changes would occur in the Infrastructure layer, such as:

* Changing the EF Core database provider.
* Updating the connection configuration.
* Adjusting database-specific configurations if necessary.
* Creating and applying the appropriate migrations.

The View, ViewModel, and application services could continue using the same repository abstractions.
