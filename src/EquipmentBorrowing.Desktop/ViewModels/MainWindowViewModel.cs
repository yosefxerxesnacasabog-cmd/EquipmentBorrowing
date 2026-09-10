using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        currentView = EquipmentViewModel;
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