using System;
using System.Diagnostics;
using System.Windows;
using Calibrator.WpfControl.Demo.Views;

namespace Calibrator.WpfControl.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Run automated tests on all views
        //RunViewTests();
        
        ShowTextBoxDemo(this, new()); // Show TextBox demo by default
    }

    private void RunViewTests()
    {
        try
        {
            var report = ViewTester.GetTestReport();
            Debug.WriteLine(report);
            Console.WriteLine(report);
            
            // Save report to file
            try
            {
                var reportPath = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    "Calibrator_Demo_Test_Report.txt");
                System.IO.File.WriteAllText(reportPath, report);
                Debug.WriteLine($"Test report saved to: {reportPath}");
            }
            catch { }
            
            // Also show in title if there are failures
            var results = ViewTester.TestAllViews();
            var failCount = 0;
            foreach (var (_, success, _) in results)
            {
                if (!success) failCount++;
            }
            
            if (failCount > 0)
            {
                Title = $"Calibrator WPF Controls Demo - ⚠ {failCount} view(s) failed tests";
            }
            else
            {
                Title = "Calibrator WPF Controls Demo - ✓ All views passed";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error running view tests: {ex.Message}");
            Title = "Calibrator WPF Controls Demo - ⚠ Test error";
        }
    }

    private void ShowButtonsDemo(object sender, RoutedEventArgs e)
    {
        ContentArea.Content = new ButtonsDemo();
    }

    private void ShowTextBoxDemo(object sender, RoutedEventArgs e)
    {
        ContentArea.Content = new TextBoxDemo();
    }

    private void ShowTextBlockDemo(object sender, RoutedEventArgs e)
    {
        ContentArea.Content = new TextBlockDemo();
    }

    private void ShowNumericDemo(object sender, RoutedEventArgs e)
    {
        ContentArea.Content = new NumericDemo();
    }

    private void ShowDropdownDemo(object sender, RoutedEventArgs e)
    {
        ContentArea.Content = new DropdownDemo();
    }

    private void ShowUniTableDemo(object sender, RoutedEventArgs e)
    {
        ContentArea.Content = new UniTableDemo();
    }

    private void ShowSmartTableDemo(object sender, RoutedEventArgs e)
    {
        ContentArea.Content = new SmartTableDemo();
    }

    private void ShowUniFormDemo(object sender, RoutedEventArgs e)
    {
        ContentArea.Content = new UniFormDemo();
    }

    private void ShowSmartContainerDemo(object sender, RoutedEventArgs e)
    {
        ContentArea.Content = new SmartContainerDemo();
    }
}
