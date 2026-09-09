using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System.Collections.ObjectModel;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ViewModelBase
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly ReturnEquipmentService _returnEquipmentService;

    public ObservableCollection<Borrowing> Borrowings { get; } = new();

    [ObservableProperty]
    private Borrowing? selectedBorrowing;

    public BorrowingsViewModel(
        IBorrowingRepository borrowingRepository,
        ReturnEquipmentService returnEquipmentService)
    {
        _borrowingRepository = borrowingRepository;
        _returnEquipmentService = returnEquipmentService;
    }

    public async Task LoadBorrowingsAsync()
    {
        var borrowings = await _borrowingRepository.GetActiveAsync();

        Borrowings.Clear();

        foreach (var borrowing in borrowings)
        {
            Borrowings.Add(borrowing);
        }
    }

    [RelayCommand]
    private async Task ReturnEquipmentAsync()
    {
        if (SelectedBorrowing is null)
        {
            return;
        }

        var success = await _returnEquipmentService.ReturnEquipmentAsync(
            SelectedBorrowing.Equipment.Id);

        if (success)
        {
            await LoadBorrowingsAsync();
        }
    }
}