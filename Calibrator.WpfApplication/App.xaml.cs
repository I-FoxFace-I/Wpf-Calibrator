using Calibrator.WpfApplication.Features.ControllersOverview.Commands;
using Calibrator.WpfApplication.Features.ControllersOverview.Queries;
using Calibrator.WpfApplication.Features.EquipmentsOverview.Commands;
using Calibrator.WpfApplication.Features.EquipmentsOverview.Queries;
using Calibrator.WpfApplication.Features.EquipmentTemplatesOverview.Commands;
using Calibrator.WpfApplication.Features.EquipmentTemplatesOverview.Queries;
using Calibrator.WpfApplication.Features.MeasuringInstrumentsOverview.Commands;
using Calibrator.WpfApplication.Features.MeasuringInstrumentsOverview.Queries;
using Calibrator.WpfApplication.Features.TransducersOverview.Commands;
using Calibrator.WpfApplication.Features.TransducersOverview.Queries;
using Calibrator.WpfApplication.Infrastructure.Authentication;
using Calibrator.WpfApplication.Infrastructure.Persistence.Repositories;
using Calibrator.WpfApplication.Services;
using Calibrator.WpfApplication.ViewModels;
using Calibrator.WpfApplication.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using System.Windows.Threading;

namespace Calibrator.WpfApplication;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            })
            .Build();
        StartMonitoring();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IPromptDialogService, PromptDialogService>();
        services.AddSingleton<IWindowNavigationService, WindowNavigationService>();
        
        // Mock Repositories - TODO: Replace with actual implementations
        services.AddSingleton<IControllerRepository, MockControllerRepository>();
        services.AddSingleton<IEquipmentRepository, MockEquipmentRepository>();
        services.AddSingleton<IEquipmentTemplateRepository, MockEquipmentTemplateRepository>();
        services.AddSingleton<IMeasuringInstrumentRepository, MockMeasuringInstrumentRepository>();
        services.AddSingleton<ITransducerRepository, MockTransducerRepository>();
        
        // User context
        services.AddSingleton<UserIdentificationContext>();
        
        // Commands
        services.AddTransient<DeleteControllerCommand>();
        services.AddTransient<UpsertControllerCommand>();
        services.AddTransient<DeleteEquipmentCommand>();
        services.AddTransient<UpsertEquipmentCommand>();
        services.AddTransient<DeleteEquipmentTemplateCommand>();
        services.AddTransient<UpsertEquipmentTemplateCommand>();
        services.AddTransient<DeleteMeasuringInstrumentCommand>();
        services.AddTransient<UpsertMeasuringInstrumentCommand>();
        services.AddTransient<DeleteTransducerCommand>();
        services.AddTransient<UpsertTransducerCommand>();
        
        // Queries
        services.AddTransient<GetControllerQuery>();
        services.AddTransient<GetControllersQuery>();
        services.AddTransient<GetEquipmentQuery>();
        services.AddTransient<GetEquipmentsQuery>();
        services.AddTransient<GetEquipmentTemplateQuery>();
        services.AddTransient<GetEquipmentTemplatesQuery>();
        services.AddTransient<GetMeasuringInstrumentQuery>();
        services.AddTransient<GetMeasuringInstrumentsQuery>();
        services.AddTransient<GetTransducerQuery>();
        services.AddTransient<GetTransducersQuery>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ControllersOverviewViewModel>();
        services.AddTransient<EditControllerDialogViewModel>();
        services.AddTransient<EquipmentsOverviewViewModel>();
        services.AddTransient<EditEquipmentDialogViewModel>();
        services.AddTransient<EquipmentTemplatesOverviewViewModel>();
        services.AddTransient<EditEquipmentTemplateDialogViewModel>();
        services.AddTransient<MeasuringInstrumentsOverviewViewModel>();
        services.AddTransient<EditMeasuringInstrumentDialogViewModel>();
        services.AddTransient<TransducersOverviewViewModel>();
        services.AddTransient<EditTransducerDialogViewModel>();

        // Views
        services.AddTransient<MainWindow>();
        services.AddTransient<DashboardView>();
        services.AddTransient<ControllersOverviewView>();
        services.AddTransient<EquipmentsOverviewView>();
        services.AddTransient<EquipmentTemplatesOverviewView>();
        services.AddTransient<MeasuringInstrumentsOverviewView>();
        services.AddTransient<TransducersOverviewView>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            await _host!.StartAsync();

            base.OnStartup(e);

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _host.Services.GetService<MainWindowViewModel>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Application startup error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }

    private void StartMonitoring()
    {

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

    }

    private void StopMonitoring()
    {
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs unhandledException)
    {
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
    }

    private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
    }
}
