using System.Threading.Tasks;

namespace Calibrator.WpfControl.ViewModels.Base;

public interface IInitializable
{
    Task InitializeAsync();
}
