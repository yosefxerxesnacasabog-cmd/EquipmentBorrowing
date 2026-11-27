using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowEquipmentViewModel(CatalogService catalog, LendingService lending) : ViewModelBase
{
    [ObservableProperty] private IReadOnlyList<Student> students = Array.Empty<Student>();
    [ObservableProperty] private IReadOnlyList<Equipment> equipment = Array.Empty<Equipment>();
    [ObservableProperty] private IReadOnlyList<CheckoutItemViewModel> checkoutItems = Array.Empty<CheckoutItemViewModel>();
    [ObservableProperty] private bool multipleItems;
    [ObservableProperty] private Student? selectedStudent;
    [ObservableProperty] private Equipment? selectedEquipment;
    [ObservableProperty] private DateTimeOffset? expectedReturnDate;
    [ObservableProperty] private string message = string.Empty;
    [ObservableProperty] private string loadStatus = string.Empty;
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(BorrowEquipmentCommand))] private bool isReady;

    private bool CanBorrow() => IsReady;

    [RelayCommand]
    public Task LoadDataAsync()
    {
        Message = string.Empty;
        return ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        IsReady = false;
        LoadStatus = "Loading students and equipment...";
        var studentId = SelectedStudent?.Id;
        var equipmentId = SelectedEquipment?.Id;
        var chosenIds = CheckoutItems.Where(choice => choice.IsSelected).Select(choice => choice.Item.Id).ToHashSet();
        try
        {
            var students = await catalog.GetStudentsAsync();
            var equipment = await catalog.GetEquipmentAsync(availableOnly: true);
            Students = students;
            Equipment = equipment;
            CheckoutItems = equipment.Select(item => new CheckoutItemViewModel(item, chosenIds.Contains(item.Id))).ToArray();
            SelectedStudent = students.FirstOrDefault(student => student.Id == studentId);
            SelectedEquipment = equipment.FirstOrDefault(item => item.Id == equipmentId);
            IsReady = students.Count > 0 && equipment.Count > 0;
            LoadStatus = students.Count == 0 ? "No students are registered."
                : equipment.Count == 0 ? "No equipment is currently available." : string.Empty;
        }
        catch (StorageException error)
        {
            Students = Array.Empty<Student>();
            Equipment = Array.Empty<Equipment>();
            CheckoutItems = Array.Empty<CheckoutItemViewModel>();
            LoadStatus = error.Message;
        }
    }

    [RelayCommand(CanExecute = nameof(CanBorrow))]
    private async Task BorrowEquipmentAsync()
    {
        var result = MultipleItems
            ? await lending.CheckoutMultipleAsync(SelectedStudent?.Id,
                CheckoutItems.Where(choice => choice.IsSelected).Select(choice => choice.Item.Id).ToArray(),
                ExpectedReturnDate?.Date)
            : await lending.CheckoutAsync(SelectedStudent?.Id, SelectedEquipment?.Id, ExpectedReturnDate?.Date);
        Message = result.Message;
        if (result.Succeeded)
        {
            SelectedStudent = null;
            SelectedEquipment = null;
            ExpectedReturnDate = null;
            foreach (var choice in CheckoutItems)
                choice.IsSelected = false;
        }

        if (result.Succeeded || result.RefreshRequired)
            await ReloadAsync();
    }
}
