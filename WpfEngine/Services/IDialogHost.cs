namespace WpfEngine.Services;

public interface IDialogHost
{
    Guid DialogId { get; }
    void CloseDialog();
}

