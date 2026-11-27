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

public partial class EquipmentManagementViewModel(CatalogService catalog, EquipmentManagementService management) : ViewModelBase
{
    private bool loading;
    private IReadOnlyList<Equipment> allItems = Array.Empty<Equipment>();

    [ObservableProperty] private IReadOnlyList<Equipment> items = Array.Empty<Equipment>();
    [ObservableProperty] private Equipment? selectedItem;
    [ObservableProperty] private string? search;
    [ObservableProperty] private string? identifier;
    [ObservableProperty] private string? itemName;
    [ObservableProperty] private string? description;
    [ObservableProperty] private string message = string.Empty;
    [ObservableProperty] private string loadStatus = string.Empty;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isLoaded;

    partial void OnSearchChanged(string? value) => ApplySearch();

    partial void OnSelectedItemChanged(Equipment? value)
    {
        Identifier = value?.Identifier;
        ItemName = value?.Name;
        Description = value?.Description;
    }

    [RelayCommand]
    private void NewItem()
    {
        SelectedItem = null;
        Identifier = ItemName = Description = null;
        Message = string.Empty;
    }

    [RelayCommand]
    public Task LoadItemsAsync()
    {
        if (IsBusy || loading) return Task.CompletedTask;
        Message = string.Empty;
        return ReloadAsync(SelectedItem?.Id, null);
    }

    private async Task ReloadAsync(int? id, string? code)
    {
        loading = true;
        IsLoaded = false;
        LoadStatus = "Loading equipment...";
        try
        {
            allItems = await catalog.GetEquipmentAsync();
            IsLoaded = true;
            ApplySearch();
            SelectedItem = Items.FirstOrDefault(item => item.Id == id || item.Identifier == code);
        }
        catch (StorageException error)
        {
            allItems = Items = Array.Empty<Equipment>();
            LoadStatus = error.Message;
        }
        finally
        {
            loading = false;
        }
    }

    private void ApplySearch()
    {
        var term = Search?.Trim() ?? string.Empty;
        var id = SelectedItem?.Id;
        Items = allItems.Where(item => item.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
            || item.Identifier.Contains(term, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (id is int selectedId)
            SelectedItem = Items.FirstOrDefault(item => item.Id == selectedId);
        if (IsLoaded)
            LoadStatus = Items.Count == 0 ? "No equipment matches." : string.Empty;
    }

    [RelayCommand]
    private async Task SaveItemAsync()
    {
        if (IsBusy || !IsLoaded)
            return;

        IsBusy = true;
        try
        {
            var id = SelectedItem?.Id;
            var code = Identifier?.Trim();
            var result = await management.SaveAsync(SelectedItem, Identifier, ItemName, Description);
            Message = result.Message;
            if (result.Succeeded)
                Search = string.Empty;
            if (result.Succeeded || result.RefreshRequired)
                await ReloadAsync(id, result.Succeeded && id is null ? code : null);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
