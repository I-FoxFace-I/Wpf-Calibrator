using Calibrator.WpfApplication.Models.Dtos;
using Calibrator.WpfApplication.Models.Enums;

namespace Calibrator.WpfApplication.Models.Entities;

public class Controller : AggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public ToolConnectionMethod ConnectionMethod { get; set; }
    public ControllerType Type { get; set; }

    public static Controller CreateNew() => new();

    public void Upsert(UpsertControllerDto dto)
    {
        if (dto.Id != Guid.Empty) Id = dto.Id;
        Name = dto.Name;
        SerialNumber = dto.SerialNumber;
        Identifier = dto.Identifier;
        ConnectionMethod = dto.ConnectionMethod;
        Type = dto.Type;
    }
}


