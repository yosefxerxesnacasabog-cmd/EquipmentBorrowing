using CommunityToolkit.Mvvm.ComponentModel;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class CheckoutItemViewModel(Equipment item, bool selected) : ObservableObject
{
    public Equipment Item { get; } = item;
    [ObservableProperty] private bool isSelected = selected;
}
