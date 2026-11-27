# Lab 5 use-case, API and acceptance traceability

The [existing Lab 4 fourteen-case suite](lab4-use-case-suite.md) is preserved.
Lab 5 adds an HTTP boundary for UC-01–09 and UC-12–16 under BR-01–19.
The historical catalog's deferred cases are not added by this API activity.
All results below link to executed evidence in the
[consolidated acceptance record](lab5-test-results.md).

| Lab 4 use case | Applicable rules | Lab 5 API | Tests | Actual implementation | Executed evidence |
| --- | --- | --- | --- | --- | --- |
| UC-01 Catalog | BR-05–07,10,18–19 | API-01 | T01,T07 | `CatalogEndpoints.MapCatalogEndpoints`; `CatalogService.GetEquipmentAsync`; desktop `EquipmentViewModel` → `HttpEquipment.GetAllAsync` | [T01](evidence/lab5/T01.json), [UI catalog](evidence/lab5/T07-catalog.png) |
| UC-02 Available equipment | BR-10,18–19 | API-01 | T01,T07 | API `available` query; `HttpEquipment.GetAvailableAsync`; `EquipmentViewModel.ShowAvailableAsync` | [T01 available=true](evidence/lab5/T01.json), [UI workflow](evidence/lab5/T07.json) |
| UC-03 Single checkout | BR-08–11,14–16,18–19 | API-10,13 | T04,T07 | `WorkflowEndpoints.CheckoutAsync/GetBorrowingAsync`; `LendingService.CheckoutAsync`; `EfBorrowingRepository.CheckoutAsync`; `BorrowEquipmentViewModel` | [T04](evidence/lab5/T04.json), [UI active loan](evidence/lab5/T07-active.png), [fresh DB](evidence/lab5/T07.json) |
| UC-04 Active borrowings | BR-14,18–19 | API-14 | T01,T06,T07 | `WorkflowEndpoints.SearchAsync`; `EfBorrowingRepository.SearchAsync`; `HttpBorrowings.GetActiveAsync`; `BorrowingsViewModel` | [T01](evidence/lab5/T01.json), [T06](evidence/lab5/T06.json), [UI](evidence/lab5/T07-active.png) |
| UC-05 Return | BR-10,13–16,18–19 | API-12,13 | T04,T07 | `WorkflowEndpoints.ReturnAsync`; `LendingService.ReturnAsync`; `EfBorrowingRepository.ReturnAsync`; `BorrowingsViewModel.ReturnEquipmentAsync` | [T04](evidence/lab5/T04.json), [UI return](evidence/lab5/T07-return.png), [history](evidence/lab5/T07-history.png) |
| UC-06 Multiple checkout | BR-08–12,14–16,18–19 | API-11 | T05,T07 | `WorkflowEndpoints.BatchAsync`; `LendingService.CheckoutMultipleAsync`; same serializable repository checkout; `HttpBorrowings.CheckoutAsync` | [T05 boundaries/rollback](evidence/lab5/T05.json), [client code walkthrough](lab5-baseline-and-architecture.md) |
| UC-07 Student history | BR-14,18–19 | API-02,03,15 | T01,T06,T07 | `StudentHistoryAsync`; `EfBorrowingRepository.GetStudentHistoryAsync`; `HttpBorrowings.GetStudentHistoryAsync`; `BorrowingReviewViewModel` | [T01](evidence/lab5/T01.json), [T06](evidence/lab5/T06.json), [client mapping](evidence/lab5/client-check.txt) |
| UC-08 Equipment history | BR-14,18–19 | API-01,07,16 | T01,T06,T07 | `EquipmentHistoryAsync`; `EfBorrowingRepository.GetEquipmentHistoryAsync`; `HttpBorrowings.GetEquipmentHistoryAsync`; `BorrowingReviewViewModel` | [T01](evidence/lab5/T01.json), [T06](evidence/lab5/T06.json), [UI history](evidence/lab5/T07-history.png) |
| UC-09 Search/filter/sort | BR-17–19 | API-14 | T06,T07 | `WorkflowEndpoints.SearchAsync`; `BorrowingReviewService`; `EfBorrowingRepository.SearchAsync` (ILIKE and stable ThenBy ID); `BorrowingReviewViewModel` | [T06](evidence/lab5/T06.json), [UI history/search](evidence/lab5/T07-history.png) |
| UC-12 Register student | BR-01–02,16,18–19 | API-04 | T02,T07 | `CreateStudentAsync`; `StudentManagementService.SaveAsync`; `EfStudentRepository.RegisterAsync`; `StudentsViewModel` | [T02](evidence/lab5/T02.json), [UI student](evidence/lab5/T07-student.png) |
| UC-13 Update student | BR-01–03,14,16,18–19 | API-02,03,05 | T02,T07 | `UpdateStudentAsync`; `StudentManagementService.SaveAsync`; `EfStudentRepository.UpdateAsync`; `HttpStudents.UpdateAsync` | [T02](evidence/lab5/T02.json), [independent refresh](evidence/lab5/T07-independent-refresh.png), [DB](evidence/lab5/T07.json) |
| UC-14 Eligibility | BR-04,08,13–14,18–19 | API-02,03,06 | T02,T07 | `SetEligibilityAsync`; `StudentManagementService.SetEligibilityAsync`; `EfStudentRepository.SetEligibilityAsync`; `HttpStudents.SetEligibilityAsync` | [T02 eligibility/status/history](evidence/lab5/T02.json), [desktop wiring](../src/EquipmentBorrowing.Desktop/ViewModels/StudentsViewModel.cs) |
| UC-15 Register equipment | BR-05–06,10,16,18–19 | API-08 | T03,T07 | `CreateEquipmentAsync`; `EquipmentManagementService.SaveAsync`; `EfEquipmentRepository.RegisterAsync`; `EquipmentManagementViewModel` | [T03](evidence/lab5/T03.json), [UI equipment](evidence/lab5/T07-equipment.png) |
| UC-16 Update equipment | BR-05–07,10,14,16,18–19 | API-01,07,09 | T03,T07 | `UpdateEquipmentAsync`; `EquipmentManagementService.SaveAsync`; `EfEquipmentRepository.UpdateAsync`; `HttpEquipment.UpdateAsync` | [T03 protected fields/history](evidence/lab5/T03.json), [UI edit](evidence/lab5/T07-equipment.png) |

