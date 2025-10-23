using Calibrator.WpfApplication.ViewModels;
using Calibrator.WpfApplication.Views.Base;

namespace Calibrator.WpfApplication.Views;

public partial class MeasuringInstrumentsOverviewView : BaseView
{
    public MeasuringInstrumentsOverviewView(MeasuringInstrumentsOverviewViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            await viewModel.InitializeAsync();
        };
    }
}

