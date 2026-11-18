using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public enum BorrowingReviewScope { All, Student, Equipment }
public sealed record BorrowingScopeOption(string Label, BorrowingReviewScope Scope);
public sealed record BorrowingStatusOption(string Label, BorrowingStatus? Status);
public sealed record BorrowingOrderOption(string Label, BorrowingSortField Field, bool Descending);

public partial class BorrowingReviewViewModel(CatalogService catalog, BorrowingReviewService review) : ViewModelBase
{
    private static readonly BorrowingScopeOption[] scopes =
    [
        new("All borrowings", BorrowingReviewScope.All),
        new("Student history", BorrowingReviewScope.Student),
        new("Equipment history", BorrowingReviewScope.Equipment)
    ];

    private static readonly BorrowingStatusOption[] statuses =
    [
        new("All statuses", null),
        new("Active", BorrowingStatus.Active),
        new("Returned", BorrowingStatus.Returned)
    ];

    private static readonly BorrowingOrderOption[] orders =
    [
        new("Checkout date (newest first)", BorrowingSortField.CheckoutDate, true),
        new("Checkout date (oldest first)", BorrowingSortField.CheckoutDate, false),
        new("Expected return (latest first)", BorrowingSortField.ExpectedReturnDate, true),
        new("Expected return (earliest first)", BorrowingSortField.ExpectedReturnDate, false)
    ];

    public IReadOnlyList<BorrowingScopeOption> Scopes => scopes;
    public IReadOnlyList<BorrowingStatusOption> Statuses => statuses;
    public IReadOnlyList<BorrowingOrderOption> Orders => orders;

    private CancellationTokenSource? pending;
    private bool refreshingSelection;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSearch), nameof(IsStudentHistory), nameof(IsEquipmentHistory))]
    private BorrowingScopeOption? selectedScope = scopes[0];

    [ObservableProperty] private BorrowingStatusOption? selectedStatus = statuses[0];
    [ObservableProperty] private BorrowingOrderOption? selectedOrder = orders[0];
    [ObservableProperty] private string? searchText;
    [ObservableProperty] private IReadOnlyList<Borrowing> results = Array.Empty<Borrowing>();
    [ObservableProperty] private IReadOnlyList<Student> students = Array.Empty<Student>();
    [ObservableProperty] private IReadOnlyList<Equipment> equipment = Array.Empty<Equipment>();
    [ObservableProperty] private Student? selectedStudent;
    [ObservableProperty] private Equipment? selectedEquipment;
    [ObservableProperty] private string message = string.Empty;
    [ObservableProperty] private bool isLoading;

    private BorrowingReviewScope Scope => SelectedScope?.Scope ?? BorrowingReviewScope.All;
    public bool IsSearch => Scope == BorrowingReviewScope.All;
    public bool IsStudentHistory => Scope == BorrowingReviewScope.Student;
    public bool IsEquipmentHistory => Scope == BorrowingReviewScope.Equipment;

    partial void OnSelectedScopeChanged(BorrowingScopeOption? value)
    {
        if (value is not null)
            _ = LoadAsync(refreshSubjects: true);
    }

    partial void OnSelectedStudentChanged(Student? value)
    {
        if (IsStudentHistory && !refreshingSelection)
            _ = LoadAsync();
    }

    partial void OnSelectedEquipmentChanged(Equipment? value)
    {
        if (IsEquipmentHistory && !refreshingSelection)
            _ = LoadAsync();
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    public Task RefreshAsync() => LoadAsync(refreshSubjects: true);

    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task ApplyFiltersAsync() => LoadAsync();

    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task ClearFiltersAsync()
    {
        SearchText = null;
        SelectedStatus = statuses[0];
        SelectedOrder = orders[0];
        return LoadAsync();
    }

    private async Task LoadAsync(bool refreshSubjects = false)
    {
        using var request = new CancellationTokenSource();
        var previous = pending;
        pending = request;
        previous?.Cancel();

        var scope = Scope;
        IsLoading = true;
        Results = Array.Empty<Borrowing>();
        Message = "Loading borrowing records...";

        try
        {
            if (refreshSubjects)
                await RefreshSubjectsAsync(scope, request.Token);

            request.Token.ThrowIfCancellationRequested();
            var (rows, empty) = scope switch
            {
                BorrowingReviewScope.Student when SelectedStudent is { } student =>
                    (await review.GetStudentHistoryAsync(student.Id, request.Token), "This student has no borrowing history."),
                BorrowingReviewScope.Student => (Array.Empty<Borrowing>(), Students.Count == 0
                    ? "No students are registered." : "Select a student to view their borrowing history."),
                BorrowingReviewScope.Equipment when SelectedEquipment is { } item =>
                    (await review.GetEquipmentHistoryAsync(item.Id, request.Token), "This equipment has no borrowing history."),
                BorrowingReviewScope.Equipment => (Array.Empty<Borrowing>(), Equipment.Count == 0
                    ? "No equipment is registered." : "Select an item to view its borrowing history."),
                _ => (await review.SearchAsync(new BorrowingSearch(SearchText, SelectedStatus?.Status,
                    SelectedOrder?.Field ?? BorrowingSortField.CheckoutDate, SelectedOrder?.Descending ?? true), request.Token),
                    "No borrowings match these criteria.")
            };

            request.Token.ThrowIfCancellationRequested();
            if (rows is null)
            {
                Message = "That record is no longer available. Refresh and select another record.";
                return;
            }

            Results = rows;
            Message = rows.Count switch { 0 => empty, 1 => "1 borrowing record.", _ => $"{rows.Count} borrowing records." };
        }
        catch (OperationCanceledException) when (request.IsCancellationRequested)
        {
            // A newer selection or search owns the screen.
        }
        catch (StorageException error)
        {
            if (!request.IsCancellationRequested)
                Message = error.Message;
        }
        finally
        {
            if (ReferenceEquals(pending, request))
            {
                pending = null;
                IsLoading = false;
            }
        }
    }

    private async Task RefreshSubjectsAsync(BorrowingReviewScope scope, CancellationToken cancellationToken)
    {
        if (scope == BorrowingReviewScope.Student)
        {
            var selectedId = SelectedStudent?.Id;
            var rows = await catalog.GetStudentsAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            refreshingSelection = true;
            try
            {
                Students = rows;
                SelectedStudent = rows.FirstOrDefault(student => student.Id == selectedId);
            }
            finally
            {
                refreshingSelection = false;
            }
        }

        if (scope == BorrowingReviewScope.Equipment)
        {
            var selectedId = SelectedEquipment?.Id;
            var rows = await catalog.GetEquipmentAsync(cancellationToken: cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            refreshingSelection = true;
            try
            {
                Equipment = rows;
                SelectedEquipment = rows.FirstOrDefault(item => item.Id == selectedId);
            }
            finally
            {
                refreshingSelection = false;
            }
        }
    }
}