## Source index

- [API catalog endpoints](../src/EquipmentBorrowing.Api/CatalogEndpoints.cs),
  [workflow endpoints](../src/EquipmentBorrowing.Api/WorkflowEndpoints.cs),
  [request/response DTOs](../src/EquipmentBorrowing.Api/MutationDtos.cs),
  [create DTOs](../src/EquipmentBorrowing.Api/CreateDtos.cs),
  [OpenAPI schemas/statuses](../src/EquipmentBorrowing.Api/openapi.json).
- [Application services](../src/EquipmentBorrowing.Application/Services),
  [EF repositories](../src/EquipmentBorrowing.Infrastructure/Repositories),
  [transaction boundary](../src/EquipmentBorrowing.Infrastructure/Persistence/PostgresOperations.cs),
  [constraints/migration](../src/EquipmentBorrowing.Infrastructure/Persistence/Migrations/20261118125230_InitialPostgreSql.cs).
- [Desktop adapters](../src/EquipmentBorrowing.Desktop/Api/HttpRepositories.cs),
  [transport handling](../src/EquipmentBorrowing.Desktop/Api/ApiTransport.cs),
  [DTO mapping](../src/EquipmentBorrowing.Desktop/Api/ApiModels.cs),
  [ViewModels](../src/EquipmentBorrowing.Desktop/ViewModels).

## Shared architecture and failure requirements

| Requirement | Implementation | Evidence |
| --- | --- | --- |
| API independent of Avalonia, all 16 contracts | API host/DI, endpoints, OpenAPI | [T01](evidence/lab5/T01.json), T02–T06 responses, [architecture](lab5-baseline-and-architecture.md) |
| No direct desktop database access; async refresh | Desktop project references, App DI, HttpRepositories, async ViewModels | [T07 external update + DB](evidence/lab5/T07.json), [UI refresh](evidence/lab5/T07-independent-refresh.png) |
| Server validates current state; BR-08–12,15–16 | LendingService + serializable EF checkout + DB constraints | [T04](evidence/lab5/T04.json), [T05](evidence/lab5/T05.json) |
| Persistent relationships/history; BR-14 | Preserved FK IDs, read includes, no destructive return | T02–T04 restart snapshots; [T07](evidence/lab5/T07.json) |
| Safe failure and unknown outcome; BR-18–19 | ApiErrors, PostgresOperations, ApiTransport readback gate | [T08 wire](evidence/lab5/T08.json), [client gate simulation](evidence/lab5/client-check.txt), [consolidated diagnostics](lab5-test-results.md#failure-feedback-and-retest) |

API-10/11 share the same validation/transaction path. Source inspection supports
architecture claims; screenshots support UI claims; fresh database captures
support persistence/rollback. Evidence types are not interchangeable.
