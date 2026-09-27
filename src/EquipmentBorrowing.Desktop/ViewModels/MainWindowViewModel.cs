using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public EquipmentViewModel EquipmentViewModel { get; }

    public BorrowingsViewModel BorrowingsViewModel { get; }

    public BorrowEquipmentViewModel BorrowEquipmentViewModel { get; }

    [ObservableProperty]
    private ViewModelBase currentView;

    public MainWindowViewModel(
        EquipmentViewModel equipmentViewModel,
        BorrowingsViewModel borrowingsViewModel,
        BorrowEquipmentViewModel borrowEquipmentViewModel)
    {
        EquipmentViewModel = equipmentViewModel;
        BorrowingsViewModel = borrowingsViewModel;
        BorrowEquipmentViewModel = borrowEquipmentViewModel;

        BorrowingsViewModel.EquipmentReturned += OnEquipmentReturned;

        currentView = EquipmentViewModel;
    }

    private async void OnEquipmentReturned(
        object? sender,
        EventArgs e)
    {
        await EquipmentViewModel.LoadEquipmentAsync();
    }

    [RelayCommand]
    private void ShowEquipment()
    {
        CurrentView = EquipmentViewModel;
    }

    [RelayCommand]
    private async Task ShowBorrowings()
    {
        await BorrowingsViewModel.LoadBorrowingsAsync();
        CurrentView = BorrowingsViewModel;
    }

    [RelayCommand]
    private void ShowBorrowEquipment()
    {
        CurrentView = BorrowEquipmentViewModel;
    }
}