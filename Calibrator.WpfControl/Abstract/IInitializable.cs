using System.Threading.Tasks;

namespace Calibrator.WpfControl.Abstract;

/// <summary>
/// Interface for objects that require asynchronous initialization
/// </summary>
public interface IInitializable
{
    /// <summary>
    /// Asynchronously initializes the object
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation</returns>
    Task InitializeAsync();
}