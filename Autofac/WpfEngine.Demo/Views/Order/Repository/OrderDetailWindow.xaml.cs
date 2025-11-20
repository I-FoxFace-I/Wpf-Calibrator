using Autofac;
using WpfEngine.Demo.ViewModels.Order.Repository;
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
using WpfEngine.Views.Windows;

namespace WpfEngine.Demo.Views.Order.Repository
{
    /// <summary>
    /// Interaction logic for DemoOrderDetailWindow.xaml
    /// </summary>
    public partial class OrderDetailWindow : WpfEngine.Views.Windows.ScopedWindow
    {
        public OrderDetailWindow(ILogger<OrderDetailWindow> logger)
            : base(logger)
        {
            InitializeComponent();
            Loaded += async (s, e) => await OnLoadedAsync();
        }

        private async Task OnLoadedAsync()
        {
            if (DataContext is OrderDetailViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}


