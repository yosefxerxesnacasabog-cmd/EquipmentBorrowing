# Campus Equipment Borrowing System

## Laboratory Activity 1

**ITSD 81 – Desktop Application Development**

**Prepared by:**

Yosef Xerxes S. Nacasabog  
Jazz Dwight W. Salac

**August 2026**

---

# Part A – Requirements and Use Case Analysis

## A. Actors

### Actor: Student

**What the actor expects the system to do:**

The student expects the system to allow them to request available equipment, borrow equipment when the requirements are satisfied, and return borrowed equipment.

---

## B. Use Cases

### Use Case 1 – Borrow Equipment

| Item | Description |
|---|---|
| **Use Case** | **Borrow Equipment** |
| **Primary Actor** | Student |
| **Preconditions** | The student is currently allowed to borrow equipment; the requested equipment exists; the equipment is available; and the student has not reached the maximum number of active borrowings. |
| **Main Action** | The student requests to borrow a piece of equipment. The system verifies the student's eligibility, checks that the equipment exists and is available, checks the student's active borrowing limit, and records the borrowing if all conditions are satisfied. |
| **Expected Result** | The borrowing is successfully recorded with the student, equipment, date borrowed, expected return date, and borrowing status. The equipment becomes unavailable for another borrowing. |
| **Possible Failure** | The student is not allowed to borrow; the equipment does not exist; the equipment is unavailable; or the student has reached the maximum number of active borrowings. |

### Use Case 2 – Return Equipment

| Item | Description |
|---|---|
| **Use Case** | **Return Equipment** |
| **Primary Actor** | Student |
| **Preconditions** | The student has an existing active borrowing for the equipment being returned. |
| **Main Action** | The student returns the borrowed equipment. The system identifies the active borrowing, marks the borrowing as returned, and changes the equipment's status to available. |
| **Expected Result** | The borrowing is marked as **Returned**, and the equipment becomes available for another student to borrow. |
| **Possible Failure** | The student does not have an active borrowing for the equipment being returned, or the borrowing record cannot be found. |

### Use Case 3 – Find Available Equipment

| Item | Description |
|---|---|
| **Use Case** | **Find Available Equipment** |
| **Primary Actor** | Student |
| **Preconditions** | Equipment records are available in the system. |
| **Main Action** | The student requests available equipment. The system checks the equipment records and identifies equipment that is currently available for borrowing. |
| **Expected Result** | The system provides the student with the available equipment that can potentially be borrowed. |
| **Possible Failure** | No equipment is available, or the requested equipment does not exist. |

---

## C. Identify Domain Concepts

| Domain Concept | Information It Contains | Rules / State | Should NOT Be Responsible For |
|---|---|---|---|
| **Student** | Student ID, name, borrowing eligibility | Whether the student is allowed to borrow | Database operations, UI, repository operations |
| **Equipment** | Equipment ID, name, availability | Whether the equipment is currently available | Database operations, UI, borrowing workflow |
| **Borrowing** | Student, equipment, date borrowed, expected return date, status | Active or Returned | Database operations, UI, finding unrelated equipment |
| **BorrowingStatus** | Active, Returned | Represents the current state of a borrowing | Database access, UI, application workflow |

---

# Part B – Architecture Explanation

## 1. Solution Structure

### Domain

The **Domain** project contains the important concepts and states of the Campus Equipment Borrowing System.

Examples include:

- `Student`
- `Equipment`
- `Borrowing`
- `BorrowingStatus`

The Domain represents the problem itself and does not handle database operations or user-interface code.

### Application

The **Application** project contains the operations performed by the system.

The main implemented application service is:

```text
BorrowEquipmentService
```

It coordinates the domain objects and repository interfaces needed to process a borrowing request.

### Infrastructure

The **Infrastructure** project contains the technical implementations of the repository interfaces.

The current implementation uses in-memory repositories:

