using Calibrator.WpfApplication.ViewModels;

namespace Calibrator.WpfApplication.Views;

public partial class TransducersOverviewView
{
    public TransducersOverviewView(TransducersOverviewViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            await viewModel.InitializeAsync();
        };
    }
}