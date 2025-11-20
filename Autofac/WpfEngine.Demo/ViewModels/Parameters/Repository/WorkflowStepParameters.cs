using WpfEngine.Data.Parameters;

namespace WpfEngine.Demo.ViewModels.Parameters.Repository;

// ========== WORKFLOW STATE ==========

public record WorkflowStepParameters : BaseModelParameters
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<WorkflowOrderItem>? OrderItems { get; set; }
}
