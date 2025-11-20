using System;
using System.Threading;
using System.Threading.Tasks;

namespace WpfEngine.Services;

public interface IWindowIdentity
{
    Guid WindowId { get; }
    Guid? ParentId { get; }
    Guid? SessionId { get; }
    bool IsDialog { get; }
}
