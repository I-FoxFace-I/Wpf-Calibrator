using System.Threading.Tasks;

namespace Calibrator.WpfApplication.ViewModels;

public interface IInitializable
{
    Task InitializeAsync();
}
