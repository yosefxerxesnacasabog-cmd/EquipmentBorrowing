using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel(
    EquipmentViewModel equipment,
    BorrowingsViewModel borrowings,
    BorrowEquipmentViewModel checkout,
    StudentsViewModel students,
    EquipmentManagementViewModel equipmentManagement) : ViewModelBase
{
    public EquipmentViewModel EquipmentViewModel { get; } = equipment;

    [ObservableProperty] private ViewModelBase currentView = equipment;

    [RelayCommand]
    private async Task ShowEquipmentAsync()
    {
        CurrentView = EquipmentViewModel;
        await EquipmentViewModel.LoadEquipmentAsync();
    }

    [RelayCommand]
    private async Task ShowBorrowingsAsync()
    {
        CurrentView = borrowings;
        await borrowings.LoadBorrowingsAsync();
    }

    [RelayCommand]
    private async Task ShowBorrowEquipmentAsync()
    {
        CurrentView = checkout;
        await checkout.LoadDataAsync();
    }
    [RelayCommand]
    private async Task ShowStudentsAsync()
    {
        CurrentView = students;
        await students.LoadStudentsAsync();
    }

    [RelayCommand]
    private async Task ShowEquipmentManagementAsync()
    {
        CurrentView = equipmentManagement;
        await equipmentManagement.LoadItemsAsync();
    }
}
