namespace Calibrator.WpfApplication.Services;

public interface IWindowNavigationService
{
    void NavigateTo<TViewModel>() where TViewModel : IWindowNavigatableViewModel;
}
