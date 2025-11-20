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
using WpfEngine.Abstract;
using WpfEngine.Enums;
using WpfEngine.Views.Windows;

namespace WpfEngine.Demo.Views.Customer.Repository
{
    /// <summary>
    /// Interaction logic for CreateCustomerDialogWindow.xaml
    /// </summary>
    public partial class CreateCustomerDialogWindow : ScopedDialogWindow
    {
        public CreateCustomerDialogWindow(ILogger<CreateCustomerDialogWindow> logger) : base(logger)
        {
            InitializeComponent();
            // Generate unique window ID
            //AssignedWindowId = Guid.NewGuid();

            Logger.LogInformation("[CREATE_ADDRESS_DIALOG_WINDOW] Window created with ID");

            // Subscribe to window events
            base.Loaded += OnLoaded;
            base.Closed += OnClosed;
        }

        // ========== IScopedView Implementation ==========

        //public Guid AssignedWindowId { get; }

        //// ========== IDialogView Implementation ==========

        //public Guid WindowId => AssignedWindowId;

        public override DialogType DialogType => DialogType.Custom;

        public override string? AppModule => "Demo";

        // ========== Event Handlers ==========

        protected void OnLoaded(object sender, RoutedEventArgs e)
        {
            Logger.LogInformation("[CREATE_ADDRESS_DIALOG_WINDOW] Window loaded");
            if (DataContext is IInitializable vm)
            {
                vm.InitializeAsync().GetAwaiter().GetResult();
            }
        }

        protected void OnClosed(object? sender, EventArgs e)
        {
            Logger.LogInformation("[CREATE_ADDRESS_DIALOG_WINDOW] Window closed with result: {DialogResult}",
                base.DialogResult);

            // Cleanup
            base.Loaded -= OnLoaded;
            base.Closed -= OnClosed;
        }
    }
}
