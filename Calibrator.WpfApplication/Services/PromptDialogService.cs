using System.Threading.Tasks;
using System.Windows;

namespace Calibrator.WpfApplication.Services;

public class PromptDialogService : IPromptDialogService
{
    public Task<bool> AskForConfirmation(string message)
    {
        var result = MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    public void Alert(string message)
    {
        MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public Task Alert<TViewModel>(TViewModel viewModel, string message) where TViewModel : class
    {
        Alert(message);
        return Task.CompletedTask;
    }
}

