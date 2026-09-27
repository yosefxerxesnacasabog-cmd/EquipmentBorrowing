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

## SQLite and Entity Framework Core

Laboratory Activity 3 extends the existing architecture from Activities 1 and 2 by replacing the in-memory repository implementations with database-backed repositories using Entity Framework Core and SQLite.

The application now follows this architecture:

```text
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
EF Core Repository
    |
    v
EquipmentBorrowingDbContext
    |
    v
SQLite Database
````

The Domain and Application layers remain separated from database-specific implementation details.

## Relational Database Design

The SQLite database contains three main tables:

- `Students` 
- `Equipment` 
- `Borrowings` 

The relationships are:

```
```

```
Students 1 -------- * Borrowings * -------- 1 Equipment
```

Each borrowing references one student and one equipment item through foreign keys.

The database design is documented in:

```
```

```
docs/database-diagram.png
```

The required SQL examples are documented in:

```
```

```
docs/database-queries.sql
```

## Entity Framework Core DbContext

The Infrastructure project contains:

```
```

```
EquipmentBorrowingDbContext
```

The DbContext provides the following DbSets:

```
```

```
Students
Equipment
Borrowings
```

Entity configurations are located under:

```
```

```
src/EquipmentBorrowing.Infrastructure/Persistence/Configurations
```

The configurations define:

-  Primary keys 
-  Required properties 
-  Maximum string lengths 
-  Foreign keys 
-  Relationships 
-  Delete behavior 
-  Indexes 
-  Borrowing status enum conversion 
-  Initial seed data 

## EF Core Migrations

The database schema is created and updated through EF Core migrations.

The migration history includes:

```
```

```
InitialCreate
SeedInitialData
```

The migrations create the required SQLite tables, foreign keys, indexes, and seed data.

The SQLite database is not recreated every time the application starts. Existing data is preserved.

## Database-Backed Repositories

The Infrastructure project now uses:

```
```

```
EfStudentRepository
EfEquipmentRepository
EfBorrowingRepository
```

The existing repository abstractions are preserved for the Application layer. The `IBorrowingRepository` abstraction was extended with `UpdateAsync` to support the return workflow.

The repositories support the required workflows:

-  Retrieving equipment 
-  Retrieving students 
-  Retrieving active borrowings 
-  Creating a borrowing 
-  Updating equipment state 
-  Returning equipment 
-  Saving changes 

EF Core asynchronous operations are used where appropriate, including:

```
```

```
FirstOrDefaultAsync()
ToListAsync()
CountAsync()
AddAsync()
SaveChangesAsync()
```

## Persistence and Application Workflow

The Avalonia application now uses SQLite data through Entity Framework Core.

The application supports:

-  Viewing equipment 
-  Viewing students during borrowing 
-  Borrowing available equipment 
-  Preventing borrowing of unavailable equipment 
-  Preventing unauthorized students from borrowing 
-  Viewing active borrowings 
-  Returning equipment 
-  Persisting borrowing and return changes in SQLite 

The application continues to use the Application services and repository abstractions. The Views and ViewModels do not directly access the DbContext or execute SQL.

## LINQ Queries

The EF Core repositories contain meaningful LINQ queries for retrieving and processing persistent data.

### Query 1 -- Count Active Borrowings

```
```

```
await _dbContext.Borrowings
    .CountAsync(
        borrowing =>
            borrowing.StudentId == studentId &&
            borrowing.Status == BorrowingStatus.Active,
        cancellationToken);
```

This query counts the active borrowing records belonging to a particular student.

### Query 2 -- Retrieve Active Borrowings

```
```

```
await _dbContext.Borrowings
    .Include(borrowing => borrowing.Student)
    .Include(borrowing => borrowing.Equipment)
    .AsNoTracking()
    .Where(borrowing => borrowing.Status == BorrowingStatus.Active)
    .ToListAsync(cancellationToken);
```

This query retrieves active borrowing records together with their related student and equipment information.

### Query 3 -- Find Active Borrowing by Equipment

```
```

```
await _dbContext.Borrowings
    .Include(borrowing => borrowing.Student)
    .Include(borrowing => borrowing.Equipment)
    .FirstOrDefaultAsync(
        borrowing =>
            borrowing.EquipmentId == equipmentId &&
            borrowing.Status == BorrowingStatus.Active,
        cancellationToken);
