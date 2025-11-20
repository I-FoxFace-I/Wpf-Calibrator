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
using WpfEngine.ViewModels;
using WpfEngine.Enums;
using WpfEngine.Views.Windows;

namespace WpfEngine.Demo.Views.Product.Repository
{
    /// <summary>
    /// Interaction logic for DemoProductCreateDialogWindow.xaml
    /// </summary>
    public partial class ProductCreateDialogWindow : WpfEngine.Views.Windows.ScopedDialogWindow
    {
        public ProductCreateDialogWindow(ILogger<ProductCreateDialogWindow> logger) : base(logger)
        {
            InitializeComponent();
        }

        public override DialogType DialogType => DialogType.Custom;

        public override string? AppModule => "Demo";
    }


}
