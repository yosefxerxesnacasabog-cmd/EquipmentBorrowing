using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    private ServiceProvider? _serviceProvider;
    private IServiceScope? _serviceScope;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        services.AddDbContext<EquipmentBorrowingDbContext>(options =>
            options.UseSqlite("Data Source=equipmentborrowings.db"));

        services.AddScoped<IStudentRepository, EfStudentRepository>();
        services.AddScoped<IEquipmentRepository, EfEquipmentRepository>();
        services.AddScoped<IBorrowingRepository, EfBorrowingRepository>();

        services.AddScoped<BorrowEquipmentService>();
        services.AddScoped<ReturnEquipmentService>();

        services.AddTransient<EquipmentViewModel>();
        services.AddTransient<BorrowingsViewModel>();
        services.AddTransient<BorrowEquipmentViewModel>();
        services.AddTransient<MainWindowViewModel>();

        _serviceProvider = services.BuildServiceProvider();
        _serviceScope = _serviceProvider.CreateScope();

        var dbContext =
            _serviceScope.ServiceProvider.GetRequiredService<EquipmentBorrowingDbContext>();

        dbContext.Database.Migrate();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel =
                _serviceScope.ServiceProvider.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}