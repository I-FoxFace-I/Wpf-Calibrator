using WpfEngine.Core.ViewModels;

namespace WpfEngine.Demo.ViewModels;

// ========== WORKFLOW STATE ==========

public record WorkflowState : ViewModelOptions
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<WorkflowOrderItem>? OrderItems { get; set; }
}
