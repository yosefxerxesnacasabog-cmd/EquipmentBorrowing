
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowEquipmentViewModel : ViewModelBase
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly BorrowEquipmentService _borrowEquipmentService;

    public ObservableCollection<Student> Students { get; } = new();

    public ObservableCollection<Equipment> Equipment { get; } = new();

    [ObservableProperty]
    private Student? selectedStudent;

    [ObservableProperty]
    private Equipment? selectedEquipment;

    [ObservableProperty]
    private DateTimeOffset? expectedReturnDate;

    [ObservableProperty]
    private string message = string.Empty;

    public BorrowEquipmentViewModel(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        BorrowEquipmentService borrowEquipmentService)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowEquipmentService = borrowEquipmentService;

        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        var students = await _studentRepository.GetAllAsync();
        var equipment = await _equipmentRepository.GetAllAsync();

        Students.Clear();

        foreach (var student in students)
        {
            Students.Add(student);
        }

        Equipment.Clear();

        foreach (var item in equipment)
        {
            Equipment.Add(item);
        }
    }

    [RelayCommand]
    private async Task BorrowEquipmentAsync()
    {
        Message = string.Empty;

        if (SelectedStudent is null)
        {
            Message = "Please select a student.";
            return;
        }

        if (SelectedEquipment is null)
        {
            Message = "Please select equipment.";
            return;
        }

        if (ExpectedReturnDate is null)
        {
            Message = "Please select an expected return date.";
            return;
        }

        if (ExpectedReturnDate.Value.Date <= DateTime.Today)
        {
            Message = "Expected return date must be in the future.";
            return;
        }

        var success = await _borrowEquipmentService.BorrowEquipmentAsync(
            SelectedStudent.Id,
            SelectedEquipment.Id,
            ExpectedReturnDate.Value.DateTime);

        if (success)
        {
            Message = "Equipment borrowed successfully.";

            await LoadDataAsync();

            SelectedStudent = null;
            SelectedEquipment = null;
            ExpectedReturnDate = null;
        }
        else
        {
            if (!SelectedStudent.IsAllowedToBorrow)
            {
                Message = "This student is not allowed to borrow equipment.";
            }
            else if (!SelectedEquipment.IsAvailable)
            {
                Message = "This equipment is currently unavailable.";
            }
            else
            {
                Message = "Unable to borrow equipment.";
            }
        }
    }
}