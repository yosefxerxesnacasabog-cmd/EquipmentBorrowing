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

public partial class StudentsViewModel(CatalogService catalog, StudentManagementService management) : ViewModelBase
{
    private bool loading;
    private IReadOnlyList<Student> allStudents = Array.Empty<Student>();

    [ObservableProperty] private IReadOnlyList<Student> students = Array.Empty<Student>();
    [ObservableProperty] private Student? selectedStudent;
    [ObservableProperty] private string? search;
    [ObservableProperty] private string? studentNumber;
    [ObservableProperty] private string? fullName;
    [ObservableProperty] private string? contact;
    [ObservableProperty] private string message = string.Empty;
    [ObservableProperty] private string loadStatus = string.Empty;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool isLoaded;

    partial void OnSearchChanged(string? value) => ApplySearch();

    partial void OnSelectedStudentChanged(Student? value)
    {
        StudentNumber = value?.StudentNumber;
        FullName = value?.Name;
        Contact = value?.Contact;
    }

    [RelayCommand]
    private void NewStudent()
    {
        SelectedStudent = null;
        StudentNumber = FullName = Contact = null;
        Message = string.Empty;
    }

    [RelayCommand]
    public Task LoadStudentsAsync()
    {
        if (IsBusy || loading) return Task.CompletedTask;
        Message = string.Empty;
        return ReloadAsync(SelectedStudent?.Id, null);
    }

    private async Task ReloadAsync(int? id, string? number)
    {
        loading = true;
        IsLoaded = false;
        LoadStatus = "Loading students...";
        try
        {
            allStudents = await catalog.GetStudentsAsync();
            IsLoaded = true;
            ApplySearch();
            SelectedStudent = Students.FirstOrDefault(student => student.Id == id || student.StudentNumber == number);
        }
        catch (StorageException error)
        {
            allStudents = Students = Array.Empty<Student>();
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
        var id = SelectedStudent?.Id;
        Students = allStudents.Where(student => student.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
            || student.StudentNumber.Contains(term, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (id is int selectedId)
            SelectedStudent = Students.FirstOrDefault(student => student.Id == selectedId);
        if (IsLoaded)
            LoadStatus = Students.Count == 0 ? "No students match." : string.Empty;
    }

    [RelayCommand]
    private async Task SaveStudentAsync()
    {
        if (IsBusy || !IsLoaded)
            return;

        IsBusy = true;
        try
        {
            var id = SelectedStudent?.Id;
            var number = StudentNumber?.Trim();
            var result = await management.SaveAsync(SelectedStudent, StudentNumber, FullName, Contact);
            Message = result.Message;
            if (result.Succeeded)
                Search = string.Empty;
            if (result.Succeeded || result.RefreshRequired)
                await ReloadAsync(id, result.Succeeded && id is null ? number : null);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task PermitAsync() => ChangeEligibilityAsync(true);

    [RelayCommand]
    private Task RestrictAsync() => ChangeEligibilityAsync(false);

    private async Task ChangeEligibilityAsync(bool allowed)
    {
        if (IsBusy || !IsLoaded)
            return;

        IsBusy = true;
        try
        {
            var id = SelectedStudent?.Id;
            var result = await management.SetEligibilityAsync(SelectedStudent, allowed);
            Message = result.Message;
            if (result.Succeeded || result.RefreshRequired)
                await ReloadAsync(id, null);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
