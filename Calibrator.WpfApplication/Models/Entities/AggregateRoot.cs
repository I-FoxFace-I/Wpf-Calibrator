namespace Calibrator.WpfApplication.Models.Entities;

public abstract class AggregateRoot
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public bool IsNew { get; set; }
}


