using CommunityToolkit.Mvvm.ComponentModel;
using WpfEngine.Data.Parameters;

namespace WpfEngine.Demo.ViewModels.Parameters.ViewModels.Parameters;

// ========== WORKFLOW STATE ==========

public record WorkflowStepParameters : BaseModelParameters
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<WorkflowOrderItem>? OrderItems { get; set; }
}