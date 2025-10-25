using Autofac;
using WpfEngine.Demo.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfEngine.Demo.Views;

namespace WpfEngine.Demo.Views
{
    /// <summary>
    /// Interaction logic for DemoOrderListWindow.xaml
    /// </summary>
    public partial class DemoOrderListWindow : ScopedWindow
    {
        public DemoOrderListWindow(
            ILifetimeScope parentScope,
            ILogger<DemoOrderListWindow> logger)
            : base(parentScope, logger, "demo-order-list")
        {
            InitializeComponent();
            Loaded += async (s, e) => await OnLoadedAsync();
        }

        private async Task OnLoadedAsync()
        {
            if (DataContext is DemoOrderListViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
