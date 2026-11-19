using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel(CatalogService catalog) : ViewModelBase
{
    [ObservableProperty] private IReadOnlyList<Equipment> equipment = Array.Empty<Equipment>();
    [ObservableProperty] private string loadStatus = string.Empty;

    private bool availableOnly;
    private int loadVersion;

    [RelayCommand]
    private Task ShowAllAsync()
    {
        availableOnly = false;
        return LoadEquipmentAsync();
    }

    [RelayCommand]
    private Task ShowAvailableAsync()
    {
        availableOnly = true;
        return LoadEquipmentAsync();
    }

    [RelayCommand]
    public async Task LoadEquipmentAsync()
    {
        var version = ++loadVersion;
        var onlyAvailable = availableOnly;
        LoadStatus = "Loading equipment...";
        try
        {
            var items = await catalog.GetEquipmentAsync(onlyAvailable);
            if (version != loadVersion)
                return;
            Equipment = items;
            LoadStatus = items.Count > 0
                ? (onlyAvailable ? "Showing available equipment." : "Showing all equipment.")
                : (onlyAvailable ? "No equipment is currently available." : "The equipment catalog is empty.");
        }
        catch (StorageException error)
        {
            if (version != loadVersion)
                return;
            Equipment = Array.Empty<Equipment>();
            LoadStatus = error.Message;
        }
    }
}
