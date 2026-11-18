using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
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

            services.AddDbContextFactory<EquipmentBorrowingDbContext>(PostgreSqlConfiguration.Configure);
            services.AddSingleton<PostgresOperations>();
            services.AddSingleton<EfStudentRepository>();
            services.AddSingleton<IStudentRepository>(provider => provider.GetRequiredService<EfStudentRepository>());
            services.AddSingleton<IStudentManagementRepository>(provider => provider.GetRequiredService<EfStudentRepository>());
            services.AddSingleton<StudentManagementService>();
            services.AddTransient<StudentsViewModel>();
            services.AddSingleton<IEquipmentRepository, EfEquipmentRepository>();
            services.AddSingleton<ILendingRepository, EfBorrowingRepository>();
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
