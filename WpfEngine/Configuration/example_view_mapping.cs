//using WpfEngine.Core.Services;
//using WpfEngine.ViewModels;
//using WpfEngine.ViewModels.Demo;
//using WpfEngine.Views;
//using WpfEngine.Views.Demo;

//namespace WpfEngine.Configuration;

///// <summary>
///// Example View mapping configuration
///// Register all ViewModel -> View mappings here
///// </summary>
//public class ExampleViewMappingConfiguration : ViewMappingConfiguration
//{
//    public override void Configure(IViewRegistry registry)
//    {
//        // ========== MAIN WINDOWS ==========
//        registry.MapWindow<MainViewModel, MainWindow>();

//        // ========== DEMO WINDOWS ==========
//        registry.MapWindow<AdvancedDemoMenuViewModel, AdvancedDemoMenuWindow>();
//        registry.MapWindow<DemoCustomerListViewModel, DemoCustomerListWindow>();
//        registry.MapWindow<DemoProductListViewModel, DemoProductListWindow>();

//        // ========== DEMO DIALOGS ==========
//        registry.MapDialog<DemoCustomerDetailViewModel, DemoCustomerDetailWindow>();
//        registry.MapDialog<DemoProductDetailViewModel, DemoProductDetailWindow>();

//        // ========== WORKFLOW ==========
//        // Workflow host window
//        registry.MapShell<DemoWorkflowHostViewModel, DemoWorkflowHostWindow>();

//        // Workflow steps (UserControls)
//        registry.MapControl<DemoWorkflowStep1ViewModel, DemoWorkflowStep1View>();
//        registry.MapControl<DemoWorkflowStep2ViewModel, DemoWorkflowStep2View>();
//        registry.MapControl<DemoWorkflowStep3ViewModel, DemoWorkflowStep3View>();

//        // ========== ADD MORE MAPPINGS AS NEEDED ==========
//        // registry.MapWindow<YourViewModel, YourWindow>();
//        // registry.MapDialog<YourDialogViewModel, YourDialogWindow>();
//        // registry.MapControl<YourStepViewModel, YourStepView>();
//    }
//}

///// <summary>
///// Usage in App.xaml.cs:
///// 
///// protected override void OnStartup(StartupEventArgs e)
///// {
/////     var builder = new ContainerBuilder();
/////     
/////     // Register modules
/////     builder.RegisterModule<CoreServicesModule>();
/////     
/////     // Register View mapping configuration
/////     builder.RegisterViewMappingConfiguration<ExampleViewMappingConfiguration>();
/////     
/////     // Build container
/////     var container = builder.Build();
/////     
/////     // Configure View mappings
/////     container.ConfigureViewMappings();
/////     
/////     // ... rest of startup
///// }
///// </summary>
