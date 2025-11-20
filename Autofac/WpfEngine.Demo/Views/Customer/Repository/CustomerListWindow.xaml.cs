using Autofac;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Windows;
using WpfEngine.Demo.ViewModels.Workflow;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.Views;
using WpfEngine.Views.Windows;
using WpfEngine.Views;

namespace WpfEngine.Demo.Views.Customer.Repository;

public partial class CustomerListWindow : WpfEngine.Views.Windows.ScopedWindow
{
    public CustomerListWindow(ILogger<CustomerListWindow> logger)
        : base(logger)
    {
        InitializeComponent();
    }
}