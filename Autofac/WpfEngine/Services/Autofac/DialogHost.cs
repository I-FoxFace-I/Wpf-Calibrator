using Microsoft.Extensions.Logging;
using WpfEngine.Services;

namespace WpfEngine.Services.Autofac;

// -------------------------
// Implementation (singleton)
// -------------------------

public sealed class DialogHost : IDialogHost
{
    private readonly ILogger<DialogHost> _logger;
    private readonly IWindowIdentity _windowIdentity;
    private readonly IScopedWindowManager _windowManager;

    public Guid DialogId => _windowIdentity.WindowId;
    public DialogHost(ILogger<DialogHost> logger, IWindowIdentity windowIdentity, IScopedWindowManager windowManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _windowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
        _windowIdentity = windowIdentity ?? throw new ArgumentNullException(nameof(windowIdentity));
    }

    public void CloseDialog()
    {
        _logger.LogInformation("[DIALOG_SERVICE] Closing dialog {DialogId}", DialogId);

        var result = _windowManager.TryCloseWindow(DialogId);

        if (result.IsSuccess)
        {
            _logger.LogInformation("[DIALOG_SERVICE] Successfully closed Dialog {DialogId}.", DialogId);
        }
        else
        {
            _logger.LogError(result.Exception, "[DIALOG_SERVICE] Error when closing Dialog {DialogId}", DialogId);
        }
    }
}