- `InMemoryStudentRepository`
- `InMemoryEquipmentRepository`
- `InMemoryBorrowingRepository`

No database is used for this laboratory activity.

### Tests

The **Tests** project contains the automated xUnit tests used to verify the borrowing service and its different outcomes.

### Console Demonstration

The project also contains `BorrowingSystemDemo`, which provides a simple console demonstration of the implemented borrowing use case.

The console program only demonstrates the application flow. The actual business logic remains in the Application project.

---

## 2. Dependency Direction

The solution separates the responsibilities between the projects.

```text
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

The Application layer depends on repository abstractions instead of directly depending on the repository implementations.

The Infrastructure layer implements those repository abstractions.

This allows the storage implementation to be changed later without changing the main application service.

---

## 3. Use Case Mapping

The implemented use case is **Borrow Equipment**.

| Item | Implementation |
|---|---|
| **Actor** | Student |
| **Use Case** | Borrow Equipment |
| **Application Service** | `BorrowEquipmentService` |
| **Domain Objects Used** | `Student`, `Equipment`, `Borrowing`, `BorrowingStatus` |
| **Repository Interfaces Used** | `IStudentRepository`, `IEquipmentRepository`, `IBorrowingRepository` |
| **Infrastructure Implementations Used** | `InMemoryStudentRepository`, `InMemoryEquipmentRepository`, `InMemoryBorrowingRepository` |

---

## 4. Reflection

### 1. Why should the application service depend on a repository interface instead of directly depending on a database implementation?

The application service should depend on a repository interface because it should focus on the business operation instead of how the data is stored. This also makes the system easier to change and test.

### 2. Which parts of your current solution could remain unchanged if SQLite were added later?

The Domain models, Application service, and repository interfaces could remain mostly unchanged. A new SQLite repository implementation could be added in the Infrastructure project.

### 3. Which project would eventually contain Avalonia Views?

Avalonia Views would eventually be placed in a separate presentation or desktop UI project. Avalonia is not required for this laboratory activity.

### 4. Should an Avalonia button directly execute database queries? Why or why not?

No. An Avalonia button should call an application service instead of directly executing database queries. This keeps the user interface, business logic, and data access responsibilities separated.

### 5. What part of your implementation represents the actual business operation requested by the actor?

`BorrowEquipmentService.BorrowEquipmentAsync()` represents the main business operation because it validates the borrowing request and creates the borrowing record when the required conditions are satisfied.

---

# Application Flow Demonstration

The implemented **Borrow Equipment** use case is demonstrated through a simple console program.

## Successful Case

```text
[TEST 1] Successful Borrowing Case
Student ID: 1 requests Equipment ID: 1

Result: SUCCESS
Borrowing record created successfully.
```

This demonstrates a successful request where the student is allowed to borrow and the requested equipment is available.

## Failure Case

```text
[TEST 2] Student Not Allowed Case
Student ID: 3 requests Equipment ID: 1

Result: FAILED
Student is not allowed to borrow equipment.
```

This demonstrates that the application rejects a borrowing request when the student is not allowed to borrow.

---

# Testing

The project uses xUnit for automated testing.

Current test result:

```text
Test summary: total: 7, failed: 0, succeeded: 7, skipped: 0
```

All seven automated tests passed successfully.

---

# Git Development

The project was developed using multiple Git commits.

The development history includes commits for the solution structure, domain models, repository abstractions, services, in-memory repositories, and automated tests.


---

# Current Status

The solution builds successfully and all automated tests currently pass.

The project demonstrates:

- C# and .NET
- Separation of responsibilities
- Domain models
- Repository abstractions
- In-memory repository implementations
- Application service
- Manual dependency injection
- Automated testing
- Successful borrowing demonstration
- Failure case demonstration
- Git-based development

The current implemented use case is **Borrow Equipment**. Return Equipment and Find Available Equipment were identified during the requirements analysis but are not part of the current implemented use case.