```

This query finds the active borrowing associated with a specific equipment item.

## EF Core Generated SQL

Two LINQ queries were inspected using EF Core's `ToQueryString()` to verify the SQL generated by Entity Framework Core.

### Generated SQL Query 1 -- Active Borrowings

LINQ:

```
```

```
var query1 = dbContext.Borrowings
    .Include(borrowing => borrowing.Student)
    .Include(borrowing => borrowing.Equipment)
    .AsNoTracking()
    .Where(borrowing => borrowing.Status == BorrowingStatus.Active);
```

Generated SQL:

```
```

```
SELECT "b"."Id", "b"."DateBorrowed", "b"."EquipmentId",
       "b"."ExpectedReturnDate", "b"."Status", "b"."StudentId",
       "s"."Id", "s"."IsAllowedToBorrow", "s"."Name",
       "e"."Id", "e"."IsAvailable", "e"."Name"
FROM "Borrowings" AS "b"
INNER JOIN "Students" AS "s" ON "b"."StudentId" = "s"."Id"
INNER JOIN "Equipment" AS "e" ON "b"."EquipmentId" = "e"."Id"
WHERE "b"."Status" = 'Active'
```

The generated SQL joins the Borrowings table with Students and Equipment and filters the results to active borrowings.

### Generated SQL Query 2 -- Active Borrowing by Equipment

LINQ:

```
```

```
var query2 = dbContext.Borrowings
    .Include(borrowing => borrowing.Student)
    .Include(borrowing => borrowing.Equipment)
    .Where(borrowing =>
        borrowing.EquipmentId == 1 &&
        borrowing.Status == BorrowingStatus.Active);
```

Generated SQL:

```
```

```
SELECT "b"."Id", "b"."DateBorrowed", "b"."EquipmentId",
       "b"."ExpectedReturnDate", "b"."Status", "b"."StudentId",
       "s"."Id", "s"."IsAllowedToBorrow", "s"."Name",
       "e"."Id", "e"."IsAvailable", "e"."Name"
FROM "Borrowings" AS "b"
INNER JOIN "Students" AS "s" ON "b"."StudentId" = "s"."Id"
INNER JOIN "Equipment" AS "e" ON "b"."EquipmentId" = "e"."Id"
WHERE "b"."EquipmentId" = 1 AND "b"."Status" = 'Active'
```

The generated SQL joins the related Student and Equipment records and filters the result by equipment ID and active borrowing status.

## Tracking and No-Tracking

The repository uses tracking according to the purpose of each operation.

Queries used for display-only data use:

```
```

```
AsNoTracking()
```

For example, retrieving all equipment and students does not require entity tracking because the data is only displayed by the UI.

The active borrowing list also uses `AsNoTracking()` because it is used for display.

Queries used when an entity must be modified use normal tracking. For example, borrowing retrieves the Student and Equipment as tracked entities so that changes such as marking equipment as borrowed can be saved through `SaveChangesAsync()`.

The return workflow also retrieves the active borrowing as a tracked entity because the borrowing and equipment state are modified before saving.

## Seed Data

The database contains initial Student and Equipment records through EF Core seed data.

The seed data includes:

-  Students who are allowed to borrow 
-  A student who is not allowed to borrow 
-  Available equipment 
-  Equipment that is initially unavailable 

The seed data is applied through an EF Core migration and is not recreated on every application startup.

## Persistence Verification

The application uses the SQLite database rather than the previous in-memory repositories.

Persistence should be verified by creating a borrowing, closing and reopening the application, and confirming that the borrowing remains stored in the SQLite database.

A return operation changes the borrowing status to Returned and makes the equipment available again.

## Activity 3 Evidence

The Activity 3 submission includes evidence for:

-  SQLite database tables 
-  Stored Student, Equipment, and Borrowing data 
-  Successful borrowing 
-  Borrowing persistence after application restart 
-  Successful return 
-  Inspected EF Core generated SQL queries 
-  Successful .NET build 

---

