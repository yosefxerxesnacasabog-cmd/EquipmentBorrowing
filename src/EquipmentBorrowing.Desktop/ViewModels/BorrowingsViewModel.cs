using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel(LendingService lending) : ViewModelBase
{
    [ObservableProperty] private IReadOnlyList<Borrowing> borrowings = Array.Empty<Borrowing>();
    [ObservableProperty] private Borrowing? selectedBorrowing;
    [ObservableProperty] private string message = string.Empty;
    [ObservableProperty] private string loadStatus = string.Empty;
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ReturnEquipmentCommand))] private bool isReady;

    private bool CanReturn() => IsReady;

    [RelayCommand]
    public async Task LoadBorrowingsAsync()
    {
        IsReady = false;
        LoadStatus = "Loading active borrowings...";
        var selectedId = SelectedBorrowing?.Id;
        try
        {
            var borrowings = await lending.GetActiveAsync();
            Borrowings = borrowings;
            SelectedBorrowing = borrowings.FirstOrDefault(loan => loan.Id == selectedId);
            IsReady = borrowings.Count > 0;
            LoadStatus = IsReady ? string.Empty : "There are no active borrowings.";
        }
        catch (StorageException error)
        {
            Borrowings = Array.Empty<Borrowing>();
            LoadStatus = error.Message;
        }
    }

    [RelayCommand(CanExecute = nameof(CanReturn))]
    private async Task ReturnEquipmentAsync()
    {
        var result = await lending.ReturnAsync(SelectedBorrowing?.Id);
        Message = result.Message;
        if (result.Succeeded || result.RefreshRequired)
            await LoadBorrowingsAsync();
    }
}
