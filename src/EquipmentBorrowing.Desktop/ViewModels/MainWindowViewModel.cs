using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public EquipmentViewModel EquipmentViewModel { get; }

    public BorrowingsViewModel BorrowingsViewModel { get; }

    [ObservableProperty]
    private ViewModelBase currentView;

    public MainWindowViewModel(
        EquipmentViewModel equipmentViewModel,
        BorrowingsViewModel borrowingsViewModel)
    {
        EquipmentViewModel = equipmentViewModel;
        BorrowingsViewModel = borrowingsViewModel;

        currentView = EquipmentViewModel;
    }

    [RelayCommand]
    private void ShowEquipment()
    {
        CurrentView = EquipmentViewModel;
    }

    [RelayCommand]
    private void ShowBorrowings()
    {
        CurrentView = BorrowingsViewModel;
    }
}