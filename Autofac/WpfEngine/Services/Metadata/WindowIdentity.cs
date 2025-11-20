namespace WpfEngine.Services.Metadata;

public sealed class WindowIdentity : IWindowIdentity
{
    public WindowIdentity(Guid windowId, Guid? parentId, Guid? sessionId, bool isDialog)
    {
        WindowId = windowId;
        ParentId = parentId;
        SessionId = sessionId;
        IsDialog = isDialog;
    }

    public Guid WindowId { get; }
    public Guid? ParentId { get; }
    public Guid? SessionId { get; }
    public bool IsDialog { get; }
}
