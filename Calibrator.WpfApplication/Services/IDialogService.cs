namespace Calibrator.WpfApplication.Services;

public interface IDialogService
{
    void Open<TViewModel, TParameter>(TParameter parameter) where TViewModel : class;
    void Close<TViewModel>(TViewModel viewModel) where TViewModel : class;
}

public interface IDialogViewModel<TParameter>
{
    TParameter Parameter { get; set; }
}

