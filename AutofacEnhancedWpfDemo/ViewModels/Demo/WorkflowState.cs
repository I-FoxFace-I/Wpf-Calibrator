namespace AutofacEnhancedWpfDemo.ViewModels.Demo;

// ========== WORKFLOW STATE ==========

public class WorkflowState
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public List<WorkflowOrderItem>? OrderItems { get; set; }
}
