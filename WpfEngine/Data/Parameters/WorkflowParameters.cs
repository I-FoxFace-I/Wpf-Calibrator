using System;
using System.CodeDom;

namespace WpfEngine.Data.Parameters;

/// <summary>
/// Parameters for workflow ViewModels
/// </summary>
public record WorkflowParameters : BaseModelParameters
{
    public Guid WorkflowId { get; init; }
    public object? InitialData { get; init; } = null;

    public WorkflowParameters(Guid workflowId, object? initialData = null)
    {
        WorkflowId = workflowId;
        InitialData = initialData;
    }
    public WorkflowParameters()
    {
        
    }
}

