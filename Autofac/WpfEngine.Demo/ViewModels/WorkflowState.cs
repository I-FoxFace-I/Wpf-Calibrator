using WpfEngine.Data.Parameters;

namespace WpfEngine.Demo.ViewModels;

// ========== WORKFLOW STATE ==========

public record WorkflowState : BaseModelParameters
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<WorkflowOrderItem>? OrderItems { get; set; }
}
