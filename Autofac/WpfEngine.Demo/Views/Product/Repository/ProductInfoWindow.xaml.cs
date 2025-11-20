using Autofac;
using WpfEngine.Demo.ViewModels.Product.Repository;
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

namespace WpfEngine.Demo.Views.Product.Repository
{
    /// <summary>
    /// Interaction logic for DemoProductInfoWindow.xaml
    /// </summary>
    public partial class ProductInfoWindow : WpfEngine.Views.Windows.ScopedWindow
    {
        public ProductInfoWindow(ILogger<ProductInfoWindow> logger)
            : base(logger)
        {
            InitializeComponent();
            Loaded += async (s, e) => await OnLoadedAsync();
        }

        private async Task OnLoadedAsync()
        {
            if (DataContext is ProductInfoViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}


