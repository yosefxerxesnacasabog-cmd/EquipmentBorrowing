using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Api;
using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            var address = Environment.GetEnvironmentVariable("EQUIPMENT_BORROWING_API_URL") ?? "http://127.0.0.1:5185/api/v1/";
            if (!Uri.TryCreate(address.TrimEnd('/') + "/", UriKind.Absolute, out var baseUri) ||
                (baseUri.Scheme != "http" && baseUri.Scheme != "https"))
                throw new InvalidOperationException("Set EQUIPMENT_BORROWING_API_URL to an absolute HTTP API address.");
            services.AddSingleton(new HttpClient { BaseAddress = baseUri, Timeout = TimeSpan.FromSeconds(15) });
            services.AddSingleton<ApiTransport>();

            services.AddSingleton<HttpStudents>();
            services.AddSingleton<IStudentRepository>(provider => provider.GetRequiredService<HttpStudents>());
            services.AddSingleton<IStudentManagementRepository>(provider => provider.GetRequiredService<HttpStudents>());
            services.AddSingleton<StudentManagementService>();
            services.AddTransient<StudentsViewModel>();
            services.AddSingleton<HttpEquipment>();
            services.AddSingleton<IEquipmentRepository>(provider => provider.GetRequiredService<HttpEquipment>());
            services.AddSingleton<IEquipmentManagementRepository>(provider => provider.GetRequiredService<HttpEquipment>());
            services.AddSingleton<EquipmentManagementService>();
            services.AddTransient<EquipmentManagementViewModel>();
            services.AddSingleton<HttpBorrowings>();
            services.AddSingleton<ILendingRepository>(provider => provider.GetRequiredService<HttpBorrowings>());
            services.AddSingleton<IBorrowingReviewRepository>(provider => provider.GetRequiredService<HttpBorrowings>());
            services.AddSingleton<BorrowingReviewService>();
            services.AddTransient<BorrowingReviewViewModel>();
            services.AddSingleton<CatalogService>();
            services.AddSingleton<LendingService>();
            services.AddTransient<EquipmentViewModel>();
            services.AddTransient<BorrowingsViewModel>();
            services.AddTransient<BorrowEquipmentViewModel>();
            services.AddTransient<MainWindowViewModel>();

            var provider = services.BuildServiceProvider();
            desktop.Exit += (_, _) => provider.Dispose();
            var viewModel = provider.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow { DataContext = viewModel };
            desktop.MainWindow.Opened += async (_, _) => await viewModel.EquipmentViewModel.LoadEquipmentAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
