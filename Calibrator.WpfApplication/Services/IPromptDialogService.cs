using System.Threading.Tasks;

namespace Calibrator.WpfApplication.Services;

public interface IPromptDialogService
{
    Task<bool> AskForConfirmation(string message);
    void Alert(string message);
    Task Alert<TViewModel>(TViewModel viewModel, string message) where TViewModel : class;
}

