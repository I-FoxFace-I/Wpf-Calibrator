using System.Threading.Tasks;
using Autofac;
using AutofacEnhancedWpfDemo.ViewModels.Demo;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Views.Demo;

// ========== ADVANCED DEMO MENU WINDOW ==========

//public partial class AdvancedDemoMenuWindow : ScopedWindow
//{
//    public AdvancedDemoMenuWindow(
//        ILifetimeScope parentScope,
//        ILogger<AdvancedDemoMenuWindow> logger)
//        : base(parentScope, logger, "advanced-demo-menu")
//    {
//        InitializeComponent();
//    }
//}

// ========== DEMO CUSTOMER LIST WINDOW ==========

//public partial class DemoCustomerListWindow : ScopedWindow
//{
//    public DemoCustomerListWindow(
//        ILifetimeScope parentScope,
//        ILogger<DemoCustomerListWindow> logger)
//        : base(parentScope, logger, "demo-customer-list")
//    {
//        InitializeComponent();
//        Loaded += async (s, e) => await OnLoadedAsync();
//    }
    
//    private async Task OnLoadedAsync()
//    {
//        if (DataContext is DemoCustomerListViewModel vm)
//        {
//            await vm.InitializeAsync();
//        }
//    }
//}

// ========== DEMO CUSTOMER DETAIL WINDOW ==========

//public partial class DemoCustomerDetailWindow : ScopedWindow
//{
//    public DemoCustomerDetailWindow(
//        ILifetimeScope parentScope,
//        ILogger<DemoCustomerDetailWindow> logger)
//        : base(parentScope, logger, "demo-customer-detail")
//    {
//        InitializeComponent();
//        Loaded += async (s, e) => await OnLoadedAsync();
//    }
    
//    private async Task OnLoadedAsync()
//    {
//        if (DataContext is DemoCustomerDetailViewModel vm)
//        {
//            await vm.InitializeAsync();
//        }
//    }
//}

// ========== DEMO PRODUCT LIST WINDOW ==========

//public partial class DemoProductListWindow : ScopedWindow
//{
//    public DemoProductListWindow(
//        ILifetimeScope parentScope,
//        ILogger<DemoProductListWindow> logger)
//        : base(parentScope, logger, "demo-product-list")
//    {
//        InitializeComponent();
//        Loaded += async (s, e) => await OnLoadedAsync();
//    }
    
//    private async Task OnLoadedAsync()
//    {
//        if (DataContext is DemoProductListViewModel vm)
//        {
//            await vm.InitializeAsync();
//        }
//    }
//}

// ========== DEMO WORKFLOW HOST WINDOW ==========


